# Feature Specification: Delete mistaken workout set rows

**Feature Branch**: `[013-delete-workout-set-row]`  
**Created**: 2026-04-23  
**Status**: Draft  
**Input**: User description: "Feature: Delete mistaken workout set row in active workout"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Delete a mistaken set row with confirmation (Priority: P1)

As a lifter in an active workout, I can remove a mistakenly logged set row only after explicitly confirming the action so my workout history remains accurate.

**Why this priority**: Correcting mistaken set records in the moment is core to reliable in-gym logging and next-weight decisions.

**Independent Test**: In an in-progress workout with at least one logged set row, attempt to delete a row, confirm deletion, and verify only that row is removed without leaving the workout screen.

**Acceptance Scenarios**:

1. **Given** I am on the active workout screen with existing set rows, **When** I choose delete for a specific set row, **Then** the system asks me to confirm before any deletion occurs.
2. **Given** I am asked to confirm deleting a set row, **When** I confirm, **Then** only the targeted set row is removed and the updated list remains on the same screen.

---

### User Story 2 - Keep data unchanged when deletion is canceled (Priority: P2)

As a lifter, I can cancel the delete confirmation so I do not lose workout data by accident.

**Why this priority**: Accidental deletion risk is high during fast mobile logging; cancel behavior protects record integrity.

**Independent Test**: Start deleting a set row, cancel at confirmation, and verify the row and surrounding list remain unchanged.

**Acceptance Scenarios**:

1. **Given** a set-row delete confirmation is shown, **When** I cancel, **Then** no set rows are removed and the workout list is unchanged.

---

### User Story 3 - Handle deletion failures clearly (Priority: P2)

As a lifter, I receive clear feedback when a set-row deletion cannot be completed so I know my data is still present and can retry.

**Why this priority**: Gym connectivity and state conflicts are common, and users need clear save-state trust.

**Independent Test**: Trigger a failed delete attempt and verify the target row remains visible with actionable feedback for retry.

**Acceptance Scenarios**:

1. **Given** I confirm deleting a set row, **When** the deletion fails, **Then** the row remains visible and I see clear failure feedback.
2. **Given** a deletion failed, **When** I retry and it succeeds, **Then** the targeted row is removed and failure feedback clears.

---

### Edge Cases

- User initiates delete on one row while another row action is in progress.
- The targeted set row was already removed elsewhere before confirmation is submitted.
- The workout status changes from in progress before the delete request completes.
- Duplicate lift entries exist in the same workout; deletion must affect only the selected row in the selected entry.
- Slow or unstable connectivity causes delayed responses; user must not see duplicate or ambiguous deletion outcomes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST be able to start deleting an existing set row from the active workout screen when the workout is in progress.
- **FR-002**: The system MUST require explicit confirmation before deleting a set row.
- **FR-003**: The system MUST perform no deletion when the user cancels confirmation.
- **FR-004**: On successful confirmed deletion, the targeted set row MUST be removed from the visible list without leaving the active workout screen.
- **FR-005**: Deletion MUST be scoped to the targeted set row and workout lift entry, with no impact to other rows or entries.
- **FR-006**: Set-row deletion controls MUST be unavailable when the workout is not in progress.
- **FR-007**: If deletion fails, the targeted row MUST remain visible and unchanged.
- **FR-008**: Failed deletions MUST show clear user-facing feedback with an explicit retry path.
- **FR-009**: During a pending delete operation, the system MUST prevent accidental duplicate delete submissions for the same row.
- **FR-010**: Existing add-set, edit-set, reorder-lift, and remove-lift flows MUST continue working without behavioral regression.

### Key Entities *(include if feature involves data)*

- **Workout Set Row**: A persisted set record within a specific workout lift entry that includes row identity, set number, reps, and optional weight.
- **Delete Confirmation State**: Temporary user decision state that captures whether deletion is confirmed or canceled before mutation occurs.
- **Workout Lift Entry Context**: The parent workout lift entry scope that ensures set-row deletion targets the correct row instance.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In mobile verification, 100% of set-row deletion attempts require explicit confirmation before data changes.
- **SC-002**: 100% of confirmed successful deletions remove only the targeted set row and keep users on the active workout screen.
- **SC-003**: 100% of canceled confirmations result in no data change.
- **SC-004**: 100% of failed deletions preserve the row and show clear actionable feedback.
- **SC-005**: No regressions are found in add-set, edit-set, reorder-lift, and remove-lift verification for this release.

## Assumptions

- This feature applies to deleting already logged set rows only, not unsaved set drafts.
- The same lifter permissions that allow set editing in an active workout also allow set deletion.
- Confirmation is a mandatory safety step and there is no undo requirement beyond cancel-before-confirm.
- The feature remains focused on workout logging accuracy and does not expand analytics/reporting scope.
- Any business-layer validation changes required for delete eligibility will be tested in the same delivery cycle.
