# Nearby Food Selection

## Requirements

### Requirement: Request the user's location on demand

The mobile web frontend SHALL request one current browser location only after the user activates the primary recommendation action. The frontend SHALL NOT continuously watch or poll the user's location.

#### Scenario: User starts a recommendation

- **WHEN** the user activates the primary recommendation action and browser geolocation is available
- **THEN** the frontend requests the current position once and submits the coordinates to the nearby-food pick endpoint

#### Scenario: Location permission is denied

- **WHEN** the browser denies the location request
- **THEN** the frontend shows an actionable permission-denied state and SHALL NOT submit a nearby-food request


<!-- @trace
source: nearby-food-random-picker
updated: 2026-08-24
code:
  - src/EatThis.Api/Application/IRandomSource.cs
  - src/EatThis.Api/Properties/launchSettings.json
  - src/EatThis.Web/src/App.vue
  - tests/EatThis.Api.Tests/ConfigurableApiFactory.cs
  - src/EatThis.Web/src/test-setup.ts
  - src/EatThis.Web/.env.example
  - src/EatThis.Web/index.html
  - docs/development.md
  - src/EatThis.Api/EatThis.Api.csproj
  - src/EatThis.Web/src/geolocation.ts
  - src/EatThis.Web/vite.config.ts
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Application/PlaceCandidateRules.cs
  - tests/fixtures/nearby-food-error.json
  - tests/EatThis.Api.Tests/RateLimitTests.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesOptions.cs
  - src/EatThis.Api/Program.cs
  - tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj
  - src/EatThis.Web/src/api/nearbyFoodApi.ts
  - tests/EatThis.Api.Tests/EatThisApiFactory.cs
  - src/EatThis.Web/tsconfig.json
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - scripts/verify-security.ps1
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Api/Infrastructure/PlaceProviderExceptions.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/appsettings.json
  - src/EatThis.Web/src/main.ts
  - docs/development-secrets.md
  - tests/fixtures/nearby-food-success.json
  - tests/EatThis.Api.Tests/SecretConfigurationTests.cs
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Web/tsconfig.app.json
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-Cjltd0xD.js
  - tests/EatThis.Api.Tests/FailureResponseTests.cs
  - src/EatThis.Api/appsettings.example.json
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Api/Application/IPlaceProvider.cs
  - src/EatThis.Web/EatThis.Web.esproj
  - src/EatThis.Web/package.json
  - src/EatThis.Web/tsconfig.node.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - EatThis.slnx
  - src/EatThis.Web/dist/assets/index-8zmJ-341.css
  - docs/ui/eatthis-mobile-web-direction.md
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - tests/EatThis.Api.Tests/ContractFixtureTests.cs
  - tests/EatThis.Api.Tests/CandidateNormalizationTests.cs
  - PRODUCT.md
tests:
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
-->

---
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


<!-- @trace
source: refine-nearby-food-search
updated: 2026-10-02
code:
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - README.md
  - docs/ui/eatthis-mobile-web-direction.md
  - src/EatThis.Api/Program.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Web/dist/assets/index-DXQgLy6m.js
  - DESIGN.md
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - src/EatThis.Web/dist/assets/index-D4IjCKov.css
  - tests/fixtures/nearby-food-success.json
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Web/src/App.vue
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Api/Application/PlaceCandidateRules.cs
  - src/EatThis.Web/dist/assets/index-CXt3g_nB.css
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - tests/EatThis.Api.Tests/CandidateNormalizationTests.cs
  - PRODUCT.md
  - src/EatThis.Web/src/styles.css
  - tests/EatThis.Api.Tests/ContractFixtureTests.cs
  - src/EatThis.Web/dist/assets/index-C7jsj6mC.js
tests:
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
-->

---
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


<!-- @trace
source: add-restaurant-type-filter
updated: 2026-10-02
code:
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Web/dist/index.html
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - src/EatThis.Web/dist/assets/index-CXt3g_nB.css
  - src/EatThis.Api/Domain/RestaurantCategory.cs
  - src/EatThis.Web/src/App.vue
  - docs/ui/eatthis-mobile-web-direction.md
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - PRODUCT.md
  - .impeccable/design.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - DESIGN.md
  - src/EatThis.Web/dist/assets/index-xJA_ogI9.css
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Web/dist/assets/index-C7jsj6mC.js
  - src/EatThis.Web/src/restaurantCategories.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
tests:
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
-->

---
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


<!-- @trace
source: add-minimum-rating-filter
updated: 2026-10-02
code:
  - src/EatThis.Web/dist/assets/index-C7jsj6mC.js
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - tests/EatThis.Api.Tests/ContractFixtureTests.cs
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - src/EatThis.Web/src/App.vue
  - src/EatThis.Web/src/styles.css
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - docs/ui/eatthis-mobile-web-direction.md
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - src/EatThis.Api/Application/PlaceCandidateRules.cs
  - README.md
  - src/EatThis.Web/dist/assets/index-CXt3g_nB.css
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Web/dist/assets/index-DXQgLy6m.js
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/fixtures/nearby-food-success.json
  - tests/EatThis.Api.Tests/CandidateNormalizationTests.cs
  - src/EatThis.Web/dist/assets/index-D4IjCKov.css
  - PRODUCT.md
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - DESIGN.md
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Api/Program.cs
tests:
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
-->

