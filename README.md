# EatThis

EatThis 是一個以手機瀏覽器為主的「今天吃什麼」附近餐飲推薦工具。使用者按下「幫我決定」後，前端只取得一次目前 GPS 位置，由後端搜尋附近餐飲地點並隨機挑選一間，最後交由 Google Maps 開啟導航。

## 功能

- 搜尋距離預設 100 公尺，可依 100 公尺級距調整至 3 公里；不自動擴大範圍。
- 五顆星可選最低評分，點左半選半星、右半選整星，支援 0.5 至 5 星；預設「不限評分」。
- 距離與評分調整不會搜尋，按下「幫我決定」才固定當次條件、定位並搜尋。搜尋途中調整只影響下一次操作。
- 不持續追蹤位置、不嵌入地圖、不提供餐廳目錄瀏覽。
- 顯示餐飲名稱、地址、距離、實際評分與外部導航連結；未評分店家顯示「尚無評分」。
- 處理定位拒絕、瀏覽器不支援定位、查無結果、服務錯誤與頻率限制等狀態。
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
  "minRating": 4.0
}
```

`latitude` 必須介於 `-90` 與 `90`，`longitude` 必須介於 `-180` 與 `180`，`radiusMeters` 必須介於 `100` 與 `3000`；API 省略半徑時仍預設為 `3000`，前端初始選擇則為 `100`。

`minRating` 可省略或為 `null`（不限評分），或為 0.5 至 5 的半星數字。字串、布林、4.3 等非半星門檻及超界值回 `400 invalid_request`，不呼叫供應商。門檻採包含邊界的比較：選 4 星時，評分 4.0、4.3、4.8 都符合；沒有評分的店家只在不限評分時參與。

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
  "rating": 4.3
}
```

常見錯誤：`400 invalid_request`、`404 no_results`、`429 rate_limited`、`502/503 provider_unavailable`。API 每個來源 IP 每分鐘最多允許 30 次推薦請求。

`rating` 保留店家實際分數，不依半星門檻四捨五入；未評分回 `null`。後端先驗證候選、按最低評分篩選，再依導航 URL 去重，最後隨機挑一間。無結果時畫面顯示當次提交條件，讓使用者自行降低評分或增加距離再搜尋，不會自動放寬條件。

### 候選範圍與 API 計費

每次搜尋只對 Google Nearby Search 回傳的最多 20 間候選店家篩選，不是範圍內所有店家的完整清單。因此無結果表示本次未找到符合條件的候選，不能推論附近沒有合格店家。

後端固定要求 `places.rating` 以顯示及篩選評分，包含「不限評分」的請求；此欄位使用 Nearby Search Enterprise 計費級別。詳見 [Google Nearby Search 欄位與計費文件](https://developers.google.com/maps/documentation/places/web-service/nearby-search)。一次使用者動作仍只有一次供應商搜尋，不另外取得評論數或評論內容。

### 升級與回滾

先部署接受 `minRating` 並回傳 `rating` 的後端，再部署含星星控制的前端；既有不傳 `minRating` 的用戶端保持不限評分。回滾時先回滾前端，再回滾後端，避免新前端連到不支援篩選的舊後端。沒有資料庫或偏好儲存遷移。

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

目前版本是 MVP，刻意不包含帳號、收藏、評論、照片、營業時間、持續定位、互動地圖與餐廳資料持久化。後端以 provider-neutral 的 `IPlaceProvider` 保留未來替換其他地點資料來源的空間。
