# Feature Specification: Rename App to RackNote

**Feature Branch**: `[021-rename-app-racknote]`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Rename the app from WeightLifting01 to RackNote as a focused product rename feature."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See RackNote Across Primary App Entry Points (Priority: P1)

As a user, I want the app name to show as RackNote anywhere the product identifies itself in
the core app shell so I immediately recognize I am using the renamed product.

**Why this priority**: The main user value is a consistent visible identity; if primary entry
points still show the old name, the rename fails.

**Independent Test**: Open the app on mobile and desktop-sized viewports, load the shell and home
entry points, and verify all visible app-name references show RackNote.

**Acceptance Scenarios**:

1. **Given** a user opens the app, **When** the app shell and primary navigation render, **Then**
   the product name is shown as RackNote and not WeightLifting01.
2. **Given** a user opens the app in a browser tab, **When** the tab and page metadata are visible,
   **Then** the displayed app name is RackNote.
3. **Given** a user uses both narrow mobile and wider viewport layouts, **When** they view primary
   entry points, **Then** the product name remains RackNote in both experiences.

---

### User Story 2 - See RackNote in User-Facing Product Documentation (Priority: P2)

As a user or contributor, I want user-facing product documentation to refer to RackNote so setup
and usage guidance matches the app identity I see in product surfaces.

**Why this priority**: Supporting documentation is part of the visible product identity and should
not conflict with the in-app rename.

**Independent Test**: Review user-facing docs in scope and verify references to the app name use
RackNote consistently.

**Acceptance Scenarios**:

1. **Given** a user reads setup or usage documentation that names the product, **When** they view
   those sections, **Then** the app name appears as RackNote.
2. **Given** documentation includes product identity text, **When** users compare docs with visible
   app branding, **Then** the names match without mixed old/new naming.

### Edge Cases

- What happens when an infrequently visited but user-facing screen still contains
  `WeightLifting01` after the rename?
- What happens when viewport-specific layouts (mobile menu vs wider toolbar) render different
  branding surfaces?
- What happens when user-facing documentation references the old name while in-app surfaces show
  the new name?
- What happens when user-visible browser metadata differs from in-app branding?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST present the app name as RackNote in user-visible primary app-shell
  branding surfaces.
- **FR-002**: The system MUST present the app name as RackNote in user-visible page and browser
  metadata where the product name is displayed.
- **FR-003**: The system MUST replace user-facing references to WeightLifting01 with RackNote in
  scoped product documentation and help text.
- **FR-004**: The system MUST keep app-name presentation consistent between mobile-sized and larger
  viewport experiences.
- **FR-005**: The system MUST NOT show transition labels such as "formerly WeightLifting01" in
  user-facing surfaces.
- **FR-006**: The feature MUST remain a focused rename and MUST NOT change existing workout logging,
  workout history behavior, or training workflow scope.
- **FR-007**: The system MUST treat any remaining user-facing WeightLifting01 reference in scoped
  surfaces as a defect against feature acceptance.

### Key Entities *(include if feature involves data)*

- **App Identity Label**: The user-facing product name shown in visible app branding and browser
  page identity surfaces.
- **User-Facing Product Documentation**: Human-readable guidance content that names the product for
  setup, usage, or navigation context.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In acceptance verification of scoped user-facing surfaces, 100% of app-name references
  display RackNote with zero visible WeightLifting01 references.
- **SC-002**: Users can complete existing core workout flows without behavioral changes while seeing
  RackNote consistently across primary entry points.
- **SC-003**: In mobile-sized and larger viewport checks of primary app surfaces, app-name
  consistency is maintained in 100% of tested views.
- **SC-004**: In scoped user-facing documentation checks, 100% of product-name mentions use
  RackNote.

## Assumptions

- The rename applies only to user-facing identity text and does not include broader visual rebrand
  work.
- Internal code identifiers, historical spec directory names, and repository naming can remain
  unchanged unless they are user-facing.
- Existing core workout capabilities and behavior are already accepted and should remain unchanged by
  this feature.
- User-facing documentation in scope means guidance currently consumed by users/contributors through
  the repository's primary documentation entry points.
