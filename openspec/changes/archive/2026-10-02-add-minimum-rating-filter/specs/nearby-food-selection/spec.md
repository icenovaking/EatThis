## ADDED Requirements

### Requirement: Choose a minimum rating using half-star controls

The frontend SHALL provide five selectable stars below the radius control with ten choices from 0.5 through 5.0 in increments of 0.5. The left half of star n SHALL select n minus 0.5 and its right half SHALL select n. It SHALL initialize to unrestricted rating, provide an explicit 不限評分 option, fill the selected rating yellow including partial stars, leave the remaining stars gray, and display the selected numeric threshold as minimum-rating text. Changing or clearing the rating SHALL NOT request location or call the API. The control SHALL expose accessible option names and checked states and support pointer, touch, keyboard focus, Space selection and directional navigation through half-star values and the unrestricted option. Selection SHALL NOT rely on color alone.

#### Scenario: Select a whole-star threshold

- **WHEN** the user selects the right half of the fourth star
- **THEN** the pending threshold is 4.0, four stars appear filled yellow, the remaining star appears gray, and the text is 4 星以上 without a location or API call

#### Scenario: Select a half-star threshold

- **WHEN** the user selects the left half of the fifth star
- **THEN** the pending threshold is 4.5, four and a half stars appear filled yellow, and the text is 4.5 星以上 without a location or API call

##### Example: Left and right halves

| Interaction | Pending minRating | Filled stars |
| --- | ---: | ---: |
| First star left | 0.5 | 0.5 |
| Fourth star left | 3.5 | 3.5 |
| Fourth star right | 4.0 | 4.0 |
| Fifth star right | 5.0 | 5.0 |

#### Scenario: Clear the threshold

- **WHEN** the user selects 不限評分 after selecting 4.5
- **THEN** the pending threshold becomes null, all five stars become gray, the text is 不限評分, and no location or API call occurs

#### Scenario: Keyboard and assistive operation

- **WHEN** the user focuses the rating group and navigates from 4.0 to the next higher numeric option using the keyboard
- **THEN** 4.5 becomes selected, the focused option is identifiable, and assistive technology can identify its minimum-rating name and selected state

### Requirement: Snapshot distance and rating for each recommendation

The frontend SHALL snapshot the selected radius and optional minimum rating before the first asynchronous operation of the recommendation action. It SHALL validate the rating as null or one of the ten half-star choices before requesting location and SHALL submit the snapshot with the one requested location. Changes to pending controls during locating or searching SHALL affect only a subsequent explicit recommendation. Result and no-results descriptions SHALL identify the submitted conditions rather than later pending changes. Duplicate recommendation actions SHALL remain disabled while busy.

#### Scenario: Submit both selected conditions

- **WHEN** the user selects a radius of 700 meters and a minimum rating of 4.5 and activates the primary action
- **THEN** the frontend requests location once and submits one request with radiusMeters 700 and minRating 4.5

#### Scenario: Change conditions while locating

- **GIVEN** the user submitted radius 700 and minRating 4.5
- **WHEN** the user changes pending values to radius 1000 and minRating 3.5 before location resolves
- **THEN** the in-flight request still submits 700 and 4.5 and its result or no-results description identifies those submitted values

#### Scenario: Reject an invalid pending threshold

- **WHEN** the frontend constructs a non-null minimum rating of 4.3, 0, or 5.5
- **THEN** the frontend presents a bounded-input error and makes no location or API call

## MODIFIED Requirements

### Requirement: Select exactly one nearby food place

The application SHALL present exactly one selected place after a successful recommendation request. Selection SHALL be made from normalized candidates after invalid candidates have been removed and the requested minimum rating has been applied. A no-results response SHALL preserve the pending controls and SHALL NOT expand the radius, lower the threshold or submit another search automatically. Recovery SHALL describe the submitted conditions and refer to candidates found in this search, not assert that every place in the geographic area was evaluated.

#### Scenario: Multiple candidates are returned

- **GIVEN** the backend returns candidates named A, B, and C
- **WHEN** the recommendation request succeeds
- **THEN** the frontend renders exactly one eligible candidate and does not render a multi-result directory as the primary result

##### Example: Candidate selection

- **GIVEN** A has rating 3.9, B has rating 4.0, C has rating 4.3 and D has no rating
- **WHEN** the backend selects a candidate for minRating 4.0
- **THEN** only B or C is selected and the frontend displays that one candidate

#### Scenario: No candidates exist below the maximum radius

- **WHEN** the backend reports no eligible candidate for submitted radius 700 meters and minRating 4.5
- **THEN** the frontend renders a no-results state naming 700 meters and 4.5 stars or higher, preserves pending controls, and offers lowering the minimum rating or increasing the radius before an explicit new search

#### Scenario: No candidates exist at the maximum radius

- **WHEN** the backend reports no eligible candidate for submitted radius 3000 meters and minRating 4.5
- **THEN** the frontend keeps pending controls, names the submitted conditions and offers lowering the threshold or explicitly retrying without exceeding 3000 meters

#### Scenario: Unrestricted search has no results

- **WHEN** a submitted search without a minimum rating returns no results
- **THEN** the frontend identifies unrestricted rating and the submitted radius, offers increasing the radius only below 3000 meters, and provides an explicit retry without automatic changes

### Requirement: Display essential place information

The selected-place view SHALL display the selected place name, address, distance in meters or a localized distance unit, and an external navigation action using the returned navigationUrl. It SHALL also display the actual normalized numeric rating when present, without rounding it to the chosen half-star threshold, or 尚無評分 when rating is missing or null. Result rating SHALL be informational, not a rating-input control.

#### Scenario: Selected place is displayed

- **WHEN** the backend returns a selected place with name, address, distanceMeters, navigationUrl and rating 4.3
- **THEN** the frontend displays those values and identifies the rating as 4.3 rather than rounding it to 4.5

#### Scenario: Rating is unavailable

- **WHEN** the frontend receives a selected place with null or omitted rating
- **THEN** the result displays 尚無評分 and does not invent a zero-star score

#### Scenario: User opens navigation

- **WHEN** the user activates the selected place's navigation action
- **THEN** the browser navigates to navigationUrl so the device can open a supported Google Maps web page or application
