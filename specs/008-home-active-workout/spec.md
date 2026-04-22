# Feature Specification: Home active workout summary and quick completion

**Feature Branch**: `008-home-active-workout`  
**Created**: 2026-04-22  
**Status**: Draft  
**Input**: User description: "Add a home-screen active workout summary card so users can quickly continue or complete their current in-progress workout without navigating into workout detail first."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See and continue active workout from home (Priority: P1)

As a lifter opening the app in the gym, I can immediately see whether I have an in-progress workout and continue it from home without extra discovery steps.

**Why this priority**: This is the fastest path back into live logging and directly supports the primary in-gym journey.

**Independent Test**: With an existing in-progress workout, open home and verify an active-workout card appears with label or fallback name, started time, and a Continue action that opens that workout.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout, **When** I land on home, **Then** the app shows an active-workout summary card with workout label (or "Workout"), started time, and Continue + Complete actions.
2. **Given** the active-workout card is visible, **When** I tap Continue, **Then** I am navigated to the existing active workout detail route for that workout.

---

### User Story 2 - Complete active workout directly from home (Priority: P1)

As a lifter who is done training, I can complete my in-progress workout from home in one tap without entering workout detail.

**Why this priority**: Reduces taps and friction at the end of a session, especially on mobile in a gym environment.

**Independent Test**: With an in-progress workout visible on home, tap Complete and verify the workout is completed, the card disappears, and a success message appears while staying on home.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout on home, **When** I tap Complete, **Then** the workout is completed immediately with no confirmation step.
2. **Given** completion succeeds, **When** the response is applied, **Then** I remain on home, the active-workout card is removed, and a success message is shown.

---

### User Story 3 - Handle completion failures and race states clearly (Priority: P2)

As a lifter, I receive clear feedback when completion cannot be saved so I never mistake a failed action for a completed workout.

**Why this priority**: Preserves trust in workout state and avoids silent data loss or ghost completion behavior.

**Independent Test**: Trigger completion failure conditions (offline/server/race), verify explicit error feedback, and confirm active-workout card remains or refreshes to accurate state.

**Acceptance Scenarios**:

1. **Given** a completion request fails due to connectivity or server error, **When** I tap Complete, **Then** I see a clear error and the workout is not presented as completed.
2. **Given** workout state changes during completion (for example already completed elsewhere), **When** home refreshes state, **Then** the UI resolves to an accurate card/no-card state with recoverable feedback.

---

### Edge Cases

- No in-progress workout exists: no active-workout summary card is rendered.
- In-progress workout label is empty: card uses fallback title "Workout."
- Rapid repeated taps on Complete: duplicate conflicting completion attempts are prevented and UI remains unambiguous.
- Completion succeeds but state refresh is delayed: user still receives consistent success feedback and eventual accurate home state.
- Continue action is tapped while workout is no longer available: user receives recoverable feedback and is guided to a valid home state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST show an active-workout summary card on home when one in-progress workout exists.
- **FR-002**: The active-workout card MUST display workout label when present, otherwise the fallback label "Workout."
- **FR-003**: The active-workout card MUST display the workout start time.
- **FR-004**: The active-workout card MUST provide a Continue action that navigates to the existing active workout detail route for that workout.
- **FR-005**: The active-workout card MUST provide a Complete action that immediately attempts to complete the in-progress workout with no confirmation step.
- **FR-006**: On successful completion from home, the system MUST keep the user on home, remove the active-workout card, and present explicit success feedback.
- **FR-007**: On completion failure (including offline, timeout, or server failure), the system MUST show explicit failure feedback and MUST NOT present the workout as completed.
- **FR-008**: The system MUST guard against duplicate completion attempts from rapid repeated taps so resulting state is clear and consistent.
- **FR-009**: The system MUST reconcile race or stale-state conditions so home reflects authoritative workout status after completion attempts.
- **FR-010**: Workout lifecycle business rules and completion validation MUST remain in backend business logic; home UI changes MUST remain presentation-focused.
- **FR-011**: Any backend business-logic changes required for home-triggered completion MUST include automated unit tests in the same change.
- **FR-012**: This feature MUST preserve existing active workout detail behavior and MUST NOT redesign workout detail screens.
- **FR-013**: This feature MUST exclude analytics/reporting redesign, workout templating/programming changes, and multi-user collaboration scope.

### Key Entities *(include if feature involves data)*

- **Active Workout Summary**: Home-facing representation of the current in-progress workout including identity, display label, started time, and availability of Continue/Complete actions.
- **Home Completion Attempt**: User-initiated request to complete the current in-progress workout from home with success/failure outcome and user feedback state.
- **Workout Lifecycle State**: Authoritative workout status used to determine whether an active-workout card should render after completion attempts.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile validation, users with an in-progress workout can continue or complete from home in one tap for each action.
- **SC-002**: In 100% of validated success cases, successful home completion removes the active-workout card and shows explicit success feedback while user remains on home.
- **SC-003**: In simulated completion-failure scenarios, 100% of cases show explicit failure feedback and preserve accurate workout state (no ghost completion).
- **SC-004**: Direct navigation to existing active workout detail route remains functional with no regression attributable to this feature.

## Assumptions

- The app continues to enforce at most one in-progress workout for a user at a time.
- Existing workout detail route and behavior remain the source of truth for detailed session interaction.
- Home already has access to active-workout state or can fetch it through existing application boundaries.
- Immediate completion from home intentionally skips confirmation in this release to minimize taps.
- If no active workout exists, home does not present completion controls.
