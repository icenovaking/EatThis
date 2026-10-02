## Context

EatThis 以 Vue mobile web 與 ASP.NET Core API 實作單次定位、附近搜尋與單一隨機推薦。目前 GooglePlacesProvider 固定六種餐飲 includedTypes，搜尋上限 20 筆，並取得評分；前端距離與評分已具備 pending/submitted 條件快照。本提案採上一輪建議的單選分類，沒有新增地圖、資料庫或套件。

## Goals / Non-Goals

**Goals:** 提供 10 個產品分類與不限類型；在一次 Google 搜尋內限制候選類型；完整保存本次條件並提供明確無結果恢復流程；維持鍵盤與手機可操作性。

**Non-Goals:** 多選、排除分類、自訂關鍵字、便當／宵夜等沒有此次明確對照的分類、分類等機率抽選、偏好持久化、分類顯示在店家資料、其他 provider 實作、照片／評論／營業狀態、調整 API 免費額度、修改距離與評分既有邊界。

## Decisions

### 以產品分類代碼建立受限契約

公開 request 使用 optional nullable string restaurantCategory；省略或 null 代表不限，不使用字串 all。有效代碼採下表精確小寫值，空字串、前後空白、大小寫不同及其他值均拒絕，不修剪或猜測。NearbySearchQuery.TryCreate 將字串驗證成 domain enum RestaurantCategory?；enum 與解析規則置於 src/EatThis.Api/Domain/RestaurantCategory.cs。Google 字串只留在 GooglePlacesProvider；前端 restaurantCategories.ts 集中代碼與中文標籤並匯出 union 型別與選項，types.ts 引用該型別。不將 provider 字串放入公開契約，也不增加只轉傳的 service。

| 代碼 | 顯示名稱 | Google includedTypes |
| --- | --- | --- |
| null | 不限類型 | restaurant, cafe, fast_food_restaurant, food_court, bakery, meal_takeaway |
| taiwanese-chinese | 台式／中式 | taiwanese_restaurant, chinese_restaurant |
| japanese | 日式 | japanese_restaurant, sushi_restaurant, ramen_restaurant |
| korean | 韓式 | korean_restaurant, korean_barbecue_restaurant |
| hot-pot | 火鍋 | hot_pot_restaurant |
| barbecue | 燒烤 | barbecue_restaurant, yakiniku_restaurant |
| italian | 義式 | italian_restaurant, pizza_restaurant |
| breakfast-brunch | 早餐／早午餐 | breakfast_restaurant, brunch_restaurant |
| fast-food | 速食 | fast_food_restaurant, hamburger_restaurant |
| vegetarian | 素食 | vegetarian_restaurant, vegan_restaurant |
| cafe-dessert | 咖啡／甜點 | cafe, coffee_shop, dessert_shop, dessert_restaurant |

替代方案是前端直接傳 Google includedTypes，但這會失去 provider-neutral 邊界與白名單控制。API 邊界擁有解析驗證，既有 provider interface 接收擴充後的 validated query，仍只有一個 Google adapter；刪掉 domain 分類會使合法值驗證與 adapter 對照失去依據，因此它承載實質契約而非轉傳包裝。

### 在單次 Google 搜尋套用類型對照

GooglePlacesProvider.SearchAsync 按 domain 分類選擇表中 includedTypes；選具體分類時替換原六種類型，不額外混入 restaurant，避免泛用類型擴大匹配。同一 includedTypes 中的類型取 OR；不送 includedPrimaryTypes、excludedTypes 或 minRating。維持 fixed field mask 含 places.rating、languageCode zh-TW、maxResultCount 20 與既有 locationRestriction。NearbyFoodService 不新增分類二次篩選，沿用 usability、評分、URL 去重與隨機選擇。IPlaceProvider 的返回候選已符合 query 指定分類；成功 response 不增加 Google type 欄位。

替代方案是先取泛用候選再依類型過濾，會受 20 筆限制漏掉符合類型的店；逐類發請求會增加呼叫與去重負擔。本方案一次有效搜尋只呼叫一次 Google，不自動重試其他類型，也不保證各子類型等機率。評分 field mask 維持原狀，仍屬 Enterprise 計費，不宣稱增加免費額度。

### 以可換行單選延續明確搜尋與條件快照

