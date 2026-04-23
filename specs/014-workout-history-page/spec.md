# Feature Specification: Workout history page

**Feature Branch**: `014-workout-history-page`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "Build a dedicated workout history page so completed workouts become part of the user’s visible training history."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Complete workout from either entry point (Priority: P1)

As a lifter finishing a session, I can complete my in-progress workout from either home or the active workout page so ending a session is quick and consistent.

**Why this priority**: Completing a session is required before it can appear in history and is the key workflow transition from live logging to past records.

**Independent Test**: With one in-progress workout, complete it from home, then repeat from active workout detail in a separate session; both attempts end with a completed workout and no active workout remains.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout on home, **When** I tap Complete Workout, **Then** the workout changes to Completed and is no longer shown as active.
2. **Given** I have an in-progress workout in active workout detail, **When** I tap Complete Workout, **Then** the workout changes to Completed and is no longer shown as active.
3. **Given** a workout is completed successfully, **When** the lifecycle change is saved, **Then** a completion timestamp is recorded.

---

### User Story 2 - See completed workouts in dedicated history page (Priority: P1)

As a lifter reviewing prior sessions, I can open a dedicated workout history page and see completed workouts with concise information that helps me recognize each session.

**Why this priority**: The user value of this feature is visible history, so the dedicated page is core scope.

**Independent Test**: Complete a workout, navigate to Workout History, and verify the completed workout appears in the list with label and completed date.

**Acceptance Scenarios**:

1. **Given** at least one completed workout exists, **When** I open Workout History, **Then** I see a list item for each completed workout.
2. **Given** a completed workout has a label, **When** it appears in history, **Then** the label is displayed.
3. **Given** a completed workout has no label, **When** it appears in history, **Then** the label displays as "Workout."
4. **Given** a completed workout appears in history, **When** I view the list item, **Then** I see the completed date.

---

### User Story 3 - Receive clear feedback for completion failures (Priority: P2)

As a lifter, I need clear feedback when completion cannot be saved so I do not assume a workout is completed when it is not.

**Why this priority**: Preventing false-complete states protects workout record trust and avoids data confusion.

**Independent Test**: Trigger completion failure conditions (network/server/race), verify a clear error state appears, and confirm no false completed state is shown.

**Acceptance Scenarios**:

1. **Given** completion fails because of connectivity or server error, **When** I attempt to complete a workout, **Then** I receive clear failure feedback and the workout remains not completed.
2. **Given** the workout state changed elsewhere before I submit completion, **When** the app reconciles the response, **Then** I see accurate state and clear feedback.

---

### Edge Cases

- User taps Complete Workout rapidly multiple times; the system avoids duplicate conflicting outcomes and presents one final authoritative state.
- A workout is already completed on another device or session; the app resolves to authoritative state without showing conflicting status.
- Workout has an empty or whitespace label; history still shows the entry with "Workout" fallback.
- No completed workouts exist; the history page shows a clear empty state and how to create history by completing a workout.
- A completion request fails after user action; the app never displays a false-success state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a user to complete an in-progress workout from the home active-workout summary.
- **FR-002**: The system MUST allow a user to complete an in-progress workout from active workout detail.
- **FR-003**: On successful completion, the system MUST change workout status to Completed.
- **FR-004**: On successful completion, the system MUST persist a non-null completion timestamp for the workout.
- **FR-005**: A completed workout MUST no longer be treated as an active workout in primary navigation surfaces.
- **FR-006**: The system MUST provide a dedicated Workout History page that users can navigate to directly.
- **FR-007**: The Workout History page MUST list completed workouts.
- **FR-008**: Each history list item MUST display workout label when present.
- **FR-009**: Each history list item MUST display "Workout" as fallback when the workout label is missing or empty.
- **FR-010**: Each history list item MUST display the workout completed date.
- **FR-011**: Completion failures (network, server, or stale-state conflict) MUST show clear user feedback and MUST NOT present a workout as completed unless completion is saved.
- **FR-012**: The feature MUST preserve a mobile-first, low-tap flow for completion and history lookup.
- **FR-013**: The feature MUST exclude completed-workout editing, analytics dashboards, advanced filtering/search, and extra history fields beyond label and completed date in this release.

### Key Entities *(include if feature involves data)*

- **Workout Session**: A user-owned workout record that transitions from In Progress to Completed and stores lifecycle timestamps.
- **Completed Workout History Item**: A user-visible summary entry for a completed workout containing display label and completed date.
- **Completion Attempt**: A user action to complete a workout from either supported entry point, resulting in success or clear failure feedback.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of successful completion actions result in workouts with Completed status and a non-null completion timestamp.
- **SC-002**: In acceptance validation, newly completed workouts appear on the dedicated Workout History page without manual correction.
- **SC-003**: In acceptance validation, 100% of history entries show either the workout label or the "Workout" fallback plus completed date.
- **SC-004**: In failure-path validation, users receive explicit completion failure feedback and no false-completed workout states are observed.

## Assumptions

- Users are already signed in and can access only their own workout records.
- Default history ordering is most recently completed first.
- Completed date is shown in the user’s local display format while preserving recorded completion moment.
- Existing in-progress workout rules remain unchanged, including at most one active workout at a time.
- A dedicated history page in this slice focuses on visibility only and does not include drill-in details.
