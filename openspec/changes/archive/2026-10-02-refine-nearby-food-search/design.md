## Context

EatThis 目前由 Vue 單頁前端呼叫 ASP.NET Core 的 POST /api/nearby-food/pick；前端只在使用者按下主要動作後取得一次 GPS，後端透過 provider-neutral 介面呼叫 Google Places，隨機選出一間並回傳名稱、地址、距離、導覽 URL 與 provider。現行前端把半徑固定為 3000 公尺，無結果時以同一位置重試 5000 公尺；後端接受 100 至 5000 公尺。Google request 沒有 languageCode，因此回傳常偏向英文。結果卡片直接把 provider 顯示為 GOOGLE。

這次同時跨越 Vue 狀態、API 邊界、Google adapter、測試與整體視覺世界，因此需要 design artifact。Impeccable 將頁面分類為 mobile-browser 的 Operate surface；使用者已固定方向為「每天會使用的城市美食工具」。新的視覺世界採口袋城市飲食指南與大眾運輸導引系統的資訊紀律，方向 seed 為 1d08afa1、grounded direction index 3；現有 pocket timetable slide rack 僅作為需要被替換的反例，不是視覺權威。

## Goals / Non-Goals

**Goals:**

- 讓使用者在搜尋前以 100 公尺級距選擇 100 至 3000 公尺，預設從 100 公尺找起。
- 確保滑桿調整沒有定位或網路副作用，只有明確的主要動作會啟動一次定位與一次搜尋。
- 將前後端半徑上限一致收斂為 3000 公尺，移除固定 5000 公尺重試。
- 由 Google adapter 固定要求 zh-TW，繁體中文可用時保留繁中，無翻譯時保留供應商 fallback。
- 以官方 Google Maps 標誌完成可辨識、低干擾且可存取的 attribution，不再顯示 raw provider。
- 以高掃描性、中文優先、無水平溢位的口袋城市飲食指南重建 320px 以上的單頁操作體驗。

**Non-Goals:**

- 不加入嵌入式地圖、店家列表、排序、篩選、評分、照片、營業時間或帳號。
- 不加入前端翻譯服務，也不因缺少 zh-TW 翻譯排除有效地點。
- 不改變公開 request/response JSON 欄位，不讓瀏覽器指定 provider、languageCode、field mask 或 upstream URL。
- 不保留 3000 公尺以上的隱藏 escape hatch，也不做自動擴大或連續搜尋。
- 不增加新的 runtime 套件、資料庫或持久化半徑偏好。

## Decisions

### 以待搜尋狀態管理可調半徑

`useNearbyFood` 擁有 `selectedRadiusMeters`，初始值為 100，並對外提供給 App 的 range input 綁定。前端常數定義 minimum 100、maximum 3000、step 100；`recommend()` 在使用者按下主要動作後讀取當下快照、驗證邊界、取得一次位置，再以該快照呼叫既有 search 路徑。input 事件只改變 ref 與格式化標籤，不得呼叫 geolocation 或 pick。

移除 `retryExpandedRadius` 與 `canRetryExpandedRadius`。無結果時保留已選半徑：低於 3000 公尺的文案要求使用者調大滑桿後再次按主要動作；等於 3000 公尺時提供重新搜尋，但不建立更大半徑。每次明確重試重新取得一次目前位置，避免沿用已過時座標。

替代方案是讓 App.vue 自行保存半徑並把值傳入 `recommend(radius)`；這會把搜尋邊界分散在 view 與 composable。由 composable 擁有待搜尋狀態可讓無副作用調整、驗證與測試集中在同一 seam。

### 在 API 邊界收斂 3 公里上限

`NearbySearchQuery.MinimumRadiusMeters` 維持 100，`MaximumRadiusMeters` 從 5000 改為 3000；公開 request shape 仍只有 latitude、longitude、radiusMeters。100 與 3000 都合法，99、3001 與 5000 都回既有 HTTP 400 invalid_request，provider 不會被呼叫。Google locationRestriction 使用驗證後的 radius，不再有任何 5000 公尺特例。

這是刻意的 breaking boundary：舊前端初始 3000 公尺仍可工作，但舊的 5000 公尺重試會失敗，因此前後端應視為同一 release 交付。替代方案是只限制前端並讓 API 保留 5000；這會讓公開契約與產品承諾分歧，因此不採用。

### 讓繁體中文偏好留在 Google adapter

