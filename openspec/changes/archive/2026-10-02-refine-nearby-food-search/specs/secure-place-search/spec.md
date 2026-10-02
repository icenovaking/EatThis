## ADDED Requirements

### Requirement: Keep provider localization server-controlled

The Google Places adapter SHALL send languageCode equal to zh-TW for every Nearby Search request. The public nearby-food endpoint SHALL NOT accept a provider-specific language option, and the adapter SHALL retain the closest available non-empty place name when Google has no zh-TW translation.

#### Scenario: Google request uses Traditional Chinese preference

- **WHEN** the adapter searches Google Places for a validated nearby-food query
- **THEN** the provider request body contains languageCode equal to zh-TW together with the validated location restriction

#### Scenario: Caller submits a provider language option

- **WHEN** a caller includes a provider-specific language field in the public nearby-food request
- **THEN** the backend does not forward the unsupported field and the Google adapter still sends languageCode equal to zh-TW

#### Scenario: Google returns localized food places

- **WHEN** Google returns valid places with Traditional Chinese names and addresses
- **THEN** the adapter preserves the returned Traditional Chinese name and address in the normalized candidate

#### Scenario: Google has no Traditional Chinese translation

- **WHEN** Google returns the closest available non-empty name for a place that has no zh-TW translation
- **THEN** the adapter retains the place when all other normalized candidate fields are valid and preserves the returned fallback name

## MODIFIED Requirements

### Requirement: Expose a bounded nearby-food pick endpoint

The backend SHALL expose POST /api/nearby-food/pick with a JSON request containing latitude, longitude, and radiusMeters. The backend SHALL accept radiusMeters from 100 through 3000 inclusive and SHALL return one normalized selected place or a stable error response.

#### Scenario: Valid request

- **WHEN** the endpoint receives latitude 25.0330, longitude 121.5654, and radiusMeters 700
- **THEN** the endpoint validates the request, invokes the configured place provider once with radiusMeters equal to 700, and returns one selected normalized place when a candidate exists

#### Scenario: Minimum radius request

- **WHEN** the endpoint receives valid coordinates and radiusMeters equal to 100
- **THEN** the endpoint accepts the request and invokes the configured place provider once

#### Scenario: Maximum radius request

- **WHEN** the endpoint receives valid coordinates and radiusMeters equal to 3000
- **THEN** the endpoint accepts the request and invokes the configured place provider once

#### Scenario: Invalid request

- **WHEN** the endpoint receives a latitude outside -90 to 90, a longitude outside -180 to 180, or a radiusMeters value outside 100 to 3000
- **THEN** the endpoint returns HTTP 400 with a stable client-readable error code and does not invoke the place provider

##### Example: Radius boundary validation

| radiusMeters | Expected result |
| ---: | --- |
| 99 | HTTP 400 invalid_request |
| 100 | accepted |
| 3000 | accepted |
| 3001 | HTTP 400 invalid_request |
| 5000 | HTTP 400 invalid_request |
