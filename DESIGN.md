---
name: EatThis
description: A calm, nearby food decision tool for the city.
colors:
  jade: "#087a63"
  jade-deep: "#075d4d"
  cool-paper: "#f4f7f5"
  white-surface: "#ffffff"
  deep-ink: "#10211d"
  muted-ink: "#5e6b66"
  rule-grey: "#cfd7d3"
  rating-yellow: "#e9b528"
  rating-empty: "#c0c9c4"
typography:
  display:
    fontFamily: "Noto Sans TC, PingFang TC, Microsoft JhengHei, sans-serif"
    fontSize: "clamp(2.45rem, 9vw, 4.6rem)"
    fontWeight: 850
    lineHeight: 1.02
    letterSpacing: "-0.035em"
  title:
    fontFamily: "Noto Sans TC, PingFang TC, Microsoft JhengHei, sans-serif"
    fontSize: "clamp(1.45rem, 5vw, 2.15rem)"
    fontWeight: 800
    lineHeight: 1.2
    letterSpacing: "-0.025em"
  body:
    fontFamily: "Noto Sans TC, PingFang TC, Microsoft JhengHei, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.8
  label:
    fontFamily: "Noto Sans TC, PingFang TC, Microsoft JhengHei, sans-serif"
    fontSize: "0.78rem"
    fontWeight: 750
    lineHeight: 1.45
    letterSpacing: "0.02em"
  scale:
    wordmark: "0.9rem"
    control: "0.88rem"
    range-scale: "0.72rem"
    note: "0.82rem"
    detail: "0.84rem"
  radius-value:
    fontSize: "clamp(1.5rem, 5vw, 2rem)"
  place-title:
    fontSize: "clamp(1.55rem, 6vw, 2.35rem)"
  compact-display:
    fontSize: "clamp(2.35rem, 13vw, 3.8rem)"
rounded:
  control: "4px"
  sheet: "6px"
spacing:
  xs: "5px"
  sm: "12px"
  md: "18px"
  lg: "24px"
  xl: "38px"
components:
  button-primary:
    backgroundColor: "{colors.jade}"
    textColor: "{colors.white-surface}"
    rounded: "{rounded.control}"
    padding: "12px 18px"
    height: "58px"
    width: "min(100%, 280px)"
  button-navigation:
    backgroundColor: "{colors.jade}"
    textColor: "{colors.white-surface}"
    rounded: "{rounded.control}"
    padding: "12px 18px"
    height: "58px"
    width: "100%"
  input-radius:
    backgroundColor: "{colors.cool-paper}"
    textColor: "{colors.deep-ink}"
    rounded: "{rounded.control}"
    height: "28px"
  input-category:
    backgroundColor: "{colors.white-surface}"
    textColor: "{colors.deep-ink}"
    rounded: "{rounded.control}"
    padding: "8px 10px"
    height: "44px"
  card-destination:
    backgroundColor: "{colors.white-surface}"
    textColor: "{colors.deep-ink}"
    rounded: "{rounded.sheet}"
    padding: "22px 20px 0"
---

# Design System: EatThis

## Overview

**Creative North Star: "The Pocket City Guide"**

EatThis is a calm, precise city utility that should feel useful in a real hand, not staged for a campaign. The visual language is a cool sheet of city paper: deep ink carries the reading load, thin rules organize the route, and one jade action color marks the next decision.

The experience moves in one vertical read from distance to decision to destination. Information stays close to its action, live place data is allowed to wrap, and the destination sheet is quiet enough that the external map action remains obvious. The system uses a Traditional Chinese-first system sans stack, restrained geometry, and flat tonal surfaces to keep a daily food choice quick and credible.

**Key Characteristics:**
- Calm city utility with transit-grade information discipline.
- One deliberate jade accent for action and selected state.
- Cool paper, white surface, deep ink, and fine rules instead of decorative chrome.
- One readable path from radius choice to a single destination.
- Traditional Chinese-first typography with safe wrapping for live place data.

## Colors

The palette is cool, paper-like, and restrained: one jade accent provides action feedback while ink, surface, and rules do the structural work.

