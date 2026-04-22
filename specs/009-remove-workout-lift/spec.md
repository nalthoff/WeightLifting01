# Feature Specification: Remove lift from in-progress workout

**Feature Branch**: `[009-remove-workout-lift]`  
**Created**: 2026-04-22  
**Status**: Draft  
**Input**: User description: "As a lifter, I want to remove a lift from my current in-progress workout when I added it by mistake, so I can keep the workout list accurate during a live session."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Remove mistaken lift entry quickly (Priority: P1)

As a lifter during an active workout, I can remove a mistaken lift entry directly from the workout list so my session reflects what I actually performed.

**Why this priority**: Correcting mistakes quickly in-session is core to fast, trustworthy mobile workout logging.

**Independent Test**: Start an in-progress workout with at least one lift entry, remove one selected entry from the active workout screen, and verify the selected entry disappears immediately from that workout list.

**Acceptance Scenarios**:

1. **Given** I have an in-progress workout with lift entries, **When** I remove one selected lift entry from the active workout screen, **Then** that entry is removed from the current workout list immediately.
2. **Given** I remove a lift entry successfully, **When** I stay on the active workout screen, **Then** the updated list remains accurate without requiring manual refresh.

---

### User Story 2 - Remove only the selected duplicate instance (Priority: P2)

As a lifter, when the same lift appears multiple times in one workout, I can remove only the specific instance I selected so other intentional duplicates remain.

**Why this priority**: Duplicate entries are already supported, so precise entry-level removal is required to avoid accidental data loss.

**Independent Test**: Add the same lift multiple times in one in-progress workout, remove one chosen instance, and verify only that exact instance is removed while others remain.

**Acceptance Scenarios**:

1. **Given** a workout contains duplicate lift entries with the same lift identity, **When** I remove one chosen entry instance, **Then** only that specific entry is removed.
2. **Given** multiple duplicate entries remain after one removal, **When** I continue logging, **Then** the remaining entries keep their own identity and are still available.

---

### User Story 3 - Show clear outcomes for failed removals (Priority: P2)

As a lifter, I get clear feedback when a remove action fails so I do not mistake a failed request for a successful saved change.

**Why this priority**: During in-gym use, ambiguous state after failed actions undermines trust and can lead to incorrect logging decisions.

**Independent Test**: Trigger remove failures such as stale entry or workout-state conflict and verify explicit error feedback while the workout list remains accurate.

**Acceptance Scenarios**:

1. **Given** the selected entry no longer exists by the time removal is submitted, **When** the request returns a not-found outcome, **Then** I see clear recoverable feedback and no unintended entries disappear.
2. **Given** the workout is no longer in a removable state, **When** I submit remove, **Then** I see conflict feedback and the visible list remains consistent with saved state.
3. **Given** a connectivity or server failure occurs during removal, **When** remove fails, **Then** I see explicit failure feedback and no ghost removal is shown as saved.

---

### Edge Cases

- Selected entry becomes stale between render and remove attempt.
- Workout transitions out of in-progress while user is on the active workout screen.
- Duplicate entry list where adjacent items have identical names and only one should be removed.
- Rapid repeated taps on remove controls for the same entry.
- User leaves and returns to the screen after a failed remove; list must still reflect authoritative saved state.
- Conditional confirmation for entries with logged sets is deferred in this slice because set logging is not yet present.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow users to remove a lift entry from the active in-progress workout screen.
- **FR-002**: The remove action MUST target a specific workout-lift entry instance, not all entries sharing the same lift identity.
- **FR-003**: Successful removal MUST update the current workout list immediately in the active session flow.
- **FR-004**: Removing an entry MUST affect only the current workout session and MUST NOT alter lift definitions or entries in other workouts.
- **FR-005**: The system MUST support removal when duplicate entries exist and MUST preserve all non-selected duplicates.
- **FR-006**: When a remove request fails because the selected entry does not exist, the user MUST receive clear feedback and the visible list MUST remain accurate.
- **FR-007**: When a remove request fails due to workout-state conflict, the user MUST receive clear feedback and the visible list MUST remain accurate.
- **FR-008**: When a remove request fails due to connectivity or general server error, the user MUST receive clear feedback and no unsaved removal may appear as persisted.
- **FR-009**: The workflow MUST keep in-gym mobile logging fast, requiring minimal interaction for non-failure removal paths.
- **FR-010**: Confirmation before removal for entries with logged sets MUST be treated as a future-ready rule for this slice, because set logging is not yet available.
- **FR-011**: This feature MUST exclude library-level lift deletion/deactivation, set editing, completed-workout editing, and analytics expansion.
- **FR-012**: Any changed business rules for removal MUST be validated with automated tests in the same delivery slice.

### Key Entities *(include if feature involves data)*

- **Workout Lift Entry**: A single lift instance attached to one in-progress workout and represented as a removable list row.
- **Remove Lift Attempt**: A user-initiated request to remove one workout lift entry, resulting in either success or explicit failure.
- **Workout Session Scope**: The boundary that defines which workout data can be modified by this feature (current in-progress workout only).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile validation, users can remove a mistaken lift entry from the active workout flow in 2 taps or fewer for the non-failure path.
- **SC-002**: In validated success cases, 100% of successful removals remove only the selected workout lift entry instance.
- **SC-003**: In validated failure scenarios (stale entry, conflict, connectivity/server), 100% of cases provide explicit user feedback and show no ghost removal.
- **SC-004**: Existing add/list workout-lift behavior remains unaffected in regression validation after remove-lift delivery.

At least one success criterion SHOULD measure the speed or ease of the mobile logging flow,
and at least one SHOULD measure whether the user can choose the next weight from the
available workout history.

## Assumptions

- The product remains single-user in this slice, with one in-progress workout context at a time.
- Set logging is not yet implemented; therefore, conditional confirmation for entries with logged sets is deferred.
- Users perform remove actions from the active workout experience while the workout is still in progress.
- Existing workout list data remains the source of truth for visible entries in the session.
- Any changed business-layer logic for this feature ships with automated test coverage.
- The feature will be delivered within the existing mobile-first workout logging experience.
