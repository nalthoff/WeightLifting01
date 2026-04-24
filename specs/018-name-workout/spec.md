# Feature Specification: Optional Workout Name

**Feature Branch**: `018-name-workout`  
**Created**: 2026-04-24  
**Status**: Draft  
**Input**: User description: "As a user, I want to optionally name a workout so I can identify it later."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Name an active workout (Priority: P1)

As a user with a workout in progress, I can enter or update an optional workout name so I can recognize the session later.

**Why this priority**: This delivers the core user value directly in the primary in-gym flow.

**Independent Test**: Start a workout, add a name, update the name, and verify the latest value is shown consistently in the active workout view.

**Acceptance Scenarios**:

1. **Given** I have a workout in progress, **When** I enter a workout name, **Then** the workout stores and shows that name.
2. **Given** I have already named a workout in progress, **When** I edit the name, **Then** the updated name replaces the previous value.

---

### User Story 2 - Keep naming optional (Priority: P2)

As a user focused on speed, I can leave workout naming blank and still save and complete my workout successfully.

**Why this priority**: The feature must not slow down or block logging for users who do not want to name sessions.

**Independent Test**: Complete workouts with a non-empty name, a blank value, and whitespace-only input; verify all complete successfully and blank/whitespace is treated as no name.

**Acceptance Scenarios**:

1. **Given** I have a workout in progress, **When** I leave the name blank and continue, **Then** the workout remains valid and can be completed.
2. **Given** I enter only whitespace as the name, **When** I save, **Then** the workout is treated as having no name.

---

### User Story 3 - Preserve clear history labels (Priority: P3)

As a user reviewing workout history, I see a consistent fallback label when a workout has no stored name.

**Why this priority**: History remains readable and predictable even when naming is skipped.

**Independent Test**: Complete an unnamed workout and verify history surfaces the existing fallback label.

**Acceptance Scenarios**:

1. **Given** a completed workout has no stored name, **When** I view history, **Then** the fallback label "Workout" is shown.
2. **Given** a workout is completed, **When** I attempt to rename it afterward, **Then** the system blocks the change.

### Edge Cases

- User clears an existing workout name before completion; the workout remains valid and saves as unnamed.
- User submits whitespace-only input; the system normalizes it to no name.
- User attempts to edit the workout name after completion; the update is rejected with a clear outcome.
- Connectivity interruptions during name update do not create contradictory name states between active workout and history displays.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow users to add an optional workout name while a workout is in progress.
- **FR-002**: The system MUST allow users to edit or clear the workout name while that workout remains in progress.
- **FR-003**: The workout name MUST remain optional, and unnamed workouts MUST still be savable and completable.
- **FR-004**: If the submitted workout name is empty or whitespace-only, the system MUST treat it as no name.
- **FR-005**: The system MUST prevent workout-name edits once a workout is completed.
- **FR-006**: The system MUST preserve the existing fallback history label "Workout" whenever a completed workout has no name.
- **FR-007**: The naming interaction MUST not add a required step that blocks the core mobile workout logging flow.
- **FR-008**: Validation and lifecycle rules for workout naming MUST be applied consistently across all user paths that display workout names.
- **FR-009**: The feature MUST only introduce workout naming and MUST NOT introduce a separate workout type field in this scope.

### Key Entities *(include if feature involves data)*

- **Workout Session**: A user workout record with lifecycle state, an optional name value, and completion state.
- **Workout Name**: Optional free-text value used to identify a workout; may be absent.
- **History Row Summary**: Completed workout list item that shows either stored workout name or fallback "Workout".

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of in-progress workouts can be completed successfully regardless of whether a workout name is provided.
- **SC-002**: 100% of completed workouts without a stored name display the fallback label "Workout" in history.
- **SC-003**: 100% of rename attempts after completion are blocked.
- **SC-004**: In mobile validation, users can continue the primary workout logging path without any required naming step.

## Assumptions

- Users are already in an authenticated workout flow and can access only their own workout sessions.
- Existing name length rules continue to apply, and out-of-range inputs are handled consistently with current validation patterns.
- A user can update workout name multiple times while the workout is in progress; the most recent successful value is authoritative.
- This feature does not change broader workout-history analytics or introduce new categorization concepts.
