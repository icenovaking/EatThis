# restaurant-repeat-prevention Specification

## Purpose

Provide varied nearby restaurant recommendations using browser-local history with a thirty-minute lifetime. Restart selection when the current eligible candidate set is exhausted while avoiding an immediate repeat across rounds when alternatives exist.

## Requirements

### Requirement: Retain bounded browser recommendation history

The frontend SHALL retain navigationUrl and display time for successfully displayed recommendations in same-origin localStorage under eatthis.recommendation-history.v1. It SHALL retain a lastShown entry, validate version 1 data, deduplicate keys case-insensitively, and expire entries when their age reaches 1800000 milliseconds. It SHALL discard invalid or future timestamps. It SHALL retain the latest 1000 entries at most with nonblank keys of at most 2048 characters. A reload or search-condition change SHALL NOT clear valid history. Failed requests SHALL NOT record a restaurant or apply a round reset.

#### Scenario: Time boundary

- **WHEN** A was displayed at 12:00:00 and the next search starts at 12:29:59.999
- **THEN** the frontend includes A in excludedNavigationUrls
- **WHEN** the next search starts at 12:30:00
- **THEN** the frontend omits A from exclusions and expires lastShown if it has that timestamp

#### Scenario: Reload and condition changes

- **WHEN** the user reloads the page or changes radius, rating, category or position within thirty minutes of seeing A
- **THEN** the next request retains A in the valid exclusion history

#### Scenario: Failed recommendation

- **WHEN** geolocation fails or the pick endpoint returns 404, 429 or provider failure
- **THEN** the frontend neither adds a shown restaurant nor resets unexpired history

#### Scenario: Storage fallback

- **WHEN** stored data is malformed or has an unsupported version
- **THEN** the frontend treats it as empty history and continues recommendation
- **WHEN** localStorage access throws
- **THEN** the frontend continues with page-memory history and does not turn a successful result into an error

#### Scenario: Retention bound

- **WHEN** storing a successful result would exceed 1000 distinct unexpired entries
- **THEN** the frontend retains the latest 1000 entries and applies repeat prevention to that retained history


<!-- @trace
source: avoid-repeated-restaurants
updated: 2026-10-03
code:
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Web/dist/assets/index-Ctu6DNAA.js
  - src/EatThis.Web/src/types.ts
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - src/EatThis.Web/src/recommendationHistory.ts
  - src/EatThis.Api/Program.cs
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
tests:
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/recommendationHistory.spec.ts
-->

---
### Requirement: Prefer candidates absent from recent history

The backend SHALL compute C from this request's provider results after existing usability, rating and case-insensitive navigationUrl deduplication. It SHALL compute U by excluding the supplied excludedNavigationUrls from C with the same comparison. When U is nonempty it SHALL select only from U using the existing random source and return an empty resetNavigationUrls array. History SHALL NOT relax search conditions or trigger additional provider requests. Exhaustion SHALL refer only to C, not all restaurants in the geographic area.

#### Scenario: Unseen candidate exists

- **WHEN** C contains A, B and C and exclusions contain A and B
- **THEN** the backend selects C with no history reset and exactly one provider search

#### Scenario: Different conditions reveal a new candidate

- **WHEN** history contains A, B and D and the current eligible set contains A, B and E
- **THEN** the backend selects E and retains exclusions for the other restaurants

#### Scenario: No eligible candidates

- **WHEN** C is empty
- **THEN** the endpoint returns HTTP 404 no_results without invoking the random source or resetting history


<!-- @trace
source: avoid-repeated-restaurants
updated: 2026-10-03
code:
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Web/dist/assets/index-Ctu6DNAA.js
  - src/EatThis.Web/src/types.ts
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - src/EatThis.Web/src/recommendationHistory.ts
  - src/EatThis.Api/Program.cs
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
tests:
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/recommendationHistory.spec.ts
-->

---
### Requirement: Restart exhausted candidates without consecutive round-boundary repeats

When C is nonempty and U is empty, the backend SHALL start a new round using C. If C has at least two candidates, it SHALL exclude lastNavigationUrl from the round's first draw. If that key is absent, it SHALL use all of C. A singleton C SHALL remain selectable. The response SHALL return all keys in C as resetNavigationUrls. On successfully displaying the response the frontend SHALL remove only those keys from history, then add the selected key with the display time and update lastShown. A round reset SHALL NOT remove history outside C.

