# EatThis mobile-web UI direction

## Brief status

- Surface: mobile-browser Operate experience.
- Job: help a hungry, undecided person make one nearby food decision from one explicit action.
- Audience scene: a person holding a phone in a real city, checking how far they are willing to walk before asking for a recommendation.
- Product truth: GPS is requested on demand, Google Places is called through the API, one place is selected, and navigation continues externally.
- Direction seed: `1d08afa1`.
- Current direction: pocket city food guide, with the information discipline of a transit wayfinding sheet and the warmth of a practical daily tool.

## Direction: pocket city food guide

EatThis is a calm, precise city utility. It starts with the user's walkable distance, makes the current choice legible at a glance, and ends with one destination sheet. There is no directory to scan and no map surface to interpret: the interface moves from distance to decision to destination in one vertical read.

## First viewport

- The header contains only the small EatThis wordmark; the city-guide descriptor, decorative separator and 今日附近 are omitted.
- `今天，吃什麼？` is the first strong content object, without the introductory location-use paragraph. The category suggestion and half-star instructional sentence are also omitted; fieldset legends and accessible option names remain.
- The distance control is a labeled range input with a visible current value. It starts at `100 公尺`, uses 100-metre steps, and ends at `3 公里`.
- Eleven native restaurant-category radio choices sit below distance: 不限類型 plus 台式／中式、日式、韓式、火鍋、燒烤、義式、早餐／早午餐、速食、素食、咖啡／甜點. Only one is checked; labels wrap with a checkmark and border for selection.
- Five stars below category select a minimum rating in 0.5-star steps from 0.5 to 5. Left half selects n−0.5 and right half n; the default and explicit reset are 不限評分. Selected portions fill yellow and the numeric threshold remains visible.
- Rating text and reset share a right-aligned intrinsic-width column with equal text-box and button-border widths. The pair wraps together below the intact star row when space is insufficient. Half-star targets stay at least 24px wide and 48px high. Pointer hover and touch selection show no rectangular half-star outline; keyboard focus stays visible.
- The primary green action is labelled `幫我決定`; its secondary line names pending radius, category and rating, even while the primary action is disabled during work.
- An initially empty polite atomic aria-live status region follows the action without visible idle content, mark, borders or reserved space. Non-idle states display operation feedback in that same region. Loading, permission, provider, and no-result states retain plain-language recovery.
- The location-use note below the action, initial 準備好了 status, GPS 只在你要求時使用 and 外部導覽 footer notes are removed. Idle content ends at the primary action with normal shell padding. The distance-control instructions and primary action's current-condition summary remain.
- A selected destination sheet follows the status and contains one name, address, distance, actual rating or 尚無評分, external map action, and the Google Maps attribution at its lower boundary.

## Material and visual system

| Role | Token | Use |
| --- | --- | --- |
| City paper | `#F4F7F5` | Page background and quiet whitespace |
| Functional surface | `#FFFFFF` | Selected destination sheet and logo contrast field |
| Deep ink | `#10211D` | Headings, primary copy, focus context |
| Secondary ink | `#5E6B66` | Supporting copy, labels, recovery guidance |
| Rule | `#CFD7D3` | Section boundaries and detail separators |
| Jade action | `#087A63` | Primary action, range value, selected status |
| Rating selection | `#E9B528` | Filled rating stars only; numeric text remains deep ink |
| Rating remainder | `#C0C9C4` | Unselected star portions |

Typography uses the existing system Traditional Chinese sans stack. Size and weight establish hierarchy; tracking remains restrained and no webfont is added. The page uses fine rules and whitespace rather than repeated cards, gradients, decorative grids, glass effects, or heavy shadows. Control and destination radii stay between 0 and 6px. Long names and addresses wrap naturally rather than being clipped.

## Topology and interaction

The page is one vertical task surface:

1. Read the purpose and the current distance.
2. Adjust the range with a pointer, keyboard, or touch gesture without triggering work.
3. Press the primary action to request one current position and one bounded search.
4. Read the announced state and, on success, one destination sheet.
5. Continue through a normal external HTTPS link to Google Maps.

Category and rating changes and reset have no location or API side effect. The primary action snapshots radius, restaurantCategory and minRating before requesting location; edits during loading affect the next action. An empty result identifies the submitted snapshot while preserving the pending controls. It offers changing a specific category or choosing unrestricted, lowering the rating, increasing distance only below 3000 metres, or explicitly retrying. No condition relaxes itself. A result displays the actual provider rating (for example 4.3) rather than rounding to a half star.

## State contract

All recommendation transitions preserve the user's current reading position: no automatic scroll to the top, result or error, and no programmatic focus movement. A persistent feedback area retains its measured height through retries, shorter results and failures. It preserves space rather than stale restaurant content or navigation links; initial idle remains compact. If the user scrolls while a request is pending, completion does not restore an older scroll offset. Bottom whitespace after a shorter result is an intentional trade-off for stability, resets with the mounted page, and is not persisted.

