# Test Plan — refine-nearby-food-search

## Phase 1 — Frontend RED contract

Update the two existing Vitest suites before production edits.

| Requirement | Planned evidence |
| --- | --- |
| Default 100m, one GPS request | `starts at 100 metres and submits the default radius after one location request` |
| Pending 700m has no side effects, then submits 700 | `updates the pending radius without locating or searching until recommend is activated` |
| Maximum 3000m and no 5000 retry API | `submits the three-kilometre maximum without exposing an expanded retry` |
| Frontend invalid radius guard | `rejects pending radii outside 100 metres through 3 kilometres before locating` |
| Range semantics and labels | `renders a 100-to-3000-metre range control and formats pending values` |
| Busy control and no-results recovery | existing busy test plus `keeps the searched radius after no results and never offers a five-kilometre action` |
| Localized and long fallback content | `renders Traditional Chinese place text unchanged` and `keeps a long Latin fallback visible inside the result structure` |
| Google attribution/no raw provider | `shows accessible Google Maps attribution without the raw provider label` |

Run only `npm run test:run -- src/composables/useNearbyFood.spec.ts src/app.spec.ts` and confirm RED failures are missing-contract failures.

## Phase 2 — Backend RED contract

Update the two existing MSTest suites before production edits.

| Requirement | Planned evidence |
| --- | --- |
| 99/100/3000/3001/5000 boundary | data-driven endpoint boundary tests with explicit status/error/provider-call assertions |
| Invalid radius never calls provider | boundary tests assert provider call count is zero |
| Public request cannot set localization | request-deserialization/endpoint test proving extra `languageCode` cannot affect provider query |
| Server-controlled zh-TW request | provider test captures JSON and asserts `languageCode` equals `zh-TW` |
| Latin fallback remains a candidate | provider test returns a non-empty Latin display name and asserts it is normalized |

The Test-Runner sub-agent runs the narrow filtered tests and reports expected RED evidence without production fixes during this phase.

## Phase 3 — GREEN implementation and focused validation

Implement the smallest backend and frontend changes that satisfy the contract. Re-run focused suites after each seam. Build-Fixer and Test-Runner sub-agents own .NET build/test execution per repository instructions.

## Phase 4 — Final quality review

Re-open every new assertion, map each checklist item to an exact passing test, run the full frontend/API validation requested by Spectra, and record assertion/gap review plus final commands in `.testagent/status.md`.
