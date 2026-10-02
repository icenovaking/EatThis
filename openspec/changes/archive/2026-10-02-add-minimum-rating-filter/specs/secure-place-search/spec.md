## ADDED Requirements

### Requirement: Validate an optional half-star minimum rating

The backend SHALL accept optional nullable minRating on the nearby-food pick request. Omission or null SHALL mean unrestricted rating. Numeric values SHALL be finite and one of 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5 or 5.0. Invalid values, types or malformed JSON SHALL return HTTP 400 with errorCode invalid_request without invoking the provider. The backend SHALL NOT round an invalid threshold into a valid one.

#### Scenario: Backward compatible unrestricted request

- **WHEN** a valid location and radius request omits minRating or supplies null
- **THEN** the validated query has no minimum-rating restriction

#### Scenario: Threshold boundaries

- **WHEN** a request supplies a minimum rating from the following input cases with otherwise valid fields
- **THEN** the endpoint follows the specified validation result

##### Example: Rating validation cases

| minRating | Expected result |
| --- | --- |
| 0.5 | valid |
| 4.0 | valid |
| 4.5 | valid |
| 5.0 | valid |
| 0 | HTTP 400 invalid_request; no provider call |
| -0.5 | HTTP 400 invalid_request; no provider call |
| 5.5 | HTTP 400 invalid_request; no provider call |
| 4.3 | HTTP 400 invalid_request; no provider call |
| "4.5" | HTTP 400 invalid_request; no provider call |
| true | HTTP 400 invalid_request; no provider call |
| [] | HTTP 400 invalid_request; no provider call |

#### Scenario: Non-finite or malformed numeric input

- **WHEN** the request contains a non-finite numeric minimum rating or malformed JSON
- **THEN** the backend returns HTTP 400 invalid_request without calling the provider or exposing a raw exception

### Requirement: Filter candidates before random selection

NearbyFoodService SHALL apply existing usability validation, inclusive minimum-rating filtering, navigation-URL deduplication and then random selection in that order. With a non-null minimum rating, only candidates with a valid rating greater than or equal to the threshold SHALL qualify. With no minimum rating, missing rating SHALL NOT exclude an otherwise usable candidate. Valid normalized ratings SHALL be finite and between 1 and 5 inclusive; null SHALL represent unavailable rating. Out-of-range or non-finite normalized ratings from a provider SHALL fail candidate usability validation. Zero eligible candidates SHALL produce HTTP 404 no_results without random-source invocation, additional provider requests, or automatic condition relaxation.

#### Scenario: Inclusive threshold comparison

- **WHEN** usable candidates have ratings 3.9, 4.0, 4.3 and null and minRating is 4.0
- **THEN** the random source receives a candidate count of two and selection returns only the 4.0 or 4.3 candidate

#### Scenario: Unrestricted candidates

- **WHEN** usable candidates have ratings 3.9, 4.0, 4.3 and null and minRating is null
- **THEN** all four candidates participate in random selection with the existing deduplication rule

#### Scenario: Rating filtering precedes deduplication

- **WHEN** candidates with the same navigation URL have ratings 3.9 and 4.3 and minRating is 4.0
- **THEN** the qualifying 4.3 candidate remains eligible regardless of its position in the provider response

#### Scenario: No rating-qualified candidate

- **WHEN** the returned candidates have ratings 3.9, 4.0, 4.3 and null and minRating is 4.5
- **THEN** the endpoint returns HTTP 404 no_results, invokes no random source and makes no additional provider request

## MODIFIED Requirements

### Requirement: Expose a bounded nearby-food pick endpoint

The backend SHALL expose POST /api/nearby-food/pick with a JSON request containing latitude, longitude, radiusMeters and optional nullable minRating. The endpoint SHALL return one normalized selected place with nullable rating or a stable error response. Existing requests without minRating SHALL remain valid with unrestricted rating. Existing latitude and longitude limits SHALL remain enforced and radiusMeters SHALL remain bounded from 100 through 3000.

#### Scenario: Valid request

- **WHEN** the endpoint receives latitude 25.0330, longitude 121.5654, radiusMeters 700 and minRating 4.5
- **THEN** the endpoint validates the request, invokes the configured place provider once and returns one selected normalized place with rating at least 4.5 when an eligible candidate exists

#### Scenario: Invalid request

- **WHEN** the endpoint receives latitude outside -90 to 90, longitude outside -180 to 180, radiusMeters outside 100 to 3000, or an invalid minRating
- **THEN** the endpoint returns HTTP 400 with a stable client-readable invalid_request error and does not invoke the place provider

### Requirement: Keep the provider credential server-side

The backend SHALL load the Google Places API key from server-side configuration or secret storage. The frontend SHALL never receive the key, include it in its bundle, or send it to the backend as a request field.

#### Scenario: Public frontend request

- **WHEN** a browser submits a nearby-food pick request
- **THEN** the browser request contains only location, bounded radius and optional minimum-rating fields and the backend adds the provider credential when calling Google

#### Scenario: Public response inspection

- **WHEN** a caller inspects a successful or failed backend response
- **THEN** the response contains no Google API key, secret configuration value, or raw provider credential

### Requirement: Query Google Places through an adapter

The initial provider adapter SHALL call Google Places Nearby Search with the validated center and radius, fixed food-related types, a server-controlled minimal field mask including places.rating, the existing server-controlled zh-TW language preference and the server-side API key. The adapter SHALL map Google results to the normalized candidate contract with nullable rating. Missing, null, non-finite or numeric ratings outside 1 through 5 SHALL normalize to null without excluding an otherwise usable place. The adapter SHALL retain the existing maximum of 20 returned candidates and SHALL NOT send minRating as a Google Nearby Search parameter.

#### Scenario: Google returns food places

- **WHEN** Google returns valid places with names, locations, addresses, Google Maps URLs and rating 4.3
- **THEN** the adapter returns candidates containing name, address, latitude, longitude, distanceMeters, navigationUrl, provider equal to google and rating equal to 4.3

#### Scenario: Google returns an unusable place

- **WHEN** a Google result lacks a required name, coordinate, or navigation URL
- **THEN** the adapter excludes that result from the normalized candidate set

#### Scenario: Google returns no valid rating

- **WHEN** an otherwise usable Google place omits rating, has null rating or has a numeric rating outside 1 through 5
- **THEN** the adapter maps its rating to null and retains the place for unrestricted selection

#### Scenario: Provider request remains bounded and server-controlled

- **WHEN** a valid minimum-rating search invokes Google
- **THEN** the Google request contains the fixed field mask including places.rating, languageCode zh-TW and maxResultCount 20, and contains no minRating parameter or caller-supplied field mask
