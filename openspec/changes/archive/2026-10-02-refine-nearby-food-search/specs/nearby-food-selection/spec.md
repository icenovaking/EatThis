## ADDED Requirements

### Requirement: Present a mobile-first daily-use search surface

The mobile web frontend SHALL present the radius control, primary recommendation action, operation state, and selected place in a single task-oriented reading order. The replacement visual system SHALL keep controls and status text visually distinct without relying on repeated rounded-card containers or decorative measurement indicators.

#### Scenario: Search surface at a phone viewport

- **WHEN** the page is rendered at a width of 390 CSS pixels
- **THEN** the current radius, radius control, and primary recommendation action are visible in the initial task area without horizontal scrolling

#### Scenario: Long result content at the minimum supported width

- **WHEN** a selected place with a long name and address is rendered at a width of 320 CSS pixels
- **THEN** the place name and address wrap within the result region, the navigation action remains usable, and the page has no horizontal overflow

### Requirement: Present provider-localized place text

The frontend SHALL render the normalized place name and address returned by the backend without client-side transliteration or replacement. A non-empty fallback name returned by the provider SHALL remain visible when a Traditional Chinese translation is unavailable.

#### Scenario: Traditional Chinese place data is returned

- **WHEN** the backend returns name equal to 老地方牛肉麵 and address equal to 新北市板橋區文化路一段 1 號
- **THEN** the selected-place view displays those Traditional Chinese strings unchanged

#### Scenario: Provider fallback text is returned

- **WHEN** the backend returns a valid non-empty Latin-script name because no zh-TW translation exists
- **THEN** the selected-place view displays that fallback name instead of hiding the recommendation

### Requirement: Attribute Google Maps place content clearly

When a selected place has provider equal to google, the frontend SHALL display an unmodified official Google Maps attribution asset inside the selected-place container. The attribution SHALL be visible, legible, and exposed to assistive technology as Google Maps; the raw provider value SHALL NOT be rendered as a standalone user-facing label.

#### Scenario: Google place result is displayed

- **WHEN** the frontend renders a selected place whose provider is google
- **THEN** the result container includes the official Google Maps attribution near its lower content boundary and does not display the standalone text GOOGLE

#### Scenario: Attribution is announced accessibly

- **WHEN** assistive technology traverses a Google-backed selected-place result
- **THEN** the attribution exposes the accessible name Google Maps

## MODIFIED Requirements

### Requirement: Enforce the search radius

The nearby-food flow SHALL expose an adjustable radius from 100 through 3000 meters inclusive, SHALL use 100-meter increments, and SHALL initialize the control to 100 meters. Changing the radius SHALL only update the pending search value; it SHALL NOT request browser location or submit a nearby-food request until the user activates the primary recommendation action. The frontend SHALL reject a radius outside the backend contract before submitting it.

#### Scenario: Default search

- **WHEN** the user starts a recommendation without changing the radius
- **THEN** the frontend requests the current browser location once and submits radiusMeters equal to 100

#### Scenario: User adjusts the pending radius

- **WHEN** the user changes the radius from 100 meters to 700 meters without activating the primary recommendation action
- **THEN** the frontend displays 700 公尺 as the pending radius and does not request browser location or submit a nearby-food request

#### Scenario: User searches with an adjusted radius

- **WHEN** the pending radius is 700 meters and the user activates the primary recommendation action
- **THEN** the frontend requests the current browser location once and submits radiusMeters equal to 700

#### Scenario: Radius reaches the maximum

- **WHEN** the user moves the radius control to its maximum value and starts a recommendation
- **THEN** the frontend submits radiusMeters equal to 3000 and does not offer a 5000-meter expansion action

#### Scenario: Radius is outside the contract

- **WHEN** the frontend receives or constructs a radius below 100 meters or above 3000 meters
- **THEN** the frontend does not submit the request and renders a bounded-input error naming the valid 100-meter to 3-kilometer range

##### Example: Display formatting and submitted values

| radiusMeters | Displayed value | Submitted value |
| ---: | --- | ---: |
| 100 | 100 公尺 | 100 |
| 900 | 900 公尺 | 900 |
| 1000 | 1 公里 | 1000 |
| 2300 | 2.3 公里 | 2300 |
| 3000 | 3 公里 | 3000 |

### Requirement: Select exactly one nearby food place

The application SHALL present exactly one selected place after a successful recommendation request. The selection SHALL be made from the normalized candidates returned by the backend, after invalid candidates have been removed. A no-results response SHALL preserve the user's selected radius and SHALL NOT expand the radius or submit another search automatically.

#### Scenario: Multiple candidates are returned

- **GIVEN** the backend returns candidates named A, B, and C
- **WHEN** the recommendation request succeeds
- **THEN** the frontend renders exactly one of A, B, or C and does not render a multi-result directory as the primary result

##### Example: Candidate selection

- **GIVEN** candidates A, B, and C are returned by the provider
- **WHEN** the backend selects a candidate
- **THEN** the response contains exactly one candidate and the frontend displays that candidate

#### Scenario: No candidates exist below the maximum radius

- **WHEN** the backend reports no normalized candidate for a selected radius below 3000 meters
- **THEN** the frontend renders a no-results state that names the searched radius and instructs the user to adjust the radius before explicitly searching again

#### Scenario: No candidates exist at the maximum radius

- **WHEN** the backend reports no normalized candidate for radiusMeters equal to 3000
- **THEN** the frontend renders a no-results state that keeps the 3000-meter selection and offers an explicit retry without expanding beyond 3000 meters
