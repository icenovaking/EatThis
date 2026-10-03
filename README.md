# EatThis

EatThis 是一個以手機瀏覽器為主的「今天吃什麼」附近餐飲推薦工具。使用者按下「幫我決定」後，前端只取得一次目前 GPS 位置，由後端搜尋附近餐飲地點並隨機挑選一間，最後交由 Google Maps 開啟導航。

## 功能

- 搜尋距離預設 100 公尺，可依 100 公尺級距調整至 3 公里；不自動擴大範圍。
- 五顆星可選最低評分，點左半選半星、右半選整星，支援 0.5 至 5 星；預設「不限評分」。
- 餐廳類型提供「不限類型」與 10 個常用分類，採可換行的單選文字選項；預設不限類型。
- 距離、餐廳類型與評分調整不會搜尋，按下「幫我決定」才固定當次條件、定位並搜尋。搜尋途中調整只影響下一次操作。
- 不持續追蹤位置、不嵌入地圖、不提供餐廳目錄瀏覽。
- 顯示餐飲名稱、地址、距離、實際評分與外部導航連結；未評分店家顯示「尚無評分」。
- 處理定位拒絕、瀏覽器不支援定位、查無結果、服務錯誤與頻率限制等狀態。
- 顯示過的餐廳在 30 分鐘內優先排除；當次符合條件的候選全部出現後開始新一輪。有兩家以上時，新一輪第一家避開上一輪最後一家，只有一家則可連續出現。
- 推薦歷史保留於同一瀏覽器、同源的 localStorage，重新整理與調整條件仍有效；每家滿 30 分鐘也可重新入選。搜尋失敗不新增或重設歷史。
- Google Places API 金鑰只存在後端；前端請求不包含任何供應商密鑰。

## 技術棧

| 層級 | 技術 |
| --- | --- |
| 前端 | Vue 3、TypeScript、Vite 7 |
| 前端測試 | Vitest、Vue Test Utils、jsdom |
| 後端 | C#、.NET 10、ASP.NET Core Minimal API |
| 後端整合 | `HttpClient`、Google Places Nearby Search API |
| 後端測試 | MSTest、ASP.NET Core `WebApplicationFactory` |
| 專案管理 | `EatThis.slnx`、npm |

## 專案結構

```text
src/
├─ EatThis.Api/             # ASP.NET Core API 與 Google Places adapter
└─ EatThis.Web/             # Vue 3 前端
tests/
└─ EatThis.Api.Tests/       # API 整合與服務測試
docs/                       # 開發、密鑰與 UI 說明
scripts/                    # 安全檢查腳本
openspec/                   # Spectra 規格與變更紀錄
```

## 快速開始

### 必要環境

- .NET SDK 10
- Node.js 24 或其他目前支援的 LTS 版本
- 可呼叫 Google Places API 的伺服器端 API 金鑰

### 1. 設定後端密鑰

在專案根目錄執行：

```powershell
dotnet user-secrets init --project src/EatThis.Api/EatThis.Api.csproj
dotnet user-secrets set "GooglePlaces:ApiKey" "<your-server-side-key>" --project src/EatThis.Api/EatThis.Api.csproj
```

請勿將金鑰放入前端 `.env`、`appsettings*.json` 或提交至 Git。部署環境可使用 `GooglePlaces__ApiKey` 注入；完整規則請參考 [`docs/development-secrets.md`](docs/development-secrets.md)。

### 2. 啟動 API

```powershell
dotnet run --project src/EatThis.Api/EatThis.Api.csproj --launch-profile EatThis.Api
```

API 預設執行於 `http://localhost:5080`，健康檢查位於 `GET /health`。

### 3. 啟動前端

另開一個終端機：

```powershell
Set-Location src/EatThis.Web
npm install
npm run dev
```

前端預設執行於 `http://localhost:5173`，Vite 會將 `/api` 請求代理至本機 API。若前後端分開部署，可在前端環境設定 `VITE_API_BASE_URL` 為 API origin；本機開發通常保持空值即可。

## API

### `POST /api/nearby-food/pick`

請求：

```json
{
  "latitude": 25.033,
  "longitude": 121.5654,
  "radiusMeters": 700,
  "minRating": 4.0,
  "restaurantCategory": "japanese"
}
```

`latitude` 必須介於 `-90` 與 `90`，`longitude` 必須介於 `-180` 與 `180`，`radiusMeters` 必須介於 `100` 與 `3000`；API 省略半徑時仍預設為 `3000`，前端初始選擇則為 `100`。

