## ADDED Requirements

### Requirement: Request the user's location on demand

The mobile web frontend SHALL request one current browser location only after the user activates the primary recommendation action. The frontend SHALL NOT continuously watch or poll the user's location.

#### Scenario: User starts a recommendation

- **WHEN** the user activates the primary recommendation action and browser geolocation is available
- **THEN** the frontend requests the current position once and submits the coordinates to the nearby-food pick endpoint

#### Scenario: Location permission is denied

- **WHEN** the browser denies the location request
- **THEN** the frontend shows an actionable permission-denied state and SHALL NOT submit a nearby-food request

### Requirement: Enforce the search radius

The nearby-food flow SHALL use a default radius of 3000 meters and SHALL permit an explicit retry at 5000 meters. The frontend SHALL reject a radius outside the backend contract before submitting it.

#### Scenario: Default search

- **WHEN** the user starts a recommendation without changing the radius
- **THEN** the frontend submits radiusMeters equal to 3000

#### Scenario: User retries after no results

- **WHEN** the initial 3000-meter search returns no normalized candidates and the user activates the expand-range action
- **THEN** the frontend submits one new request with radiusMeters equal to 5000

#### Scenario: Radius exceeds the maximum

- **WHEN** the frontend receives or constructs a radius greater than 5000 meters
- **THEN** the frontend does not submit the request and renders a bounded-input error

### Requirement: Select exactly one nearby food place

The application SHALL present exactly one selected place after a successful recommendation request. The selection SHALL be made from the normalized candidates returned by the backend, after invalid candidates have been removed.

#### Scenario: Multiple candidates are returned

- **GIVEN** the backend returns candidates named A, B, and C
- **WHEN** the recommendation request succeeds
- **THEN** the frontend renders exactly one of A, B, or C and does not render a multi-result directory as the primary result

##### Example: Candidate selection

- **GIVEN** candidates A, B, and C are returned by the provider
- **WHEN** the backend selects a candidate
- **THEN** the response contains exactly one candidate and the frontend displays that candidate

#### Scenario: No candidates are returned

- **WHEN** the backend reports that no normalized candidate exists within the requested radius
- **THEN** the frontend renders a no-results state and offers the explicit 5000-meter retry action

### Requirement: Display essential place information

The selected-place view SHALL display the selected place name, address, distance in meters or a localized distance unit, and an external navigation action using the returned navigationUrl.

#### Scenario: Selected place is displayed

- **WHEN** the backend returns a selected place with name, address, distanceMeters, and navigationUrl
- **THEN** the frontend displays those values in the selected-place view

#### Scenario: User opens navigation

- **WHEN** the user activates the selected place's navigation action
- **THEN** the browser navigates to navigationUrl so the device can open a supported Google Maps web page or application

### Requirement: Provide accessible operation states

The frontend SHALL expose distinct idle, locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error, and rate-limited states. Status changes SHALL be available to assistive technology, and the primary action SHALL prevent duplicate submissions while locating or searching.

#### Scenario: Search is in progress

- **WHEN** the browser is locating the user or the frontend is waiting for the backend response
- **THEN** the frontend communicates progress, disables duplicate recommendation submissions, and preserves a usable cancel or retry path where supported

#### Scenario: Backend failure is returned

- **WHEN** the backend returns a provider-error or rate-limited error
- **THEN** the frontend renders a user-readable recovery message without exposing API keys, raw upstream responses, or internal exception details

### Requirement: Do not require an embedded map

The MVP nearby-food flow SHALL complete its selection and navigation behavior without rendering an interactive Google Maps or Leaflet map inside the frontend.

#### Scenario: User receives a recommendation

- **WHEN** a recommendation succeeds
- **THEN** the user can see the selected place and activate the external navigation link without an embedded map
