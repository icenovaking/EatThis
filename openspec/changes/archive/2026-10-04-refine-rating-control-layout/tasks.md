## 1. 建立回歸驗證

- [x] 1.1 在 src/EatThis.Web/src/app.spec.ts 為 Present concise search guidance while retaining action feedback 建立回歸：五段指定文案不存在、保留目前條件／定位說明／準備好了／兩段頁尾，以及群組名稱與 ARIA 引用有效；於 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run -- src/app.spec.ts，先記錄預期文案斷言失敗。

## 2. 調整介面

- [x] 2.1 完成「精準刪除五段文案並保留動作區文字」：在 src/EatThis.Web/src/App.vue 刪除指定五段與孤立 brand-rule、清理失效 aria-describedby，於 src/EatThis.Web/src/styles.css 清除僅對應刪除內容的規則與空白；以上述 App 回歸測試轉綠及差異檢查證明 action-detail、action-note、state-panel、footer-note 與 stateCopy 保留。
- [x] 2.2 完成「移除半星 hover 外框並保留鍵盤焦點」及 Preserve half-star operation with unobstructed pointer feedback：移除 src/EatThis.Web/src/styles.css 的 rating-half hover 外框，保留 focus-visible、原生 radio 與半星語意；沿用 App 的半星／重設／無 GPS 或 API 呼叫回歸，並以手機觸控與桌面鍵盤檢查證明框線行為符合設計，無瀏覽器時明記未驗證。
- [ ] 2.3 完成「評分文字與重設選項共用右側欄」：在 src/EatThis.Web/src/App.vue 與 src/EatThis.Web/src/styles.css 建立共用右欄及窄螢幕整組換行，保留至少 24px × 44px 的半星目標；以 320、390、768、1280 CSS px 的 null、0.5、4.5、5 畫面確認無水平溢出、按鈕文字不折行、文字盒與按鈕外框左右邊界誤差不超過 1 CSS px。

## 3. 文件與整體驗證

- [x] 3.1 同步 DESIGN.md 與 docs/ui/eatthis-mobile-web-direction.md，記錄移除 hover 框、右欄及窄螢幕行為、五段文案刪除與動作區保留；逐項對照本變更 proposal、design 與 delta spec，確認不存在舊 hover 或頁首說明要求。
- [x] 3.2 於 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build，确认半星與搜尋狀態回歸通過、建置成功；檢查已追蹤 dist 產物僅由 build 生成，記錄實際瀏覽器驗證結果及未執行項目，不以 DOM 測試宣稱手機外觀通過。
