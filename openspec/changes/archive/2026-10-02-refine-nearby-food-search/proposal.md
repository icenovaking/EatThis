## Summary

將 EatThis 的附近餐飲搜尋改為可控制距離、繁體中文優先且適合日常快速操作的城市美食工具，並以新的視覺系統取代目前容易被辨識為通用 AI 版型的介面。

## Motivation

目前搜尋半徑固定為 3 公里，使用者無法先從步行可及的近距離開始；無結果流程還會擴大到 5 公里，超出使用者希望的範圍。Google Places 請求未指定偏好語言，導致店名與地址常以英文顯示；結果卡片也直接呈現內部 provider 值 `GOOGLE`，既不清楚也不是理想的 Google Maps attribution。現有暖米色、磚紅色、重複圓角卡片與裝飾性刻度的視覺語言亦不符合「每天會使用的城市美食工具」定位。

## Proposed Solution

- 在主要搜尋動作前提供 100 至 3000 公尺的距離滑桿，預設 100 公尺、每格 100 公尺，並以公尺或公里即時顯示所選範圍。
- 距離調整只更新待搜尋值，不自動取得定位或呼叫 API；使用者按下主要動作後才取得一次目前位置並以選定距離搜尋。
- **BREAKING**：將前後端允許的最大搜尋半徑由 5000 公尺降為 3000 公尺，移除固定 5000 公尺重試流程；無結果時改為引導使用者自行調大範圍，已在 3000 公尺時則提供明確的再次搜尋方式。
- Google Places Nearby Search 請求固定加入繁體中文偏好 `zh-TW`；有繁體中文資料時回傳繁中名稱與地址，無對應翻譯時接受供應商最接近的名稱，不因缺少翻譯排除地點。
- 保留 provider-neutral 回應契約，但不再把 provider 原始值當作使用者標籤；Google Places 資料在結果容器底部使用官方 Google Maps 標誌完成清楚且低干擾的 attribution。
- 以「口袋城市飲食指南」為視覺世界重做單頁介面：採冷調明亮底色、深墨文字與單一玉綠重點色，以資訊比例、細分隔線與功能性距離刻度建立層級，避免暖奶油加磚紅、裝飾性儀表、過度圓角卡片及大寫英文微標籤。
- 維持一次只推薦一間、沒有嵌入地圖、外部開啟 Google Maps，以及既有定位、載入、錯誤與無障礙狀態契約。

## Alternatives Considered

- 僅修改現有配色：無法修正重複卡片、裝飾刻度與結果層級造成的通用 AI 版型感。
- 保留 5 公里作為無結果逃生路徑：與使用者明確不想找太遠及 3 公里上限衝突。
- 完全移除 Google 標示：不利於辨識資料來源，且不符合 Google Maps Platform 對無嵌入地圖之 Places 內容的 attribution 要求。
- 沒有繁體中文就排除地點：會不必要地縮小候選集合，並讓翻譯資料完整度影響隨機推薦。

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `nearby-food-selection`: 將固定 3 公里與 5 公里重試改為使用者可調的 100 至 3000 公尺搜尋，並重新定義無結果、結果資訊、attribution 與日常操作介面要求。
- `secure-place-search`: 將後端半徑上限改為 3000 公尺，並要求 Google adapter 使用繁體中文偏好，同時維持 server-controlled provider 邊界。

## Impact

- Affected specs: `openspec/specs/nearby-food-selection/spec.md`, `openspec/specs/secure-place-search/spec.md`
- Affected code:
  - Modified: `src/EatThis.Web/index.html`, `src/EatThis.Web/src/App.vue`, `src/EatThis.Web/src/styles.css`, `src/EatThis.Web/src/composables/useNearbyFood.ts`, `src/EatThis.Web/src/app.spec.ts`, `src/EatThis.Web/src/composables/useNearbyFood.spec.ts`, `src/EatThis.Web/dist/`, `src/EatThis.Api/Domain/NearbySearchQuery.cs`, `src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs`, `tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs`, `tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs`, `PRODUCT.md`, `docs/ui/eatthis-mobile-web-direction.md`
  - New: `src/EatThis.Web/src/assets/google-maps-logo.svg`, `DESIGN.md`
  - Removed: none
- External behavior: clients sending radiusMeters from 3001 through 5000 will receive the existing invalid-request response after this change.
- Dependencies: no new runtime package; the Google Maps attribution asset SHALL come from Google's official asset package and remain unmodified.
