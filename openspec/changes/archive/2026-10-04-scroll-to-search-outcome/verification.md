# Verification — scroll-to-search-outcome

Date: 2026-10-04

## Automated evidence

Frontend working directory: `F:\Website\EatThis\src\EatThis.Web`.

- RED: `npm run test:run -- src/app.spec.ts` before implementation: 10 failed, 51 passed. The new completion assertions observed zero scroll calls instead of one; failures covered success, terminal errors, motion preferences and the newer operation's completion.
- GREEN: the same command after implementation: 61 passed.
- `npm run test:run`: 141 passed across four files.
- `npm run typecheck`: passed.
- `npm run build`: passed; tracked dist rebuilt, with `index-CGbW_SCq.js` replacing `index-Cu4tK6bz.js`.
- Repository working directory `F:\Website\EatThis`: `git diff --check` passed; `spectra validate scroll-to-search-outcome` valid.

## Requirement-to-test evidence

All names below are in `src/EatThis.Web/src/app.spec.ts`.

| Requirement | Test evidence |
| --- | --- |
| New content exists before one scroll; progress, busy clicks, edits and rerenders do not scroll; one GPS/API per action; no focus calls | `scrolls once to each rendered restaurant and never during progress or pending edits` |
| Five terminal failures and repeated identical failures each scroll to current status | `scrolls to rendered %s feedback for every accepted action` (permission-denied, unsupported-geolocation, no-results, provider-error, rate-limited) |
| Completion-time preference, smooth/instant and missing matchMedia | `uses the completion motion preference %s` (true, false, undefined) |
| Unmount, unavailable method and detached target safely skip | `safely skips scrolling for %s` (unmount, missing-method, detached-target) |
| Superseded completion cannot scroll; current completion still scrolls once | `does not scroll an older completion after a newer action has started` |
| Retain natural measured space without stale restaurant actions or restoring scroll offsets | `retains measured feedback space across retries without stale results or scroll restoration` |

## Review

The handler wraps only the explicit recommendation action, waits for recommendation completion and Vue's DOM update, and uses local element references. It does not attach scrolling to lifecycle updates or resize observation. Existing height retention, review counts, recovery controls, request snapshots and recommendation history remain in place. DESIGN.md and the UI direction now document one completion scroll in place of the old all-phase viewport preservation requirement.

Sharp-edge review: no new API calls, external parameters, configuration, secrets or backend changes. Busy actions are rejected before starting another request; operation identity and unmount guards prevent stale completion scrolling. No programmatic focus movement is introduced. Missing browser capabilities fail without retrying the API.

## Pending real-browser acceptance

Browser discovery returned `[]`; no connected browser is available. Task 3.2 remains unchecked. DOM tests prove call targets, rendered text, counts and parameters, **not actual viewport visibility, animation, native permission UI, touch, keyboard focus retention or screen-reader announcements**.

Still required: real mobile and desktop checks at 320 CSS px, typical phone width and desktop; first result, retries, short/long cards and all five failure states; smooth and reduced motion; user scrolling while pending and after completion; condition edits; keyboard and live-region behavior. No deployment or commit was performed.
