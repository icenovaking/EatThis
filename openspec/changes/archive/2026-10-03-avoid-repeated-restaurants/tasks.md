## 1. 合約與後端抽選

- [x] 1.1 先在 tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs 與 tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs 加入 Extend the public contract compatibly and validate history 的失敗案例：省略/null、1000/1001 項、空白/null/錯誤型別/2049 字元、頂層欄位與 resetNavigationUrls；驗證無效輸入零 provider 呼叫，透過 Test-Runner 子代理確認新測試先失敗。
- [x] 1.2 實作「有界的排除合約」：在 src/EatThis.Api/Contracts/NearbyFoodContracts.cs 定義可選歷史輸入與獨立成功 DTO，在 src/EatThis.Api/Domain/NearbySearchQuery.cs 的驗證入口拒絕無效歷史，在 src/EatThis.Api/Program.cs 映射加法式成功回應；不將歷史送給 Google。以 1.1 測試通過驗證 Extend the public contract compatibly and validate history。
- [x] 1.3 在 tests/EatThis.Api.Tests/RandomSelectionTests.cs 加入 Prefer candidates absent from recent history 與 Restart exhausted candidates without consecutive round-boundary repeats 的固定亂數案例，涵蓋未見餐廳優先、A/C/B 後下一輪只能 A/C、第二輪內不重複、單一候選、空集合、大小寫鍵、last 不在集合、條件切換及每次僅一次 provider 呼叫；由 Test-Runner 子代理驗證先失敗。
- [x] 1.4 在 src/EatThis.Api/Application/NearbyFoodService.cs 實作「候選耗盡與新一輪」，以 C 與 U 決定抽選集合及 ResetNavigationUrls，保留 Filter candidates before random selection 的可用性/評分/去重順序；用 1.3 測試確認不放寬條件、只重設 C 且新輪第一家避開 last。

## 2. 瀏覽器歷史與流程整合

- [x] 2.1 在 src/EatThis.Web/src/recommendationHistory.spec.ts 先以注入時鐘與儲存替身定義 Retain bounded browser recommendation history：1799999/1800000 毫秒、未來/損壞時間、未知版本、大小寫去重、1000 筆界線、只移除指定鍵後新增本次結果與儲存拋錯回退；執行前端 test:run 確認新案例先失敗。
- [x] 2.2 在 src/EatThis.Web/src/recommendationHistory.ts 實作「有期限的推薦歷史」，依 design 的 version 1 shape、清理、保留最新 1000 筆、重設合併順序與頁面記憶體 fallback；以 2.1 全數通過驗證重新載入可沿用有效紀錄，儲存失敗不阻止推薦。
- [x] 2.3 在 src/EatThis.Web/src/composables/useNearbyFood.spec.ts 與 src/EatThis.Web/src/api/nearbyFoodApi.spec.ts 加入 request 帶歷史、成功才更新、失敗不重設、重建 composable、修改條件仍保留、reset 不影響其他鍵及舊回應相容案例；以 test:run 確認整合前先失敗。
- [x] 2.4 在 src/EatThis.Web/src/types.ts 擴充 optional 歷史 request 與 resetNavigationUrls response，在 src/EatThis.Web/src/composables/useNearbyFood.ts 每次送出前讀取有效歷史，selected 時移除 reset 鍵後記錄結果；用 2.3 測試與 typecheck 驗證 Retain bounded browser recommendation history 及跨輪行為。

## 3. 回歸與交接

- [x] 3.1 回歸驗證 Keep the provider credential server-side、Filter candidates before random selection 及 Validate an optional restaurant category：由 Test-Runner 子代理執行 .NET 測試並只回報摘要或失敗 log；由 Build-Fixer 子代理驗證 API 合約可編譯。確認原有類型、評分、provider-neutral 合約及無 secret 洩漏仍通過。
- [x] 3.2 執行前端 test:run、typecheck 與 build，檢查沒有額外自動重試；以受控 A/B/C 候選在瀏覽器驗證換輪、重新整理、切換條件與單一候選行為，將自動測試與手動證據分別記錄，工具不可用則明列尚未執行的瀏覽器項目。
- [x] 3.3 更新 README.md 說明 30 分鐘、候選耗盡重開、跨輪避開上一家、瀏覽器同源保存及本次最多 20 筆上游候選的範圍，列出儲存不可用與極端超過 1000 筆的限制；內容審閱對照兩份 delta spec，執行 spectra analyze 與 spectra validate 確認提案、設計與實作證據一致。
