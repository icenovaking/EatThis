## Context

目前 useNearbyFood 每次定位後呼叫 pick，NearbyFoodService 在可用性、評分與 navigationUrl 去重後獨立抽籤。Google adapter 每次最多回傳 20 筆，沒有跨請求歷史。此功能是 Feature，沿用既有 endpoint 與候選識別，不增加上游查詢。

## Goals / Non-Goals

In scope：30 分鐘歷史、新一輪、跨輪防連續、同源瀏覽器持久化、API 合約與驗證。
Out of scope：跨裝置帳號同步、所有地理餐廳的完整遍歷、同時多分頁抽選的原子保證、自訂時間設定、UI 改版、資料庫及額外 Google 查詢。

## Decisions

### 有期限的推薦歷史

新增 recommendationHistory 模組，管理讀取、驗證、到期清理、成功結果合併與保存；以單一 localStorage adapter 和可注入時鐘測試。不是單純 forwarding wrapper：刪除它會失去期限、重設及持久化行為。useNearbyFood 負責呼叫時機，不再建立 store/service 包裝層。

儲存 key 為 eatthis.recommendation-history.v1，shape 為 { version: 1, entries: [{ navigationUrl, shownAt }], lastShown: { navigationUrl, shownAt } | null }。時間使用瀏覽器 epoch milliseconds。只保存識別與時間，不保存名稱、座標或完整上游資料。navigationUrl 依既有後端 OrdinalIgnoreCase 比對；前端對 Google URL 的 ASCII 字元使用同等不分大小寫鍵值，保留原始 URL 傳送。

now - shownAt >= 1800000 即到期；未來時間、非有限數值、無效 shape 或 version 不作有效歷史。lastShown 同樣到期。相同鍵只保留最新時間。上限 1000 筆、每個鍵最多 2048 字元，超過時保留最新 1000 筆；這高於既有每分鐘 30 次限制在半小時內的正常單一來源使用量，跨網路極端流量僅保證保留範圍內不重複。

每次抽選前重新讀取儲存以支援依序使用不同分頁；讀寫拋錯時沿用頁面記憶體，不阻止推薦。失敗與未顯示的結果不記錄。成功賦值 place 並進入 selected 時視為顯示，使用當下時間更新。替代方案 sessionStorage 不符合關閉後保留期待，伺服器帳號儲存超出需求。

### 有界的排除合約

NearbyFoodRequest 與 SearchRequest 加入 excludedNavigationUrls?: string[] | null 及 lastNavigationUrl?: string | null；省略或 null 等同空歷史。清單最多 1000 項，每項及 lastNavigationUrl 必須非空白且長度不超過 2048；無效型別或超限回傳既有 400 invalid_request，不呼叫 provider。重複鍵以不分大小寫集合處理。不接受前端決定任意上游網址；這些字串只作集合比較，絕不發送 HTTP 請求或轉送 Google。

成功回應保持原有 place 欄位在頂層，新增 resetNavigationUrls: string[]，平常為 []，重開一輪時為本次全部合格候選的鍵。在 Contracts 定義專用成功回應 DTO，由 Program 將 PickResult.Success 的 Place 與 ResetNavigationUrls 映射進去，provider 的 PlaceCandidate 不加入抽選狀態。前端將新欄位視為可選以容忍舊後端。替代方案只有布林 reset 會迫使前端清空其他搜尋條件的歷史，因此不用。

### 候選耗盡與新一輪

NearbyFoodService.PickAsync 先沿用既有可用性、評分及 navigationUrl 去重取得 C，再以請求排除集合 H 取得 U = C - H。

1. C 為空：既有 404 no_results，不抽籤、不重設。
2. U 非空：只在 U 抽選，resetNavigationUrls 為 []。
3. U 為空且 C 非空：開始新一輪；C 至少兩家時排除 lastNavigationUrl 後抽選，若 last 不在 C 則從完整 C 抽；C 只有一家則選該家。resetNavigationUrls 為整個 C，包含上一輪最後一家。
4. 前端收到成功結果，僅移除 resetNavigationUrls 對應歷史，然後加入本次顯示的餐廳與時間，更新 lastShown 並保存。先移除再加入，確保本輪第一家不會下一次又被抽到。

其他搜尋範圍的歷史不清空；改條件或位置直接使用當次 C 與相同 H。每家到期即重新參選，因此輪次是當下候選和未到期歷史的動態結果，不凍結第一次搜尋清單。替代方案前端遇重複重試會浪費上游額度，禁止採用。

## Implementation Contract

- Observable behavior：穩定候選 A、B、C 在期限內每輪各顯示一次；A → C → B 後只能 A 或 C 作新一輪第一家，之後本輪仍逐一抽取。
- Boundary：A 12:00 顯示，12:29:59.999 仍排除，12:30:00 可參選；新一輪可提前解除當次 C 的期限。候選只有 A 時每次可顯示 A。
- Search changes：歷史 A、B、D，當次 C = A、B 時重設僅 A、B，D 保留；若 C = A、B、E，優先抽 E。
- Failure：定位失敗、404、429、provider error 不加入或重設有效歷史；到期清理仍可進行。儲存損壞當作空紀錄，儲存不可用回退頁面記憶體。結果不因儲存失敗轉成錯誤頁。
- Acceptance：使用注入時間與固定亂數驗證期限、集合資格與新一輪；用 endpoint 測試驗證無效輸入不呼叫 provider、每次搜尋只呼叫 provider 一次與頂層合約相容性；前端測試驗證重新建立 composable、換條件、成功/失敗紀錄。瀏覽器手動驗證重新整理後有效，與自動測試證據分開記錄。
- 範圍只限 proposal 所列檔案及上述行為，provider adapter 與版面不改動。

## Risks / Trade-offs

- 上游每次候選不同 → 以本次合格 C 判斷耗盡，不宣称全部地理餐廳已遍歷。
- navigationUrl 變更可能視作另一家 → 沿用既有去重識別，避免此功能另加供應商專屬 ID 合約。
- 關閉 localStorage 或清除網站資料 → 降級記憶體或空歷史，重新載入後無法保留；不阻塞挑選。
- 多分頁同時請求 → 讀取最新紀錄改善依序操作，但不保證同時請求不重複。
- 調整系統時鐘 → 丟棄未來紀錄，避免異常長時間排除。

## Migration Plan

先部署相容舊請求的後端，再部署前端。無資料庫遷移；localStorage key 版本隔離。回滾前端即停止歷史功能，舊頁面忽略新增回應欄位；回滾後端時新前端仍能顯示舊回應，但不承諾排除策略。

## Open Questions

無阻擋實作的未決事項。
