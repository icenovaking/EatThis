# 驗證紀錄：add-restaurant-type-filter

日期：2026-10-02。範圍：餐廳類型契約、單次 Google mapping、前端操作與文件；沒有呼叫真實 Google API。

## TDD 與檢查結果

- 前端 production 修改前：28 個新增行為案例因缺少 category state／radio surface 失敗，55 個案例通過。API JSON serializer 已支援 additive field，另以暫時移除 restaurantCategory 的 mutation 驗證兩個新序列化案例失敗，隨後還原。
- 後端 production 修改前：20 個新增案例失敗、1 個通過；非法 category 回 200 而非 400，具體分類仍送泛用 includedTypes。由 Test-Runner 執行並確認是行為失敗，不是编譯錯誤。
- npm run test:run：83 passed，0 failed。
- npm run typecheck：通過。
- npm run build：通過；更新 repository 已追蹤的 dist/index.html 與 Vite hashed assets。
- npm run security:check：通過。
- Test-Runner 的 dotnet test tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj --verbosity quiet：120 passed，0 failed，0 skipped。
- Build-Fixer 的 dotnet build EatThis.slnx --no-incremental：0 warnings，0 errors。
- Impeccable doctor：findings 為空；先前 design.json 維護內容已保留。

## Requirement | Evidence

| Requirement | Evidence |
| --- | --- |
| 11 個單選中文標籤、預設／重設、選取零副作用 | offers eleven labeled native radios with one default and no selection side effects |
| 10 個合法 frontend code 一次搜尋 | submits product category %s in one search |
| 非法 pending code/type 零定位／API | rejects invalid pending category %j before locating |
| locating 三條件快照與下次明確搜尋 | snapshots category before locating and uses later pending category on the next explicit action |
| searching/no-results pending 編輯不污染本次條件且無重試 | preserves submitted category after an empty search without retry or relaxation |
| selected/no-results submitted 中文分類與恢復提示 | keeps submitted category in %s while pending edits wait for next action |
| 最大距離／不限條件明確重試 | offers an explicit retry at maximum radius and unrestricted category without automatic search |
| JSON 保留產品 code/null、rating、radius | serializes restaurantCategory %s with rating and radius |
| 省略/null、10 個合法 API code、response 不變 | Valid_category_reaches_provider_and_preserves_response_shape |
| 10 個非法 API 值/type、zero provider calls | Invalid_category_returns_stable_error_without_provider_call |
| 11 組完整 includedTypes、一次 call、rating/zh-TW/20 | Category_mapping_uses_one_bounded_google_request |
| caller filter 不可覆蓋與 no-results 無重試 | Unsupported_filters_cannot_override_actual_google_category_request |
| provider-neutral Japanese/1000/4、結果實際 4.3 | Alternative_provider_receives_validated_category_and_exact_rating_combination |
| 無合格候選不 fallback、不抽選 | Category_search_without_qualified_candidate_does_not_fallback_or_draw |

## Sharp-edge audit（spectra-audit discipline）

依 Scoundrel、Lazy Developer、Confused Developer 三個視角檢查變更：公開 request 僅接受產品分類精確白名單，Google 類型與 field mask 留在 server，原限流不變；省略/null 明確維持舊搜尋；空白、大小寫不同、未知字串與非字串回 400 而非默默不限；domain query 使用 enum，未知 enum 不落入泛用 mapping。前端驗證在定位前，pending/submitted 分開，無結果不放寬或 fan-out。沒有發現需要修正的安全 sharp edge。

## 瀏覽器檢查狀態

已嘗試 browser runtime 連線，回覆 No browser is available；依 troubleshooting 查詢 connected browsers 為空。沒有 agent-run browser screenshot、pointer/keyboard interaction 或人工使用者驗證。

待人工驗證：320px 與桌面分類自然換行、無水平溢出、點擊目標、勾選與焦點可見、native radio arrows/Space、長條件摘要、忙碌期間編輯及無結果恢復後需再次明確搜尋。以上視覺／原生鍵盤項目不得由 Vitest DOM 成功推論通過。單一畫面的 corner/padding/geometry 未加入 unit assertions，依 TDD test-scope exclusion 留給此 browser 檢查。

## 部署

後端先、前端後；回滾反序。無資料或偏好遷移。尚未部署、提交或封存。
