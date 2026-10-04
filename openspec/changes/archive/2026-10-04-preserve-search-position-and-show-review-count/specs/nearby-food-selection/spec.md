
## ADDED Requirements

### Requirement: Preserve the viewport throughout recommendations

The frontend SHALL preserve the current document scroll position throughout recommendation state changes without automatically navigating to the top, result or error, or programmatically moving focus. It SHALL prevent content removal or replacement from shortening the document enough to clamp scroll position. Initial idle SHALL reserve no result space. Retained layout space SHALL NOT expose a stale restaurant as a current result or navigation action.

#### Scenario: First recommendation completes

- **WHEN** the user starts a recommendation at a scrolled position with no subsequent scroll input and an unchanged viewport
- **THEN** locating, searching and selected DOM updates preserve scrollY within 1 CSS pixel of its pre-update value without a transient jump, and the existing live region announces progress

#### Scenario: Repeated recommendation changes result height

- **WHEN** a displayed restaurant is replaced through another recommendation by a shorter or taller result
- **THEN** clearing the old result, progress and replacement preserve scroll position even near the page bottom and only the new restaurant is actionable on success

#### Scenario: Recommendation fails

- **WHEN** an initial or repeated recommendation ends in permission-denied, unsupported-geolocation, no-results, provider-error or rate-limited
- **THEN** each state preserves scroll position, displays existing recovery feedback and exposes no actionable previous restaurant

#### Scenario: User scrolls while waiting

- **WHEN** the user scrolls from scrollY 600 to 800 while a request is pending
- **THEN** completion preserves the latest position within 1 CSS pixel under an unchanged viewport and does not restore 600 or focus the result

#### Scenario: Initial idle and busy behavior remain intact

- **WHEN** the page initializes and the user then starts a recommendation and activates the action again while busy
- **THEN** idle reserves no result space and the duplicate activation starts no additional location or API request

### Requirement: Return and display restaurant review totals

The Google provider SHALL request places.userRatingCount in the existing Nearby Search call. The backend SHALL propagate it through the selected candidate and success response as nullable userRatingCount without additional upstream calls or changes to eligibility, selection probability or repeat prevention. Valid values SHALL be integers from 0 through 2147483647. Missing, null, negative, fractional, string and out-of-range upstream values SHALL normalize to null without failing an otherwise usable result. The frontend SHALL accept omitted and null fields, suppress invalid counts, and render valid counts beside the rating using zh-TW thousands separators and 則評論. Counts SHALL include reviews with or without text; this feature SHALL NOT fetch review text.

#### Scenario: Display a review count

- **WHEN** the selected restaurant has rating 4.9 and userRatingCount 1234
- **THEN** the API returns 1234 and the rating row displays its star, 4.9 and （1,234 則評論）, wrapping naturally at narrow viewport widths without horizontal overflow

#### Scenario: Normalize count boundaries

- **WHEN** an otherwise usable restaurant supplies a count from the following table
- **THEN** its response and display follow the table without excluding the restaurant

##### Example: Count boundaries

| Upstream value | Response count | Display |
| --- | --- | --- |
| 0 | 0 | （0 則評論） |
| 2147483647 | 2147483647 | （2,147,483,647 則評論） |
| missing or null | null | omitted |
| -1 | null | omitted |
| 1.5 | null | omitted |
| "1234" | null | omitted |
| 2147483648 | null | omitted |

#### Scenario: Compatible old response

- **WHEN** the frontend receives a response without userRatingCount
- **THEN** the existing rating or 尚無評分 remains visible without a count or empty parentheses

#### Scenario: Count without rating

- **WHEN** rating is null and userRatingCount is 12
- **THEN** the row displays 尚無評分 and （12 則評論） without inventing a rating

#### Scenario: Preserve the existing search contract

- **WHEN** an explicit recommendation is processed
- **THEN** the existing server-owned field mask includes places.userRatingCount, no Details request is added, and radius, category, minimum rating and history continue to determine selection independently of count
