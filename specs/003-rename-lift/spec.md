# Feature Specification: Rename Lift

**Feature Branch**: `[003-rename-lift]`  
**Created**: 2026-04-20  
**Status**: Draft  
**Input**: User description: "Add a feature that lets a user rename an existing lift so they can correct mistakes or update exercise names without recreating the lift."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Rename an Existing Lift (Priority: P1)

As a user, I want to open an existing lift from `Settings -> Lifts` and edit its name so I can
correct mistakes or rename exercises without creating a replacement lift.

**Why this priority**: Renaming an existing lift is the smallest valuable slice that keeps the
exercise list accurate while preserving the original lift record.

**Independent Test**: Open `Settings -> Lifts`, choose an existing lift, rename it with a valid
name, save it, and verify the updated name appears on the page without creating a second lift.

**Acceptance Scenarios**:

1. **Given** the user is viewing existing lifts in `Settings -> Lifts`, **When** they open a lift
   for editing, enter a valid new name, and save, **Then** the system updates that lift and shows
   the new name on the page.
2. **Given** the user edits a lift name, **When** they submit an empty or whitespace-only value,
   **Then** the system blocks the rename and tells them a valid name is required.
3. **Given** the user edits a lift name, **When** they submit a name that is effectively unchanged,
   **Then** the system does not create a duplicate or misleadingly present the lift as renamed.
4. **Given** the user attempts to rename a lift, **When** the save does not complete, **Then** the
   system tells the user the rename failed and keeps the previous saved name as the visible source
   of truth.

---

### User Story 2 - Keep Lift Names Consistent Across Usage (Priority: P2)

As a user, I want a renamed lift to show its updated name anywhere lifts are presented so I can
trust I am selecting the right exercise going forward.

**Why this priority**: A rename is only useful if the corrected name remains consistent across lift
selection experiences instead of leaving stale or conflicting names in the product.

**Independent Test**: Rename a lift in `Settings -> Lifts`, then open another lift-selection flow
and verify the updated name is shown for that same lift going forward.

**Acceptance Scenarios**:

1. **Given** a lift is renamed successfully, **When** the user later opens any flow that presents
   available lifts, **Then** that lift is shown by its updated name rather than its previous name.
2. **Given** another existing lift already uses the requested target name, **When** the user tries
   to save the rename, **Then** the system blocks the change and explains that the name is already
   in use.
3. **Given** a lift list has already been loaded elsewhere, **When** the renamed lift is shown in
   later list reads, refreshes, or reopened selection views, **Then** the system uses the updated
   canonical name and does not surface both old and new names for the same lift.

### Edge Cases

- What happens when the user submits an empty or whitespace-only edited name?
- What happens when the requested new name matches another existing lift name?
- What happens when the requested new name differs from another lift only by letter case or
  surrounding whitespace?
- What happens when the user opens the edit flow but makes no effective name change?
- What happens when the rename fails because of connectivity or server problems?
- What happens when another lift-selection view is already open or was previously loaded when the
  rename completes?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow the user to select an existing lift from `Settings -> Lifts`
  and enter a rename flow for that lift.
- **FR-002**: The system MUST show the lift's current name when the user begins editing it.
- **FR-003**: The system MUST allow the user to submit a replacement name for the selected lift.
- **FR-004**: The system MUST apply a successful rename to the existing lift record rather than
  creating a new lift.
- **FR-005**: The system MUST show the updated name on `Settings -> Lifts` after a confirmed save.
- **FR-006**: The system MUST reflect a successfully renamed lift by its updated name anywhere the
  current lift list is read going forward.
- **FR-007**: The system MUST reject empty and whitespace-only renamed values.
- **FR-008**: The system MUST NOT allow a lift to be renamed to a name already used by another
  existing lift.
- **FR-009**: The system MUST evaluate rename conflicts using normalized names so differences in
  casing or surrounding whitespace do not bypass the duplicate-name rule.
- **FR-010**: The system MUST allow the user to keep the current lift name without treating that
  unchanged value as a conflict with itself.
- **FR-011**: The system MUST clearly indicate whether the rename succeeded or failed.
- **FR-012**: The system MUST keep a failed rename from leaving the unsaved name visible as the
  apparent source of truth in lift-management or later lift-selection experiences.
- **FR-013**: The feature MUST NOT create, delete, merge, or deactivate lifts as part of the rename
  flow.

### Key Entities *(include if feature involves data)*

- **Lift**: A durable exercise definition with a stable identity, a user-facing name, and ongoing
  use in lift-selection experiences.
- **Lift Rename Attempt**: A user-requested change from a lift's current saved name to a proposed
  replacement name, which either completes successfully or is rejected while leaving the original
  saved name unchanged.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In manual verification on a mobile-sized viewport, users can complete the rename flow
  for an existing lift in 30 seconds or less.
- **SC-002**: In 100% of successful rename attempts, the updated name appears on `Settings -> Lifts`
  and in later lift-selection views for that same lift without creating a second visible lift
  entry.
- **SC-003**: In 100% of invalid rename attempts caused by blank, whitespace-only, or conflicting
  names, the system prevents the save and preserves the currently saved lift name.
- **SC-004**: In 100% of failed save attempts, the user is clearly informed that the rename did not
  complete and the unsaved name does not remain the apparent source of truth.

## Assumptions

- `Settings -> Lifts` remains the primary place where users manage existing lifts.
- Renaming changes a lift's display name but does not replace the lift's identity or its use in
  existing workout-related references.
- Duplicate-name prevention is part of this rename feature because allowing two existing lifts to
  converge on the same visible name would make later lift selection ambiguous.
- Current and future lift-selection experiences consume lift names from one authoritative source and
  should show the updated name on future reads after a successful rename.
- Manual verification should focus first on mobile-sized viewport behavior because the product is
  optimized for in-gym phone use.
- Any new or changed rename validation rules will ship with automated tests in the same change.
