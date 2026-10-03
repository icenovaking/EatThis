# Secure Place Search Delta

## MODIFIED Requirements

### Requirement: Keep the provider credential server-side

The backend SHALL load the Google Places API key from server-side configuration or secret storage. The frontend SHALL never receive the key, include it in its bundle, or send it to the backend as a request field.

#### Scenario: Public frontend request

- **WHEN** a browser submits a nearby-food pick request
- **THEN** the browser request contains only location, bounded radius, optional minimum-rating and restaurant-category fields, and optional excludedNavigationUrls and lastNavigationUrl history fields and the backend adds the provider credential when calling Google

#### Scenario: Public response inspection

- **WHEN** a caller inspects a successful or failed backend response
- **THEN** the response contains no Google API key, secret configuration value, or raw provider credential

### Requirement: Filter candidates before random selection

NearbyFoodService SHALL apply existing usability validation, inclusive minimum-rating filtering, navigation-URL deduplication, the restaurant-repeat-prevention selection policy and then random selection in that order. With a non-null minimum rating, only candidates with a valid rating greater than or equal to the threshold SHALL qualify. With no minimum rating, missing rating SHALL NOT exclude an otherwise usable candidate. Valid normalized ratings SHALL be finite and between 1 and 5 inclusive; null SHALL represent unavailable rating. Out-of-range or non-finite normalized ratings from a provider SHALL fail candidate usability validation. Zero eligible candidates SHALL produce HTTP 404 no_results without random-source invocation, additional provider requests, or automatic condition relaxation.

#### Scenario: Inclusive threshold comparison

- **WHEN** usable candidates have ratings 3.9, 4.0, 4.3 and null and minRating is 4.0 with no history supplied
- **THEN** the random source receives a candidate count of two and selection returns only the 4.0 or 4.3 candidate

#### Scenario: Unrestricted candidates

- **WHEN** usable candidates have ratings 3.9, 4.0, 4.3 and null and minRating is null
- **THEN** all four candidates participate in random selection with the existing deduplication rule when no history is supplied

#### Scenario: Rating filtering precedes deduplication

- **WHEN** candidates with the same navigation URL have ratings 3.9 and 4.3 and minRating is 4.0 with no history supplied
- **THEN** the qualifying 4.3 candidate remains eligible regardless of its position in the provider response

#### Scenario: No rating-qualified candidate

- **WHEN** the returned candidates have ratings 3.9, 4.0, 4.3 and null and minRating is 4.5
- **THEN** the endpoint returns HTTP 404 no_results, invokes no random source and makes no additional provider request

### Requirement: Validate an optional restaurant category

The nearby-food endpoint SHALL accept optional nullable restaurantCategory as a provider-neutral string. Omission and null SHALL mean unrestricted category. The only valid non-null codes SHALL be taiwanese-chinese, japanese, korean, hot-pot, barbecue, italian, breakfast-brunch, fast-food, vegetarian and cafe-dessert. Membership SHALL use exact case-sensitive comparison without trimming. NearbySearchQuery SHALL carry a validated domain category rather than raw Google type strings. Invalid values or JSON types SHALL return HTTP 400 with errorCode invalid_request and an error message naming restaurant category without calling the provider. Existing location, radius and minimum-rating validation SHALL remain enforced. Successful responses SHALL preserve existing place fields and SHALL add resetNavigationUrls as specified by restaurant-repeat-prevention.

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
