## Why

目前只能以距離與最低評分挑選附近餐飲，使用者即使已經想吃日式或火鍋，也無法限制推薦方向。加入少量常用分類，讓使用者先表達想吃什麼，再維持一次操作得到一間店的流程。

## What Changes

- 餐廳類型採單選，提供「不限類型」與台式／中式、日式、韓式、火鍋、燒烤、義式、早餐／早午餐、速食、素食、咖啡／甜點共 10 個分類；預設不限類型。
- 使用可換行的文字選項與 native radio 語意，放在搜尋距離與最低評分之間；選取狀態包含標記、外框與鍵盤焦點。
- 調整類型不觸發定位或搜尋；按「幫我決定」固定本次距離、最低評分與類型，結果及無結果訊息使用本次條件。
- 公開 request 新增 optional nullable restaurantCategory；舊客戶端省略或傳 null 時維持現有餐飲搜尋。非法類型回 HTTP 400 invalid_request，且不呼叫 provider。
- 後端白名單把產品分類映射為 Google includedTypes，在單次 Nearby Search 篩選候選店後沿用評分、去重與隨機選擇；維持 places.rating、zh-TW 與 20 筆上限。
- 無结果保留條件，提供由使用者改類型、距離或評分後重新搜尋的提示，不自動放寬條件。

## Capabilities

### New Capabilities

無；延伸既有能力。

### Modified Capabilities

- `nearby-food-selection`: 新增餐廳類型單選、搜尋條件快照與包含類型的恢復提示。
- `secure-place-search`: 新增產品分類驗證與後端控制的 Google 類型對照。

## Impact

- Affected specs: nearby-food-selection、secure-place-search。
- API: 新增 optional restaurantCategory，不變更成功 response；後端先部署，前端後部署。無新增套件或儲存遷移。
- Affected code:
  - New: `src/EatThis.Api/Domain/RestaurantCategory.cs`、`src/EatThis.Web/src/restaurantCategories.ts`。
  - Modified: `src/EatThis.Api/Contracts/NearbyFoodContracts.cs`、`src/EatThis.Api/Domain/NearbySearchQuery.cs`、`src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs`、`src/EatThis.Web/src/types.ts`、`src/EatThis.Web/src/composables/useNearbyFood.ts`、`src/EatThis.Web/src/App.vue`、`src/EatThis.Web/src/styles.css`。
  - Modified tests: `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs`、`tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs`、`tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs`、`tests/EatThis.Api.Tests/ProxyRestrictionTests.cs`、`src/EatThis.Web/src/app.spec.ts`、`src/EatThis.Web/src/composables/useNearbyFood.spec.ts`、`src/EatThis.Web/src/api/nearbyFoodApi.spec.ts`。
  - Modified documentation: `README.md`、`PRODUCT.md`、`DESIGN.md`、`.impeccable/design.json`、`docs/ui/eatthis-mobile-web-direction.md`。
  - Removed: 無。
