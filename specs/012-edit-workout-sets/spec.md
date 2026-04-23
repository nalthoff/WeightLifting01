# Feature Specification: Edit workout set entries

**Feature Branch**: `012-edit-workout-sets`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "As a user in an active workout, I want to edit prior set entries so I can correct mistakes during the workout without leaving the workout screen."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Edit a logged set inline (Priority: P1)

As a lifter in an active workout, I can edit a previously logged set row inline and explicitly save the row so I can quickly correct mistakes without interrupting my session.

**Why this priority**: Correcting incorrect reps or weight is a core logging reliability task during a live workout.

**Independent Test**: Open an in-progress workout with existing sets, edit reps and weight for one set row, save that row, and confirm the updated values appear immediately without leaving the screen.

**Acceptance Scenarios**:

1. **Given** I am viewing an in-progress workout with existing set rows, **When** I edit reps and/or weight for a set row and tap save, **Then** the row updates immediately on the same screen.
2. **Given** I am editing a set row, **When** I save successfully, **Then** only that row is updated and the rest of the workout screen remains in place.

---

### User Story 2 - Keep row state clear during save failures (Priority: P2)

As a lifter, when a set edit cannot be saved, I can still see my unsaved changes and a clear retry path so I know what is and is not persisted.

**Why this priority**: Gym connectivity can be unreliable, so user trust depends on clear saved vs unsaved row state.

**Independent Test**: Attempt a set-row save during a simulated failure and verify unsaved input is preserved, an error is shown for the row, and retry is available.

**Acceptance Scenarios**:

1. **Given** a set-row save fails, **When** the failure is returned, **Then** the row keeps the unsaved input and shows clear failure feedback.
2. **Given** a failed row save, **When** I retry and save succeeds, **Then** the row transitions to saved state and shows the latest values.

---

### User Story 3 - Enforce editing boundaries for active sessions (Priority: P2)

As a lifter, I can edit set rows only in workouts that are currently in progress so completed records are protected.

**Why this priority**: This protects workout record integrity while allowing correction during the active session.

**Independent Test**: Verify set editing controls are available in in-progress workouts and unavailable for non-in-progress workouts.

**Acceptance Scenarios**:

1. **Given** a workout is in progress, **When** I view prior set rows, **Then** row edit controls for reps and weight are available.
2. **Given** a workout is not in progress, **When** I view prior set rows, **Then** row edit controls are not available.

---

### Edge Cases

- Weight is blank and reps are valid during an edit.
- Reps are non-positive or weight is invalid during an edit save.
- Duplicate lift entries in one workout keep set edits isolated by workout-lift entry identity.
- Connectivity drops mid-save and the row must not appear saved until persistence succeeds.
- Concurrent edits to the same set resolve using last-write-wins behavior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to enter inline edit mode for any existing set row in an in-progress workout.
- **FR-002**: Inline editing MUST allow updates to reps and weight only.
- **FR-003**: Set number and set order MUST remain read-only and immutable during edit workflows.
- **FR-004**: Users MUST explicitly save each edited row; changes MUST NOT require navigation away from the workout screen.
- **FR-005**: On successful save, the edited row values MUST update immediately on the active workout screen.
- **FR-006**: If save fails, the row MUST preserve unsaved input and clearly indicate unsaved state.
- **FR-007**: Failed row saves MUST provide clear row-level feedback and an explicit retry path.
- **FR-008**: Editing controls MUST be available only while workout status is in progress.
- **FR-009**: Set edits MUST remain scoped to the targeted workout-lift entry and MUST NOT affect other entries.
- **FR-010**: Validation failures for reps or weight MUST be shown inline at row level before or at save attempt.
- **FR-011**: Concurrent updates to the same set MUST resolve using last-write-wins behavior.
- **FR-012**: Existing add-set, remove-lift, and reorder-lift user flows MUST continue to work without behavioral regression.

### Key Entities *(include if feature involves data)*

- **Workout Set Entry**: A logged set row with immutable identity and set number, plus editable reps and weight values.
- **Set Row Edit Session**: The temporary in-row state that tracks user edits, save attempts, unsaved status, and retry ability for one set row.
- **Workout Lift Entry Context**: The parent scope that ensures set edits apply only to the correct lift entry instance within a workout.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile verification, users can edit and save an existing set row without leaving the active workout screen in 100% of tested flows.
- **SC-002**: 100% of successful set-row saves display updated reps/weight immediately in the edited row.
- **SC-003**: 100% of failed set-row saves preserve unsaved input, show explicit failure feedback, and present retry from the same row.
- **SC-004**: 100% of edit attempts in non-in-progress workouts are blocked.
- **SC-005**: No regressions are observed in add-set, remove-lift, and reorder-lift verification coverage for this feature release.

## Assumptions

- Set editing is used by the same user type currently allowed to log workouts.
- Reps remain required and weight remains optional during row editing.
- Last-write-wins conflict resolution is acceptable for this feature scope.
- Inline row-level messaging is sufficient to communicate saved vs unsaved state.
- This feature only covers editing existing set rows and does not expand into new analytics or programming workflows.
