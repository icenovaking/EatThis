## MODIFIED Requirements

### Requirement: Present concise search guidance while retaining action feedback

The frontend SHALL omit the visible copy 附近的城市飲食指南, 今日附近, 先決定你願意走多遠，EatThis 只在你按下按鈕後取一次位置，替你選一間。, 選一種想吃的類型，或交給我們決定, and 點星星左半選半星，右半選整星. It SHALL preserve the EatThis wordmark, main heading, distance guidance, control labels and accessible option names. References to removed descriptive elements SHALL be removed or updated to existing elements. The frontend SHALL retain the primary action's pending-condition summary. It SHALL remove the static note 按下後才會使用目前位置；結果會交給 Google Maps 開啟路線。 and both footer notes GPS 只在你要求時使用 and 外部導覽 in every state. While idle, it SHALL omit the 準備好了 status content and its visible panel decorations without reserving their layout space. It SHALL preserve an initially empty polite atomic status live region and populate it with operation feedback when state becomes non-idle. Operation and recovery messages, result content, external navigation and Google Maps attribution SHALL retain their existing state-dependent behavior.

#### Scenario: Initial page presents the retained guidance

- **WHEN** the frontend initializes with unrestricted category and rating
- **THEN** the five previously removed strings, static location-use note, 準備好了 status and both footer notes are absent, the status live region is empty without a visible panel, and the primary action displays 目前條件 with 100 公尺, 不限類型 and 不限評分

#### Scenario: Removing explanatory text preserves accessible controls

- **WHEN** assistive technology traverses the category and minimum-rating groups
- **THEN** each group and option retains its accessible name and checked state, and no aria-describedby reference points to a deleted description

#### Scenario: Search feedback remains available

- **WHEN** an explicit recommendation starts, succeeds or fails
- **THEN** the existing locating, searching, selected, permission-denied, unsupported-geolocation, no-results, provider-error and rate-limited feedback is rendered for its corresponding state in the status live region, including existing recovery actions and result details, while removed static notes remain absent

##### Example: Non-idle feedback

| State | Observable result |
| --- | --- |
| locating | Current-position progress and a disabled primary action |
| searching | Nearby-search progress and a disabled primary action |
| selected | Submitted-condition summary, one restaurant result and external navigation; Google Maps attribution for a Google result |
| permission-denied | Permission failure and instructions to allow location |
| unsupported-geolocation | Unsupported-location explanation and browser recovery guidance |
| no-results | Submitted conditions and explicit recovery without automatic retry |
| provider-error | Provider failure and retry guidance |
| rate-limited | Rate-limit message and retry guidance using available retry timing |

#### Scenario: Pending edits preserve the quiet idle surface

- **WHEN** the user changes the idle controls to 700 meters, 日式 and 4.5 stars without activating the primary action
- **THEN** the primary action shows those pending conditions, the status region remains empty, the removed notes remain absent, and neither geolocation nor the nearby-food API is called
