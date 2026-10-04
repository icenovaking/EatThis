## 1. 回歸契約

- [x] 1.1 在 src/EatThis.Web/src/app.spec.ts 更新 Present concise search guidance while retaining action feedback 的舊文字保留測試：初始頁無定位說明／準備好了／兩段頁尾，保有空 live region、按鈕條件與可存取名稱；新增 idle 改為 700 公尺、日式、4.5 星仍無狀態文案及定位／API 呼叫的斷言。於 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run -- src/app.spec.ts，記錄預期缺席斷言失敗。

## 2. 初始介面精簡

- [x] 2.1 完成「移除靜態下方文案與初始狀態裝飾」：在 src/EatThis.Web/src/App.vue 刪除 action-note、footer-note 及 idle 文案，在 src/EatThis.Web/src/styles.css 清理專用樣式與失效 selector，維持 action-detail；以上述初始缺席、條件摘要斷言及差異檢查確認範圍。
- [x] 2.2 完成「依既有搜尋狀態顯示操作回饋」：在 src/EatThis.Web/src/App.vue 保留穩定空 live region，非 idle 時才放入可見狀態內容與樣式。更新 src/EatThis.Web/src/app.spec.ts 的狀態 selector，沿用並補齊規格例表八種非 idle 狀態、錯誤復原、Google attribution 與 busy 禁止重複提交的缺漏案例，以 App 測試全綠驗證；不改動 useNearbyFood 的資料與流程。

## 3. 文件與驗證

- [x] 3.1 更新 DESIGN.md 與 docs/ui/eatthis-mobile-web-direction.md：初始畫面以主按鈕結束、保留目前條件、非 idle 才出現回饋；對照 delta spec 確認不再要求保留靜態下方文字，不回改封存歷史。
- [x] 3.2 於 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build，確認通過並檢查 dist 只由建置更新；记录測試、建置及待驗限制。
- [ ] 3.3 在 320、390、768 CSS px 驗收初始頁无被刪除區塊的文字、圓點、線條與占位；搜尋後的進度、錯誤及結果仍出現，螢幕閱讀器能接收空 live region 的更新。記錄實測結果；無瀏覽器／輔助科技時記錄待驗並維持本任務未完成，不阻擋其餘可執行任務。
