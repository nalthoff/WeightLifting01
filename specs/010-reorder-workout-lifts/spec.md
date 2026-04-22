# Feature Specification: Reorder workout lifts

**Feature Branch**: `010-reorder-workout-lifts`  
**Created**: 2026-04-22  
**Status**: Draft  
**Input**: User description: "As a user, I want to reorder lifts in my current in-progress workout so the list matches the sequence I am actually performing in the gym."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reorder current workout sequence quickly (Priority: P1)

As a lifter in an active workout, I can reorder lift entries so the workout list matches the sequence I am actually performing.

**Why this priority**: This is the primary in-gym correction flow and directly supports fast, trustworthy workout logging.

**Independent Test**: With an in-progress workout containing multiple lift entries, reorder one entry from its current spot to a new position and verify the on-screen order updates immediately and remains correct after refresh.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout with at least two lift entries, **When** I move one entry to a different position, **Then** the list immediately reflects the new order in the active workout screen.
2. **Given** I successfully reorder entries, **When** I stay in or return to the active workout view, **Then** the saved order remains consistent without manual correction.

---

### User Story 2 - Preserve entry identity while reordering duplicates (Priority: P2)

As a lifter, when the same lift appears multiple times in one workout, I can reorder specific instances without merging, deleting, or altering other entries.

**Why this priority**: Duplicate lift entries are allowed, so reorder behavior must remain precise at the entry-instance level.

**Independent Test**: In a workout containing duplicate lift names, reorder one selected duplicate and verify only its position changes while all entries remain present and distinct.

**Acceptance Scenarios**:

1. **Given** my workout includes duplicate lift entries, **When** I reorder one selected entry instance, **Then** only its placement changes and all duplicates remain intact.
2. **Given** I complete a reorder that includes duplicates, **When** I review the workout list, **Then** each entry remains uniquely represented with no accidental merge or removal.

---

### User Story 3 - Handle failed reorder attempts clearly (Priority: P2)

As a lifter, I receive explicit feedback when a reorder cannot be saved so I do not mistake an unsaved list order for a persisted one.

**Why this priority**: Clear failure outcomes are required for confidence during live workout logging, especially in unstable connectivity conditions.

**Independent Test**: Trigger failed reorder attempts (for example, stale state or network/server failure) and verify the user receives clear feedback and the visible order reconciles with authoritative saved state.

**Acceptance Scenarios**:

1. **Given** a reorder request cannot be persisted, **When** the save fails, **Then** I receive clear error feedback and the UI does not imply the failed order was saved.
2. **Given** workout state changes while I am reordering, **When** my reorder request conflicts with current state, **Then** I get clear conflict feedback and the list resolves to authoritative saved order.

---

### Edge Cases

- Reordering the first item to last position and last item to first position.
- Reordering in lists containing duplicate lift names where only one instance should move.
- Rapid repeated reorder actions before prior saves complete.
- Reorder attempt after the workout is no longer in progress.
- Connectivity interruption or server failure during reorder persistence.
- Stale client order compared to the latest saved server order.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow users to reorder lift entries within the active in-progress workout.
- **FR-002**: A successful reorder MUST persist the new entry order immediately.
- **FR-003**: The active workout list MUST reflect successful reorder results immediately without requiring manual refresh.
- **FR-004**: Reordering MUST operate on entry instances and MUST NOT merge, duplicate, remove, or otherwise mutate entry identity.
- **FR-005**: Reordering MUST support workouts that contain duplicate lift entries and preserve all non-moved entries.
- **FR-006**: Reordering MUST apply only to the current in-progress workout and MUST NOT modify completed or historical workouts.
- **FR-007**: If reorder persistence fails, the system MUST provide clear user feedback and MUST NOT present failed order changes as saved.
- **FR-008**: If reorder conflicts with current workout state, the system MUST provide clear conflict feedback and resolve the visible order to authoritative saved state.
- **FR-009**: The primary reorder flow MUST remain efficient for mobile in-gym logging with minimal interaction overhead.
- **FR-010**: This feature MUST exclude editing completed-workout order, modifying lift library data, and expanding scope into set/rep/weight logging or analytics features.
- **FR-011**: Any changed business rules related to reorder behavior MUST be covered by automated tests in the same delivery slice.

### Key Entities *(include if feature involves data)*

- **Workout Lift Entry**: A single lift instance in a workout list with stable identity and relative order among peer entries.
- **Workout Lift Order**: The ordered sequence of workout lift entry identifiers that defines display and logging sequence for one in-progress workout.
- **Reorder Attempt**: A user-initiated request to change entry sequence that results in either persisted success or explicit failure.
- **Workout Session Scope**: The boundary indicating reorder effects are limited to one active in-progress workout and never historical sessions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile validation, users can complete a successful reorder action in 3 interactions or fewer for the primary path.
- **SC-002**: In validation scenarios, 100% of successful reorder actions remain persisted after view refresh or return.
- **SC-003**: In validation scenarios, 100% of reorder actions on duplicate entries preserve all entry instances without unintended merge or deletion.
- **SC-004**: In failure scenarios (connectivity, stale state, conflict), users receive explicit feedback and zero failed reorder attempts appear persisted.
- **SC-005**: Regression validation confirms completed and historical workouts remain unchanged by current-workout reorder operations.

## Assumptions

- The product continues to operate in a single-user context for this feature.
- Reorder actions are performed from the active workout experience while the workout is still in progress.
- Existing workout entry identity is stable and can be used to reorder duplicates safely.
- The active workout data source remains authoritative for persisted order state.
- Any business-layer reorder logic introduced or changed will include automated test coverage.
- This feature focuses on list sequence management only and does not expand workout tracking scope.
