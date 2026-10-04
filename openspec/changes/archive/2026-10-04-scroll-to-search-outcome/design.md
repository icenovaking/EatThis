## Context

目前 App.vue 的 recommend 事件直接啟動 useNearbyFood，回饋區以量測高度保留空間，沒有主動捲動。使用者已確認希望在每次明確查詢完成後直接看到餐廳或失敗提示。這是操作偏好調整，非後端問題。

## Goals / Non-Goals

In scope：完成後一次性捲動、成功與失敗目標、減少動態效果、卸載保護、前端驗證與設計文件更新。
Out of scope：評論數、篩選條件、候選與歷史演算法、API 契約、增加查詢、改變焦點、捲到整頁最底端、部署與提交。

## Decisions

### 完成後定位對應內容

**Supersedes**: preserve-search-position-and-show-review-count / 維持結果區高度與使用者捲動自主權

成功時以 App.vue 新渲染的餐廳 article 為目標，將頂端對齊可視範圍上方；不足以對齊時接受瀏覽器可達範圍內的位置，至少讓店名可見。失敗時以包含該次訊息的 status 元素為目標，復原提示接續其後。選擇元素定位，避免捲到 document 底部的保留空白或跨過店名。

保留既有回饋容器高度量測、初始 idle 零預留與搜尋期間清除舊結果。此處取代的是「完成後仍禁止自動捲動」，不必刪除搜尋期間避免塌縮的機制。使用者在等待期間自行捲動，查詢完成仍依已確認的流程定位一次；完成後再滑動或編輯條件，不拉回結果。

### 以明確操作生命週期觸發一次

在 App.vue 以本地 async handler 包裝 recommend：忙碌時立即返回；接受操作後等待 recommend 完成，再等待 Vue DOM 更新，選擇當次終態的元素並呼叫一次 scrollIntoView，block 為 start。不要綁定每次 onUpdated、ResizeObserver 或 place 深層變化來捲動，以免評論數渲染、換行、條件編輯造成反覆定位。

接受的操作包含前端條件驗證直接失敗，不要求經過 searching；同樣的失敗終態連續出現時，每次新的明確操作仍各定位一次。卸載後或舊操作已被新的接受操作取代，不再對舊 DOM 發出捲動。以元件生命週期旗標及必要的操作序號防護即可，不新增模組或抽象層。

### 動態偏好與可及性

在捲動當下讀取 prefers-reduced-motion: reduce；一般使用 smooth，減少動態效果時使用明確立即捲動行為（instant）。現有 html 設有 smooth，不能只把選項改為 auto 就假定沒有動畫；保留 styles.css 的減少動態效果規則。matchMedia 不存在時安全退回立即捲動；目標或捲動方法不可用時維持可閱讀內容，不拋錯也不重試 API。

不呼叫 focus，不新增 tabindex；既有 polite live region 繼續提供狀態通知。每次操作只有一次捲動指令，完成後使用者可自由滑動，沒有持續追蹤。

## Implementation Contract

- 起始 idle、locating、searching、僅改條件、重複忙碌點擊：零主動捲動。
- selected：新 article 已存在且內容為本次餐廳後，向該 article 發出一次捲動。
- permission-denied、unsupported-geolocation、no-results、provider-error、rate-limited：各終態渲染後，向當次 status 提示發出一次捲動；復原操作保持可用。
- 連續兩次相同失敗狀態也各一次，成功後一般重繪不新增捲動。卸載或新操作取代後，不執行舊完成回呼的定位。
- smooth 與 instant 由完成時的偏好決定；不移動焦點、不增加定位/API 呼叫。
- 單元測試驗證目標元素、內容渲染時序、呼叫次數與參數；真實手機/桌面驗證可見位置與動態效果，兩者分開記錄。

## Risks / Trade-offs

- 自動捲動會離開等待時使用者自行選擇的位置 → 這是已確認的新行為，限制為每次完成一次，不在之後重繪重複移動。
- 短頁面無法讓卡片頂端精準贴齊視窗 → 以瀏覽器可達位置且店名/錯誤內容可見作驗收，禁止補造固定高度空白來強求對齊。
- 瀏覽器不可用時無法證實動畫與視窗位置 → 自動測試照常進行，獨立的人工驗收任務保持未完成。

## Migration Plan

本次為前端變更，重建追蹤中的 dist。新 delta 移除全程維持 viewport 的舊 requirement，新增完成後定位的 requirement；封存時確認主規格不再同時含有衝突條文。舊封存文件保持歷史原樣，新的設計以 Supersedes 表達決策替代。回退前端版本即可恢復原行為，無資料遷移。

## Open Questions

產品行為已確認，無待決事項。真實瀏覽器可用性於 apply 檢查並記錄。
