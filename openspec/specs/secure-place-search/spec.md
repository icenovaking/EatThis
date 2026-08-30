# Secure Place Search

## Requirements

### Requirement: Expose a bounded nearby-food pick endpoint

The backend SHALL expose POST /api/nearby-food/pick with a JSON request containing latitude, longitude, and radiusMeters. The endpoint SHALL return one normalized selected place or a stable error response.

#### Scenario: Valid request

- **WHEN** the endpoint receives latitude 25.0330, longitude 121.5654, and radiusMeters 3000
- **THEN** the endpoint validates the request, invokes the configured place provider once, and returns one selected normalized place when a candidate exists

#### Scenario: Invalid request

- **WHEN** the endpoint receives a latitude outside -90 to 90, a longitude outside -180 to 180, or a radiusMeters value outside 100 to 5000
- **THEN** the endpoint returns HTTP 400 with a stable client-readable error code and does not invoke the place provider


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
### Requirement: Keep the provider credential server-side

The backend SHALL load the Google Places API key from server-side configuration or secret storage. The frontend SHALL never receive the key, include it in its bundle, or send it to the backend as a request field.

#### Scenario: Public frontend request

- **WHEN** a browser submits a nearby-food pick request
- **THEN** the browser request contains only the location and bounded radius fields, and the backend adds the provider credential when calling Google

#### Scenario: Public response inspection

- **WHEN** a caller inspects a successful or failed backend response
- **THEN** the response contains no Google API key, secret configuration value, or raw provider credential


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

The initial provider adapter SHALL call Google Places Nearby Search with the validated center and radius, fixed food-related types, a minimal field mask, and the server-side API key. The adapter SHALL map Google results to the normalized candidate contract.

#### Scenario: Google returns food places

- **WHEN** Google returns valid places with names, locations, addresses, and Google Maps URLs
- **THEN** the adapter returns candidates containing name, address, latitude, longitude, distanceMeters, navigationUrl, and provider equal to google

#### Scenario: Google returns an unusable place

- **WHEN** a Google result lacks a required name, coordinate, or navigation URL
- **THEN** the adapter excludes that result from the normalized candidate set


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