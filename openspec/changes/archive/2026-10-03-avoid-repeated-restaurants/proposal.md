## Why

目前每次推薦獨立抽籤，使用者連續挑選時經常看到同一家餐廳。加入 30 分鐘內優先不重複與候選耗盡後的新一輪，兼顧多樣性及候選少時仍可繼續挑選。

## What Changes

- 結果成功顯示後記錄餐廳與時間，30 分鐘內優先排除；滿 30 分鐘重新具備資格。
- 本次搜尋符合條件的候選全已出現時，自動開始新一輪，每輪內優先不重複。
- 有兩家以上候選時，新一輪第一家避開上一輪最後一家；只有一家可連續出現。
- 同一瀏覽器同源保存紀錄，重新整理與調整距離、評分、類型不清空歷史。
- 擴充既有 pick API 的可選歷史輸入與成功回應重設資訊，後端一次取得候選後完成排除與抽籤，不自動追加上游請求。
- 全部候選指本次上游回傳且通過既有條件的候選集合，不宣稱涵蓋地理範圍內所有餐廳。

## Capabilities

### New Capabilities

- `restaurant-repeat-prevention`: 有期限的瀏覽器推薦歷史、候選耗盡後重新抽選、跨輪避免連續同一家與有界 API 合約。

### Modified Capabilities

- `secure-place-search`: 更新允許的前端欄位、排除後抽選順序及加法式成功回應合約，保留既有安全限制。

## Impact

- Affected specs: restaurant-repeat-prevention；與 nearby-food-selection、secure-place-search 一起適用。
- Affected code:
  - New: `src/EatThis.Web/src/recommendationHistory.ts`, `src/EatThis.Web/src/recommendationHistory.spec.ts`
  - Modified: `src/EatThis.Web/src/app.spec.ts`, `src/EatThis.Web/dist/index.html`, `src/EatThis.Web/src/types.ts`, `src/EatThis.Web/src/composables/useNearbyFood.ts`, `src/EatThis.Web/src/composables/useNearbyFood.spec.ts`, `src/EatThis.Web/src/api/nearbyFoodApi.spec.ts`, `src/EatThis.Api/Contracts/NearbyFoodContracts.cs`, `src/EatThis.Api/Domain/NearbySearchQuery.cs`, `src/EatThis.Api/Application/NearbyFoodService.cs`, `src/EatThis.Api/Program.cs`, `tests/EatThis.Api.Tests/RandomSelectionTests.cs`, `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs`, `tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs`, `README.md`
  - Removed: 無。
- API: POST /api/nearby-food/pick 新增可選輸入與加法式成功回應欄位；舊呼叫者省略歷史仍可抽選。
- 不增加套件、資料庫或上游欄位，不改版面。


- 建置產物：更新 src/EatThis.Web/dist/assets 下對應的雜湊 JavaScript bundle；App 測試新增 localStorage 隔離以避免測試間共用歷史。
