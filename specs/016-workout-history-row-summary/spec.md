# Feature Specification: Workout history row summary details

**Feature Branch**: `016-workout-history-row-summary`  
**Created**: 2026-04-24  
**Status**: Draft  
**Input**: User description: "Extend the existing workout history feature so each completed-workout row shows date, duration in HH:MM, and number of lifts while preserving current completed-only and most-recent-first behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scan enriched completed workout history (Priority: P1)

As a lifter reviewing prior sessions, I can open Workout History and quickly scan each completed workout row with label, completion date, duration, and lift count so I can understand what I did in past workouts.

**Why this priority**: The primary user value is faster and clearer review of completed sessions without adding extra navigation or analytics features.

**Independent Test**: Open Workout History with completed workouts available and verify each row includes label, date, duration in HH:MM, and number of lifts, ordered most recent first.

**Acceptance Scenarios**:

1. **Given** completed workouts exist, **When** I open Workout History, **Then** I see completed workouts listed in descending completion time order.
2. **Given** a completed workout row is shown, **When** I inspect the row, **Then** it shows the workout label, completion date, duration in HH:MM, and lift count.
3. **Given** a completed workout contains zero lifts, **When** it appears in history, **Then** the row shows a lift count of 0.

---

### User Story 2 - Keep existing history behavior while extending row details (Priority: P1)

As a user, I can continue to use the existing Workout History page behavior without regressions while seeing additional row summary fields.

**Why this priority**: The enhancement must preserve currently working behavior so users do not lose reliable history access while new row details are added.

**Independent Test**: Verify dedicated history page navigation, completed-only listing, ordering, and existing empty/error states still behave correctly after the row summary enhancement.

**Acceptance Scenarios**:

1. **Given** I navigate to Workout History, **When** the page loads, **Then** the existing dedicated route and page access remain unchanged.
2. **Given** history has no completed workouts, **When** I open the page, **Then** the existing empty-history feedback remains clear and functional.
3. **Given** history loading fails, **When** I open the page, **Then** the existing load-error feedback still appears and list rendering stays stable.

---

### Edge Cases

- A completed workout has missing or invalid start or completion timestamps; the history page continues rendering and shows a safe fallback for duration.
- Workout duration rounds across minute boundaries (very short or long sessions); duration still displays in HH:MM format.
- Completed workouts with zero lifts display a lift count of 0 without hiding the row.
- Existing empty-state and error-state messages remain visible and unaffected by the new row summary fields.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST keep Workout History limited to completed workouts.
- **FR-002**: The system MUST keep Workout History ordered by completion time with newest workout first.
- **FR-003**: Each workout history row MUST display the workout label using existing label behavior.
- **FR-004**: Each workout history row MUST display the workout completion date.
- **FR-005**: Each workout history row MUST display workout duration in `HH:MM`.
- **FR-006**: Workout duration MUST be calculated as completion timestamp minus start timestamp for the workout.
- **FR-007**: Each workout history row MUST display number of lifts in the completed workout.
- **FR-008**: Lift count MUST represent workout lifts (exercises), not set rows.
- **FR-009**: Workouts with no lifts MUST display a lift count of `0`.
- **FR-010**: Missing or invalid timestamp data MUST NOT break history rendering and MUST show a safe duration fallback.
- **FR-011**: Existing history empty-state and load-error behavior MUST continue to work unchanged.
- **FR-012**: The feature MUST exclude analytics dashboards, charting, filtering, search, and drill-down details.

### Key Entities *(include if feature involves data)*

- **Completed Workout History Row**: A user-visible summary of a completed workout containing label, completion date, formatted duration, and lift count.
- **Workout Duration**: The elapsed time between workout start and workout completion, formatted as `HH:MM`.
- **Lift Count**: The number of workout lifts associated with a completed workout, displayed as an integer including zero.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation scenarios with completed workouts, 100% of history rows show label, completion date, duration in `HH:MM`, and lift count.
- **SC-002**: In validation scenarios, completed workout ordering remains newest first for 100% of returned history lists.
- **SC-003**: In validation scenarios for zero-lift workouts, 100% of rows display a lift count of `0`.
- **SC-004**: In validation scenarios with invalid or missing timestamps, history pages remain usable and continue displaying all available rows without page failure.
- **SC-005**: Existing empty-history and load-error user flows continue to pass regression checks after the enhancement.

## Assumptions

- The existing dedicated Workout History route and page remain the primary entry point for reviewing completed workouts.
- The existing label display behavior is retained, including current fallback handling when labels are absent.
- Duration display uses a stable `HH:MM` representation suitable for quick scanning in a gym workflow.
- Any missing timestamp case displays a safe, non-crashing fallback value while preserving row visibility.
- No additional filtering, sorting controls, analytics, or drill-in details are introduced in this feature slice.
