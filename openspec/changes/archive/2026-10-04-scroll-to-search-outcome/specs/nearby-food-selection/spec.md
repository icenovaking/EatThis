## REMOVED Requirements

### Requirement: Preserve the viewport throughout recommendations

**Reason**: The user has replaced the always-preserve-position preference with a single automatic scroll to each completed recommendation outcome.
**Migration**: Follow Scroll to the completed recommendation outcome. Preserve the existing pending-search layout stability, idle compactness and removal of stale results; replace the prohibition on completion scrolling.

## ADDED Requirements

### Requirement: Scroll to the completed recommendation outcome

After each accepted explicit recommendation action completes and its outcome DOM is rendered, the frontend SHALL issue exactly one automatic scroll to the selected restaurant article on success or the current status message on failure. The target SHALL be positioned at the start of the viewport subject to the browser's available scroll range, with the restaurant name or failure message visible. The frontend SHALL NOT target the document bottom or retained blank space. It SHALL NOT programmatically move focus. It SHALL preserve the existing live region, pending-search layout stability, compact initial idle and removal of stale restaurants during retries.

The frontend SHALL use smooth scrolling normally and immediate scrolling when reduced motion is requested at completion time. If motion preference detection is unavailable, it SHALL use immediate scrolling. If the target or scrolling method is unavailable, it SHALL preserve readable outcome content without throwing or retrying the search. Completion callbacks for unmounted components or superseded actions SHALL NOT initiate scrolling.

#### Scenario: First successful recommendation

- **WHEN** the user activates the action and a restaurant named 老地方牛肉麵 is rendered after locating and searching
- **THEN** the frontend issues one scroll to that new article after its name is in the DOM, makes no scroll call during locating or searching, and does not move focus

#### Scenario: Repeated recommendation changes result height

- **GIVEN** an existing restaurant article is displayed
- **WHEN** a new explicit recommendation replaces it with a shorter or taller restaurant article
- **THEN** the frontend retains pending-search layout stability, removes the previous actionable restaurant, and scrolls once to the new article when it is rendered instead of scrolling to retained bottom whitespace

#### Scenario: Failed recommendation

- **WHEN** an accepted recommendation completes with one of the following outcomes
- **THEN** the frontend scrolls once to the rendered status message and preserves the associated recovery content

##### Example: Terminal failure targets

| State | Scroll target content |
| --- | --- |
| permission-denied | Location permission failure |
| unsupported-geolocation | Unsupported location capability |
| no-results | Submitted conditions and no eligible restaurant |
| provider-error | Validation or provider failure |
| rate-limited | Rate-limit feedback |

#### Scenario: Same failure on successive actions

- **WHEN** two distinct accepted actions each finish with unsupported-geolocation without entering searching
- **THEN** the frontend issues one status-targeted scroll per action, for two total, without requiring a change of terminal state

#### Scenario: User scrolls while waiting or after completion

- **WHEN** the user scrolls from scrollY 600 to 800 while a recommendation is pending and then its result arrives
- **THEN** completion performs its one outcome-targeted scroll instead of preserving 800, and subsequent user scrolling, condition edits or content resizing do not trigger another scroll

#### Scenario: Idle and duplicate activation

- **WHEN** the page initializes, pending controls change, or the primary action is activated again while locating or searching
- **THEN** those events initiate no automatic scroll, idle reserves no result space, and busy duplicate activation starts no additional geolocation or API request

#### Scenario: Motion preference at completion

- **WHEN** a recommendation completes under one of the following current preference conditions
- **THEN** its one scroll follows the table and does not move focus

##### Example: Motion behavior

| Preference at completion | Behavior |
| --- | --- |
| prefers-reduced-motion is false | smooth |
| prefers-reduced-motion is true | immediate without animation |
| preference detection unavailable | immediate without animation |
| preference changes from false to true while waiting | immediate without animation |

#### Scenario: No active target at completion

- **WHEN** a component is unmounted, an action is superseded, or its target or scrolling method is unavailable before the completion callback runs
- **THEN** that callback issues no scroll, throws no error, and starts no additional request
