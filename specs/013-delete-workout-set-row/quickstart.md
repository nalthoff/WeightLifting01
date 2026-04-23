# Quickstart: Delete mistaken workout set rows

## Goal

Validate that lifters can delete mistakenly logged set rows from an in-progress workout with mandatory confirmation, clear cancel behavior, and trustworthy failure handling.

## Prerequisites

- Local frontend, backend, and SQL-compatible database are running.
- An in-progress workout exists with at least one lift entry and at least two set rows.
- A non-in-progress workout exists for eligibility verification.

## 1) Validate backend delete behavior

1. Run unit tests for delete rules: in-progress gating, workout/lift/set ownership checks, and delete outcome mapping.
2. Run integration tests to confirm successful deletion removes only the targeted row.
3. Run contract tests for delete endpoint success and failure response shapes (404/409/500).

## 2) Validate mobile confirmation happy path

1. Open the active workout screen at a phone-sized viewport.
2. Select delete on a specific set row.
3. Confirm the confirmation prompt appears before mutation.
4. Confirm deletion and verify only the selected row is removed in place.
5. Verify user remains on the active workout screen.

## 3) Validate cancel behavior

1. Start deleting a set row.
2. Cancel from confirmation.
3. Verify no set rows are removed and list order/content remain unchanged.

## 4) Validate failure and retry trust behavior

1. Simulate network/server failure for confirmed delete.
2. Confirm targeted row remains visible.
3. Confirm clear failure feedback is shown with retry path.
4. Retry deletion and confirm successful removal clears failure feedback.

## 5) Validate boundaries and isolation

1. Verify set-row delete controls are unavailable for non-in-progress workouts.
2. In a workout with duplicate lift entries, delete a set from one entry and confirm sets in other entries are unchanged.
3. Attempt repeated taps while deletion is pending and confirm duplicate submissions are prevented.

## 6) Regression checks

1. Confirm add-set behavior still works for the same workout.
2. Confirm edit-set behavior still works for remaining set rows.
3. Confirm remove-lift and reorder-lift flows remain functional.
4. Confirm active workout load/refresh behavior remains stable after deletion.