`minRating` 可省略或為 `null`（不限評分），或為 0.5 至 5 的半星數字。字串、布林、4.3 等非半星門檻及超界值回 `400 invalid_request`，不呼叫供應商。門檻採包含邊界的比較：選 4 星時，評分 4.0、4.3、4.8 都符合；沒有評分的店家只在不限評分時參與。

`restaurantCategory` 可省略或為 `null`（不限類型）；具體分類只接受下表代碼，區分大小寫且不修剪空白。空字串、未知代碼、Google 類型字串及非字串值回 `400 invalid_request`，不呼叫供應商。各分類在同一次 Google 搜尋中匹配其中任一 listed type，具體分類不混入泛用 `restaurant`。

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

成功回應會返回一個標準化地點：

```json
{
  "name": "Example Food Shop",
  "address": "Taipei City",
  "latitude": 25.0331,
  "longitude": 121.5655,
  "distanceMeters": 420,
  "navigationUrl": "https://www.google.com/maps/place/example",
  "provider": "google",
  "rating": 4.3,
  "resetNavigationUrls": []
}
```

常見錯誤：`400 invalid_request`、`404 no_results`、`429 rate_limited`、`502/503 provider_unavailable`。API 每個來源 IP 每分鐘最多允許 30 次推薦請求。

`rating` 保留店家實際分數，不依半星門檻四捨五入；未評分回 `null`。後端先驗證候選、按最低評分篩選，再依導航 URL 去重、套用近期歷史，最後隨機挑一間。無結果時畫面顯示當次提交條件，讓使用者自行改選餐廳類型／不限類型、降低評分或增加距離再搜尋，不會自動放寬條件。

請求可附加 `excludedNavigationUrls`（最多 1000 個非空白字串）與 `lastNavigationUrl`（上一個結果）；各字串最多 2048 字元，省略或 `null` 表示沒有紀錄。無效型別或超限回 `400 invalid_request`。這些鍵只作不分大小寫的比較，不會轉送供應商或作為網路請求網址。

成功回應的 `resetNavigationUrls` 平常為空；新一輪時包含當次合格候選的鍵。前端先移除這些鍵的紀錄，再記錄本次顯示結果，保留其他條件的歷史。例如 A → C → B 後，新一輪只能從 A 或 C 開始。候選耗盡以本次最多 20 筆上游結果篩選後的集合判定，不額外搜尋。

歷史只保存導航鍵與顯示時間，最多保留最新 1000 筆；極端超量時只保證保留範圍內的排除。儲存不可用時降級至頁面記憶體，重新整理後無法保留；清除網站資料會清除紀錄。不跨裝置同步，也不保證多分頁同時請求不重複。

### 候選範圍與 API 計費

Google 先按所選類型篩選候選，店家分類涵蓋不完整且不保證各子類型等機率。每次搜尋只對 Google Nearby Search 回傳的最多 20 間候選店家篩選，不是範圍內所有店家的完整清單。因此無結果表示本次未找到符合條件的候選，不能推論附近沒有合格店家。

後端固定要求 `places.rating` 以顯示及篩選評分，包含「不限評分」的請求；此欄位使用 Nearby Search Enterprise 計費級別。詳見 [Google Nearby Search 欄位與計費文件](https://developers.google.com/maps/documentation/places/web-service/nearby-search)。一次使用者動作仍只有一次供應商搜尋，不另外取得評論數或評論內容。

### 升級與回滾

先部署接受歷史欄位並回傳 `resetNavigationUrls` 的後端，再部署前端；既有不傳歷史的用戶端仍能抽選，原有地點欄位保留。回滾時先回滾前端，再回滾後端。新前端可顯示未帶重設資訊的舊回應，但舊後端不提供排除策略。沒有資料庫遷移，瀏覽器使用版本化的歷史儲存鍵。

## 測試與檢查

前端指令需在 `src/EatThis.Web` 執行：

```powershell
npm run test:run       # 前端測試
npm run typecheck      # TypeScript / Vue 型別檢查
npm run build          # 型別檢查並建立 production bundle
npm run security:check # 檢查前端與設定是否洩漏供應商密鑰
```

後端測試需在專案根目錄執行：

```powershell
dotnet test tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj
```

更多本機開發說明請參考 [`docs/development.md`](docs/development.md)。

## 設計邊界

目前版本是 MVP，刻意不包含帳號、收藏、評論、照片、營業時間、持續定位、互動地圖與完整餐廳資料持久化；瀏覽器只保留短期推薦歷史。後端以 provider-neutral 的 `IPlaceProvider` 保留未來替換其他地點資料來源的空間。
