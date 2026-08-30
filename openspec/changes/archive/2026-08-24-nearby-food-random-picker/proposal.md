## Why

EatThis needs a mobile-browser experience that helps a person decide what to eat nearby without embedding an interactive map. The product should use the user's GPS position, retrieve nearby restaurant or food-stall candidates through Google Places API, choose one at random, and open the selected location in Google Maps while keeping the Google API key out of the browser and source repository.

## What Changes

- Add a Vue.js mobile web frontend as a dedicated project under the EatThis solution.
- Add an ASP.NET Core Web API backend as a separate project under the EatThis solution.
- Add a backend endpoint that validates the browser's GPS coordinates and search radius, calls Google Places Nearby Search, normalizes the response, randomly selects one candidate, and returns only the required place information and navigation URL.
- Keep the Google API key in server-side configuration or secret storage; the frontend SHALL never receive it or call Google Places directly.
- Let the frontend request location permission on demand, invoke the backend search flow, display the selected restaurant or food stall, and navigate to the external Google Maps page or app when the address link is selected.
- Keep a provider-neutral place-search contract so a future OSM/Overpass provider can replace Google without changing the core frontend selection flow.
- Use Impeccable to define and validate the mobile web UI, including loading, permission denial, no-result, API failure, and selected-place states.
- Add automated tests for the backend contract, input boundaries, provider mapping, random-selection behavior, and frontend-visible error states.

## Capabilities

### New Capabilities

- nearby-food-selection: Use the browser's current location to find nearby food places within a bounded radius, randomly select one, display its essential information, and provide an external navigation link.
- secure-place-search: Provide a server-side, provider-neutral place-search contract that protects provider credentials, validates requests, normalizes Google Places results, and supports a future OSM provider.

### Modified Capabilities

(none)

## Impact

- Affected specs: nearby-food-selection, secure-place-search
- Affected code:
  - New: src/EatThis.Web/
  - New: src/EatThis.Api/
  - New: tests/EatThis.Web.Tests/
  - New: tests/EatThis.Api.Tests/
  - Modified: EatThis.slnx
  - Modified: .gitignore
  - Modified: openspec/config.yaml
- Affected external systems: Google Places API, Google Maps external navigation URLs, browser Geolocation API.
- UI design workflow: Impeccable product context and mobile-web design guidance SHALL inform the frontend design artifact and implementation tasks.
