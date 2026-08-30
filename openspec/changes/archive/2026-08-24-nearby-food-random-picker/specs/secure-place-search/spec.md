## ADDED Requirements

### Requirement: Expose a bounded nearby-food pick endpoint

The backend SHALL expose POST /api/nearby-food/pick with a JSON request containing latitude, longitude, and radiusMeters. The endpoint SHALL return one normalized selected place or a stable error response.

#### Scenario: Valid request

- **WHEN** the endpoint receives latitude 25.0330, longitude 121.5654, and radiusMeters 3000
- **THEN** the endpoint validates the request, invokes the configured place provider once, and returns one selected normalized place when a candidate exists

#### Scenario: Invalid request

- **WHEN** the endpoint receives a latitude outside -90 to 90, a longitude outside -180 to 180, or a radiusMeters value outside 100 to 5000
- **THEN** the endpoint returns HTTP 400 with a stable client-readable error code and does not invoke the place provider

### Requirement: Keep the provider credential server-side

The backend SHALL load the Google Places API key from server-side configuration or secret storage. The frontend SHALL never receive the key, include it in its bundle, or send it to the backend as a request field.

#### Scenario: Public frontend request

- **WHEN** a browser submits a nearby-food pick request
- **THEN** the browser request contains only the location and bounded radius fields, and the backend adds the provider credential when calling Google

#### Scenario: Public response inspection

- **WHEN** a caller inspects a successful or failed backend response
- **THEN** the response contains no Google API key, secret configuration value, or raw provider credential

### Requirement: Restrict the backend proxy behavior

The backend SHALL construct provider requests from server-controlled place types, field masks, radius limits, and endpoint configuration. The backend SHALL NOT relay arbitrary upstream URLs, arbitrary provider query parameters, or arbitrary field masks supplied by the browser.

#### Scenario: Caller submits unsupported proxy parameters

- **WHEN** a caller includes an unrecognized field, provider URL, or provider-specific query option
- **THEN** the backend ignores or rejects the unsupported input and does not forward it to Google

### Requirement: Query Google Places through an adapter

The initial provider adapter SHALL call Google Places Nearby Search with the validated center and radius, fixed food-related types, a minimal field mask, and the server-side API key. The adapter SHALL map Google results to the normalized candidate contract.

#### Scenario: Google returns food places

- **WHEN** Google returns valid places with names, locations, addresses, and Google Maps URLs
- **THEN** the adapter returns candidates containing name, address, latitude, longitude, distanceMeters, navigationUrl, and provider equal to google

#### Scenario: Google returns an unusable place

- **WHEN** a Google result lacks a required name, coordinate, or navigation URL
- **THEN** the adapter excludes that result from the normalized candidate set

### Requirement: Return stable failure responses

The backend SHALL map validation, empty-result, rate-limit, provider-authentication, provider-quota, and upstream-service failures to stable HTTP statuses and error codes without returning raw provider messages.

#### Scenario: No normalized candidate exists

- **WHEN** the provider response contains no usable food candidate
- **THEN** the endpoint returns HTTP 404 with a no-results error code

#### Scenario: Provider quota or authentication failure

- **WHEN** Google rejects the request because of credentials or quota
- **THEN** the endpoint returns HTTP 502 or 503 with a provider-unavailable error code and excludes the upstream response body

#### Scenario: Backend rate limit is exceeded

- **WHEN** a caller exceeds the backend's configured request rate
- **THEN** the endpoint returns HTTP 429 with retry guidance and does not call Google

### Requirement: Preserve a provider-neutral replacement contract

The backend SHALL define a provider interface that accepts a validated nearby search query and returns normalized place candidates. The Google adapter SHALL be the only implementation in this change, and an OSM/Overpass adapter SHALL be replaceable without changing the endpoint response contract or primary frontend selection flow.

#### Scenario: Provider implementation is replaced

- **WHEN** an OSM/Overpass adapter implements the same nearby search contract
- **THEN** the endpoint continues to return the same normalized fields and the Vue frontend continues to use the same selection and navigation states

### Requirement: Protect provider data and credentials in operation

The backend SHALL avoid logging API keys and raw Google responses, SHALL use restricted environment-specific provider credentials, and SHALL apply a server-side request limit before invoking Google.

#### Scenario: Diagnostic logging is enabled

- **WHEN** the backend records a request, response, or failure for diagnostics
- **THEN** the log contains correlation and stable error information without API keys or raw provider payloads
