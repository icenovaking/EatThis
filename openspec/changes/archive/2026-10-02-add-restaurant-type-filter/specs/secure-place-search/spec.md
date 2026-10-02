## ADDED Requirements

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

## MODIFIED Requirements

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
