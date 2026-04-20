# Feature Specification: Create Lift

**Feature Branch**: `[001-create-lift]`  
**Created**: 2026-04-20  
**Status**: Draft  
**Input**: User description: "Build the feature to let a user create a lift so it can be used in workouts."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Lift From Settings Lifts Page (Priority: P1)

As a user, I want to open `Settings -> Lifts` and add a lift so I can use it in workouts.

**Why this priority**: A user cannot select a custom lift in workouts until it exists, so
creating a lift is the smallest valuable slice that unlocks later workout entry.

**Independent Test**: Open `Settings -> Lifts`, enter a valid lift name, save it, and verify
the lift appears in the selectable lift list immediately without a manual reload.

**Acceptance Scenarios**:

1. **Given** the user is in `Settings` and opens the `Lifts` page, **When** they manually
   enter a valid lift name and submit it, **Then** the system creates the lift successfully.
2. **Given** the user is in `Settings`, **When** they want to manage lifts, **Then** they can
   navigate to a dedicated `Lifts` page for that purpose.
3. **Given** a lift has been created successfully, **When** the user opens a workout flow
   that lets them choose a lift, **Then** the new lift appears in the selectable lift list
   immediately.
4. **Given** the user submits an empty or whitespace-only lift name, **When** validation runs,
   **Then** the system blocks creation and tells the user that a name is required.
5. **Given** the user submits a valid lift name but the save does not complete,
   **When** the failure is detected, **Then** the system tells the user the lift was not
   created and does not show it as available for selection.

### Edge Cases

- What happens when the user enters an empty lift name?
- What happens when the user enters only spaces as the lift name?
- How does the system handle a save failure or network interruption during creation?
- How does the system prevent the user from assuming a failed creation succeeded?
- What happens if the lift list is already open in a workout flow when a new lift is created?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a `Settings` section entry for `Lifts`.
- **FR-002**: The system MUST provide a dedicated `Settings -> Lifts` page where a user can
  manage lifts.
- **FR-003**: The system MUST allow the user to manually type the lift name.
- **FR-004**: The system MUST require a lift name before allowing creation to complete.
- **FR-005**: The system MUST reject empty and whitespace-only lift names.
- **FR-006**: The system MUST persist a successfully created lift so it can be used in
  future workout flows.
- **FR-007**: The system MUST make a successfully created lift available in the selectable
  lift list immediately after save without requiring a manual reload or app restart.
- **FR-008**: The system MUST clearly indicate whether lift creation succeeded or failed.
- **FR-009**: The system MUST keep a failed creation attempt from appearing as an available
  lift in workout selection.

### Key Entities *(include if feature involves data)*

- **Lift**: A user-selectable exercise definition with a required name that can later be used
  in workout entry flows.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In usability testing on a mobile-sized viewport, users can complete the lift
  creation flow from Settings in 30 seconds or less.
- **SC-002**: In 100% of successful creation attempts, the newly created lift is available in
  the selectable workout lift list immediately after save.
- **SC-003**: In 100% of empty or whitespace-only submissions, the system prevents creation.
- **SC-004**: In 100% of failed save attempts, the user is clearly informed that the lift was
  not created.

## Assumptions

- This feature applies to the primary app user who is managing their own workout setup.
- The Settings section is the correct place to manage lift creation and related lift setup.
- Lift management is presented on a dedicated `Settings -> Lifts` page rather than mixed into
  a general Settings page.
- Immediate availability means the user does not need to manually refresh, restart, or leave
  and re-enter the app before the new lift can be selected in workouts.
- Lift-name uniqueness is out of scope for this feature unless raised in a later clarification
  or follow-up feature.
