# Feature Specification: Add per-lift set logging

**Feature Branch**: `011-add-per-lift-sets`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "As a lifter using the active workout screen, I want to add set rows quickly under each lift so I can log completed work with minimal taps during a gym session."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add sets directly under a lift entry (Priority: P1)

As a lifter in an active workout, I can add a new set row from a specific workout-lift entry so I can log completed work quickly with minimal taps.

**Why this priority**: This is the core in-gym logging action and delivers immediate value during active sessions.

**Independent Test**: With an in-progress workout that has at least one lift entry and no sets yet, add a set from that entry and verify a new row appears only under that entry after save succeeds.

**Acceptance Scenarios**:

1. **Given** I am on the active workout page and a lift entry has no sets, **When** I tap Add Set for that entry and save succeeds, **Then** one new set row is shown under that same entry.
2. **Given** I add another set to the same lift entry, **When** save succeeds, **Then** the new row is appended with the next set number in sequence.

---

### User Story 2 - Keep duplicate lift entries independent (Priority: P2)

As a lifter, when the same lift appears multiple times in one workout, each workout-lift entry keeps its own set list so logging one entry does not change another.

**Why this priority**: Duplicate lifts are a normal workout pattern, and cross-entry leakage would break trust in logged data.

**Independent Test**: In a workout containing two entries for the same lift name, add sets to one entry and verify the other entry remains unchanged until explicitly updated.

**Acceptance Scenarios**:

1. **Given** two workout-lift entries reference the same lift, **When** I add a set to the first entry, **Then** only the first entry's set list changes.
2. **Given** each duplicate entry has sets, **When** I view set numbers, **Then** each entry shows its own sequential numbering starting from Set 1.

---

### User Story 3 - Show clear outcomes when add-set fails (Priority: P2)

As a lifter, I get explicit failure feedback when a set cannot be saved so I do not mistake an unsaved row for logged work.

**Why this priority**: Reliable and transparent failure handling is required in gym environments with variable connectivity.

**Independent Test**: Simulate add-set failures (offline, server error, conflict) and verify users receive explicit errors while no failed row is displayed as saved.

**Acceptance Scenarios**:

1. **Given** I attempt to add a set and persistence fails, **When** the request returns a failure, **Then** I receive clear error feedback and no ghost saved row is shown.
2. **Given** an add-set attempt fails, **When** I continue using the screen, **Then** visible set numbering and list state remain internally consistent for that entry.

---

### Edge Cases

- A newly created workout-lift entry starts with zero sets and does not auto-create Set 1.
- Add-set failures due to offline, server errors, or conflicts do not create ghost saved rows.
- Repeated taps during intermittent connectivity do not produce duplicate persisted rows for one intentional action.
- Optional weight remains blank without blocking set creation when reps are provided.
- A refresh or navigation away and back preserves all successfully saved sets and numbering.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The active workout view MUST display a repeating set list section within each workout-lift entry.
- **FR-002**: Users MUST be able to initiate Add Set from a specific workout-lift entry.
- **FR-003**: A successful add-set action MUST create exactly one new workout set entry tied to the selected workout-lift entry.
- **FR-004**: Set numbering MUST be assigned automatically per workout-lift entry and displayed in ascending order (Set 1, Set 2, Set 3, ...).
- **FR-005**: Each new set row MUST support reps input and optional weight input where weight may be blank.
- **FR-006**: Workout-lift entries in the same workout, including duplicates of the same lift, MUST maintain independent set lists and numbering sequences.
- **FR-007**: New workout-lift entries MUST begin with zero sets and MUST NOT auto-create a default set row.
- **FR-008**: New set rows MUST be persisted so successful rows remain present after refresh or navigation.
- **FR-009**: If add-set persistence fails for any reason, the system MUST present explicit failure feedback and MUST NOT present the failed row as saved.
- **FR-010**: After any failed add-set attempt, visible set list and numbering state for the affected workout-lift entry MUST remain consistent and trustworthy.
- **FR-011**: The primary add-set flow MUST support mobile-first, low-interaction use during in-progress workouts.
- **FR-012**: This slice MUST exclude advanced analytics, workout templates/programming workflows, completed-workout editing, broad workout history dashboards, and unrelated lift-library changes.
- **FR-013**: Business rules that govern workout set creation and validation MUST be enforced by backend business logic boundaries rather than UI presentation behavior.
- **FR-014**: Persisted data model updates required for workout set entries MUST include schema evolution steps and versioned migration artifacts.

### Key Entities *(include if feature involves data)*

- **Workout Lift Entry**: One lift instance in an in-progress workout that owns its own set list scope.
- **Workout Set Entry**: One logged set tied to exactly one workout-lift entry, including set number, reps, optional weight, and audit timestamps needed for lifecycle tracking.
- **Add Set Attempt**: A user-triggered action to create a workout set entry with an explicit result of either persisted success or explicit failure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile validation, users complete the primary add-set path for a focused lift entry in 3 interactions or fewer.
- **SC-002**: In validation scenarios, 100% of successful add-set actions remain visible after refresh or navigation away and back.
- **SC-003**: In validation scenarios with duplicate lifts in one workout, 100% of add-set actions affect only the targeted workout-lift entry.
- **SC-004**: In failure simulations (offline, server error, conflict), users receive explicit failure feedback for 100% of failed add-set attempts.
- **SC-005**: In failure simulations, 0 failed add-set attempts appear as persisted rows and set numbering remains sequential for all displayed saved rows.

## Assumptions

- The feature is used from the existing in-progress active workout experience by authenticated lifters.
- Reps are required for a valid set entry while weight may be omitted without blocking save.
- Existing workout session scope and permissions remain unchanged for this slice.
- The backend remains the authoritative source for persisted workout-set state.
- Any new or changed backend business logic for set creation/validation will be covered by automated tests in this delivery slice.
- This feature extends active-workout logging only and does not change completed-workout or analytics experiences.
