# Secure Place Search

## Requirements

### Requirement: Expose a bounded nearby-food pick endpoint

The backend SHALL expose POST /api/nearby-food/pick with a JSON request containing latitude, longitude, radiusMeters and optional nullable minRating. The endpoint SHALL return one normalized selected place with nullable rating or a stable error response. Existing requests without minRating SHALL remain valid with unrestricted rating. Existing latitude and longitude limits SHALL remain enforced and radiusMeters SHALL remain bounded from 100 through 3000.

#### Scenario: Valid request

- **WHEN** the endpoint receives latitude 25.0330, longitude 121.5654, radiusMeters 700 and minRating 4.5
- **THEN** the endpoint validates the request, invokes the configured place provider once and returns one selected normalized place with rating at least 4.5 when an eligible candidate exists

#### Scenario: Invalid request

- **WHEN** the endpoint receives latitude outside -90 to 90, longitude outside -180 to 180, radiusMeters outside 100 to 3000, or an invalid minRating
- **THEN** the endpoint returns HTTP 400 with a stable client-readable invalid_request error and does not invoke the place provider


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
### Requirement: Keep the provider credential server-side

The backend SHALL load the Google Places API key from server-side configuration or secret storage. The frontend SHALL never receive the key, include it in its bundle, or send it to the backend as a request field.

#### Scenario: Public frontend request

- **WHEN** a browser submits a nearby-food pick request
- **THEN** the browser request contains only location, bounded radius and optional minimum-rating fields and the backend adds the provider credential when calling Google

#### Scenario: Public response inspection

- **WHEN** a caller inspects a successful or failed backend response
- **THEN** the response contains no Google API key, secret configuration value, or raw provider credential


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
### Requirement: Restrict the backend proxy behavior

The backend SHALL construct provider requests from server-controlled place types, field masks, radius limits, and endpoint configuration. The backend SHALL NOT relay arbitrary upstream URLs, arbitrary provider query parameters, or arbitrary field masks supplied by the browser.

#### Scenario: Caller submits unsupported proxy parameters

- **WHEN** a caller includes an unrecognized field, provider URL, or provider-specific query option
- **THEN** the backend ignores or rejects the unsupported input and does not forward it to Google


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
### Requirement: Query Google Places through an adapter

The initial provider adapter SHALL call Google Places Nearby Search with the validated center and radius, server-controlled food-related types selected by the validated restaurant category, a server-controlled minimal field mask including places.rating, the existing server-controlled zh-TW language preference and the server-side API key. The adapter SHALL map Google results to the normalized candidate contract with nullable rating. Missing, null, non-finite or numeric ratings outside 1 through 5 SHALL normalize to null without excluding an otherwise usable place. The adapter SHALL retain the existing maximum of 20 returned candidates and SHALL NOT send minRating as a Google Nearby Search parameter.

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

#### Scenario: Category mapping precedes candidate selection

- **WHEN** the adapter receives japanese, radiusMeters 1000 and minRating 4.0
- **THEN** it makes one Nearby Search request using japanese_restaurant, sushi_restaurant and ramen_restaurant as includedTypes and radius 1000, keeps the fixed field mask including places.rating, zh-TW and maximum 20, and sends neither minRating nor a broad restaurant fallback


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
### Requirement: Preserve a provider-neutral replacement contract

The backend SHALL define a provider interface that accepts a validated nearby search query and returns normalized place candidates. The Google adapter SHALL be the only implementation in this change, and an OSM/Overpass adapter SHALL be replaceable without changing the endpoint response contract or primary frontend selection flow.

#### Scenario: Provider implementation is replaced

- **WHEN** an OSM/Overpass adapter implements the same nearby search contract
- **THEN** the endpoint continues to return the same normalized fields and the Vue frontend continues to use the same selection and navigation states


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
### Requirement: Protect provider data and credentials in operation

The backend SHALL avoid logging API keys and raw Google responses, SHALL use restricted environment-specific provider credentials, and SHALL apply a server-side request limit before invoking Google.

#### Scenario: Diagnostic logging is enabled

- **WHEN** the backend records a request, response, or failure for diagnostics
- **THEN** the log contains correlation and stable error information without API keys or raw provider payloads

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

<!-- @trace
source: refine-nearby-food-search
updated: 2026-10-02
-->

---
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

<!-- @trace
source: add-minimum-rating-filter
updated: 2026-10-02
-->

---
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

<!-- @trace
source: add-minimum-rating-filter
updated: 2026-10-02
-->

