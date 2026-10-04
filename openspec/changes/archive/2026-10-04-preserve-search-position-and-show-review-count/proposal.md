## Why

使用者每次按「幫我決定」後遇到畫面往上跳，需要重複下滑。餐廳卡片目前只有評分，缺少評論數，難以理解評分所依據的數量。

## What Changes

- 推薦開始、定位、搜尋、成功與失敗全程保持使用者目前捲動位置，不主動捲至頂端、結果或移動焦點。
- 避免清除舊結果及新內容較短導致結果區塌縮；使用者在等待期間自行捲動時尊重最新位置。
- 既有 Google Nearby Search 增加評論總數欄位，經既有 API 回傳，在評分旁呈現例如「4.9（1,234 則評論）」。缺值不當成零。
- 補齊資料契約、狀態轉換與真實瀏覽器捲動驗收。

## Capabilities

### New Capabilities

無。

### Modified Capabilities

- `nearby-food-selection`: 新增推薦全程維持捲動位置與餐廳評論數呈現需求。

## Impact

- Affected specs: nearby-food-selection
- Affected code (Modified):
  - `src/EatThis.Web/src/App.vue`
  - `src/EatThis.Web/src/styles.css`
  - `src/EatThis.Web/src/types.ts`
  - `src/EatThis.Web/src/app.spec.ts`
  - `src/EatThis.Web/src/composables/useNearbyFood.spec.ts`
  - `src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs`
  - `src/EatThis.Api/Program.cs`
  - `src/EatThis.Api/Contracts/NearbyFoodContracts.cs`
  - `tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs`
  - `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs`
  - `tests/EatThis.Api.Tests/ContractFixtureTests.cs`
  - `tests/EatThis.Api.Tests/ProxyRestrictionTests.cs`
  - `tests/fixtures/nearby-food-success.json`
  - `DESIGN.md`
  - `docs/ui/eatthis-mobile-web-direction.md`
  - `src/EatThis.Web/dist/index.html`
- 建置會更新 src/EatThis.Web/dist/assets 下的雜湊資源；不新增套件或端點。成功回應新增可空的 userRatingCount，舊回應缺少欄位仍可呈現。