App.vue 在距離與評分之間加入 fieldset/legend「餐廳類型」，11 個同名 native radio 呈現為文字選項。使用現有 jade、rule、radius tokens，已選狀態有勾選標記及外框，焦點可見；點擊目標至少 44 CSS px 高，窄螢幕自然换行且不需要橫向滑動。字串使用中文標籤，不向使用者顯示 Google identifier。

useNearbyFood 新增 selectedRestaurantCategory，預設 null；SearchConditions 同時保存 radiusMeters、minRating、restaurantCategory。recommend 在任何 await 前驗證並複製三個 pending 值，定位及搜尋只使用 snapshot。忙碌時可編輯 pending 值，僅影響下一次推薦；主要按鈕禁止重複提交。調整分類與切回不限均不觸發 GPS/API。

主要按鈕顯示 pending 三個條件，selected/no-results 訊息顯示 submitted 三個條件。無結果不改變控制值；具體分類可提示改其他類型或不限，有限評分可提示降低評分，距離小於 3000 時可提示擴大距離，所有建議需再按主要按鈕才搜尋。若三個條件皆已不限／最大距離，提供明確重試。本方案優於下拉選單的隱藏選項及多選的空集合／OR 語意負担；第一版以單選控制選擇成本。

## Implementation Contract

- In scope: 上述分類表、optional request field、query 驗證、Google mapping、radio UI、三條件 snapshot、訊息、mock regression 與文件。
- Out of scope: Goals / Non-Goals 所列功能與改變現有成功 response、定位策略、radius/minRating 規則。
- JSON 範例：{"latitude":25.033,"longitude":121.5654,"radiusMeters":1000,"minRating":4,"restaurantCategory":"japanese"}。Nullable 不限範例 restaurantCategory:null；舊 request 省略此欄位與 null 等價。
- Invalid cases: "", "all", "Japanese", " japanese ", "restaurant", "pizza_restaurant", 123, true, [], {} 回 HTTP 400 invalid_request，provider 呼叫 0 次；錯誤文案包含餐廳類型而不回傳 raw provider 或例外。
- Success: japanese＋1000＋4 在一次 Google request 內使用三種日式 includedTypes，再從有效且 rating >=4 的候選選出一間，實際評分不四捨五入。沒有 eligible candidate 維持 HTTP 404 no_results，無額外搜尋。
- Compatibility: response shape 不變，新增 request field 與 domain record optional parameter 放於既有參數之後；舊呼叫端與 provider double 能維持編譯，分類 query 能到達 provider，不洩漏 Google 型別。
- Automated acceptance: frontend 的 composable、app、API 序列化測試涵蓋 11 個選項、radio 語意、無副作用、忙碌快照、分類無結果及 JSON；backend endpoint 驗證完整有效／非法表，Google provider mock 驗證所有 11 組 includedTypes、一次呼叫、zh-TW、20 與原 field mask；provider-neutral double 驗證 domain category 傳遞。
- Manual acceptance: 320px 與桌面檢查換行、可見選取和焦點、鍵盤 radio、長條件摘要、忙碌編輯及無結果恢復。無 browser 工具時明確記錄未驗證，不以 DOM checks 代替 browser 證據。

## Risks / Trade-offs

- [Google 店家分類涵蓋不完整] → 提示「本次未找到符合條件的店家」，提供改類型／不限；不宣稱搜盡所有店。
- [最多 20 筆、寬分類子類型數量不同] → 文件說明從當次候選隨機選取，不保證子類型等機率。
- [前後端各有代碼清單] → frontend 與 backend 測試依本設計完整對照表驗證，不只檢查清單非空。
- [分類 UI 加長表單] → 保留緊湊文字選項與自然換行，操作仍由單一主按鈕完成。
- [舊後端忽略新欄位] → 必須先部署後端，確認分類 mapping 生效後才部署前端。

## Migration Plan

無資料或偏好遷移；先後端、後前端。回滾先前端、後後端，避免分類選取表面成功卻被舊後端忽略。更新 PRODUCT.md、DESIGN.md、UI 方向文件與 README，並依 DESIGN.md 再生成 .impeccable/design.json，不把先前維護修改誤算成此提案的實作。

## Open Questions

無阻擋提案的未決項；本提案採上一輪建議的單選與 10 個分類，若後續改成多選須先更新需求與契約。