The result rating row adds an available review total such as （1,234 則評論）, representing ratings with or without written text. Counts use zh-TW grouping, wrap naturally, and are informational only. A valid zero displays （0 則評論）; absent, null or invalid counts omit the parentheses. 尚無評分 can coexist with a known count. The backend requests the total in the existing Nearby Search call and adds no Details request or review-content fetch.

| State | Visible proof | Primary action | Assistive-technology behavior |
| --- | --- | --- | --- |
| idle | Current conditions and one recommendation action; no static lower notes or ready panel | `幫我決定` | Heading, labeled controls, action and an empty polite status region |
| locating | Browser location is being requested | Disabled recommendation action | Polite live region announces location work |
| searching | Nearby search is in progress | No duplicate submit | Polite live region announces search progress |
| selected | One place name, address, distance, navigation action, and attribution | `在地圖中開啟` | Result remains a single semantic destination sheet |
| permission-denied | Permission recovery guidance | `再試一次` through the primary action | Error is announced and no API request is sent |
| unsupported-geolocation | Browser capability explanation | Browser upgrade/retry guidance | No silent fallback location is used |
| no-results | Submitted radius, category, rating and bounded recovery instruction | Adjust and explicitly search, or retry at the bound | Empty state is announced with the next action |
| provider-error | Service failure explanation | `再試一次` through the primary action | Upstream details and credentials stay hidden |
| rate-limited | Wait and retry timing | Retry after the supplied wait | Timing is announced in the live region |

## Responsive and accessibility rules

- The layout is mobile-first from 320px upward. Category options and condition summaries wrap without horizontal scrolling; the primary action follows all three condition controls.
- At 320px, long Traditional Chinese and Latin names, addresses, and the navigation action stay inside the destination sheet and wrap at safe boundaries.
- Focus rings use a high-contrast jade outline with a clear offset. Links retain an underline offset and visible destination intent.
- The range input exposes `min=100`, `max=3000`, `step=100`, an explicit label, and a formatted `aria-valuetext`.
- Rating uses eleven native named radio choices (ten thresholds plus unrestricted), descriptive accessible names, checked states and visible focus; arrows and Space use native radio behavior. Each half target is at least 24px wide and 44px high. Five stars stay together and supporting text/reset wrap without per-device hard-coding.
- Status text, headings, normal link semantics, and the Google Maps image alt text carry meaning independently of color.
- `prefers-reduced-motion` disables authored transitions; all useful content remains visible without motion.
- Selection color and scrollbar treatment derive from the same paper, ink, rule, and jade palette.

## Attribution provenance

`src/EatThis.Web/src/assets/google-maps-logo.svg` is the DarkGray non-outlined SVG from Google's official Maps attribution asset package. It is kept at its supplied aspect ratio and displayed at 18 CSS pixels high with clear space inside the destination sheet. The image has the accessible label `Google Maps`; it is not recolored or altered.

## Release boundary

This surface ships together with the API's 100-to-3000-metre validation and server-controlled Google localization. The public request and normalized response remain provider-neutral. No embedded map, extra place list, new provider, translation service, stored preference, photo, written review content, or database change is part of this direction. Review totals are an optional nullable response field; the frontend tolerates older responses without that field.

The rating extension adds optional nullable minRating and nullable rating; filtering occurs before deduplication and random selection, and unrated candidates qualify only when unrestricted. The fixed places.rating request uses Nearby Search Enterprise billing, even for unrestricted searches, and considers only the at most 20 candidates returned by Google. No-results copy means this search found no eligible candidate, not that the whole geographic area has none. Deploy the backend before the frontend; rollback the frontend first. Archive refine-nearby-food-search before add-minimum-rating-filter so its distance/localization baseline remains intact.

Automated contract/DOM checks and browser verification are distinct. The half-star change's 320px, 390px and desktop pointer/touch/keyboard screenshots and assistive-technology checks remain pending when no browser runtime is connected; this is tracked in the change tasks rather than inferred from passing Vitest cases.

## Restaurant-category verification

Google includedTypes follows the server-owned product mapping documented in README.md. Category filtering happens in one Nearby Search request before rating filtering; the maximum remains 20 candidates and places.rating still uses Enterprise billing. Classification coverage is incomplete; no-results describes this search rather than all nearby businesses.

Agent-run Vitest verifies the eleven radios, selection/reset without GPS/API, pending/submitted snapshots during locating/searching, no-results recovery and one-result behavior. Browser connection attempted for this change but no browsers were available. 320px/desktop wrapping, pointer targets, visible focus and native arrows/Space interaction remain pending manual verification; DOM checks are not browser evidence.
