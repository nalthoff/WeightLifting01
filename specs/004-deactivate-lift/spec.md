# Feature Specification: Deactivate Lift

**Feature Branch**: `[004-deactivate-lift]`  
**Created**: 2026-04-20  
**Status**: Draft  
**Input**: User description: "Add a feature that lets a user deactivate an existing lift from `Settings -> Lifts` so their lift list stays clean without permanently removing the lift record."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Deactivate an Existing Lift (Priority: P1)

As a user, I want to deactivate a lift I no longer use so it stops appearing in my normal lift
lists without being permanently removed.

**Why this priority**: Deactivation is the core value of the feature because it keeps the active
exercise list clean while preserving the lift record for future use.

**Independent Test**: Open `Settings -> Lifts`, choose an active lift, confirm deactivation, and
verify the lift no longer appears in default active-only lift lists.

**Acceptance Scenarios**:

1. **Given** the user is viewing active lifts in `Settings -> Lifts`, **When** they choose a lift
   and confirm deactivation, **Then** the system marks that lift as inactive and removes it from
   the default active-only list.
2. **Given** the user starts the deactivate flow, **When** they cancel at the confirmation step,
   **Then** the lift remains active and visible in the default list.
3. **Given** the user confirms deactivation, **When** the change does not save successfully,
   **Then** the system tells the user the action failed and keeps the lift visible as active.

---

### User Story 2 - View Inactive Lifts in Management (Priority: P2)

As a user, I want to change the lift-management filter to include inactive lifts so I can review
which lifts are hidden from normal selection flows.

**Why this priority**: After deactivation exists, users need a reliable way to inspect inactive
lifts without cluttering the default active list used during routine workout setup.

**Independent Test**: Deactivate a lift, switch the `Settings -> Lifts` filter from active-only to
include inactive lifts, and verify the inactive lift becomes visible while the default view remains
active-only.

**Acceptance Scenarios**:

1. **Given** at least one lift is inactive, **When** the user views `Settings -> Lifts` with the
   default filter, **Then** only active lifts are shown.
2. **Given** inactive lifts exist, **When** the user changes the filter to include inactive lifts,
   **Then** the list shows both active and inactive lifts.
3. **Given** the user later opens another lift-selection flow after a lift was deactivated,
   **When** that flow reads the default active lift list, **Then** the inactive lift is not shown.

### Edge Cases

- What happens when the user opens the deactivate flow and dismisses or cancels confirmation?
- What happens when the deactivate request fails because of connectivity or server problems?
- What happens when a lift-selection view reads active lifts after another screen has just
  deactivated a lift?
- What happens when no inactive lifts exist and the user enables the filter to include inactive
  lifts?
- What happens when product wording could imply permanent deletion instead of reversible
  deactivation?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow the user to choose an existing active lift from
  `Settings -> Lifts` and start a deactivate flow.
- **FR-002**: The system MUST require explicit user confirmation before a selected lift is
  deactivated.
- **FR-003**: The system MUST mark a deactivated lift as inactive rather than permanently removing
  its record.
- **FR-004**: The system MUST remove a successfully deactivated lift from default active-only lift
  lists going forward.
- **FR-005**: The system MUST keep inactive lifts out of normal lift-selection experiences that use
  the default active-only lift list.
- **FR-006**: The `Settings -> Lifts` experience MUST provide a filter that lets the user view
  active-only lifts or include inactive lifts.
- **FR-007**: The default filter state in `Settings -> Lifts` MUST show active lifts only.
- **FR-008**: The system MUST show inactive lifts in `Settings -> Lifts` when the user chooses the
  filter state that includes inactive lifts.
- **FR-009**: The system MUST clearly indicate whether a deactivate attempt succeeded, was cancelled,
  or failed.
- **FR-010**: The system MUST NOT leave a lift appearing inactive in the user-visible source of
  truth unless the deactivate change was saved successfully.
- **FR-011**: The system MUST use terminology that communicates reversible deactivation rather than
  permanent deletion throughout this feature.
- **FR-012**: The feature MUST preserve the ability for lifts to be reactivated in the future
  without requiring re-creation of the lift record.
- **FR-013**: The feature MUST NOT introduce workout-history, sets, reps, or other workout logging
  structures as part of deactivation.

### Key Entities *(include if feature involves data)*

- **Lift**: A durable exercise definition with a stable identity, a user-facing name, and an
  active or inactive availability state.
- **Lift Availability State**: The current visibility status of a lift that determines whether it
  appears in default lift-selection flows or only in management views that include inactive lifts.
- **Deactivate Lift Attempt**: A user-confirmed request to change a lift from active to inactive,
  which either succeeds and updates future active-only reads or fails and leaves the lift active.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a mobile-sized viewport, users can deactivate an existing lift from
  `Settings -> Lifts` with confirmation in 30 seconds or less.
- **SC-002**: In 100% of successful deactivate attempts, the affected lift is excluded from default
  active-only lift lists and later default selection flows.
- **SC-003**: In 100% of cancelled or failed deactivate attempts, the lift remains visible as active
  in the default active-only view.
- **SC-004**: Users can reliably switch between active-only and include-inactive views in
  `Settings -> Lifts`, with inactive lifts appearing only when the broader filter is selected.

## Assumptions

- `Settings -> Lifts` remains the primary place where users manage lift availability.
- A lift's inactive state is reversible in the product model even if reactivation is not delivered
  in this feature.
- Default lift-selection experiences consume an active-only lift list and should automatically stop
  showing inactive lifts after a successful deactivate.
- The feature should use user-facing language such as "deactivate" or "inactive" instead of
  "delete" to avoid implying permanent removal.
- Any new or changed business rules around lift availability will be covered by automated tests in
  the same change.
