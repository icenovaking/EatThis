# EatThis mobile-web UI direction

## Brief status

- Surface: mobile-browser Operate experience.
- Job: help a hungry, undecided person make one nearby food decision from one explicit action.
- Product truth: GPS is requested on demand, Google Places is called through the API, one place is selected, and navigation continues externally.
- Build-path assumption: no user-selected image/comp round was available in this unattended apply flow; this implementation proceeds code-first for the asset-free MVP and does not store that as a project-wide Impeccable default.
- Selected direction assumption: the assigned Impeccable concept seed `rw-timetable-slide-rack` is used for this implementation because its fixed cells and one-dimensional movement make radius, status, and one selected result legible without an embedded map.

## Direction: pocket timetable slide rack

EatThis is a small printed timetable pulled open on a lit goldenrod work surface. Each state is a slide in one continuous rack. The user does not browse a directory: they pull one decision, watch the location/search mark move, and receive one opaque reversed place plate that can be handed to Google Maps.

### First viewport

- A warm goldenrod work surface fills the viewport; the title `今天吃什麼？` is the first readable object, without an eyebrow or decorative kicker.
- A narrow tick rail states the active radius as `3 KM` and names the current step in text: `準備定位`, `正在搜尋`, or `已選出一間`.
- The primary recommendation control is a vermilion reversed plate labelled `幫我決定`; it is the only dominant action in idle.
- Supporting copy says why location is needed and that the search uses one current position, not continuous tracking.
- Loading turns the rail into a moving light that travels along the rack once; the control becomes disabled textually and visually.
- The selected result is an opaque ivory slide with name, address, distance, provider-neutral result wording, and one vermilion `在地圖中開啟` action.

### Material and visual system

- Surface palette: goldenrod work surface, slate-tinted slide edges, carbon ink, bible ivory prose panes, vermilion action/recovery marks, bottle-green confirmation marks.
- Use a single warm surface and one elevation language: either a 1px rule or a soft offset shadow per element, never both. No glass blur, gradient text, hard block shadow, or decorative grid overlay.
- Typography uses a deliberate Taiwanese-Chinese sans/serif system with weight, reversal, rule, and spacing carrying hierarchy. Do not use monospace as a costume; measurements such as `3 KM` may use tabular numerals.
- Labels are short, concrete, and in Traditional Chinese. Demonstration places are explicitly synthetic in tests; runtime place names come from the API.
- Controls use drawn SVG line icons only when an icon adds meaning; text labels remain present and primary.

### Topology and interaction

- The page is one continuous vertical rack at narrow widths. The tick rail is the persistent orientation cue; content slides do not become a multi-column directory.
- Idle → locating → searching is a single authored sequence. The location mark moves once, then settles; reduced-motion users receive the settled state immediately.
- Selected state locks one slide in place. The external navigation link is a normal HTTPS link with visible destination intent; no embedded Google Maps or Leaflet surface is introduced.
- No-result state keeps the empty rack visible and offers one explicit `擴大到 5 公里再試` action.
- Provider error and rate-limit states name the problem, show recovery timing when available, and never show raw upstream text or credentials.

### State contract

| State | Visible proof | Primary action | Assistive-technology behavior |
| --- | --- | --- | --- |
| idle | Location purpose, `3 KM`, one recommendation plate | `幫我決定` | Heading and action are immediately discoverable |
| locating | Location request is in progress | Disabled recommendation control | `aria-live="polite"` announces locating |
| searching | Provider request is in progress | No duplicate submit | Announces search progress and preserves recovery copy |
| selected | One place name, address, distance, navigation action | `在地圖中開啟` | Result heading is programmatically discoverable |
| permission-denied | Plain-language permission recovery | `再試一次` / browser settings guidance | Error is announced and does not submit API request |
| unsupported-geolocation | Browser capability explanation | Manual browser upgrade/retry guidance | No silent fallback location |
| no-results | Empty bounded search explanation | `擴大到 5 公里再試` | Announces no result and one explicit retry |
| provider-error | Service unavailable explanation | `再試一次` | Raw provider error is never announced |
| rate-limited | Wait/retry guidance | Disabled until retry window, then `再試一次` | Announces retry timing when available |

### Responsive and accessibility rules

- Mobile-first layout must remain usable from 320px wide upward; the selected slide never requires horizontal scrolling for essential fields or the navigation action.
- Desktop width may add breathing room around the rack but must not turn the experience into a map or directory.
- Focus rings are high-contrast and visible on keyboard navigation; disabled controls have text and state cues beyond color.
- Status messages use semantic live regions, headings, labels, and normal link semantics. Color is never the only distinction between error, loading, and success.
- Respect `prefers-reduced-motion`; all useful content is visible without entrance animation.

## Challenger decisions and raises

The challengers were weighed on exactly two axes: audience identification and product clarity. The selected direction remains one visual world; challenger material is a discipline donation, not a costume.

| Challenger | Verdict | Kept discipline |
| --- | --- | --- |
| CRT oscilloscope / signal bench | declined: its measurement scene loses both food identification and the one-decision clarity | detented, legible state transitions; the rail must make progress measurable |
| Interactive type specimen | declined: live typography is memorable but does not explain the food decision | one shared scale and deliberate weight changes; avoid a pile of unrelated component styles |
| Tensegrity breathing column | declined: force simulation loses both audience recognition and navigation clarity | balanced/loading/failed states must be visibly distinct and recoverable |
| Alphabet storm | competitive: it could add delight, but transformation would compete with the utilitarian decision | one authored transition only, with readable text preserved throughout |
| Pocket airline timetable slide rack | selected direction | fixed cells, lateral/vertical rack movement, and one reversed confirmation plate |
| Busytown cutaway cross-section | competitive: labelled food places are identifiable, but a continuous scene would resemble the explicitly excluded map | labels must name real content, and narrow screens may pan a rack but never become an embedded map |

## Craft-floor checks before handoff

- Verify body and secondary text contrast at least 4.5:1 and large text at least 3:1.
- Verify real Traditional Chinese copy at 320px, 390px, and desktop widths; fix overflow rather than shrinking the primary action into ambiguity.
- Verify idle, hover/focus, disabled, locating, searching, selected, empty, permission, provider-error, and rate-limited states in the rendered UI.
- Verify browser surfaces: selection color, focus ring, link underline offset, and scrollbar treatment derive from the palette.
- Run the hookless web detector once on changed UI targets after implementation and fix mechanical findings.
- Do not add an embedded map, icon tile dashboard, gradient text, hard block shadow, or color-only status cue.
