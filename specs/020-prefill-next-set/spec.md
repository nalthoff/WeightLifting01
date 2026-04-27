# Feature Specification: Prefill Next Set

**Feature Branch**: `020-prefill-next-set`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "Add a mobile-first workout logging improvement: when a lifter records a set for a specific lift entry by tapping the '+' add-set action, the next set input for that same lift entry should automatically prefill with the just-logged values so logging repeated working sets is faster and requires fewer taps."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Faster Repeated Set Logging (Priority: P1)

As a lifter logging an in-progress workout on my phone, when I record a set for a lift entry, I want the next set form for that same lift entry to already contain the same weight and reps so I can keep moving with minimal typing.

**Why this priority**: Repeated set logging is a core in-gym action, and reducing taps in this moment directly improves workout flow and speed.

**Independent Test**: Open an in-progress workout, add a set with weight and reps for one lift entry, and confirm the same lift entry's next set form immediately shows those same values as defaults.

**Acceptance Scenarios**:

1. **Given** an in-progress workout with at least one lift entry, **When** the user adds a set with valid weight and reps, **Then** the next set form for that same lift entry is prefilled with the newly logged weight and reps.
2. **Given** multiple lift entries in the same workout, **When** the user adds a set to one lift entry, **Then** only that lift entry receives updated prefilled defaults and other lift entries remain unchanged.

---

### User Story 2 - Reliable Prefill During Logging Errors (Priority: P2)

As a lifter entering sets quickly, I want my current input to remain stable if saving a set fails so I can retry without re-entering values or seeing confusing changes.

**Why this priority**: Clear, predictable behavior during save failures prevents interruptions and avoids accidental loss of effort in the primary logging journey.

**Independent Test**: Attempt to add a set while simulating a save failure, and confirm the current form values remain intact with no false prefill update.

**Acceptance Scenarios**:

1. **Given** a user enters weight and reps and the add-set attempt fails, **When** the error is shown, **Then** the same input values remain in place and no new prefill values are applied.
2. **Given** a failed add-set attempt followed by a successful retry, **When** the set is saved, **Then** the next set form updates to reflect the successfully logged values.

### Edge Cases

- If a user logs a set with reps but intentionally leaves weight blank, the next set form keeps weight blank and still prefills reps.
- If a user changes weight or reps before submitting the next set, the system respects the edited values and treats them as the source for the subsequent prefill after a successful save.
- If connectivity is unstable and multiple save attempts occur, only successfully logged sets update future prefilled defaults.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST prefill the next set form for a lift entry after a successful add-set action using the weight and reps from the just-logged set.
- **FR-002**: The system MUST apply prefill behavior independently per lift entry so values do not carry across different lift entries.
- **FR-003**: The system MUST preserve intentionally blank weight values during prefill while still prefilling reps from the just-logged set.
- **FR-004**: The system MUST keep the user-entered draft unchanged when an add-set action fails.
- **FR-005**: The system MUST update prefill defaults only after a successful set save.
- **FR-006**: The system MUST keep this behavior available in the mobile-first active workout logging flow where repeated set entry occurs.
- **FR-007**: Users MUST be able to continue manually editing prefilled values before submitting each new set.

### Key Entities *(include if feature involves data)*

- **Set Entry Draft**: The current weight and reps input state for the next set of a specific lift entry.
- **Logged Set**: The most recently saved set for a specific lift entry, used as the source for next-set defaults.
- **Lift Entry Logging Context**: The per-lift boundary that ensures prefilled values remain scoped to the selected lift entry.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In validation sessions, users can log three consecutive repeated sets for a single lift entry with at least 30% fewer manual field edits than before this feature.
- **SC-002**: In validation sessions, at least 90% of repeated-set logging attempts for the same lift entry are completed without users retyping both fields.
- **SC-003**: In validation sessions, 100% of failed add-set attempts preserve the current draft inputs without unexpected value changes.
- **SC-004**: In mobile viewport verification, users can complete the repeated-set logging flow for a lift entry in under 20 seconds for three consecutive sets.

## Assumptions

- The feature applies to in-progress workouts where set entry is currently allowed.
- The product continues to treat weight as optional and reps as required in the add-set flow.
- Users may override any prefilled value before saving the next set.
- Existing workout lifecycle and history behavior remain unchanged outside this prefill interaction.
- No additional recommendation or progression guidance is introduced as part of this feature.
