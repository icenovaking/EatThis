## Context

useNearbyFood 的 recommend 會先清空 place，App.vue 隨即移除結果卡片。這是頁高下降造成瀏覽器捲動位置被限縮的候選原因，尚未經真實手機重現。使用者明確選擇全程保持原本位置，包括成功後不自動捲到結果。現有 Google adapter 已取得 rating，但未取得評論數。

## Design Source

使用者提供的「新增評論數.png」是現況問題截圖，不是像素規格或替換設計。沒有 .dc.html 或設計 bundle；不從手機截圖推算 CSS 像素。視覺沿用 DESIGN.md 與現有評分列、字體、色彩及間距。

## Goals / Non-Goals

In scope：搜尋狀態切換的捲動穩定性、評論數從 Google 到 API 到卡片的傳遞、回歸驗證與設計文件。
Out of scope：評論內容、評論數篩選或排序、自动捲動到結果、改變推薦機率或歷史規則、新增端點、部署與提交。

## Decisions

### 維持結果區高度與使用者捲動自主權

由 App.vue 擁有持續存在的回饋與結果容器，在會改變內容的 Vue 更新之前量測自然高度，保留此頁面生命週期的高度高水位作為 min-height；新結果較高時增加，較短、錯誤或重試時不立即縮回。保留的是空間，不是可操作的舊餐廳。原本 place 清除及失敗不顯示舊結果的語意不變。初次 idle 不預留結果空間，維持既有精簡頁面契約。

不使用固定卡片高度，不累加每次量測的 padding；避免量測已套用 min-height 導致循環成長。以實際內容量測或獨立內層容器計算自然高度；DOM 量測、觀察器與卸載清理皆留在前端呈現層。不要新增只轉呼叫的抽象模組。結果區的 scroll anchoring 必須受控，避免瀏覽器因替換內容自動改變 scrollY；是否需要限制 overflow-anchor 由真實瀏覽器驗證決定。

不得呼叫 scrollIntoView、主動 focus 或以搜尋開始的 scrollY 在完成時強制還原，後者會覆蓋使用者等待期間的自行捲動。必要時僅針對同次 DOM 更新的非使用者位移補償，不加入平滑捲動。視窗旋轉或縮放可以重新量測，不宣稱能控制瀏覽器原生權限視窗造成的移動。

### 沿用現有契約傳遞評論數

GooglePlacesProvider 的 field mask 增加 places.userRatingCount，在 GooglePlace 讀取可選 JSON 值，將 0 至 2147483647 的整數正規化為可空 int。缺少、null、負數、小數、字串或溢位皆變成 null，單一無效評論数不使整個搜尋失敗。

NearbyFoodContracts 的 PlaceCandidate 與 NearbyFoodResponse 加入可空 UserRatingCount（必要時使用尾端可選參數保留既有呼叫）；Program 的成功映射傳遞數值，前端 PlaceResult 加入 userRatingCount?: number | null。既有 IPlaceProvider、NearbyFoodService 的選店演算法不需新增抽象或變更。Google API key、field mask 仍由後端擁有，每次搜尋維持同一次上游請求，不補查 Details。

官方欄位為含有文字及僅評分的總數：https://developers.google.com/maps/documentation/places/web-service/reference/rest/v1/places 。沒有文字評論清單下載。

### 評分列並列數量與缺值處理

App.vue 評分列以既有星號與分數搭配「（1,234 則評論）」；數量使用 zh-TW 千分位，不用 K 縮寫。0 顯示「（0 則評論）」；缺少或無效數量省略整段括號。無 rating 時保留「尚無評分」，若 count 有效仍顯示數量。320 CSS px 可自然換行，不水平溢出。前端亦檢查整數與範圍，以相容旧後端及不合約的回應。

## Implementation Contract

- 固定 viewport、沒有使用者捲動時，各搜尋狀態 DOM 更新前後 scrollY 差距不得超過 1 CSS px；不得短暫跳頂再捲回。等待期間使用者自行捲動後，以其最新位置為準。
- idle→locating→searching→selected、selected→重試→較短或較長結果，以及 no-results、permission-denied、unsupported-geolocation、provider-error、rate-limited 均維持位置與現有回饋、復原文案。
- 初始 idle 不預留空白。頁面已有結果後允許保留底部空間以避免收縮；重載重設，不寫入 localStorage。持續 live region 宣告狀態，不主動搶焦點；忙碌時仍阻擋重複請求。
- API 成功 JSON 新增 userRatingCount: integer|null；不改請求或錯誤格式。評論數不影響候選資格、排序、随机選擇或歷史記錄。
- 以 provider/endpoint/fixture 測試驗證數值傳遞與異常值，以前端測試驗證文案及狀態。真實 iOS Safari、Android Chrome 與桌面瀏覽器驗證版面更新前後位置及連續重試；jsdom 不作為捲動或觸控證據。

## Risks / Trade-offs

- 保留高度會在短結果或失敗下留下空白 → 接受此取捨以滿足全程不跳動；不以主動捲動隱藏空白。
- 使用者回報可能另有焦點或瀏覽器原因 → apply 先重現並記錄狀態與 scrollY，再確認修復，不能把候選原因寫成已證實。
- 無真實瀏覽器 → 可完成自動測試與程式，但人工驗收必須明確標為未執行。

## Migration Plan

先提供後端可空欄位再發布前端；前端相容缺少欄位的舊回應。失敗時回退相應前端與 API 版本，無資料遷移。建置更新追蹤中的 dist 資源。

## Open Questions

無需使用者補充的產品決策。捲動根因與瀏覽器實測結果由實作驗證記錄。