Rating stars add a functional yellow (#e9b528) for selection and rating gray (#c0c9c4) for the remaining area. These colors are confined to rating semantics; they do not replace jade actions or serve as text colors. Numeric labels remain deep ink, so the rating does not depend on color alone.

### Half-star rating control

Five authored SVG stars sit after restaurant category and before the primary action. Ten native radio inputs cover the left and right halves; a separate unrestricted radio resets the value to null. Each left half selects n−0.5, each right half selects n. Selected portions fill yellow without changing on hover; the numeric label identifies the minimum threshold, such as 4.5 星以上.

The shared star-size token is clamp(48px, 8vw, 56px), making each half target at least 24px wide and 48px high. The five-star row stays together. The numeric label and unrestricted selection share an intrinsic-width grid column at the right edge; the text box and reset border have equal widths. This pair wraps together below the stars when space is insufficient. Pointer hover and touch selection add no rectangular half-star outline; keyboard focus retains the existing jade outline. The group exposes native keyboard radio behavior and accessible names. No inline per-device geometry is used.

The primary action names pending distance, restaurant category and rating, including while busy. Operation messages and recovery name the submitted snapshot. The destination sheet includes one small yellow SVG star alongside the actual rating number or 尚無評分; this result is informational rather than editable.

### Primary

- **Jade Action:** The primary action, current radius value, selected-state mark, and focus context.
- **Deep Jade:** The pressed and hover state for actions; it keeps interaction within the same color family.

### Neutral

- **Cool Paper:** The page background and quiet whitespace around the task.
- **White Surface:** The destination sheet and the clear field behind the attribution asset.
- **Deep Ink:** Headlines, primary copy, and high-importance metadata.
- **Muted Ink:** Supporting copy, labels, recovery guidance, and secondary notes.
- **Rule Grey:** One-pixel section boundaries and detail separators.

**The One Accent Rule.** Jade is the only chromatic action color; reserve it for an action, a selected state, or the current value that needs attention.

## Typography

**Display Font:** Noto Sans TC (with PingFang TC, Microsoft JhengHei, and sans-serif fallbacks)

**Body Font:** Noto Sans TC (with PingFang TC, Microsoft JhengHei, and sans-serif fallbacks)

**Label/Mono Font:** The same system sans stack; no separate mono face is introduced.

**Character:** Heavy, compact headings make the decision legible at a glance. Body and label text stay open and practical, with no webfont dependency so Traditional Chinese rendering remains dependable across mobile browsers.

### Hierarchy

- **Display** (850, `clamp(2.45rem, 9vw, 4.6rem)`, 1.02): The opening decision headline.
- **Title** (800, `clamp(1.45rem, 5vw, 2.15rem)`, 1.2): Result and recovery headings.
- **Body** (400, `1rem`, 1.8): Explanatory copy and readable place information.
- **Label** (750, `0.78rem`, 1.45, slight tracking): Range endpoints, metadata labels, and quiet interface notes.

The supporting scale gives the wordmark, range control, compact notes, live radius value, and destination title their own documented roles without making every utility line compete with the opening decision.

**The Clear Hierarchy Rule.** Use weight, scale, and whitespace to show what happens next; do not make every line compete with the opening decision.

## Layout

The page is a single vertical task surface capped at a readable 760px shell. The shell uses 24px horizontal breathing room on regular widths and 16px at the compact breakpoint; the first viewport keeps the headline, radius value, range, restaurant category, rating, action, and live status in one sequence. Sections are separated by fine rules and deliberate vertical intervals rather than a grid of independent cards.

The header contains only the EatThis wordmark. The city-guide descriptor, 今日附近, introductory location paragraph, category suggestion and half-star instructional sentence are omitted. The distance guidance, primary action's pending-condition summary, location note below it, initial ready status, operation and recovery messages, result content and both footer notes remain visible under their existing conditions. Removed descriptions have no dangling ARIA references; fieldset legends and option labels continue to name the controls.

The layout is mobile-first from 320px upward. At the compact breakpoint (480px and below), the shell tightens its gutters, the headline scales down, and the destination sheet reduces its inset padding. The destination sheet uses a two-column detail rhythm, but its value column always has a minimum width of zero so long names and addresses wrap safely.

**The One Surface Rule.** Keep the recommendation journey in one vertical read: choose a walkable radius, make one request, announce the state, and show one destination.

## Elevation & Depth

This is a flat paper system. There are no ambient box shadows or gradients in the shipped interface. Depth comes from the contrast between the cool-paper page and the white destination surface, one-pixel rules, spacing, and the stronger ink weight of the result heading. Interactive depth is communicated by the jade state shift rather than by lifting controls.

**The Flat Paper Rule.** Surfaces stay flat at rest; use tonal contrast, rules, and state color to establish hierarchy before adding any visual effect.

## Shapes

Controls use gently squared 4px corners, while the destination sheet uses a restrained 6px radius. Borders are thin and quiet, with no pill silhouettes or oversized rounding. The range thumb is the only circular control detail, making the adjustable value obvious without changing the overall geometry. Live names, addresses, and URLs wrap at safe boundaries rather than being clipped.

## Components

The component language is restrained and tactile: one clear action, one quiet reading surface, and feedback that stays in the same visual vocabulary.

### Buttons

- **Shape:** Gently squared corners (4px), with a minimum height of 58px.
- **Primary:** Jade action surface, white text, 12px 18px internal padding, and a width capped at 280px on the main action.
- **Hover / Active:** Deep jade surface and border keep the interaction within the accent family.
- **Focus:** A 3px jade focus outline with a 4px offset makes keyboard focus explicit.
- **Disabled:** Muted ink surface communicates that location or search work is in progress.

### Cards / Containers

- **Corner Style:** Restrained destination-sheet radius (6px).
- **Background:** White surface on cool paper.
- **Shadow Strategy:** No shadow; rules, whitespace, and tonal contrast provide separation.
- **Border:** One-pixel rule-grey outline, with matching internal detail separators.
- **Internal Padding:** 22px 20px on regular widths, reduced to 18px 14px at the compact breakpoint.

### Restaurant Category

Eleven native radio choices present one selected category: 不限類型 plus ten product categories. The group sits between distance and rating and wraps naturally at narrow widths. Text options use control corners, fine rule borders, deep ink and a jade border with an authored SVG checkmark when selected. Focus uses a visible jade outline. Each target is at least 44 CSS pixels high; selection never starts location or API work. Pending edits while busy affect the next action, while result and recovery text retains the submitted three-condition snapshot.

### Inputs / Fields

- **Style:** The radius input is a full-width, 28px-high native range control with a quiet rule-grey track and white thumb outlined in jade.
- **Focus:** The shared 3px jade outline and 4px offset applies to keyboard focus.
- **State:** The formatted value sits beside the label and remains visible while the thumb moves; adjusting the control does not initiate work.

### Navigation

- **Style:** The external map action fills the destination sheet width and uses the same 58px jade action treatment as the primary button.
- **Intent:** Its second line names the web/app handoff, while normal link semantics and a visible focus ring preserve destination clarity.

### State Panel

The live state is a rule-bounded status row with a small mark and plain-language copy. The mark changes from outlined to filled jade for the selected state, while error and recovery messages remain understandable without color.

## Do's and Don'ts

Concrete guardrails for extending the shipped system:

### Do:

- **Do** use the cool-paper, white-surface, deep-ink, muted-ink, rule-grey, and jade roles for their stated functions.
- **Do** keep one primary action visually dominant and pair it with a short secondary detail line.
- **Do** preserve fine rules and whitespace as the main structural devices.
- **Do** let user-provided or provider-returned names and addresses wrap naturally.
- **Do** keep interactive controls keyboard reachable with a visible high-contrast focus treatment.
- **Do** preserve official attribution assets at their supplied aspect ratio and with an accessible name.

### Don't:

- **Don't** introduce gradients, heavy shadows, warm multi-accent palettes, or ornamental background effects.
- **Don't** turn every section into a separate card, badge, pill, or decorative chrome element.
- **Don't** truncate live place names, addresses, or navigation intent to protect a fixed visual shape.
- **Don't** rely on color alone to communicate status, errors, or recovery.
- **Don't** expose raw provider identifiers as user-facing labels.
