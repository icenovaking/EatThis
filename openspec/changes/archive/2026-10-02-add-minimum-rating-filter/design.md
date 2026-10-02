## Context

本變更為 Feature，承接已實作但尚未封存的 refine-nearby-food-search。目前 App.vue 提供 100..3000 公尺選擇，useNearbyFood 在按鈕動作快照距離；NearbyFoodService.PickAsync 取得候選、驗證、去重後抽選。GooglePlacesProvider 的固定 field mask 尚無評分，PlaceCandidate 和 SearchRequest 尚無評分欄位。沿用既有 PRODUCT.md 與 DESIGN.md 的 Operate 介面，只增加使用者明確指定的黃色星星，不重做視覺世界。

## Goals / Non-Goals

**Goals / In scope:** 五顆星的 0.5 精度最低評分控制、完整前後端有界契約、供應商中立評分、抽選前篩選、評分顯示、條件快照、無結果復原、無障礙與手機操作。

**Non-Goals / Out of scope:** 評論數門檻、評論內容、評分排序、收藏、列表、多次 Google 請求補足候選、Text Search 遷移、儲存偏好、自動放寬條件、改動距離範圍或完成前一變更的剩餘視覺驗證。

## Decisions

### 以五顆星提供半星選擇

App.vue 在搜尋距離與主要動作之間呈現五顆同形 SVG 星星，十個原生 radio 選項的 label 覆蓋各星左右半部；同一具名 radio group 另有「不限評分」選項。第 n 顆左半對應 n-0.5、右半對應 n，使用十個明確值，不以滑鼠座標運算閾值。預設 null，全部灰色；已選區域持久填黃色，半星裁切精確至一半，旁邊顯示「4.5 星以上」。不以 hover 狀態覆蓋持久選取文字。採 radio 而非 dropdown 或只靠拖曳，讓手機直接操作半星、键盤原生方向鍵遞增／遞減 0.5、Space 選取、Tab 進出群組。箭頭抵達不限選項時使用 null，不把 0 當 API 值。每個選項可供輔助技術讀出「最低評分 4.5 星以上」，顯示 focus，星形對輔助技術隱藏。

styles.css 使用共享評分色 token 與可伸縮佈局，保留既有玉綠主動作；黃色是評分語意色。每個半星 label 至少 24 CSS px 寬與 44 CSS px 高，整排十個半星能在 320px 頁面中容納，reset 和文字必要時換行。不得以很小的 icon 充當實際 touch target。

### 以可空半星契約保存搜尋快照

NearbyFoodRequest 與 NearbySearchQuery 新增 double? MinRating，置於現有參數尾端並給 null 預設；SearchRequest 新增 minRating?: number | null，PlaceCandidate 與 PlaceResult 新增 nullable rating，既有 C# constructor 呼叫保持相容。缺省／null 不限，只有 0.5、1、1.5、2、2.5、3、3.5、4、4.5、5 可成為門檻。NearbySearchQuery.TryCreate 驗證 finite、上下限與半星倍數；錯誤數值或 JSON 型別回 HTTP 400 invalid_request 且 provider 零呼叫，不把 4.3 門檻四捨五入。Program.cs 的 pick endpoint 改為在 rate-limit 之後以 HttpRequest.ReadFromJsonAsync<NearbyFoodRequest> 讀取 JSON，捕捉 JsonException／不支援的 content type 對應 HTTP 400 ApiErrorResponse.InvalidRequest；取消訊號不吞掉，不改其他 endpoint。這是必要的 request binding 修正，因目前 typed body binding 的錯誤不能保證回專案 errorCode。保留原座標及半徑驗證與 3000 公尺上限。

useNearbyFood 新增 selectedMinRating 與最後提交的條件快照；recommend 在任何 await 前快照 radiusMeters 與 minRating，通過驗證後才定位。search 使用快照值，不在定位完成後重讀待搜尋值。使用者仍可在 busy 時調整下一次條件，但當次結果與 no-results 描述使用提交值。調整星星或 reset 不能呼叫 GPS／pick。API serializer 直接傳遞 minRating；缺省舊 client 和舊 fixture rating 缺省由前端視為未評分。

### 取得正規化評分後才篩選抽選

GooglePlacesProvider 固定 mask 增加 places.rating，GooglePlace 使用可空數字，MapCandidate 轉為 rating；有限 1..5 評分保留原始值（例如 4.3），missing、null、越界數字轉 null，合法店家不因缺評分而整筆被丟棄。不請求 userRatingCount 或 reviews，也不增加 provider-specific 公開參數。1..5 以外或 non-finite 的替代 provider 評分在 PlaceCandidateRules 不視為有效 candidate，null 則合法，避免 NaN／Infinity 進入 JSON；Google adapter 先正規化後再套共同規則。

