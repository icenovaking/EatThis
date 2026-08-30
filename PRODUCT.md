# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

delegated: Vue.js mobile-browser frontend and ASP.NET Core Web API backend, maintained as two projects under EatThis.slnx.

## Users

Inferred from the confirmed brief: a person using a phone browser who is hungry or undecided and wants one nearby restaurant or food-stall suggestion without browsing a directory.

## Product Purpose

EatThis uses the phone's current GPS position to search for nearby food places within a bounded radius, randomly chooses one result, shows the essential place information, and lets the user continue in Google Maps web or app. Success means the user can move from one explicit action to one credible place and an external navigation action with minimal friction.

## Positioning

The product's distinct mechanism is a single bounded, user-initiated GPS search followed by one random choice, rather than an embedded map or a multi-result restaurant directory. The initial data provider is Google Places behind the API; the public selection contract remains provider-neutral for a future OSM/Overpass adapter.

## Operating Context

The primary flow runs in a mobile browser. The browser requests one current geolocation only after the user activates the recommendation action, sends the location and selected radius to the EatThis API, and opens the selected place's external HTTPS navigation URL. The MVP does not continuously track location and does not render an interactive map.

## Capabilities and Constraints

- The default search radius is 3000 meters; an explicit no-result retry can use 5000 meters.
- The backend accepts latitude -90..90, longitude -180..180, and radius 100..5000 meters.
- The backend owns Google Places credentials and provider request construction; the browser never receives the key.
- The response contains one selected place with name, address, coordinates, distance, navigation URL, and provider.
- The flow must expose idle, locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error, and rate-limited states.
- Out of scope for the MVP: interactive map rendering, OSM implementation, persistent place storage, reviews, photos, opening-hours UI, accounts, continuous tracking, and social features.

## Brand Commitments

The product name is EatThis. No logo, palette, typeface, imagery, or additional brand reference has been confirmed; future design work must not invent a commercial claim or imply supplied brand assets that do not exist.

## Evidence on Hand

There is no supplied restaurant directory, review corpus, photo library, testimonial, logo, or other marketing asset in the repository. Google Places results are live provider data and must be treated as runtime data rather than authored product proof. Demonstration values in tests and UI fixtures must be labeled synthetic where shown.

## Product Principles

- Ask for location only at the moment it is useful.
- Reduce decision fatigue to one clear recommendation.
- Keep provider cost and credentials behind a bounded backend boundary.
- Make recovery visible when location, results, or provider access fails.
- Preserve a replaceable place contract so data-source changes do not reshape the core user flow.

## Accessibility & Inclusion

The mobile web flow must remain usable at narrow widths, expose state changes to assistive technology, keep primary controls keyboard reachable, and never use color as the only status signal. Permission, no-result, rate-limit, and provider-error messages must name the problem and the next recovery action in plain language.
