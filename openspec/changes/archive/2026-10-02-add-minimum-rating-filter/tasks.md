## 1. 契約與失敗案例

- [x] 1.1 重查 refine-nearby-food-search、目前半徑／zh-TW／attribution 行為與 CodeGraph 索引；完成「以增量交付管理計費與規格依賴」的依賴紀錄，確認只新增評分、不恢復 5000 retry。驗證目標為 current source、前一變更 tasks 與本 change delta 比對；有索引時改碼前執行 codegraph_impact，無索引不建立索引。
- [x] 1.2 先在 tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs、tests/EatThis.Api.Tests/RandomSelectionTests.cs、tests/EatThis.Api.Tests/CandidateNormalizationTests.cs、tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs 加入 Validate an optional half-star minimum rating、Filter candidates before random selection、Expose a bounded nearby-food pick endpoint、Query Google Places through an adapter 的失敗案例：null／omitted、0.5／5、4.3／0／5.5／string／boolean／array／malformed input、inclusive comparison、filter-before-dedup、null candidate、invalid normalized rating、零候選不抽選及固定 rating mask。依 code-testing-agent skill 建立測試，Test-Runner sub-agent 依 run-tests skill 在 F:/Website/EatThis 執行 focused tests，證明 discovery 成功且因尚缺新行為而 red。
- [x] 1.3 在 src/EatThis.Web/src/app.spec.ts、src/EatThis.Web/src/composables/useNearbyFood.spec.ts 與 src/EatThis.Web/src/api/nearbyFoodApi.spec.ts 先加入 Choose a minimum rating using half-star controls、Snapshot distance and rating for each recommendation、Select exactly one nearby food place、Display essential place information 的測試；涵蓋十個半星值、預設／reset、3.5／4／4.5 點選、aria checked／名稱、無 GPS/API 副作用、locating 中修改兩條件不影響當次 request、invalid pending 不定位、busy 防重送及 missing rating fallback。在 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run，確認新測試只因缺少上述行為而失敗。

## 2. 後端評分與抽選

- [x] 2.1 完成「以可空半星契約保存搜尋快照」的 API 部分：在 src/EatThis.Api/Contracts/NearbyFoodContracts.cs 與 src/EatThis.Api/Domain/NearbySearchQuery.cs 增加尾端預設 null 的 MinRating／Rating；讓缺省／null unrestricted、十個值合法、非法型別／值 HTTP 400 invalid_request 且 provider 零呼叫。在 src/EatThis.Api/Program.cs 的 pick handler 明確讀取 JSON、捕捉 binding 格式／型別與 content-type 錯誤回 ApiErrorResponse.InvalidRequest，保留 request cancellation 與 rate limit，不增加代理層。Test-Runner focused endpoint tests 驗證且保留 radius 100..3000／座標邊界。
- [x] 2.2 完成「取得正規化評分後才篩選抽選」的 Google 部分與 Query Google Places through an adapter：GooglePlacesProvider 固定 mask 加 places.rating，GooglePlace 讀可空 rating，MapCandidate 保留 finite 1..5 原值，其餘 numeric/missing/null 轉 null；保留既有 zh-TW、20 candidates、types、secret boundary，不向 Google 送 minRating。Test-Runner focused provider tests 驗證 request JSON、4.3 原值及 missing／越界 mapping。
- [x] 2.3 完成 Filter candidates before random selection：在 PlaceCandidateRules.IsUsable 維持 null 合法且拒絕 non-finite／越界 normalized rating，NearbyFoodService.PickAsync 依 validation → inclusive threshold → URL dedup → random 執行，無候選保留 no_results 且 random 零呼叫。Test-Runner focused RandomSelectionTests 與 CandidateNormalizationTests 驗證 3.9／4.0／4.3／null 的 4.0 門檻、unrestricted、4.5 空集合及重複 URL。
- [x] 2.4 確保 Keep the provider credential server-side 與 provider-neutral additive response 相容：更新 tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs、tests/EatThis.Api.Tests/ContractFixtureTests.cs 與 tests/fixtures/nearby-food-success.json，檢查 nullable rating、未提供門檻之舊請求及 no secret。由 Build-Fixer sub-agent 在 F:/Website/EatThis 執行 dotnet build EatThis.slnx，接著 Test-Runner 依 run-tests skill 跑全 API suite；驗收為 build 成功與 API 全通過，只回報失敗的詳細 logs。

## 3. 半星介面與搜尋狀態

