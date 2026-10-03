## ADDED Requirements

### Requirement: Present concise search guidance while retaining action feedback

The frontend SHALL omit the visible copy 附近的城市飲食指南, 今日附近, 先決定你願意走多遠，EatThis 只在你按下按鈕後取一次位置，替你選一間。, 選一種想吃的類型，或交給我們決定, and 點星星左半選半星，右半選整星. It SHALL preserve the EatThis wordmark, main heading, distance guidance, control labels and accessible option names. References to removed descriptive elements SHALL be removed or updated to existing elements. The frontend SHALL retain the primary action's pending-condition summary, the location-use note below it, the idle status, operation and recovery messages, result content and both footer notes with their existing display conditions.

#### Scenario: Initial page presents the retained guidance

- **WHEN** the frontend initializes with unrestricted category and rating
- **THEN** none of the five removed strings is visible, while the primary action displays 目前條件 with 100 公尺, 不限類型 and 不限評分, the location-use note is visible, the 準備好了 status is visible, and the GPS 只在你要求時使用 and 外部導覽 footer notes remain visible

#### Scenario: Removing explanatory text preserves accessible controls

- **WHEN** assistive technology traverses the category and minimum-rating groups
- **THEN** each group and option retains its accessible name and checked state, and no aria-describedby reference points to a deleted description

#### Scenario: Search feedback remains available

- **WHEN** an explicit recommendation starts, succeeds or fails
- **THEN** the existing locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error and rate-limited feedback is rendered for its corresponding state, including existing recovery actions and result details

### Requirement: Preserve half-star operation with unobstructed pointer feedback

The frontend SHALL retain five stars with ten half-star options and an explicit unrestricted reset. Pointer hover and touch selection SHALL NOT draw a rectangular outline around a star half. Keyboard focus SHALL remain visibly identifiable and native radio navigation SHALL remain operable. The current rating text and unrestricted option SHALL form a shared right-aligned column with equal rendered widths; when space is insufficient the pair SHALL wrap together without clipping controls or causing horizontal page overflow. The rating control SHALL retain half-star targets at least 24 CSS pixels wide and 44 CSS pixels high.

#### Scenario: Touch selects and clears a half-star threshold

- **WHEN** the user taps the left half of the fifth star and then activates 不限評分
- **THEN** the pending value changes to 4.5 with four and a half yellow stars and 4.5 星以上, then to null with five gray stars and 不限評分, without a rectangular touch-hover outline, a location request or an API request

#### Scenario: Keyboard selection retains identifiable focus

- **WHEN** the user navigates from the 4-star option to the 4.5-star option with the keyboard
- **THEN** the focused option is visibly identifiable, its checked state and accessible name reflect 4.5, and no location or API request occurs

#### Scenario: All rating actions remain usable on narrow screens

- **WHEN** the rating control is used at 320 CSS pixels with unrestricted rating or a threshold of 0.5, 4.5 or 5
- **THEN** all ten half-star options and the unrestricted reset remain reachable, their labels remain readable, and the page has no horizontal overflow
