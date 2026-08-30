## Context

EatThis currently contains only an empty solution container and project configuration. The feature is a greenfield mobile-browser web application: the browser obtains the user's current location, the user requests a nearby food recommendation, the backend queries Google Places, and the browser opens the selected place in Google Maps. The product intentionally does not embed an interactive map.

The repository must contain two separately buildable application projects under EatThis:

- A Vue.js frontend project registered in the solution through the Visual Studio JavaScript Project System, using a Vue package manifest and Vite build.
- An ASP.NET Core Web API backend project that owns the Google Places integration and the provider-neutral place-search contract.

The primary product constraint is credential and cost control. The browser SHALL never receive the Google API key, and the application SHALL issue one bounded search request only when the user asks for a recommendation.

## Goals / Non-Goals

**Goals:**

- Deliver one focused mobile-web flow from location permission to a randomly selected nearby food place.
- Keep the Vue frontend and ASP.NET Core API as separate projects under EatThis.slnx.
- Keep Google Places credentials exclusively in backend configuration or secret storage.
- Return a provider-neutral place result so an OSM/Overpass provider can replace Google later.
- Use Impeccable to design and validate the mobile web operation surface and all user-visible states.
- Make validation, provider mapping, random selection, rate limiting, and error behavior testable without live Google requests.

**Non-Goals:**

- Embedding Google Maps, Leaflet, or any interactive map in the MVP.
- Implementing OSM/Overpass in this change; only the provider boundary and normalized result contract are required.
- Persisting a restaurant directory, Google place content, reviews, photos, or opening-hours snapshots.
- Continuous location tracking, user accounts, favorites, social sharing, payments, or restaurant administration.
- Guaranteeing that every restaurant or street vendor within the radius is returned; the selection set is limited to the provider response.

## Decisions

### Decision: Separate Vue JSPS frontend and ASP.NET Core API projects

Create a Vue.js frontend project at src/EatThis.Web/EatThis.Web.esproj with package.json, TypeScript, Vite, and browser-facing UI code. Create an ASP.NET Core Web API project at src/EatThis.Api/EatThis.Api.csproj. Add both project descriptors to EatThis.slnx. The API MAY reference the frontend project for build or publish orchestration only when the project-system integration requires it; the frontend SHALL remain independently buildable with its npm build command.

This follows the repository's empty .slnx structure and keeps browser concerns, API credentials, and provider integration in separate deployable boundaries. A Vue JSPS project is preferred over embedding Vue files inside the API project because the user explicitly requires two projects and future provider changes must not require frontend restructuring.

### Decision: Backend-owned Google Places provider

Expose one application endpoint, POST /api/nearby-food/pick. The API SHALL validate the request, construct a fixed Google Places Nearby Search request, apply a minimal field mask, map the response to the normalized contract, choose one candidate, and return only the selected result.

The Google API key SHALL be read from server-side configuration or secret storage and sent to Google through a request header. The API SHALL NOT accept arbitrary upstream URLs, arbitrary Google field masks, or arbitrary provider types from the browser. This prevents the endpoint from becoming an unrestricted proxy and keeps cost-bearing behavior bounded.

### Decision: Normalized provider-neutral place contract

The backend provider boundary SHALL accept a nearby search query and return normalized place candidates:

- Query: latitude, longitude, and radiusMeters.
- Candidate: name, address, latitude, longitude, distanceMeters, navigationUrl, and provider.
- Optional provider details SHALL NOT be required by the frontend selection flow.

The Google adapter maps the provider's Google Maps URL to navigationUrl. A future OSM adapter can map its own external navigation URL or coordinate-based navigation link without changing the Vue component contract.

### Decision: On-demand geolocation and bounded radius

The Vue frontend SHALL request browser geolocation only after the user activates the primary recommendation action. The default radius SHALL be 3000 meters; the API SHALL reject values above 5000 meters. The frontend SHALL not poll or watch the user's location continuously.

The first request SHALL use the default radius. A no-result response SHALL offer a user-initiated retry at 5000 meters, creating a second explicit search rather than an automatic unbounded expansion.

### Decision: Random selection occurs after provider normalization

The API SHALL select one candidate from the normalized provider response after invalid coordinates, missing names, and unusable navigation URLs are removed. The initial Google implementation SHALL select uniformly from the candidates returned by one Nearby Search response. The design SHALL not claim mathematical uniformity over every physical business in the radius because the provider controls result coverage and ranking.

The API SHALL return a no-result response when the normalized candidate set is empty. It SHALL not silently fall back to another provider in this change.

### Decision: Impeccable owns the mobile-web UI direction

The frontend surface is an Operate experience focused on one decision: help the user choose where to eat with minimal friction. Before visual implementation, use Impeccable product-context and new-work/shape guidance to establish the product record, surface brief, visual system, responsive layout, and craft-floor checks.

The UI SHALL model these observable states:

- idle: location and radius explanation plus one primary recommendation action;
- locating: permission request and location acquisition feedback;
- searching: provider request feedback with duplicate-submit prevention;
- selected: selected place card with name, address, distance, and external navigation action;
- permission-denied or unsupported: actionable location permission/browser guidance;
- no-results: explicit empty state with a 5000-meter retry action;
- provider-error or rate-limited: clear retry guidance without exposing provider credentials or raw upstream errors.