- [x] 3.1 完成「以可空半星契約保存搜尋快照」的 frontend 部分與 Snapshot distance and rating for each recommendation：types.ts 新增 optional nullable minRating、nullable/optional rating 相容欄位；useNearbyFood 暴露 selectedMinRating null 預設、recommend await 前驗證與快照兩條件，search 只讀快照，保存 submitted conditions 用於結果與 recovery。於 F:/Website/EatThis/src/EatThis.Web 用 focused composable／API tests 驗證序列化、無副作用、異步調整和 invalid 值。
- [ ] 3.2 UI 修改前讀 Impeccable craft-floor；完成「以五顆星提供半星選擇」與 Choose a minimum rating using half-star controls：App.vue 建立五個 SVG 星與十個 radio half-label、不限 option、persistent yellow／gray 填色與 numeric label，styles.css 提供共享評分 token、每半星至少 24x44 CSS px、visible focus 與手機換行。DOM tests 驗證 checked names、left/right mapping、reset 和 text；browser 驗證 pointer/touch／原生 arrows／Space／Tab。
- [x] 3.3 完成「顯示評分並保留無結果條件」、Display essential place information 與 Select exactly one nearby food place：App.vue 結果顯示實際 4.3／尚無評分，no-results 使用 submitted conditions 並提示降低 threshold／在未達 3000 時增加 radius，pending controls 保留、不自動重試；名稱／地址／導航／attribution 維持。於 F:/Website/EatThis/src/EatThis.Web 執行 app.spec.ts 與 composable suite 驗證 submitted 700／4.5、pending 1000／3.5、3000 邊界與 unrestricted empty state。

## 4. 驗證與交付文件

- [x] 4.1 更新 README.md、PRODUCT.md、DESIGN.md 與 docs/ui/eatthis-mobile-web-direction.md，記錄最低評分半星控制、default unrestricted、nullable rating、submit snapshot、recovery、rating yellow token、Enterprise 計費（包含 unrestricted request）與 20 candidates 限制及 backend-first rollout／frontend-first rollback。內容審查確認不宣稱完整範圍搜尋、不添加 userRatingCount／reviews 或未實作 storage，並確保「以增量交付管理計費與規格依賴」完整落在文件。
- [x] 4.2 在 F:/Website/EatThis/src/EatThis.Web 執行 npm run test:run、npm run typecheck、npm run build、npm run security:check；核對 src/EatThis.Web/dist/ 為產生結果，涵蓋 rating 控制且保留 attribution。驗收為全部成功；在 F:/Website/EatThis 執行 git diff --check、Spectra validation 與 spectra-audit，修正本變更的 warning／error 及 dangerous defaults／type confusion／silent failures。
- [ ] 4.3 以同一 local origin 批次檢查 320px、390px、desktop 的不限、3.5、4、4.5、5、busy 條件快照、no-results、missing rating、長結果、focus、touch／mouse／keyboard 與輔助技術名稱；保存 screenshot 至 .impeccable/review/，確認每半星可點、文字和選取一致、無 overflow／自動搜尋。依 Impeccable 有界驗證流程集中修正並最多確認一次；缺 browser／讀屏工具則明列未完成項目，不能把 Vitest pass 記成 browser pass。

## 本次實作驗證紀錄（2026-10-02）

- 後端 Test-Runner 在 F:/Website/EatThis 確認 endpoint 新案例先 RED（13 failed／13 passed），實作後完整 API suite 84 passed／0 failed／0 skipped。評分篩選及 Google 映射的暫時 mutation 各被 4 與 3 個測試捕捉，且已還原。
- 前端在 F:/Website/EatThis/src/EatThis.Web 確認缺少半星介面與狀態時 29 failed／24 passed；最後 npm run test:run 為 53 passed／0 failed。npm run typecheck、npm run build、npm run security:check 全部成功，production dist 已更新。
- Build-Fixer 在 F:/Website/EatThis 完成 dotnet build EatThis.slnx：0 warnings／0 errors。Spectra validation、git diff --check 均通過；artifact analysis 只有 6 項補充例子的 Suggestion，沒有 Warning／Critical。
- Impeccable detector 對 App.vue 與 styles.css 回傳空 findings；這是靜態掃描，不是視覺驗證。
- Spectra audit discipline：不信任 public minRating 型別，嚴格拒絕數字字串／非法值並在 provider 前失敗；null 明確表示不限、不隱性降低門檻；provider credential／mask／types／language 仍由 server 控制，rate limit 與取消訊號保留。未發現本範圍未處理的 dangerous default、type confusion 或 silent failure。
- 3.2 的介面程式與 DOM 驗證已完成，但其 browser pointer/touch／arrows／Space／Tab 驗證未完成，因此保留未勾選。4.3 的手機／桌面截圖、實際 focus／overflow／讀屏及 finish review 亦未完成：Browser runtime 連線成功，但 getForUrl 回報 No browser is available，browsers.list 為空。沒有取得截圖或宣稱瀏覽器通過；續作應先完成這兩項驗證。
- refine-nearby-food-search 尚有自己的視覺驗證未完成；本輪未修改其 tasks 或封存任何變更。
