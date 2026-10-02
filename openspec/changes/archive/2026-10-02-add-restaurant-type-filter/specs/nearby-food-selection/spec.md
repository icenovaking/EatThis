## ADDED Requirements

### Requirement: Choose one restaurant category

The frontend SHALL offer a native radio group named 餐廳類型 between the distance and minimum-rating controls. It SHALL provide exactly eleven options in this order: 不限類型, 台式／中式, 日式, 韓式, 火鍋, 燒烤, 義式, 早餐／早午餐, 速食, 素食, 咖啡／甜點. Exactly one option SHALL be selected, initially 不限類型 represented by null. Product category codes SHALL be taiwanese-chinese, japanese, korean, hot-pot, barbecue, italian, breakfast-brunch, fast-food, vegetarian and cafe-dessert respectively. Selection SHALL expose a visible selected marker, border and keyboard focus independently of color; options SHALL wrap to remain operable on narrow viewports. Changing category SHALL NOT request location or submit a search.

#### Scenario: Default category selection

- **WHEN** the frontend initializes
- **THEN** the eleven labeled radio options are available and only 不限類型 is checked

#### Scenario: Change category and reset

- **WHEN** the user selects 日式 and then 不限類型 without activating the primary action
- **THEN** pending restaurantCategory changes to japanese and then null, with one checked radio throughout and zero geolocation or API calls

#### Scenario: Keyboard selection

- **WHEN** a user focuses the category radio group and uses native arrow-key and Space interaction
- **THEN** the focused option can be selected with visible focus and an accessible Chinese label without pointer input

### Requirement: Snapshot category with the submitted search conditions

The frontend SHALL validate pending category membership before requesting location and SHALL reject invalid values without location or API calls. The recommendation action SHALL copy radiusMeters, minRating and restaurantCategory before awaiting location. The resulting request SHALL use that immutable snapshot; editing pending controls while busy SHALL affect only the next explicit recommendation. The primary action SHALL describe pending category, radius and rating; selected and no-results messages SHALL describe submitted category, radius and rating. Duplicate submissions SHALL remain blocked while locating or searching. Category-specific recovery SHALL offer changing category or selecting unrestricted category without automatic relaxation or retry.

#### Scenario: Submit a category-specific search

- **WHEN** the user activates the primary action with japanese, radiusMeters 1000 and minRating 4.0
- **THEN** one location request is followed by one nearby-food request containing those three conditions and the result message names 日式, 1 公里 and 4 星以上

#### Scenario: Edit pending category during location or search

- **WHEN** a japanese recommendation is awaiting location or its API response and the user changes pending category to hot-pot
- **THEN** the in-flight request and result or no-results message still use japanese, the primary action describes 火鍋 as pending, and a later explicit action submits hot-pot

#### Scenario: Invalid pending category

- **WHEN** the frontend receives or constructs an empty, unknown or non-string non-null category value
- **THEN** it shows an invalid-condition error naming restaurant category and invokes neither geolocation nor the nearby-food API

## MODIFIED Requirements

### Requirement: Select exactly one nearby food place

The application SHALL present exactly one selected place after a successful recommendation request. Selection SHALL be made from normalized candidates after invalid candidates have been removed and the requested minimum rating has been applied. A no-results response SHALL preserve the pending controls and SHALL NOT expand the radius, lower the threshold, change the restaurant category or submit another search automatically. Recovery SHALL describe the submitted conditions and refer to candidates found in this search, not assert that every place in the geographic area was evaluated.

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

#### Scenario: Category-specific recommendation has no results

- **WHEN** a submitted japanese search at 1000 meters and minimum rating 4.0 has no eligible candidate
- **THEN** the frontend names 日式, 1 公里 and 4 星以上 from the submitted snapshot, preserves pending controls and offers an explicit change of category or unrestricted category without starting a request