NearbyFoodService.PickAsync 順序為可用性驗證、minRating inclusive 篩選、NavigationUrl 去重、IRandomSource 抽選。門檻為 null 時不按評分排除；門檻非空時必須有 rating 且 rating >= minRating。條件全集保留後均勻抽選，不優先最高分。零候選回原 HTTP 404 no_results，且不呼叫 random source。比對原始數值，不以展示文字或半星 rounding 決定資格。

### 顯示評分並保留無結果條件

結果以文字呈現供應商的實際數字，例如「★ 4.3」，缺值顯示「尚無評分」，不可假造 0 星或把 4.3 改成 4.5。結果分數不是可操作輸入。不更動名稱、地址、外部 navigationUrl 或官方 attribution。no-results 使用最後提交快照顯示搜尋距離及「4.5 星以上」／「不限評分」；有門檻時提示降低評分，未到 3000 時可提示增加距離，已達上限則提示降低門檻或明確重試，不做自動 API call。不宣稱範圍內不存在合格店家，只表示本次未找到。

### 以增量交付管理計費與規格依賴

Nearby Search 沒有 minRating 參數，因此留在既有 application service 篩選，不新增 pass-through service、storage 或 provider adapter。契約邊界由 NearbySearchQuery 和 PlaceCandidate 擁有，唯一外部 adapter 為 GooglePlacesProvider；它實際負責 Google JSON、distance、localization、評分正規化，不是薄轉送。移除此 adapter 將中斷 Google 搜尋；新增轉送層無必要。

根據 Google 文件 https://developers.google.com/maps/documentation/places/web-service/nearby-search，固定要求 rating 將目前 Pro 欄位請求提升至 Enterprise，包含不限評分搜尋；先文件揭露，不增加自動請求或用戶端計費流程。最多 20 個候選仍沿用，不能保證全範圍合格餐廳皆被納入。先完成並封存 refine-nearby-food-search 再封存本變更，合併其單一結果/no-results 規格內容，不恢復基準文件尚存的 5000 retry。Implementation 可從目前工作樹開始，先重查現行程式與前一變更狀態。

## Implementation Contract

- 接口範例：POST /api/nearby-food/pick 的 JSON 為 {"latitude":25.033,"longitude":121.5654,"radiusMeters":700,"minRating":4.5}。成功回既有全部 normalized 欄位以及 rating:4.7，缺評分則 rating:null。
- 行為範例：候選 A=3.9、B=4.0、C=4.3、D=null，minRating=4 只能抽 B 或 C；null 時四者均能參與；minRating=4.5 回 no_results。
- 錯誤：0、-0.5、5.5、4.3、字串、布林、非有限／非 JSON 數值不能觸發 provider；穩定 HTTP 400 invalid_request。不存在合格候選回 HTTP 404 no_results；credential、quota、rate-limit 與定位錯誤維持原契約。
- 驗收：API endpoint、provider mapping、RandomSelectionTests、candidate normalization 與 contract fixture 測試覆蓋上述值；frontend tests 覆蓋十個選項／不限、左右半星、無副作用、async 快照、busy 重複提交防止、missing rating fallback 與 no-results 文案。
- 手動驗收：320px、390px、desktop 的 touch／mouse 半星點選、keyboard 方向鍵／Space／Tab、focus、黃色半星、讀屏名稱、long result、no-results 與無水平 overflow，保存截圖。測試工具無法提供的 browser／輔助技術結果必須明列未驗證，不得把 DOM assertion 當 browser pass。
- In scope / Out of scope 依 Goals / Non-Goals；實作前依 AGENTS.md 檢查 CodeGraph 索引，有索引則 impact 後才修改程式；沒有索引不得擅自建立。UI 修改前讀 Impeccable craft-floor；.NET tests 由 Test-Runner sub-agent、API build 由 Build-Fixer sub-agent 執行。

## Risks / Trade-offs

- [計費級別升高] → README 明列 rating 使用 Enterprise；維持一次動作一次 provider 請求與 rate limit。
- [20 候選可能漏掉範圍內合格店] → no-results 用「本次未找到」，文件揭露限制，不追加擴充搜尋。
- [半星 touch target 與手機寬度] → 每半星 24x44 以上、文字可換行，批次檢查 320px／390px／desktop。
- [與前一變更 delta 重疊] → 先封存前一變更再封存本變更，重讀完整 requirement 並保留距離、新視覺與 attribution 規格。
- [既有 client／fixture 沒有評分] → 可空欄位與 constructor default，不限請求維持行為，展示缺值 fallback。
- [await 期間条件改變造成誤導] → 所有 API 和結果狀態依提交快照，pending 值只用於下一次按鈕。

## Migration Plan

先補失敗契約案例，再後端與前端實作；backend additive 契約先部署，接著 frontend（避免新 frontend 對舊 backend 靜默失去篩選）。更新 fixtures、產品與計費文件及 production dist。回滾先 frontend，再 backend；停用 UI 不會刪除資料，沒有 storage migration。提案驗證不執行測試或部署。

## Open Questions

無阻擋設計的未決問題。預設不限、0.5..5.0、null 評分行為與黃色填色由本提案明確定義。
