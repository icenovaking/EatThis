# 驗證紀錄

## 自動驗證

- 前端 npm run test:run：103 passed，0 failed。
- 前端 npm run typecheck、npm run build、npm run security:check：通過。
- Test-Runner 回報後端完整測試：153 passed，0 failed，0 skipped；完整 solution 非增量建置：0 warnings、0 errors。
- git diff --check：通過。
- TDD：歷史模組最小介面建立後 6 個行為測試失敗，再轉綠；流程整合 7 個測試失敗，再轉綠。後端契約與抽選分別由 Test-Runner 確認紅燈後實作。

## Requirement | Evidence

| Requirement | Evidence |
| --- | --- |
| 半小時內不會重複 | expires at thirty minutes, not one millisecond before；Unseen_candidate_has_priority_across_search_conditions |
| 符合的都出現了之後才能開始重複 | Exhausted_round_after_A_C_B_excludes_last_B；Second_round_draws_A_and_B_once_after_C |
| 新一輪避開上一輪最後一家 | Exhausted_round_after_A_C_B_excludes_last_B |
| 只有一家可連續出現 | Singleton_can_repeat_and_reset_after_deduplication_and_filters |
| 重新整理、改條件仍保留 | records only displayed success and preserves history across reload and changed conditions |
| 重設只限當次候選 | persists across instances and resets only current candidates before adding the result |
| 失敗不清除紀錄 | does not update history or retry on HTTP %s；does not change history when location permission is denied |
| 儲存失敗仍可推薦 | retains page memory when storage writes fail despite readable stale data |
| 有界輸入與合約相容 | History_count_and_key_length_bounds_are_enforced；Invalid_history_returns_invalid_request_without_provider；Omitted_null_or_empty_history_preserves_top_level_place |

## 瀏覽器驗證限制

Browser runtime 回報 No browser is available，依疑難排解列出可用瀏覽器得到空陣列。
尚未執行實際頁面的 A/B/C 換輪、重新整理、切換條件及單一候選操作；上述自動測試不等同真人或代理瀏覽器驗證。
依任務 3.2 的工具不可用處理方式，明列未執行項目，保留作後續人工驗收。

## 範圍

修改僅涉及本功能的程式、測試、README、Spectra artifacts 與前端建置產物。實作驗證當時尚未 commit、archive 或部署。