#### Scenario: Complete a round

- **WHEN** A, C and B have appeared in that order within thirty minutes and the next eligible set is A, B and C
- **THEN** the next result is A or C and resetNavigationUrls contains A, B and C

##### Example: Subsequent round

- **GIVEN** the new round first selects C and no entry expires
- **WHEN** the next two searches return the same eligible set
- **THEN** A and B each appear once in either order, and C does not appear again until the following round

#### Scenario: One available restaurant

- **WHEN** only A qualifies and A is both excluded and the last result
- **THEN** A is returned successfully and its history is reset and recorded at its new display time

#### Scenario: Reset is scoped to current candidates

- **WHEN** history contains A, B and D and the exhausted current set is A and B
- **THEN** resetNavigationUrls contains only A and B and the frontend preserves D


<!-- @trace
source: avoid-repeated-restaurants
updated: 2026-10-03
code:
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Web/dist/assets/index-Ctu6DNAA.js
  - src/EatThis.Web/src/types.ts
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - src/EatThis.Web/src/recommendationHistory.ts
  - src/EatThis.Api/Program.cs
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
tests:
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/recommendationHistory.spec.ts
-->

---
### Requirement: Extend the public contract compatibly and validate history

The endpoint SHALL accept optional nullable excludedNavigationUrls and lastNavigationUrl. Omitted or null exclusions SHALL mean an empty array, and an omitted or null last key SHALL mean no previous result. Exclusions SHALL contain at most 1000 nonblank strings of at most 2048 characters each; a supplied last key SHALL obey the same string limits. Invalid types, blank keys or exceeded limits SHALL return HTTP 400 invalid_request without provider invocation. Duplicate keys SHALL compare case-insensitively. History strings SHALL only be used for comparison and SHALL NOT be fetched or forwarded to the provider. Success SHALL retain existing top-level place fields and add resetNavigationUrls. The frontend SHALL interpret an omitted resetNavigationUrls as an empty array for compatibility with an older backend.

#### Scenario: Legacy caller

- **WHEN** a valid request omits both history fields
- **THEN** the backend selects from all eligible candidates and returns existing place fields plus an empty resetNavigationUrls array

#### Scenario: Reject invalid history

- **WHEN** exclusions contain 1001 items, a null element, a blank key, a 2049-character key, or a non-string element, or lastNavigationUrl is blank or not a string
- **THEN** the endpoint returns HTTP 400 invalid_request without calling the provider

#### Scenario: Duplicate and unrelated keys

- **WHEN** a valid request contains duplicate keys differing only in ASCII case and keys not present in C
- **THEN** duplicates behave as one exclusion and unrelated keys do not change C or provider request parameters

#### Scenario: Old response compatibility

- **WHEN** the frontend receives a valid selected place without resetNavigationUrls
- **THEN** it displays the restaurant and records it without resetting other history

<!-- @trace
source: avoid-repeated-restaurants
updated: 2026-10-03
code:
  - src/EatThis.Api/Application/NearbyFoodService.cs
  - src/EatThis.Api/Domain/NearbySearchQuery.cs
  - src/EatThis.Web/dist/index.html
  - src/EatThis.Api/Contracts/NearbyFoodContracts.cs
  - src/EatThis.Web/dist/assets/index-Ctu6DNAA.js
  - src/EatThis.Web/src/types.ts
  - tests/EatThis.Api.Tests/RandomSelectionTests.cs
  - src/EatThis.Web/dist/assets/index-CMq0UGrf.js
  - src/EatThis.Web/src/recommendationHistory.ts
  - src/EatThis.Api/Program.cs
  - src/EatThis.Web/src/composables/useNearbyFood.ts
  - README.md
  - tests/EatThis.Api.Tests/NearbyFoodEndpointTests.cs
  - tests/EatThis.Api.Tests/ProviderNeutralContractTests.cs
tests:
  - src/EatThis.Web/src/api/nearbyFoodApi.spec.ts
  - src/EatThis.Web/src/app.spec.ts
  - src/EatThis.Web/src/composables/useNearbyFood.spec.ts
  - src/EatThis.Web/src/recommendationHistory.spec.ts
-->