## 1. 重現與回歸契約

- [x] 1.1 驗證 Preserve the viewport throughout recommendations：先在可用真實瀏覽器重現清除餐廳前後的 scrollY 與頁高，記錄首次、重試及短結果；無瀏覽器則記錄未執行，禁止把候選根因當成證實。
- [x] 1.2 在前端 app.spec.ts 與 useNearbyFood.spec.ts 先加入失敗測試，覆蓋位置保留所需的持續結果容器、初始 idle 不預留、各錯誤不顯示舊餐廳、忙碌阻擋重複請求及評論數文案邊界；保存 RED 證據，DOM 測試不宣稱能驗證真實捲動。
- [x] 1.3 為 Return and display restaurant review totals 在 GooglePlacesProviderTests、NearbyFoodEndpointTests 與 ContractFixtureTests 加入資料傳遞、一次上游請求及 0、最大值、缺少、null、負數、小數、字串、溢位的失敗測試；委派 Test-Runner 執行 .NET 測試並保存 RED 證據。

## 2. 實作資料與呈現

- [x] 2.1 完成「沿用現有契約傳遞評論數」：GooglePlacesProvider 正規化可選值，NearbyFoodContracts 增加 UserRatingCount，Program 成功映射傳遞，前端 types.ts 相容可選可空值；更新成功 fixture，以 provider、endpoint 及 fixture 測試驗證無效數量不破壞搜尋及不增加 Details 請求。
- [ ] 2.2 完成「維持結果區高度與使用者捲動自主權」：在 App.vue 與 styles.css 的持續回饋容器量測自然高度，更新前保留高度高水位，不顯示舊餐廳、不固定卡片高度、不累加空白；清理觀察器，不主動移動焦點或覆蓋等待期間的新捲動位置。以前端回歸測試及後續瀏覽器 scrollY 驗收確認。
- [ ] 2.3 完成「評分列並列數量與缺值處理」：App.vue 以 zh-TW 顯示有效整數、0 與最大值；缺少與非法值不顯示括號，無評分但有數量仍顯示數量；以 app.spec.ts 驗證文案與 320 CSS px 真實瀏覽器確認無水平溢出。

## 3. 驗證與交付記錄

- [x] 3.1 從 F:\Website\EatThis 委派 Test-Runner 執行後端測試，確認端點、候選與重複預防回歸通過；在 F:\Website\EatThis\src\EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build，確認前端通過並更新追蹤的 dist 產物；API 型別編譯錯誤依 AGENTS.md 交由 Build-Fixer 處理。
- [ ] 3.2 真實 iOS Safari、Android Chrome 與桌面瀏覽器驗收：首次、連續重試、長短餐廳名稱及地址、所有失敗狀態、頁尾附近與等待時自行捲動；固定 viewport 下更新前後 scrollY 差距至多 1 CSS px且無瞬間跳頂，鍵盤焦點不被程式搬移，狀態朗讀保留。記錄各平台證據，未執行的環境保留待辦。
- [x] 3.3 同步 DESIGN.md 與 docs/ui/eatthis-mobile-web-direction.md 的捲動、留白及評論數規則，建立本變更 verification.md 記錄 RED/GREEN、瀏覽器證據或缺口；以內容審查確認未把 DOM 測試當成手機驗收，執行 git diff --check 與 Spectra 驗證確認交付文件一致。
