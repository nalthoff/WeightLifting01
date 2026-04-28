# Feature Specification: Backfill Past Workout

**Feature Branch**: `[023-backfill-past-workout]`  
**Created**: 2026-04-28  
**Status**: Draft  
**Input**: User description: "As a lifter, I want to add a workout I already did on an earlier calendar day so my workout history stays complete and my same-lift context for choosing the next weight stays trustworthy."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Log missed workout on chosen day (Priority: P1)

As a lifter, I want to record a previously completed session with a required past date, start time, and duration so my history reflects when I actually trained.

**Why this priority**: Accurate chronology is the core user value and directly affects trust in workout history.

**Independent Test**: Create a historical workout by selecting a prior day, save it as completed, and verify it appears in history at the expected position relative to nearby completed workouts.

**Acceptance Scenarios**:

1. **Given** I am logging a missed workout, **When** I enter a past calendar day, hour/minute start time, and duration and save, **Then** the workout is stored as completed for that chosen training time window.
2. **Given** I have entered date, time, and duration, **When** I save the historical workout, **Then** the workout appears in history in the expected chronological position.
3. **Given** one of date, time, or duration is missing, **When** I attempt to save, **Then** the app clearly prompts me to complete the required fields.

---

### User Story 2 - Keep lift and set entry parity (Priority: P1)

As a lifter, I want to add lifts and sets to a past workout using the same entry model as live logging so I can quickly capture what I actually performed.

**Why this priority**: Historical entries are only useful for next-weight decisions if lift and set details are captured with familiar workflow parity.

**Independent Test**: Start a historical workout flow, add multiple lifts and sets, complete the workout, and verify details are available in the same history/detail surfaces as other completed workouts.

**Acceptance Scenarios**:

1. **Given** I am creating a historical workout, **When** I add lifts and sets, **Then** I can record exercise and set details using the same mental model as live workout entry.
2. **Given** a historical workout has minimal logged content, **When** I complete it, **Then** it still appears consistently in history and detail views.
3. **Given** a historical workout is saved as completed, **When** I open it from history, **Then** the recorded lifts and sets are shown consistently with other completed workouts.

---

### User Story 3 - Catch up while a live workout exists (Priority: P2)

As a lifter, I want to log a previous session even when I currently have an active workout so I can backfill missed history without interrupting today’s training flow.

**Why this priority**: Users frequently remember missed logging mid-session; forcing them to end or discard a live workout creates friction and data loss risk.

**Independent Test**: Keep one workout active, create and complete a historical workout, and verify the active workout remains available while the historical workout appears in completed history.

**Acceptance Scenarios**:

1. **Given** I have an active workout in progress, **When** I log and complete a historical workout, **Then** the active workout remains intact and available.
2. **Given** I am in a catch-up scenario, **When** I complete the historical workout, **Then** it appears in completed history according to chosen training-day chronology.

### Edge Cases

- Multiple completed workouts share the same calendar day; ordering remains predictable and consistent with existing history rules.
- Local day-boundary ambiguity (late-night/early-morning training) still results in user-understandable history placement based on entered date and time.
- Required date, time, and duration inputs remain easy to complete on mobile (simple date selector plus hour/minute and duration controls).
- Historical workouts with minimal lift/set data remain visible and stable in history and detail surfaces.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST require users to enter a past calendar date for a historical workout.
- **FR-002**: System MUST require users to enter a start time (hour and minute) for a historical workout.
- **FR-003**: System MUST require users to enter workout duration for a historical workout before save.
- **FR-004**: Users MUST be able to add lifts and sets to a historical workout using the same core logging interaction model as live workouts.
- **FR-005**: System MUST include completed historical workouts in workout history in chronological order that matches the entered historical date/time and existing ordering behavior.
- **FR-006**: System MUST allow historical workout logging even when another workout is currently in progress.
- **FR-007**: System MUST preserve the in-progress workout state when a historical workout is created and completed during catch-up.
- **FR-008**: System MUST present clear, low-friction mobile feedback for save success and failure so users can trust historical entries were captured.
- **FR-009**: System MUST keep historical workout data available in existing history/detail surfaces used for next-weight decisions.

### Key Entities *(include if feature involves data)*

- **HistoricalWorkoutSession**: A completed workout entry recorded after the fact, anchored to required past date/time and required duration inputs.
- **HistoricalSessionTimingInput**: User-provided date, hour/minute start time, and duration used to place a historical workout correctly within completed workout chronology.
- **WorkoutEntrySet**: Exercise and set records attached to a historical workout session using the same structure as live workout logging.
- **ActiveWorkoutContext**: Existing in-progress workout state that must remain unaffected while a historical session is backfilled.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 90% of users can complete a historical workout entry on mobile in under 2 minutes during usability validation.
- **SC-002**: In validation scenarios, 100% of saved historical workouts appear in workout history in the user-expected chronological position for the selected training day.
- **SC-003**: At least 95% of users successfully add lifts and sets to a historical workout on the first attempt without guidance.
- **SC-004**: In catch-up scenarios with an active workout present, 100% of test runs preserve active workout continuity while successfully saving the historical workout.

## Assumptions

- Users are intentionally backfilling previously completed sessions rather than drafting future workouts.
- Date, start time, and duration are mandatory for historical entries but must be captured in a simple, low-friction mobile UI.
- Existing history and detail surfaces remain the primary places users confirm saved workouts and review prior same-lift context.
- The selected training day is the authoritative user intent for historical ordering behavior.
- Historical logging must preserve mobile-first speed and clarity comparable to current live logging flow.