---
### Requirement: Provide accessible operation states

The frontend SHALL expose distinct idle, locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error, and rate-limited states. Status changes SHALL be available to assistive technology, and the primary action SHALL prevent duplicate submissions while locating or searching.

#### Scenario: Search is in progress

- **WHEN** the browser is locating the user or the frontend is waiting for the backend response
- **THEN** the frontend communicates progress, disables duplicate recommendation submissions, and preserves a usable cancel or retry path where supported

#### Scenario: Backend failure is returned

- **WHEN** the backend returns a provider-error or rate-limited error
- **THEN** the frontend renders a user-readable recovery message without exposing API keys, raw upstream responses, or internal exception details


<!-- @trace
source: nearby-food-random-picker
updated: 2026-08-24
code:
  - src/EatThis.Api/Application/IRandomSource.cs
  - src/EatThis.Api/Properties/launchSettings.json
  - src/EatThis.Web/src/App.vue
  - tests/EatThis.Api.Tests/ConfigurableApiFactory.cs
  - src/EatThis.Web/src/test-setup.ts
  - src/EatThis.Web/.env.example
  - src/EatThis.Web/index.html
  - docs/development.md
  - src/EatThis.Api/EatThis.Api.csproj
  - src/EatThis.Web/src/geolocation.ts
  - src/EatThis.Web/vite.config.ts
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Application/PlaceCandidateRules.cs
  - tests/fixtures/nearby-food-error.json
  - tests/EatThis.Api.Tests/RateLimitTests.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesOptions.cs
  - src/EatThis.Api/Program.cs
  - tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj
  - src/EatThis.Web/src/api/nearbyFoodApi.ts
  - tests/EatThis.Api.Tests/EatThisApiFactory.cs
  - src/EatThis.Web/tsconfig.json
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - scripts/verify-security.ps1
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Api/Infrastructure/PlaceProviderExceptions.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/appsettings.json
  - src/EatThis.Web/src/main.ts
  - docs/development-secrets.md
  - tests/fixtures/nearby-food-success.json
  - tests/EatThis.Api.Tests/SecretConfigurationTests.cs
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Web/tsconfig.app.json
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-Cjltd0xD.js
  - tests/EatThis.Api.Tests/FailureResponseTests.cs
  - src/EatThis.Api/appsettings.example.json
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Api/Application/IPlaceProvider.cs
  - src/EatThis.Web/EatThis.Web.esproj
  - src/EatThis.Web/package.json
  - src/EatThis.Web/tsconfig.node.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - EatThis.slnx
  - src/EatThis.Web/dist/assets/index-8zmJ-341.css
  - docs/ui/eatthis-mobile-web-direction.md
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - tests/EatThis.Api.Tests/ContractFixtureTests.cs
  - tests/EatThis.Api.Tests/CandidateNormalizationTests.cs
  - PRODUCT.md
tests:
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
-->

---
### Requirement: Do not require an embedded map

The MVP nearby-food flow SHALL complete its selection and navigation behavior without rendering an interactive Google Maps or Leaflet map inside the frontend.

#### Scenario: User receives a recommendation

- **WHEN** a recommendation succeeds
- **THEN** the user can see the selected place and activate the external navigation link without an embedded map

<!-- @trace
source: nearby-food-random-picker
updated: 2026-08-24
code:
  - src/EatThis.Api/Application/IRandomSource.cs
  - src/EatThis.Api/Properties/launchSettings.json
  - src/EatThis.Web/src/App.vue
  - tests/EatThis.Api.Tests/ConfigurableApiFactory.cs
  - src/EatThis.Web/src/test-setup.ts
  - src/EatThis.Web/.env.example
  - src/EatThis.Web/index.html
  - docs/development.md
  - src/EatThis.Api/EatThis.Api.csproj
  - src/EatThis.Web/src/geolocation.ts
  - src/EatThis.Web/vite.config.ts
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Application/PlaceCandidateRules.cs
  - tests/fixtures/nearby-food-error.json
  - tests/EatThis.Api.Tests/RateLimitTests.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesOptions.cs
  - src/EatThis.Api/Program.cs
  - tests/EatThis.Api.Tests/EatThis.Api.Tests.csproj
  - src/EatThis.Web/src/api/nearbyFoodApi.ts
  - tests/EatThis.Api.Tests/EatThisApiFactory.cs
  - src/EatThis.Web/tsconfig.json
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - scripts/verify-security.ps1
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Api/Infrastructure/PlaceProviderExceptions.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/appsettings.json
  - src/EatThis.Web/src/main.ts
  - docs/development-secrets.md
  - tests/fixtures/nearby-food-success.json
  - tests/EatThis.Api.Tests/SecretConfigurationTests.cs
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Web/tsconfig.app.json
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-Cjltd0xD.js
  - tests/EatThis.Api.Tests/FailureResponseTests.cs
  - src/EatThis.Api/appsettings.example.json
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Api/Application/IPlaceProvider.cs
  - src/EatThis.Web/EatThis.Web.esproj
  - src/EatThis.Web/package.json
  - src/EatThis.Web/tsconfig.node.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - EatThis.slnx
  - src/EatThis.Web/dist/assets/index-8zmJ-341.css
  - docs/ui/eatthis-mobile-web-direction.md
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - tests/EatThis.Api.Tests/ContractFixtureTests.cs
  - tests/EatThis.Api.Tests/CandidateNormalizationTests.cs
  - PRODUCT.md