The UI SHALL be usable without color-only status cues, expose status updates to assistive technology, keep primary controls keyboard reachable, and remain usable at narrow mobile widths.

### Decision: Secret handling and request protection

Use local development secrets outside tracked files, production environment or secret-manager configuration, separate development and production Google keys, API restrictions, and server-side application restrictions where the deployment supports them. Add local secret file patterns to .gitignore and commit only a redacted example configuration.

The API SHALL validate latitude and longitude ranges, accept only the bounded radius, apply a server-side rate limit per client identity or request source, and avoid logging API keys or raw provider responses. The frontend request payload SHALL contain location and user-selected radius only.

### Decision: Tests use provider doubles and browser capability mocks

The API test suite SHALL use an injectable provider double and deterministic random selection control. It SHALL verify request validation, no-result handling, provider mapping, navigation URL mapping, upstream failure translation, and the guarantee that the Google key is not part of the public response.

The Vue test suite SHALL mock browser geolocation and the backend HTTP client. It SHALL verify the state transitions for permission denial, loading, selected place, no results, retry at 5000 meters, provider error, and navigation link rendering. No test SHALL call live Google Places or create a real browser location permission prompt.

## Implementation Contract

### Observable behavior

When a user activates the primary action, the Vue application obtains one current browser location and sends a request to POST /api/nearby-food/pick. The API returns either one selected place or a typed error response. The selected card displays the name, address, and distance, and its navigation action opens the returned navigationUrl in the user's browser or Google Maps application.

### Request contract

The request body SHALL be JSON:

    {
      "latitude": 25.0330,
      "longitude": 121.5654,
      "radiusMeters": 3000
    }

latitude SHALL be between -90 and 90. longitude SHALL be between -180 and 180. radiusMeters SHALL be between 100 and 5000; omitted radius SHALL resolve to 3000.

### Success contract

A successful response SHALL be JSON with this shape:

    {
      "name": "Example Food Shop",
      "address": "Taipei City",
      "latitude": 25.0331,
      "longitude": 121.5655,
      "distanceMeters": 420,
      "navigationUrl": "https://www.google.com/maps/...",
      "provider": "google"
    }

The response SHALL contain exactly one selected candidate. The public response SHALL not contain the Google API key, raw upstream payload, reviews, photos, or provider credentials.

### Failure contract

- Invalid coordinates or radius: HTTP 400 with a stable client-readable error code.
- No normalized candidates: HTTP 404 with a no-results error code.
- Backend rate limit: HTTP 429 with retry guidance and no upstream payload.
- Google authentication, quota, or upstream failure: HTTP 502 or 503 with a stable provider-unavailable error code and no raw provider message.
- Browser permission denial, unsupported geolocation, or frontend network failure: rendered as an Impeccable-defined user-facing state without exposing internal details.

### Acceptance criteria

- EatThis.slnx contains separately buildable Vue frontend and ASP.NET Core API projects.
- A browser bundle and frontend network payload contain no Google API key.
- The API rejects out-of-range coordinates and any radius greater than 5000 meters.
- A valid request invokes the injected Google provider once, selects one normalized candidate, and returns one navigationUrl.
- The frontend prevents duplicate submission while locating or searching.
- The frontend renders selected, permission-denied, no-results, retry, and provider-error states with accessible status messaging.
- Tests pass without live Google credentials, live Google requests, or real location prompts.
- Replacing the Google provider with a provider implementing the same normalized contract does not require changing the primary selection component.

### Scope boundaries

In scope: the two-project solution structure, Vue mobile-web UI, browser geolocation request, backend proxy endpoint, Google Places adapter, normalized provider contract, random selection, external navigation link, secret protection, rate limiting boundary, automated tests, and Impeccable design validation.

Out of scope: interactive map rendering, OSM implementation, persistent place storage, Google reviews/photos/opening-hours UI, authentication, continuous tracking, deployment-provider selection, and production billing-plan negotiation.

## Risks / Trade-offs

- [Google result coverage and category mapping] → Use a fixed set of restaurant, cafe, fast-food, and food-related place types, normalize defensively, and expose no-result behavior instead of fabricating data.
- [Google response limits and ranking] → Define randomness over the returned candidate set and keep the radius explicit in the UI.
- [API cost or key abuse] → Issue one request per explicit action, cap radius and provider fields server-side, rate-limit the endpoint, restrict keys, and monitor quota and billing.
- [Browser location denial or inaccurate GPS] → Explain permission use before requesting it, provide actionable retry guidance, and do not silently use a fallback location.
- [Future OSM data shape differs from Google] → Keep provider-specific fields optional and make navigationUrl the only link contract required by the frontend.
- [Vue project integration with the .NET solution adds Node tooling] → Use the Visual Studio JavaScript Project System project descriptor and document npm prerequisites and build orchestration as part of the implementation tasks.
- [External navigation behavior differs by device] → Use a normal HTTPS navigation URL and verify behavior on mobile Safari, mobile Chrome, and desktop browsers.
