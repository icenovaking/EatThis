# Verification

## Scope and current status

Implemented review totals through Google Nearby Search, the existing response contract and the rating row. Implemented a persistent feedback container with a measured height high-water mark, removal of stale results, observer cleanup and no scroll/focus restoration. Existing selection, location and history behavior remains unchanged. No commit, deployment or archive was performed during this apply verification.

Task 1.1 follows its explicit unavailable-browser branch: browser connection returned `No browser is available`, and the supported discovery returned an empty list. The reported mobile jump has **not** been reproduced by the agent. Removing the old result and reducing document height remains a candidate cause, not a confirmed browser diagnosis.

Tasks 2.2 and 2.3 have implementation and automated evidence but remain unchecked because each task also requires browser acceptance. Task 3.2 is entirely pending. No DOM test is evidence of actual scrollY stability, touch layout or screen-reader behavior.

## Frontend evidence

Working directory: F:\Website\EatThis\src\EatThis.Web.

- RED: npm run test:run -- src/app.spec.ts src/composables/useNearbyFood.spec.ts — 10 failed, 99 passed. Expected failures: absent review-count display and absent persistent feedback container.
- GREEN: same command — 109 passed.
- Regression mutation: temporarily removed the existing place reset in recommend, then ran npm run test:run -- src/composables/useNearbyFood.spec.ts -t "clears the previous result before a retry" — 1 intended failure (old result remained), 60 tests outside the filter. Restored source in a finally block; there is no production composable diff.
- Final full suite: npm run test:run — 128 passed across 4 files (48 App, 61 composable, 12 history, 7 API), exit 0. Includes the restored regression case.
- npm run typecheck — exit 0.
- npm run build — exit 0, 17 modules, refreshed tracked dist index and hashed CSS/JS.

The height test supplies controlled natural heights 80, 640, 400 and 720. It verifies pre-update capture before result removal, no shrinking for smaller content, no cumulative growth on repeated observations, expansion for taller content and observer disconnection. These assertions test the presentation lifecycle across state transitions, not a browser layout engine. Cosmetic spacing is excluded from TDD assertions and remains subject to browser inspection.

## Backend evidence

Test-Runner executed all .NET commands from F:\Website\EatThis.

- RED: dotnet test tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj --filter "FullyQualifiedName~GooglePlacesProviderTests|FullyQualifiedName~NearbyFoodEndpointTests|FullyQualifiedName~ContractFixtureTests" --verbosity quiet — 28 failed, 99 passed, 0 skipped. Runtime contract assertions were written before production edits.
- GREEN: same command — 127 passed.
- Full suite: dotnet test EatThis.slnx --verbosity quiet — 168 passed, 0 failed, 0 skipped, exit 0.
- The first full run exposed an old exact field-mask assertion in ProxyRestrictionTests; its expected mask was updated to include userRatingCount, and the full run passed afterward. No selector algorithm changes or compiler errors.

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| 「全程保持原本位置」— retain result space rather than stale content | App test `retains measured feedback space across retries without stale results or scroll commands`; tests measure lifecycle decisions only, real scrollY acceptance pending |
| 「全程保持原本位置」— failures | App test `keeps reserved feedback space and removes old navigation on %s`, covering all five failure states; browser acceptance pending |
| Preserve retry clearing and duplicate blocking | Composable test `clears the previous result before a retry and keeps it cleared after failure`, mutation detected and restored |
| 「新增評論數」— 1234, zero and maximum | App test `shows review count %s beside the actual rating` |
| 「新增評論數」— missing, invalid, and missing rating | App tests `omits an unavailable or invalid review count %s without losing the rating` and `shows a known review count even when the rating is absent` |
| Count normalization and one upstream call | `Google_review_count_is_normalized_without_removing_usable_places`, including missing/null/negative/fraction/string/overflow/bool/object and exact field mask, URL and CallCount=1 |
| Nullable success contract | `Success_propagates_nullable_review_count_without_changing_selection`, for null, 0, 1234 and int.MaxValue |
| Shared fixture | `Api_success_response_matches_the_shared_frontend_contract_fixture`, including count=1234 |

## Pending browser acceptance

No connected desktop, iOS Safari or Android Chrome browser is available. Still required on those platforms:

- Fixed viewport: measure scrollY immediately before and after each DOM update, including the intermediate frame; difference at most 1 CSS pixel with no jump-and-restore.
- First recommendation, repeated recommendation, shorter/taller name and address, and every failure state, including near the document bottom.
- User scrolling from 600 to 800 while waiting: completion stays at the latest position, never restores 600.
- Initial idle has no reserved space; subsequent shorter results can retain intentional bottom whitespace. Repeated searches do not cumulatively grow the reserved region.
- At 320 CSS pixels and desktop width, rating and count wrap without horizontal overflow, including 2,147,483,647; navigation remains reachable.
- Native touch and keyboard interaction, focus behavior while disabling the action, and polite state announcements through assistive technology.

## Sharp-edge review

Field selection remains server-owned, credentials remain backend-only, and no extra call or arbitrary upstream parameter was introduced. Optional malformed count metadata degrades to null instead of failing the restaurant; frontend also validates type, range and integrality. Zero is a valid count. Counts do not affect selection eligibility, probability or history. Existing Google attribution and navigation semantics remain intact.