在 `GoogleNearbySearchRequest` 增加序列化為 `languageCode` 的 server-controlled 欄位，每次固定為 `zh-TW`。provider-neutral query、public request 與 normalized candidate 不新增語言欄位。Google 回傳的 displayName.text 與 formattedAddress 照原值正規化；只要名稱非空且其他必要欄位有效，就保留候選，不檢查字集或回傳語言。

替代方案是在前端翻譯字串或把 languageCode 開放給 browser。前者增加成本與錯譯風險，後者洩漏 provider concern 並削弱 proxy 限制，因此都不採用。

### 以 Google Maps 官方標誌取代 raw provider

normalized response 的 provider 仍為 `google`，供前端選擇 attribution 規則；App 不再直接插值 provider。Google result 的 selected-place container 底部放置從 Google 官方 attribution asset package 取得且未修改的 `google-maps-logo.svg`，高度維持 16 至 19 CSS px，左右與上方至少 10px、下方至少 5px clear space，並提供 accessible name `Google Maps`。標誌靠近結果與外部導覽動作，但不與店名同列競爭。

替代方案是完全移除標示或保留純文字 GOOGLE。前者有 attribution 風險，後者不符合新實作應使用 Google Maps 名稱的清楚性要求。

### 以口袋城市飲食指南重建 Operate 介面

頁面採 Restrained color strategy，使用冷調城市紙白 `#F4F7F5`、純白功能表面 `#FFFFFF`、深墨 `#10211D`、次要墨 `#5E6B66`、規則線 `#CFD7D3` 與唯一玉綠重點色 `#087A63`。不得延用 goldenrod、ivory、vermilion 組合，不使用漸層、發光、玻璃、厚陰影、裝飾性格線或每個區塊都包圓角卡片。圓角只在實際控制或結果邊界使用 0 至 6px；大區塊以留白和 1px 分隔線建立層級。

排版使用系統可用的繁體中文 workhorse sans stack，不新增 webfont。首屏從小型 EatThis wordmark、簡短任務說明、醒目的目前距離、功能性 range scale、主要動作依序向下；`今天吃什麼？` 不再佔據多數 viewport。range 是 signature interaction：刻度、thumb、目前距離與鍵盤操作共享同一語意，不再保留裝飾性的 3 KM／5 KM rail 或移動光點。

結果區以 destination sheet 呈現：店名是最大資訊，地址與距離以細分隔線排列，外部地圖動作緊接內容，Google Maps attribution 在容器下緣。長繁中與 Latin 店名、地址在 320px 自然換行。狀態沿用 aria-live，錯誤、載入與成功皆有文字，不以顏色單獨表達。motion 只允許 range thumb 與狀態內容的短暫回饋；prefers-reduced-motion 下直接切換。

`src/EatThis.Web/index.html` 的 body 第一個子節點加入可在 production build 保留的方向 contract comment，包含 THESIS、OWN-WORLD、STORY、FIRST VIEWPORT、FORM 與 seed 1d08afa1，以及 FINISH 原文：unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance。實作完成後以 build output 搜尋 seed，確認 contract 未被移除。根目錄 `DESIGN.md` 必須在最後一次視覺修正後依實際成品產生，而不是預先描述未完成畫面；既有 `docs/ui/eatthis-mobile-web-direction.md` 同步改寫為新方向與狀態表。

### 使用 TDD 與有界視覺驗證收尾

先更新失敗測試，再改 production code。前端以 Vitest 驗證預設 100、調整無副作用、700 公尺 submit、3000 上限、無 5000 action、繁中與 fallback 顯示、raw GOOGLE 消失及 attribution accessible name。後端驗證 99/100/3000/3001/5000 邊界、provider 未被非法 request 呼叫，以及序列化 request 含 languageCode zh-TW。

完成後執行前端 test:run 與 build；.NET 測試由 Test-Runner sub-agent 執行，API/refactor build 由 Build-Fixer sub-agent 使用 .NET SDK 10 執行。若出現 NETSDK1045，先檢查 SDK 選擇，不得降級 target framework。視覺驗證在同一 local origin 以 390px、320px 與 desktop 一次批次截圖，涵蓋 idle、busy、no-results、中文結果、長 Latin fallback 與 attribution；集中修正一次後最多再確認一次。實作階段在編輯 UI 前載入 Impeccable craft floor，完成後跑一次 detector，再由 finish reviewer 對照方向 contract；最後由 documenter 產生根目錄 DESIGN.md。

## Implementation Contract

**Behavior**