tests:
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
-->

---
### Requirement: Present a mobile-first daily-use search surface

The mobile web frontend SHALL present the radius control, primary recommendation action, operation state, and selected place in a single task-oriented reading order. The replacement visual system SHALL keep controls and status text visually distinct without relying on repeated rounded-card containers or decorative measurement indicators.

#### Scenario: Search surface at a phone viewport

- **WHEN** the page is rendered at a width of 390 CSS pixels
- **THEN** the current radius, radius control, and primary recommendation action are visible in the initial task area without horizontal scrolling

#### Scenario: Long result content at the minimum supported width

- **WHEN** a selected place with a long name and address is rendered at a width of 320 CSS pixels
- **THEN** the place name and address wrap within the result region, the navigation action remains usable, and the page has no horizontal overflow

<!-- @trace
source: refine-nearby-food-search
updated: 2026-10-02
-->

---
### Requirement: Present provider-localized place text

The frontend SHALL render the normalized place name and address returned by the backend without client-side transliteration or replacement. A non-empty fallback name returned by the provider SHALL remain visible when a Traditional Chinese translation is unavailable.

#### Scenario: Traditional Chinese place data is returned

- **WHEN** the backend returns name equal to 老地方牛肉麵 and address equal to 新北市板橋區文化路一段 1 號
- **THEN** the selected-place view displays those Traditional Chinese strings unchanged

#### Scenario: Provider fallback text is returned

- **WHEN** the backend returns a valid non-empty Latin-script name because no zh-TW translation exists
- **THEN** the selected-place view displays that fallback name instead of hiding the recommendation

<!-- @trace
source: refine-nearby-food-search
updated: 2026-10-02
-->

---
### Requirement: Attribute Google Maps place content clearly

When a selected place has provider equal to google, the frontend SHALL display an unmodified official Google Maps attribution asset inside the selected-place container. The attribution SHALL be visible, legible, and exposed to assistive technology as Google Maps; the raw provider value SHALL NOT be rendered as a standalone user-facing label.

#### Scenario: Google place result is displayed

- **WHEN** the frontend renders a selected place whose provider is google
- **THEN** the result container includes the official Google Maps attribution near its lower content boundary and does not display the standalone text GOOGLE

#### Scenario: Attribution is announced accessibly

- **WHEN** assistive technology traverses a Google-backed selected-place result
- **THEN** the attribution exposes the accessible name Google Maps

<!-- @trace
source: refine-nearby-food-search
updated: 2026-10-02
-->

---
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

<!-- @trace
source: add-minimum-rating-filter
updated: 2026-10-02
-->

---
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

<!-- @trace
source: add-minimum-rating-filter
updated: 2026-10-02
-->

---
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


<!-- @trace
source: add-restaurant-type-filter
updated: 2026-10-02
code:
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Web/dist/index.html
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - src/EatThis.Web/dist/assets/index-CXt3g_nB.css
  - src/EatThis.Api/Domain/RestaurantCategory.cs
  - src/EatThis.Web/src/App.vue
  - docs/ui/eatthis-mobile-web-direction.md
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - PRODUCT.md
  - .impeccable/design.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - DESIGN.md
  - src/EatThis.Web/dist/assets/index-xJA_ogI9.css
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Web/dist/assets/index-C7jsj6mC.js
  - src/EatThis.Web/src/restaurantCategories.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
tests:
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
-->

---
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

<!-- @trace
source: add-restaurant-type-filter
updated: 2026-10-02
code:
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Api/Infrastructure/GooglePlacesProvider.cs
  - src/EatThis.Web/dist/index.html
  - tests/EatThis.Api.Tests/GooglePlacesProviderTests.cs
  - src/EatThis.Web/dist/assets/index-CXt3g_nB.css
  - src/EatThis.Api/Domain/RestaurantCategory.cs
  - src/EatThis.Web/src/App.vue
  - docs/ui/eatthis-mobile-web-direction.md
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - PRODUCT.md
  - .impeccable/design.json
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
  - DESIGN.md
  - src/EatThis.Web/dist/assets/index-xJA_ogI9.css
  - tests/EatThis.Api.Tests/ProxyRestrictionTests.cs
  - src/EatThis.Web/src/types.ts
  - src/EatThis.Web/dist/assets/index-C7jsj6mC.js
  - src/EatThis.Web/src/restaurantCategories.ts
  - src/EatThis.Web/src/styles.css
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
tests:
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
-->