---
### Requirement: Validate an optional restaurant category

The nearby-food endpoint SHALL accept optional nullable restaurantCategory as a provider-neutral string. Omission and null SHALL mean unrestricted category. The only valid non-null codes SHALL be taiwanese-chinese, japanese, korean, hot-pot, barbecue, italian, breakfast-brunch, fast-food, vegetarian and cafe-dessert. Membership SHALL use exact case-sensitive comparison without trimming. NearbySearchQuery SHALL carry a validated domain category rather than raw Google type strings. Invalid values or JSON types SHALL return HTTP 400 with errorCode invalid_request and an error message naming restaurant category without calling the provider. Existing location, radius and minimum-rating validation SHALL remain enforced. Successful response fields SHALL remain unchanged.

#### Scenario: Backward-compatible unrestricted request

- **WHEN** an otherwise valid request omits restaurantCategory or supplies null
- **THEN** the validated query has unrestricted category and the provider uses the existing six food-related includedTypes

#### Scenario: Valid category is passed to a provider

- **WHEN** an otherwise valid request contains one of the ten allowed category codes
- **THEN** validation succeeds and IPlaceProvider receives that category in the validated query independently of Google-specific strings

#### Scenario: Reject invalid category before provider invocation

- **WHEN** an otherwise valid request contains one of the following invalid inputs
- **THEN** it returns HTTP 400 invalid_request and provider invocation count is zero

##### Example: Invalid category inputs

| restaurantCategory | Expected result |
| --- | --- |
| "" | HTTP 400 invalid_request |
| "all" | HTTP 400 invalid_request |
| "Japanese" | HTTP 400 invalid_request |
| " japanese " | HTTP 400 invalid_request |
| "restaurant" | HTTP 400 invalid_request |
| "pizza_restaurant" | HTTP 400 invalid_request |
| 123 | HTTP 400 invalid_request |
| true | HTTP 400 invalid_request |
| [] | HTTP 400 invalid_request |
| {} | HTTP 400 invalid_request |


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
### Requirement: Map product categories to one bounded provider search

The Google adapter SHALL select includedTypes from the following complete server-controlled mapping. Multiple mapped types SHALL match any listed type within one Nearby Search request; a specific category SHALL replace the unrestricted list rather than append broad restaurant types. The adapter SHALL NOT accept caller-supplied Google includedTypes, includedPrimaryTypes, excludedTypes or field masks. It SHALL preserve languageCode zh-TW, maximum 20 results, the existing location restriction and the fixed field mask including places.rating. One accepted search SHALL invoke Google once without category fan-out, automatic fallback or a second classification search. IPlaceProvider implementations SHALL return normalized candidates satisfying the validated category. Existing minimum-rating filtering, URL deduplication, random selection and HTTP 404 no_results behavior SHALL continue without extra provider calls; the service SHALL NOT infer category from place names.

#### Scenario: Apply the complete category mapping

- **WHEN** the adapter searches with each of the category values below
- **THEN** the request contains exactly the corresponding includedTypes and the same fixed language, field mask and result limit

##### Example: Category mapping

| restaurantCategory | includedTypes |
| --- | --- |
| omitted or null | restaurant, cafe, fast_food_restaurant, food_court, bakery, meal_takeaway |
| taiwanese-chinese | taiwanese_restaurant, chinese_restaurant |
| japanese | japanese_restaurant, sushi_restaurant, ramen_restaurant |
| korean | korean_restaurant, korean_barbecue_restaurant |
| hot-pot | hot_pot_restaurant |
| barbecue | barbecue_restaurant, yakiniku_restaurant |
| italian | italian_restaurant, pizza_restaurant |
| breakfast-brunch | breakfast_restaurant, brunch_restaurant |
| fast-food | fast_food_restaurant, hamburger_restaurant |
| vegetarian | vegetarian_restaurant, vegan_restaurant |
| cafe-dessert | cafe, coffee_shop, dessert_shop, dessert_restaurant |

#### Scenario: Caller attempts to override provider filters

- **WHEN** a public request includes japanese together with arbitrary includedTypes, includedPrimaryTypes, excludedTypes or fieldMask fields
- **THEN** the adapter ignores the unsupported fields and uses only the server-controlled japanese mapping and fixed field mask

#### Scenario: No candidate qualifies after category search

- **WHEN** the single category-filtered provider search returns no usable rating-qualified candidate
- **THEN** the endpoint returns HTTP 404 no_results without random selection or an additional provider request

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