- 初始 radius 顯示 100 公尺；slider 可選 100 至 3000 公尺且每格 100 公尺。
- 只移動 slider 不觸發 GPS、API 或 loading state。按主要動作才取得一次當前位置並提交當下 radius。
- 成功仍只顯示一間；no-results 不自動重試、不自動改 radius，也不顯示 5 公里 action。
- Google result 優先使用 provider 回傳的繁中名稱與地址，沒有翻譯時仍顯示有效 fallback。
- 結果不顯示 raw provider；Google result 在同一結果容器內顯示官方 Google Maps attribution。

**Interface / data shape**

- Public request 保持 `{ latitude: number, longitude: number, radiusMeters?: number }`，但有效 radius 變為 100..3000。
- Public response 欄位與 provider-neutral `IPlaceProvider.SearchAsync(NearbySearchQuery, CancellationToken)` 不變。
- Google request 新增 `languageCode: "zh-TW"`；此欄位不進入 public contract。
- Composable 對 App 暴露 selectedRadiusMeters 與既有狀態；固定 5 公里 retry API 從 composable view contract 移除。

**Failure modes**

- radius 小於 100 或大於 3000：前端不 submit；API 直接呼叫則回 HTTP 400 invalid_request 且不呼叫 provider。
- Google 沒有 zh-TW 翻譯：顯示 Google 提供的最近 fallback，不把它當成 provider error 或 no-results。
- 官方 attribution asset 無法取得或來源無法驗證：不得以自製或修改過的 logo 取代；implementation 保持未完成並回報阻塞。
- provider、rate-limit、permission 與 unsupported-geolocation 錯誤沿用既有 stable state 與 plain-language recovery。

**Acceptance criteria**

- `npm run test:run` 與 `npm run build` 在 `src/EatThis.Web` 通過。
- Test-Runner sub-agent 執行 `dotnet test tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj` 全數通過；Build-Fixer sub-agent 執行 `dotnet build EatThis.slnx` 成功。
- production build 可搜尋到 seed `1d08afa1`。
- 320px 畫面無水平 overflow；390px 首要 task area 同時可辨識 radius、slider 與主要 action。
- 中文結果、長 Latin fallback、no-results 與 Google Maps attribution 均完成一次 browser 驗證；Google Maps accessible name 可由 accessibility tree 辨識。
- Impeccable detector 只跑一次，finish reviewer 完成 verdict，最後的 DESIGN.md 與 UI direction 文件描述實際 shipped world。

**Scope boundaries**

- In scope：Vue radius state與版面、前後端 3 公里邊界、Google zh-TW request、Google attribution、相關測試與設計文件。
- Out of scope：地圖、更多店家、持久化設定、新 provider、新翻譯服務、照片與評論、資料庫及 runtime dependency。

## Risks / Trade-offs

- [100 公尺預設值提高 no-results 機率] → no-results 明確顯示已搜尋距離並把 slider 留在原位，引導使用者自行增加後再次搜尋。
- [zh-TW 不是每間店都有翻譯] → 保留 Google fallback，不對字集做過濾，也不宣稱 100% 中文。
- [後端先收窄會讓舊版 5 公里重試失敗] → 前端與後端同 release，部署順序採新前端先、後端限制後；rollback 則先恢復後端再恢復舊前端。
- [官方 logo 可能與新視覺系統不完全一致] → 不修改 logo，以留白、位置與周邊層級整合，而不是重新上色。
- [完整視覺替換造成響應式或狀態回歸] → TDD 覆蓋狀態與內容，browser 以一輪多 viewport/多狀態批次檢查，最多一輪集中修正與一輪確認。
- [本機缺少 .NET SDK 10] → 將 NETSDK1045 視為 toolchain blocker，選用 SDK 10 後重跑，不修改 target framework。

## Migration Plan

1. 先合併前端可調半徑、移除 5 公里路徑、Google attribution 與新視覺；此版本送出的 100..3000 對舊後端仍合法。
2. 再部署後端 3000 上限與 Google zh-TW request；公開 JSON shape 不需資料 migration。
3. 以 100、3000、3001 三個 radius 做 endpoint smoke check，再以手機 browser 完成繁中結果與導覽 smoke check。
4. Rollback 時先恢復後端 5000 上限，再恢復會送出 5000 retry 的舊前端，避免舊前端遇到 400。

## Open Questions

無；半徑、預設值、視覺定位、繁中 fallback、Google Maps attribution 與 3 公里上限均已由使用者確認。
