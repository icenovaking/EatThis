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
### Requirement: Display essential place information

The selected-place view SHALL display the selected place name, address, distance in meters or a localized distance unit, and an external navigation action using the returned navigationUrl.

#### Scenario: Selected place is displayed

- **WHEN** the backend returns a selected place with name, address, distanceMeters, and navigationUrl
- **THEN** the frontend displays those values in the selected-place view

#### Scenario: User opens navigation

- **WHEN** the user activates the selected place's navigation action
- **THEN** the browser navigates to navigationUrl so the device can open a supported Google Maps web page or application


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