# Feature Specification: View Lift History Inline

**Feature Branch**: `022-view-lift-history`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Build a mobile-first workout entry enhancement so lifters can view recent same-lift performance without leaving the active entry screen."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Expand same-lift history inline during entry (Priority: P1)

As a lifter entering sets in an active workout, I want a View History action on each lift so I can open recent same-lift session history inline and decide my next working weight without leaving workout entry.

**Why this priority**: This is the primary in-gym decision moment where users need quick context while maintaining logging speed.

**Independent Test**: Open an in-progress workout with multiple lifts, select View History for one lift, and verify history expands inline for that lift without leaving the active entry screen.

**Acceptance Scenarios**:

1. **Given** an active workout entry screen with at least one lift, **When** the user selects View History on a lift, **Then** an expandable inline history panel opens in that lift section.
2. **Given** an inline history panel is opened for a lift, **When** the panel renders, **Then** the user remains on the active workout entry screen without route or page change.
3. **Given** multiple lifts are displayed, **When** the user opens history on one lift, **Then** the shown history is scoped only to that selected lift.

---

### User Story 2 - See only the most recent completed same-lift sessions (Priority: P1)

As a lifter making a next-weight decision, I want inline history to show only the last three completed sessions for the exact lift so I get fast, relevant context without excess information.

**Why this priority**: The core value depends on high-signal recency and exact-lift relevance; broader history would add noise and slow decisions.

**Independent Test**: For a lift with more than three completed sessions, open View History and verify exactly three most recent completed sessions appear for that exact lift.

**Acceptance Scenarios**:

1. **Given** more than three completed sessions exist for a lift, **When** the user opens View History, **Then** only the three most recent completed sessions for that exact lift are shown.
2. **Given** fewer than three completed sessions exist for a lift, **When** the user opens View History, **Then** all available completed sessions for that exact lift are shown.
3. **Given** no completed sessions exist for a lift, **When** the user opens View History, **Then** a clear inline empty-history message is shown.

---

### User Story 3 - Maintain confidence during loading and failure states (Priority: P2)

As a lifter logging in a real gym environment, I want inline history loading and failure states to be clear and non-disruptive so I can continue workout entry even when history is temporarily unavailable.

**Why this priority**: Connectivity variability is common during training, and unclear failure behavior can interrupt the primary logging flow.

**Independent Test**: Trigger a history load failure while on active workout entry and verify an inline error state is shown without forcing navigation away from workout entry.

**Acceptance Scenarios**:

1. **Given** inline history is requested, **When** history is loading, **Then** the lift section shows a visible inline loading state.
2. **Given** history cannot be loaded, **When** the request fails, **Then** the lift section shows clear inline error feedback while keeping workout entry usable.
3. **Given** one lift history request fails, **When** the user continues interacting with the workout entry screen, **Then** other lift entry actions remain available.

### Edge Cases

- A lift has exactly one or two completed sessions; the panel shows only those sessions without placeholders for missing rows.
- A user opens history for one lift and then for another lift in the same workout; each lift preserves correct same-lift scoping and does not mix history rows.
- A completed workout contains the same lift name variant issue (for example, renamed or similarly named lifts); only entries tied to the exact lift identity are shown.
- Inline history fails to load due to temporary connectivity loss; the user remains in active workout entry and can continue logging sets.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a View History action for each lift shown in the active workout entry screen.
- **FR-002**: The system MUST open lift history inside an expandable inline panel within the selected lift section.
- **FR-003**: The system MUST keep users on the active workout entry screen when opening or viewing inline history.
- **FR-004**: The system MUST show history only for the exact selected lift identity.
- **FR-005**: The system MUST limit inline history to completed workout instances only.
- **FR-006**: The system MUST display at most the three most recent completed workout instances for the selected exact lift.
- **FR-007**: The system MUST show all available completed instances when fewer than three exist.
- **FR-008**: The system MUST show a clear inline empty-history state when no completed history exists for the selected exact lift.
- **FR-009**: The system MUST provide clear inline load-failure feedback if history retrieval fails, without interrupting active workout entry usage.
- **FR-010**: The feature MUST preserve existing active workout entry behavior for set logging and lift management outside the inline history interaction.
- **FR-011**: The inline history interaction MUST support fast mobile-first workout entry usage and avoid introducing additional navigation steps.
- **FR-012**: The feature MUST exclude analytics dashboards, charting, cross-lift comparison, and standalone history-page navigation from this scope.

### Key Entities *(include if feature involves data)*

- **Active Workout Lift Entry**: A lift row within an in-progress workout where users log sets and can trigger inline history.
- **Lift Session History Snapshot**: A user-visible record of a completed workout instance for one exact lift, shown in recency order.
- **Inline History Panel State**: The per-lift panel state that governs collapsed, loading, loaded, empty, and error display conditions during workout entry.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation sessions, users can open same-lift inline history from an active workout lift in one action.
- **SC-002**: In validation scenarios, 100% of inline history items shown are for completed sessions of the exact selected lift.
- **SC-003**: In validation scenarios, 100% of inline history panels display no more than three completed workout instances.
- **SC-004**: In mobile-view validation, users remain on the active workout entry screen for 100% of inline history interactions.
- **SC-005**: In connectivity-failure validation, 100% of failed history loads display actionable inline feedback while preserving workout entry usability.

## Assumptions

- The active workout entry screen already has stable per-lift sections where an additional View History action can be placed.
- Exact-lift matching follows existing product lift identity rules and does not rely on loose text matching.
- Completed workout instances already exist as the trusted source for historical performance display.
- The inline history panel is informational only and does not introduce edit actions for historical data.
- Any business-layer logic introduced or changed for selecting eligible history will ship with automated unit tests.
- The feature is delivered within the existing product platform constraints and deployment expectations.
