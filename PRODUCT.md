# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

delegated: Vue.js mobile-browser frontend and ASP.NET Core Web API backend, maintained as two projects under EatThis.slnx.

## Users

A person using a phone browser who is hungry or undecided and wants one nearby restaurant or food-stall suggestion without browsing a directory.

## Product Purpose

EatThis uses the phone's current GPS position to search for nearby food places within a bounded radius, randomly chooses one result, shows the essential place information, and lets the user continue in Google Maps web or app. Success means the user can move from one explicit action to one credible place and an external navigation action with minimal friction.

## Positioning

EatThis is a single-decision city food tool. It asks for one current position only when the user acts, searches within the user's chosen walkable radius, and returns exactly one normalized place instead of a restaurant directory.

## Operating Context

The primary flow runs in a mobile browser. The browser requests one current geolocation only after the user activates the recommendation action, sends the location and selected radius to the EatThis API, and opens the selected place's external HTTPS navigation URL. The MVP does not continuously track location and does not render an interactive map.

## Capabilities and Constraints

- The pending search radius starts at 100 metres and is adjustable from 100 to 3000 metres in 100-metre increments.
- Five stars provide ten minimum-rating choices from 0.5 to 5 in half-star steps; their left and right halves select fractional and whole thresholds. The default and explicit reset are unrestricted rating (null).
- Restaurant category is a single choice: unrestricted (null), taiwanese-chinese, japanese, korean, hot-pot, barbecue, italian, breakfast-brunch, fast-food, vegetarian or cafe-dessert. Defaults to unrestricted; native labeled radio options wrap between distance and rating.
- Moving radius, category or rating controls has no GPS or API side effect; a primary action snapshots all three values and performs one location request and one search. Pending edits while busy affect the next action; results and recovery name the submitted snapshot.
- The backend accepts latitude -90..90, longitude -180..180, and radius 100..3000 metres. Invalid radii return a stable `invalid_request` response without calling the provider.
- A no-results response keeps pending controls and identifies the submitted radius, category and rating. Recovery offers changing a specific category or selecting unrestricted, lowering the rating or increasing distance below 3000 metres, followed by an explicit action; conditions never relax automatically.
- The API accepts omitted/null restaurantCategory as unrestricted; only the ten exact lowercase product codes are valid. Empty, whitespace-padded, unknown, case-mismatched or non-string values return HTTP 400 invalid_request with zero provider calls. Google types remain server-owned and specific categories replace broad includedTypes in a single request; provider-neutral validated queries carry a nullable domain enum.
- The API accepts omitted/null minRating or numeric half-star values 0.5..5. Invalid numeric values, types and malformed JSON return stable invalid_request without calling the provider. Rating filtering is inclusive, before URL deduplication and random selection; unrated candidates qualify only for unrestricted searches.
- Google Places is requested with a server-controlled `languageCode` of `zh-TW`. Returned Traditional Chinese text is preserved, and a valid non-empty provider fallback remains visible when no translation exists.
- The response contains one selected place with name, address, coordinates, distance, navigation URL, provider and nullable rating. Actual ratings such as 4.3 are displayed without half-star rounding; null or omitted values display 尚無評分. The browser does not expose provider-specific request options.
- Google Nearby Search returns at most 20 candidates per call. Google category labels are incomplete and subcategories are not equally weighted. Rating filtering is not an exhaustive search of every nearby store. The fixed places.rating field uses Nearby Search Enterprise billing, including unrestricted searches.
- Rollout is backend first, frontend second; rollback is frontend first. Restaurant categories deploy backend first and roll back frontend first; no stored preference or database migration is involved. Archive refine-nearby-food-search before add-minimum-rating-filter to preserve its distance and localization requirements.
- Google-backed results include the unmodified official Google Maps attribution asset with the accessible name `Google Maps`; the raw provider value is not a user-facing label.
- The flow exposes idle, locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error, and rate-limited states.
- Out of scope: interactive map rendering, a place directory, OSM implementation, persistent radius preferences, reviews, photos, opening-hours UI, accounts, continuous tracking, and social features.

## Brand Commitments

The product name is EatThis. The shipped interface uses a cool city-paper surface, deep ink typography, a single jade action color, fine rules, and a restrained destination-sheet treatment. Yellow is reserved for selected rating stars; gray indicates unselected portions. Numeric text and checked radio semantics carry meaning independently of color. It makes no marketing claims and treats live Google Places values as runtime data.

## Evidence on Hand

There is no supplied restaurant directory, review corpus, photo library, testimonial, or other marketing asset in the repository. The Google Maps logo in `src/EatThis.Web/src/assets/google-maps-logo.svg` is copied verbatim from Google's official attribution asset package; runtime place names and addresses come from the API.

## Product Principles

- Ask for location only at the moment it is useful.
- Start close, make distance explicit, and let the user decide when to widen the search.
- Reduce decision fatigue to one clear recommendation.
- Keep provider cost and credentials behind a bounded backend boundary.
- Preserve the returned language and a useful fallback rather than hiding a valid place.
- Make recovery visible when location, results, or provider access fails.
- Keep a replaceable place contract so data-source changes do not reshape the core user flow.

## Accessibility & Inclusion

The mobile web flow remains usable from 320 CSS pixels upward, exposes state changes through semantic live regions, keeps the range and primary action keyboard reachable, and never uses color as the only status signal. The distance input has an explicit label, min/max/step values, and a formatted accessible value. Permission, no-result, rate-limit, and provider-error messages name the problem and the next recovery action in plain language.

The rating control is a native named radio group with accessible minimum-rating labels, checked state and visible focus; arrows change selection and Space selects the focused option. Each half-star target is at least 24 CSS pixels wide and 44 high, while reset and text wrap on narrow screens. Browser and assistive-technology verification are tracked separately from automated DOM checks.

The restaurant category control is a native radio group with Chinese accessible labels, an explicit selected SVG marker and border, visible jade focus, and naturally wrapping targets at least 44 CSS pixels high. Category changes have no location or API side effects.
