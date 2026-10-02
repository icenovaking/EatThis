## 1. 先建立失敗的回歸案例

- [x] 1.1 在 tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs、tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs、tests/EatThis.Api.Tests/ProxyRestrictionTests.cs 與 tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs 建立 Validate an optional restaurant category、Map product categories to one bounded provider search 與 Query Google Places through an adapter 的 regression：以 raw JSON 驗證省略／null、10 個合法值、10 個非法輸入、未知 provider filter 不可覆蓋、完整 11 組 mapping、一次 Google 呼叫、評分欄位與 zh-TW/20 不變；provider-neutral category 傳遞案例於 domain 型別建立後補齊。由 Test-Runner sub-agent 執行相關 .NET 測試確認新增案例在 production code 未改前因缺少分類行為失敗，避免只因編譯錯誤而產生假紅燈。
- [x] 1.2 在 src/EatThis.Web/src/composables/useNearbyFood.spec.ts、src/EatThis.Web/src/app.spec.ts 與 src/EatThis.Web/src/api/nearbyFoodApi.spec.ts 建立 Choose one restaurant category、Snapshot category with the submitted search conditions 與 Select exactly one nearby food place 的 regression：驗證完整 11 個中文 radio、預設／切回不限、零 GPS/API 副作用、三條件 JSON、locating/searching 中編輯不影響本次結果或 no-results、無結果不自動放寬與阻擋重複提交；從 src/EatThis.Web 執行 npm run test:run -- src/composables/useNearbyFood.spec.ts src/app.spec.ts src/api/nearbyFoodApi.spec.ts，確認新增案例先因缺少功能失敗。

## 2. 實作 API 分類契約與單次 provider 篩選

- [x] 2.1 完成「以產品分類代碼建立受限契約」與 Validate an optional restaurant category：在 src/EatThis.Api/Domain/RestaurantCategory.cs 定義 nullable domain enum 與精確字串解析，在 src/EatThis.Api/Contracts/NearbyFoodContracts.cs 及 src/EatThis.Api/Domain/NearbySearchQuery.cs 加入 optional restaurantCategory 並更新 invalid_request 文案，省略/null 維持舊行為、非法值無 provider 呼叫、response 不變；補齊 provider-neutral double 收到分類的案例，由 Test-Runner sub-agent 執行 endpoint 與 ProviderNeutralContractTests 驗證。
- [x] 2.2 完成「在單次 Google 搜尋套用類型對照」、Map product categories to one bounded provider search 與 Query Google Places through an adapter：GooglePlacesProvider.SearchAsync 以完整表選擇 includedTypes，具體分類替換泛用清單，不加入 primary/excluded filters、不多發請求，保留 zh-TW、20、locationRestriction 與 places.rating；由 Test-Runner sub-agent 執行 GooglePlacesProviderTests、ProxyRestrictionTests 驗證所有 11 組 mapping 及未知公開 provider 欄位不會轉傳。

## 3. 實作餐廳類型選擇與條件快照

- [x] 3.1 在 src/EatThis.Web/src/restaurantCategories.ts 集中 10 個代碼與 11 個中文選項，src/EatThis.Web/src/types.ts 擴充 SearchRequest，useNearbyFood 的 recommend 驗證 pending category 並在 await 前固定三條件；完成 Snapshot category with the submitted search conditions，locating/searching 時的 pending 編輯僅影響下次動作；以 composable 與 API 序列化 regression 驗證 null、japanese、busy snapshot、非法 category 零定位／API，並執行 npm run typecheck。
- [x] 3.2 完成「以可換行單選延續明確搜尋與條件快照」與 Choose one restaurant category：App.vue 在距離與評分間呈現 native fieldset/radio，styles.css 沿用 jade/rule/radius tokens、加入標記與可見焦點、44px 最小點擊高度及自然換行；主要按鈕含 pending 三條件。執行 app.spec.ts，驗證完整標籤、恰一項 checked、中文 accessible names、radio group 與點擊後無立即搜尋。
- [x] 3.3 完成 Select exactly one nearby food place 的分類恢復行為：App.vue 的 selected/no-results 與恢復提示使用 submitted snapshot，具體分類提示改類型／不限，評分與距離提示沿用既有上限，最大距離且不限條件仍可明確重試；以 app 與 composable regression 驗證 japanese/1000/4 的訊息、busy 改 hot-pot 後不污染本次訊息、pending 控制保留、零自動 retry 與只呈現一間結果。

## 4. 文件、完整驗證與審查

- [x] 4.1 更新 README.md、PRODUCT.md、DESIGN.md、docs/ui/eatthis-mobile-web-direction.md 與 .impeccable/design.json，記錄單選分類、完整對照、條件快照、分類涵蓋限制、20 筆候選限制與評分仍屬 Enterprise；設計衍生檔加入實際餐廳類型控制預覽。以文件對照 design.md 的完整 mapping 與狀態行為、JSON parse 及 Impeccable doctor 無 stale sidecar 驗證；保留工作開始前已存在的設計衍生檔維護修改。
- [x] 4.2 從 src/EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build、npm run security:check；所有 dotnet test 由 Test-Runner sub-agent 依 run-tests skill 執行 tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj 全套，任何編譯錯誤交由 Build-Fixer sub-agent 修復；執行 spectra-audit，確認新分類無 raw provider filter 轉傳、未知分類不默默放寬與無額外 Google 呼叫，記錄失敗與修復結果並確保檢查通過。
- [x] 4.3 在 320px 與桌面 browser 驗證選項自然換行、標記／焦點、鍵盤 arrows/Space、長條件摘要、busy 編輯及分類 no-results 恢復後需明確按鈕搜尋；記錄 agent-run browser 證據，工具不可用時記錄待人工驗證，不能以 DOM 檢查或使用者回報冒充 agent-run browser 結果。
