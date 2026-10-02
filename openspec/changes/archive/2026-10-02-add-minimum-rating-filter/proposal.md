## Why

目前 EatThis 只能依距離隨機推薦，使用者無法排除低於期待評分的店家。加入可操作半顆星的最低評分控制，讓使用者以熟悉的星星圖示設定門檻，仍維持一次只推薦一間的快速流程。

## What Changes

- 搜尋距離下方提供五顆星，每顆左右半部對應半星／整星，支援 0.5..5.0、step 0.5；已選範圍填黃色、其餘灰色，並顯示最低評分文字。
- 預設不限評分，提供「不限評分」重設；調整星星不觸發定位或搜尋，主要動作一次快照距離與評分。
- 公開 request 新增 optional nullable minRating，缺省或 null 表示不限；response 新增 nullable rating。後端驗證門檻並在去重及隨機抽選前排除不符合的候選。
- Google adapter 取得 places.rating，轉為供應商中立的可空評分；未評分店家只在不限評分時保留。
- 結果顯示實際評分或「尚無評分」；無符合結果時提示降低評分或增加距離，維持 HTTP 404 no_results，絕不自動放寬條件。
- 保留既有 100..3000 公尺距離選擇、zh-TW、外部導航、官方 attribution 與單一結果流程；承接 refine-nearby-food-search 的現行程式行為。

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `nearby-food-selection`: 五顆星的半星最低評分控制、條件快照、評分結果與無結果復原。
- `secure-place-search`: 有界可選評分契約、Google 評分正規化、抽選前評分篩選與安全相容性。

## Impact

- Affected specs: `openspec/specs/nearby-food-selection/spec.md`, `openspec/specs/secure-place-search/spec.md`
- Affected code:
  - New: none
  - Modified: `src/EatThis.Web/src/App.vue`, `src/EatThis.Web/src/styles.css`, `src/EatThis.Web/src/types.ts`, `src/EatThis.Web/src/composables/useNearbyFood.ts`, `src/EatThis.Web/src/app.spec.ts`, `src/EatThis.Web/src/composables/useNearbyFood.spec.ts`, `src/EatThis.Web/src/api/nearbyFoodApi.spec.ts`, `src/EatThis.Web/dist/`, `src/EatThis.Api/Program.cs`, `src/EatThis.Api/Contracts/NearbyFoodContracts.cs`, `src/EatThis.Api/Domain/NearbySearchQuery.cs`, `src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs`, `src/EatThis.Api/Application/NearbyFoodService.cs`, `src/EatThis.Api/Application/PlaceCandidateRules.cs`, `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs`, `tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs`, `tests/EatThis.Api.Tests/RandomSelectionTests.cs`, `tests/EatThis.Api.Tests/CandidateNormalizationTests.cs`, `tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs`, `tests/EatThis.Api.Tests/ContractFixtureTests.cs`, `tests/fixtures/nearby-food-success.json`, `README.md`, `PRODUCT.md`, `DESIGN.md`, `docs/ui/eatthis-mobile-web-direction.md`
  - Removed: none
- API: additive fields; omitted minRating retains unrestricted selection. No new runtime dependency.
- External service: requesting places.rating triggers Nearby Search Enterprise billing; selection is limited to at most 20 returned candidates, not an exhaustive search of all nearby stores. Reference: https://developers.google.com/maps/documentation/places/web-service/nearby-search
- Coordination: finalize and archive refine-nearby-food-search before archiving this change; its outstanding visual review remains its own task.
