# Feature Specification: Delete in-progress workout

**Feature Branch**: `[015-delete-active-workout]`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "Build workout delete for in-progress sessions so users can discard a workout they do not want to keep."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Confirm before deleting active workout (Priority: P1)

As a lifter in an active session, I can choose Delete Workout and must explicitly confirm before the workout is permanently removed.

**Why this priority**: This protects users from accidental destructive actions while preserving a fast in-gym workflow.

**Independent Test**: From the active workout page with an in-progress workout, trigger delete, verify confirmation is required, and verify dismissing confirmation keeps all data unchanged.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout open on the active workout page, **When** I choose Delete Workout, **Then** the system presents a clear confirmation step before any data is removed.
2. **Given** the confirmation is shown, **When** I cancel or dismiss it, **Then** the workout remains in progress and no workout data changes.

---

### User Story 2 - Permanently delete confirmed in-progress workout (Priority: P1)

As a lifter who does not want to keep a session, I can confirm deletion and permanently remove the workout and its in-progress data.

**Why this priority**: Permanent removal is the primary user outcome and prevents unwanted sessions from being retained.

**Independent Test**: Confirm deletion for an in-progress workout and verify the workout cannot be retrieved as active or historical data afterward.

**Acceptance Scenarios**:

1. **Given** I confirm deletion of an in-progress workout, **When** deletion succeeds, **Then** the workout and associated in-progress data are permanently removed.
2. **Given** deletion succeeds, **When** the active workout view refreshes, **Then** the workout is no longer active and I see clear success feedback plus guidance to return home and start a new workout.

---

### User Story 3 - Handle delete failures and race conditions safely (Priority: P2)

As a lifter, I receive clear feedback when deletion cannot be completed so I never mistake a failed request for successful deletion.

**Why this priority**: Destructive actions require reliable outcome clarity to preserve trust and avoid confusion.

**Independent Test**: Simulate network/server errors and stale-state conflicts while deleting, then verify clear failure feedback and authoritative state recovery.

**Acceptance Scenarios**:

1. **Given** deletion fails because of network or server error, **When** I submit confirmation, **Then** I see failure feedback and the app does not claim the workout was deleted.
2. **Given** workout state changes before deletion completes, **When** the request resolves, **Then** I receive recoverable feedback and the page reconciles to authoritative state.

---

### Edge Cases

- User taps confirm delete repeatedly; the system processes at most one destructive request and keeps the final state consistent.
- Workout is already removed before confirmation submission; user receives a clear not-found outcome and guidance to continue from a valid state.
- Workout is no longer in progress at delete time; deletion is rejected with clear conflict feedback.
- Confirmation is closed by navigation or backdrop dismissal; no deletion occurs.
- Temporary connectivity loss occurs during delete; the UI shows failure and does not display false success.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow workout deletion only from the active workout page in this release.
- **FR-002**: The system MUST allow deletion only when the workout is in progress.
- **FR-003**: The system MUST require explicit user confirmation before deleting an in-progress workout.
- **FR-004**: The system MUST perform no data changes when the user cancels or dismisses the confirmation.
- **FR-005**: On confirmed success, the system MUST permanently delete the workout and all associated in-progress workout data.
- **FR-006**: After successful deletion, the system MUST stop treating the workout as active in user-visible state.
- **FR-007**: After successful deletion, the system MUST show clear success feedback and guide the user to start or continue from home.
- **FR-008**: The system MUST prevent duplicate destructive processing from repeated confirmation submissions.
- **FR-009**: The system MUST return clear not-found behavior when deletion targets a workout that no longer exists.
- **FR-010**: The system MUST return clear conflict behavior when deletion targets a workout that is not in progress.
- **FR-011**: The system MUST provide explicit failure feedback for connectivity, timeout, or server failures and MUST NOT display false-success states.
- **FR-012**: The system MUST reconcile stale or race-condition responses to an authoritative final state after delete attempts.
- **FR-013**: Workouts deleted by this feature MUST NOT appear in workout history views because the deleted records no longer exist.
- **FR-014**: Existing completed-workout history behavior MUST remain unchanged.
- **FR-015**: The feature MUST exclude deletion from home, undo or restore behavior, and archive/recycle-bin views.

### Key Entities *(include if feature involves data)*

- **In-Progress Workout**: An active workout session eligible for deletion only while its lifecycle state is in progress.
- **Delete Confirmation Attempt**: A user-initiated destructive workflow that requires explicit confirmation before permanent deletion.
- **Workout Deletion Outcome**: The authoritative result of a delete attempt (success, not found, conflict, or failure) that drives user feedback and state reconciliation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile validation, users can complete confirmed deletion from the active workout page in two taps or fewer after opening the delete action.
- **SC-002**: In 100% of successful confirmation cases, the deleted workout and associated in-progress data are absent from subsequent active and history reads.
- **SC-003**: In failure-path validation (network, server, or stale-state conflicts), 0 false-success outcomes are observed.
- **SC-004**: In acceptance validation, deleting an in-progress workout does not regress visibility of legitimately completed workout history entries.

## Assumptions

- Users manage one active in-progress workout at a time.
- Permanent deletion is intentional and irreversible for this release.
- Workout deletion applies to user-owned data within existing access boundaries.
- Default history behavior continues to show completed workouts only.
- Any new or changed workout lifecycle business rules ship with automated tests in the same delivery slice.
