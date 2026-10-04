## Context

App.vue 的 action-note 與 footer-note 是固定說明；state-panel 則同時承載 idle、locating、searching、selected 及錯誤狀態。使用者的新截圖要求刪除初始畫面主按鈕以下區塊。必須分開靜態文案與動態回饋，並更新前次保留文字的測試與規格。

## Design Source

2026-10-04 使用者提供「去除文字.png」，紅框涵蓋按鈕外定位說明、準備好了提示與兩段頁尾。此圖為刪除範圍標註，不是尺寸設計稿，沒有 .dc.html；需求在本文件完整描述，不依賴桌面檔案。

| 元素 | 目標 |
| --- | --- |
| action-note | 刪除，不留占位 |
| idle state-panel | 不顯示文字、圓點及上下分隔線 |
| footer-note | 刪除兩段文字及容器 |
| action-detail | 保留目前條件摘要 |
| 非 idle 狀態 | 保留既有回饋、復原與结果 |

## Goals / Non-Goals

Goals：初始可見內容在幫我決定按鈕結束，清除被刪區塊的額外間距；搜尋後回饋完整。

Non-Goals：不變更半星對齊、距離、類型、GPS 請求時機、API、歷史紀錄、資料模型、套件或後端。保留結果中的 Google Maps attribution；它不屬於紅框中的靜態頁尾。

## Decisions

### 移除靜態下方文案與初始狀態裝飾

**Supersedes**: refine-rating-control-layout / 精準刪除五段文案並保留動作區文字

只取代該決定中保留 action-note、idle 提示與 footer-note 的部分；先前移除的五段文案仍維持移除。從 App.vue 刪除 action-note、footer-note 及其專用樣式，清理共用 selector 中失效項，不影響 action-detail。idle 時 state-panel 的可見內容、圖示、邊框與間距均不存在，不以透明或 visibility:hidden 保留空間。action-block 及頁面外殼只保留合理底部安全留白，不增加空白補回刪除區塊。

### 依既有搜尋狀態顯示操作回饋

使用既有 state 判斷 idle 與非 idle，無需新增 hasSearched 等重複狀態。保留穩定掛載的空 role=status、aria-live=polite、aria-atomic=true 區域，初始不放文案與裝飾；非 idle 才呈現 state-panel 的內容及樣式，讓定位、查詢與錯誤更新可被輔助科技接收。可用穩定外層 live region 包覆條件渲染的視覺內容。

替代方案整個刪除 state-panel 會讓操作失敗無回饋，故不採用。單純更換 idle 文案仍占空間，不符合截圖意圖。搜尋成功、無結果與錯誤的文案、提交條件快照、復原提示均維持現況。

## Implementation Contract

- 範圍：App.vue 模板／idle 文案、styles.css 的相關樣式、app.spec.ts 回歸及兩份設計文件。主規格以 MODIFIED 完整替換現有同名 requirement；不回改封存歷史。
- 初始與改條件：idle 時看不到 action-note、準備好了、兩段頁尾或狀態裝飾；主按鈕保有目前條件。改成 700 公尺、日式、4.5 星仍不呼叫定位／API，不顯示操作狀態。
- 顯式搜尋：保留 locating、searching、selected、permission-denied、unsupported-geolocation、no-results、provider-error、rate-limited 的文字及可用復原方式，狀態 live region 從空內容更新，busy 時禁用重複提交。
- 結果：餐廳資訊、導航與 Google Maps attribution 保持。靜態下方說明與頁尾在所有狀態都不再出現。
- 介面：state、selectedRadiusMeters、selectedRestaurantCategory、selectedMinRating、submittedConditions、recommend 與請求回應格式不變。
- 自動驗收：先改舊文案保留測試為新缺席要求並取得預期失敗；更新狀態區 selector 後，保留既有操作回饋測試，補齊缺漏的 idle 到非 idle 分支。驗證空 live region、條件摘要與無自動定位／API 副作用。
- 執行位置：F:/Website/EatThis/src/EatThis.Web；npm run test:run -- src/app.spec.ts、npm run test:run、npm run typecheck、npm run build。
- 視覺驗收：320、390、768 CSS px 初始頁面沒有框內文字、圓點、分隔線或原區塊占位，只有頁面安全留白；搜尋後能看到進度／錯誤／結果。螢幕閱讀器驗證從空區域更新能宣告狀態。若瀏覽器不可用，完成可執行工作並記錄視覺與輔助科技驗證為待驗，不宣稱通過。

## Risks / Trade-offs

- [動態區域初次掛載未宣告] → 保留空的 live region，更新內部內容並另行驗證輔助科技。
- [移除頁尾時誤刪結果 attribution] → 刪除只限 footer-note，沿用 Google Maps attribution 測試。
- [舊測試要求文字存在] → 明確更新被新需求取代的斷言，其餘操作狀態回歸維持。

## Migration Plan

僅前端部署，無資料遷移。回滾還原本次模板、樣式及建置產物。設計文件於實作更新，主規格於封存同步。
