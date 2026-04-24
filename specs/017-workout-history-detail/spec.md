# Feature Specification: Workout history detail flow

**Feature Branch**: `017-workout-history-detail`  
**Created**: 2026-04-24  
**Status**: Draft  
**Input**: User description: "Build a workout history detail flow so a user can open a past completed workout from the Workout History list and review what they performed."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Open completed workout detail from history (Priority: P1)

As a lifter, I can open a completed workout from the Workout History list and see a dedicated detail view so I can review exactly what was performed in that session.

**Why this priority**: Opening past workout details is the core user outcome of this feature and directly supports next-session training decisions.

**Independent Test**: With at least one completed workout in history, open the history page, select a row, and verify a detail view opens for that selected workout.

**Acceptance Scenarios**:

1. **Given** completed workouts are listed in history, **When** I select a workout row, **Then** the app opens a detail view for that specific completed workout.
2. **Given** I opened a completed workout detail view, **When** the detail content loads, **Then** I can review that workout without entering an edit flow.

---

### User Story 2 - Review completed workout structure and values (Priority: P1)

As a lifter, I can review the completed workout summary, lifts, and set rows so I can understand past performance and pick my next working weight.

**Why this priority**: The detail view must provide complete and trustworthy workout context, not only navigation.

**Independent Test**: Open a completed workout detail and verify date, duration, workout name/type when present, lift entries, and set rows with weight and reps are displayed.

**Acceptance Scenarios**:

1. **Given** a completed workout detail view is open, **When** the workout has summary data, **Then** completion date, duration, and workout name/type (when present) are shown.
2. **Given** a completed workout has lifts and sets, **When** I view the detail, **Then** each lift and its set rows show recorded reps and weight values.
3. **Given** some set rows have no recorded weight, **When** I view the detail, **Then** the detail view shows a clear fallback value instead of a blank or broken display.

---

### User Story 3 - Preserve history list behavior and resilient loading (Priority: P2)

As a user, I can keep using Workout History reliably while opening details, including clear feedback when data cannot be loaded.

**Why this priority**: The feature extends history behavior and must avoid regressions in the existing completed-only, newest-first list flow.

**Independent Test**: Validate history ordering/filters remain unchanged and confirm loading, not-found, and offline error states are actionable when opening detail.

**Acceptance Scenarios**:

1. **Given** completed workouts exist, **When** I return to Workout History, **Then** the list still shows completed workouts only in newest-first order.
2. **Given** I select a completed workout that no longer exists, **When** detail load fails, **Then** I see a clear error and can return to Workout History.
3. **Given** network issues occur while opening detail, **When** the request fails or is delayed, **Then** I see loading feedback and a retry-capable error state rather than a silent failure.

---

### Edge Cases

- A completed workout contains zero lifts; the detail view still loads and clearly communicates that no lifts were recorded.
- A lift has zero set rows; the lift still renders without crashing and without hiding other workout data.
- Some set values are missing or null (for example optional weight); fallback display remains readable and consistent.
- A selected completed workout is unavailable or deleted; users receive an explicit not-found state with a path back to history.
- Intermittent connectivity causes slow or failed detail loading; users receive clear loading/error feedback and can retry.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow users to open a completed workout detail view directly from a workout row in Workout History.
- **FR-002**: The system MUST ensure the opened detail corresponds to the selected history row.
- **FR-003**: The completed workout detail view MUST display completion date and workout duration.
- **FR-004**: The completed workout detail view MUST display workout name/type information when it exists.
- **FR-005**: The completed workout detail view MUST display all recorded lift entries for that workout.
- **FR-006**: For each displayed lift, the system MUST display its set rows.
- **FR-007**: Set rows in the completed workout detail view MUST show recorded reps and weight values.
- **FR-008**: When optional set values are missing, the detail view MUST display a clear fallback value and remain usable.
- **FR-009**: The Workout History list MUST continue to show completed workouts only.
- **FR-010**: The Workout History list MUST preserve newest-first ordering behavior.
- **FR-011**: The feature MUST provide clear loading feedback while opening a completed workout detail.
- **FR-012**: The feature MUST provide actionable error feedback for unavailable or failed detail loads, including a way back to Workout History.
- **FR-013**: Completed workout detail in this slice MUST be read-only and MUST NOT introduce editing or deletion of completed workout data.
- **FR-014**: This slice MUST exclude analytics dashboards, search/filter enhancements, and progression recommendation features.

### Key Entities *(include if feature involves data)*

- **Completed Workout History Item**: A summary row representing a completed workout that can be selected to open detail.
- **Completed Workout Detail**: A read-only representation of one completed workout including summary data and performed lifts/sets.
- **Lift Entry**: A performed exercise entry within a completed workout.
- **Set Row**: A recorded set within a lift entry containing reps and optional weight.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In acceptance validation, users can open a completed workout from history in one tap/click from the selected row.
- **SC-002**: 100% of successfully loaded completed workout detail views show completion date, duration, and workout name/type when present.
- **SC-003**: 100% of successfully loaded completed workout detail views render lifts and associated set rows with reps and weight values without page failure.
- **SC-004**: Regression validation confirms existing Workout History behavior remains completed-only and newest-first.
- **SC-005**: In failure-path validation, unavailable or failed detail loads always show explicit, actionable feedback with a route back to Workout History.

## Assumptions

- Workout History remains the entry point for reviewing past completed workouts.
- Past workout details are reviewed in read-only mode for this release.
- Duration shown in detail reuses the existing duration concept already visible in history summaries.
- A clear fallback value is acceptable when optional fields such as weight are missing.
- Existing access model for workouts remains unchanged and users only review workouts they are permitted to view.
