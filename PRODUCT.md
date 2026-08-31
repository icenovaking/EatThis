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
- Moving the radius control has no GPS or API side effect; a primary action snapshots the current value and performs one location request and one search.
- The backend accepts latitude -90..90, longitude -180..180, and radius 100..3000 metres. Invalid radii return a stable `invalid_request` response without calling the provider.
- A no-results response keeps the selected radius and asks the user to adjust it or explicitly try again; the product never expands the radius automatically.
- Google Places is requested with a server-controlled `languageCode` of `zh-TW`. Returned Traditional Chinese text is preserved, and a valid non-empty provider fallback remains visible when no translation exists.
- The response contains one selected place with name, address, coordinates, distance, navigation URL, and provider. The browser does not expose provider-specific request options.
- Google-backed results include the unmodified official Google Maps attribution asset with the accessible name `Google Maps`; the raw provider value is not a user-facing label.
- The flow exposes idle, locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error, and rate-limited states.
- Out of scope: interactive map rendering, a place directory, OSM implementation, persistent radius preferences, reviews, photos, opening-hours UI, accounts, continuous tracking, and social features.

## Brand Commitments

The product name is EatThis. The shipped interface uses a cool city-paper surface, deep ink typography, a single jade action color, fine rules, and a restrained destination-sheet treatment. It makes no marketing claims and treats live Google Places values as runtime data.

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
