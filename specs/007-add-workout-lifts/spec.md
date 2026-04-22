# Feature Specification: Add lifts to in-progress workout

**Feature Branch**: `007-add-workout-lifts`  
**Created**: 2026-04-22  
**Status**: Draft  
**Input**: User description: "As a lifter, I want to add lifts to an in-progress workout so I can log what I am doing during the current session."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add lifts from active workout screen (Priority: P1)

As a lifter in an active workout, I can tap a primary Add Lift action on the active workout screen, choose from my active lift library, and immediately see the selected lifts added into the current workout flow.

**Why this priority**: This is the core in-gym logging step after session start and unlocks practical workout logging progression.

**Independent Test**: Start a workout, open Add Lift from the active workout screen, add one or more active lifts, and verify they appear immediately in the current workout experience without manual refresh.

**Acceptance Scenarios**:

1. **Given** I am on an in-progress workout screen, **When** I tap the primary Add Lift action, **Then** a lift picker opens from that screen.
2. **Given** the lift picker is open, **When** I select one active lift and confirm add, **Then** that lift appears immediately in the current workout flow.
3. **Given** the lift picker is open, **When** I add multiple active lifts, **Then** all successful additions appear immediately in the current workout flow.

---

### User Story 2 - Handle duplicate lift additions intentionally (Priority: P2)

As a lifter, I can add the same lift more than once in a workout for now, so repeated lift blocks are possible without being blocked by duplicate prevention rules.

**Why this priority**: Duplicate-allowed behavior is explicitly required for this slice and affects backend rules and user expectations.

**Independent Test**: Add the same active lift multiple times in one workout and verify each successful add is retained and visible.

**Acceptance Scenarios**:

1. **Given** I already added a lift in the current workout, **When** I add that same lift again, **Then** the second addition succeeds and is visible as a separate workout-lift entry.
2. **Given** duplicate additions are allowed, **When** I continue adding the same lift intentionally, **Then** the system does not block the action solely because it is already present.

---

### User Story 3 - Stay resilient under picker and network edge cases (Priority: P2)

As a lifter, I receive clear feedback when lifts cannot be selected or saved, so I never mistake failed actions for successful ones during a live workout.

**Why this priority**: In-gym reliability and clarity are required for trust in workout logs.

**Independent Test**: Exercise empty-library and failed-add scenarios to verify users get explicit feedback and no ghost additions appear.

**Acceptance Scenarios**:

1. **Given** no active lifts are available, **When** I open Add Lift, **Then** I see a clear empty-state message with guidance to create or activate lifts in Settings.
2. **Given** an add request fails due to connectivity or server issues, **When** I attempt to add a lift, **Then** I see a clear failure message and the failed lift does not appear as saved.
3. **Given** I tap add rapidly, **When** requests complete, **Then** the UI presents a clear and unambiguous result for each successful addition without silent loss.

---

### Edge Cases

- Lift picker opens with zero active lifts: user receives clear empty-state guidance, not a blank control.
- Add-lift request fails (offline, timeout, 5xx): user sees explicit error and no client-only ghost workout-lift entry.
- Rapid repeated taps while adding lifts: resulting state stays clear and consistent, with each successful add reflected.
- If a lift later becomes deactivated in the library, workout entries already added in the current session remain visible for that session.
- If active workout context is missing or expired, user is guided back to start/continue a workout before adding lifts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST expose a prominent Add Lift action on the active workout screen in the primary mobile workout flow.
- **FR-002**: Tapping Add Lift MUST open a lift picker from the active workout experience.
- **FR-003**: The lift picker MUST list active lifts from the existing lift library.
- **FR-004**: The user MUST be able to add one or more selected lifts into the current in-progress workout.
- **FR-005**: Added lifts MUST appear immediately in the current workout flow without requiring manual refresh.
- **FR-006**: The same lift MUST be allowed to be added multiple times in a workout for this release.
- **FR-007**: On add failure (including offline, timeout, or server error), the system MUST show a clear failure outcome and MUST NOT present unsaved lift additions as persisted.
- **FR-008**: If no active lifts exist, the picker MUST show a clear empty state and direct users to create/activate lifts in Settings.
- **FR-009**: Backend business logic MUST own workout-lift association rules and validation; Angular components MUST remain presentation-focused.
- **FR-010**: Data changes needed for workout-lift association persistence MUST be delivered through tracked SQL migration artifacts.
- **FR-011**: New or changed backend business rules for adding workout lifts MUST include automated unit tests, with integration/contract coverage where API behavior changes.
- **FR-012**: New production classes introduced by this feature MUST be organized as one class per file.
- **FR-013**: This feature MUST treat the current app mode as single-user for now.
- **FR-014**: This feature MUST exclude removing lifts from workouts, set/rep/weight logging expansion, templates/programming, and analytics dashboards from this release scope.

### Key Entities *(include if feature involves data)*

- **Workout Lift Entry**: A persisted association between an in-progress workout and a chosen lift, representing one added lift instance in the workout flow.
- **Lift Picker Item**: A selectable representation of an active lift from the lift library used during Add Lift interaction.
- **Workout Lift Add Attempt**: A user add action outcome indicating success (persisted workout-lift entry) or explicit failure (no persisted entry).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On phone-sized viewports, users can add a lift from the active workout screen in about 3 taps or fewer.
- **SC-002**: 100% of successful add actions are persisted and visible in the current workout flow immediately.
- **SC-003**: Duplicate add attempts for the same lift are accepted for this release and produce visible, persisted entries when requests succeed.
- **SC-004**: In add-failure scenarios (offline, timeout, server failure), users always receive explicit failure feedback and zero ghost additions appear as saved.

## Assumptions

- The app is operating in single-user mode for now, and workout/lift operations are scoped to that user context.
- Active workout context already exists from the prior start-workout slice and is available before Add Lift is used.
- Existing lift library management under Settings remains the source of truth for lift definitions.
- Only active lifts are selectable in this slice; inactive lifts are not displayed in the picker.
- Removing workout lifts is intentionally deferred to a later feature.
- Any new or changed business-layer logic will ship with automated unit tests, plus integration/contract tests where boundary behavior changes.
