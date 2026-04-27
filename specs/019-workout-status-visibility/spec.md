# Feature Specification: Workout Lifecycle Status Visibility

**Feature Branch**: `019-workout-status-visibility`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Feature: Workout lifecycle status visibility and history gating"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Active Workout Status Clarity (Priority: P1)

As a lifter viewing an active workout, I can immediately see a status badge that indicates the workout is in progress.

**Why this priority**: The active workout view is the core in-gym journey where status confusion most directly disrupts logging flow.

**Independent Test**: Start a workout, open the workout detail view, and confirm a clearly visible "In Progress" status badge appears without additional interaction.

**Acceptance Scenarios**:

1. **Given** a workout is active, **When** the user opens its detail view, **Then** the view shows an "In Progress" status badge.
2. **Given** a workout transitions from active to finished, **When** the detail view refreshes or reloads, **Then** the badge updates to the finished state.

---

### User Story 2 - Completed Workout Status Clarity (Priority: P1)

As a lifter reviewing a finished workout, I can immediately see a status badge that indicates the workout is completed.

**Why this priority**: Users frequently review finished sessions to guide next-weight decisions, and unclear status undermines confidence in historical data.

**Independent Test**: Open a completed workout from history and confirm a clearly visible "Completed" status badge appears in the detail view.

**Acceptance Scenarios**:

1. **Given** a workout is completed, **When** the user opens it from history, **Then** the detail view shows a "Completed" status badge.
2. **Given** the completed workout detail is revisited later, **When** the user loads the page again, **Then** the completed badge remains visible and accurate.

---

### User Story 3 - History and Progress Gating (Priority: P1)

As a lifter using history-based progress views, I only see completed workouts so unfinished sessions do not affect progress review.

**Why this priority**: History accuracy is required for trustworthy progression decisions; including unfinished sessions creates misleading context.

**Independent Test**: Have one active workout and one completed workout, open history-based progress views, and verify only the completed workout is present.

**Acceptance Scenarios**:

1. **Given** a user has both active and completed workouts, **When** they open any history-based progress view, **Then** only completed workouts are shown.
2. **Given** a workout is completed and receives a completion timestamp, **When** the user opens history-based progress views, **Then** that workout becomes eligible for inclusion.

### Edge Cases

- A workout marked completed without a completion timestamp is excluded from history-based progress views.
- If the system receives an unknown workout status value, the UI fails safely without displaying misleading status text.
- If a user completes a workout and immediately refreshes or navigates between views, status presentation remains consistent.
- Actions intended only for active workouts remain unavailable once a workout is completed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST represent workout lifecycle using exactly two user-facing states: In Progress and Completed.
- **FR-002**: The system MUST display a visible status badge on active workout detail views.
- **FR-003**: The system MUST display a visible status badge on completed workout detail views accessed from history contexts.
- **FR-004**: The system MUST record an end timestamp when a workout transitions from In Progress to Completed.
- **FR-005**: The system MUST include only completed workouts in history-based progress views.
- **FR-006**: The system MUST exclude any workout lacking a valid completion timestamp from history-based progress views.
- **FR-007**: The system MUST keep status-dependent behavior consistent so active-only actions are not available for completed workouts.
- **FR-008**: The system MUST preserve existing workout lifecycle terminology so users consistently see "In Progress" and "Completed" labels across applicable views.

### Key Entities *(include if feature involves data)*

- **Workout Session**: A user workout instance with lifecycle status, start timestamp, optional completion timestamp, and detail view visibility.
- **Workout Status Badge**: A visual indicator on workout detail views that communicates the current lifecycle state.
- **History Progress Record**: A completed workout entry eligible for appearance in history-based progress views.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation, 100% of active and completed workout detail views display an explicit status badge.
- **SC-002**: In validation, 100% of completed workouts have a non-null end timestamp.
- **SC-003**: In validation, 0 in-progress workouts appear in history-based progress views.
- **SC-004**: In mobile-view manual verification, users can correctly identify whether a workout is active or finished within 2 seconds.

## Assumptions

- Existing workout lifecycle states remain limited to In Progress and Completed for this feature scope.
- Existing completion flows are reused, with this feature reinforcing lifecycle visibility and gating correctness.
- History-based progress views are defined as any user-facing view that summarizes completed workout outcomes over time.
- User access and identity behavior remain unchanged from current product behavior.
- Standard validation and regression coverage will verify timestamp recording and history filtering behavior.
