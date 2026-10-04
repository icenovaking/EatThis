## 1. 建立操作契約測試

- [x] 1.1 在 src/EatThis.Web/src/app.spec.ts 先為 Scroll to the completed recommendation outcome 加入失敗測試：成功只定位已渲染的新 article，各五種終態錯誤只定位 status，連續相同錯誤仍每次一次；驗證呼叫當下的餐廳名稱、目標與次數，保存 npm run test:run -- src/app.spec.ts 的 RED 證據。
- [x] 1.2 補齊零捲動與偏好測試：idle、定位、搜尋、忙碌重按、條件編輯及一般重繪無額外捲動；減少動態效果與查詢期間偏好切換使用即時定位，缺少 matchMedia 安全回退，目標/方法不可用、卸載或舊操作被取代不拋錯或發出舊捲動。以測試確認現有焦點、單次 GPS/API 與高度保留契約，更新先前全程不捲動的斷言。

## 2. 實作與文件

- [x] 2.1 在 src/EatThis.Web/src/App.vue 完成「完成後定位對應內容」與「以明確操作生命週期觸發一次」：包裝明確操作、忙碌防重、等待結果與 DOM 更新後選擇 article/status、卸載及過期回呼防護；沿用高度保留，保留無舊結果與評論數。以第 1 組測試 GREEN 驗證時序、目標與次數。
- [x] 2.2 完成「動態偏好與可及性」：捲動當下選擇 smooth 或 instant，不移動焦點，必要時調整 src/EatThis.Web/src/styles.css 的動態偏好配合。以第 1 組偏好與缺能力測試驗證；同步 DESIGN.md 與 docs/ui/eatthis-mobile-web-direction.md，內容審查確認 Preserve the viewport throughout recommendations 已由本次新需求取代、等待期間仍穩定、完成後只定位一次。

## 3. 交付驗證

- [x] 3.1 工作目錄 F:\Website\EatThis\src\EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build，確認回歸與編譯通過並更新 dist；在專案根目錄執行 git diff --check 與 spectra validate scroll-to-search-outcome，建立本變更 verification.md 保存命令結果、測試對照及未執行項目，不宣稱 DOM 測試證實視窗位置。
- [ ] 3.2 真實手機瀏覽器與桌面驗收：320 CSS px、一般手機寬度及桌面確認首次、重試、長短卡片、五種失敗都在完成後讓店名或錯誤可見；確認定位/搜尋階段不主動捲動、一般平滑與減少動態效果立即、等待時自行滑動後仍完成定位一次、完成後滑動或改條件不被拉回，鍵盤焦點與狀態朗讀正常。記錄平台與證據；沒有可用瀏覽器時保持本任務未完成。
