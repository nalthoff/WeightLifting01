# Quickstart: Delete in-progress workout

## Goal

Validate that a user can permanently delete an in-progress workout from the active workout page with explicit confirmation and reliable success/failure feedback.

## Prerequisites

- Local frontend, backend, and SQL-compatible database are running.
- A user has one in-progress workout with at least one lift entry and one set.
- At least one completed workout exists to verify history behavior is unchanged.

## 1) Validate backend delete behavior

1. Run backend unit tests for in-progress-only deletion and outcome mapping (`Deleted`, `NotFound`, `Conflict`).
2. Run integration tests verifying successful deletion removes the workout aggregate and associated lift/set rows.
3. Run contract tests validating response shape and status behavior for 200/404/409.

## 2) Validate mobile confirmation and happy path

1. Open the active workout page at a phone-sized viewport.
2. Trigger Delete Workout.
3. Verify an explicit confirmation prompt appears before any mutation.
4. Confirm deletion.
5. Verify success feedback is shown and active workout state clears with guidance to return home/start new workout.

## 3) Validate confirmation cancel path

1. Trigger Delete Workout.
2. Cancel or dismiss the confirmation.
3. Verify workout remains in progress and all workout data is unchanged.

## 4) Validate failure and race handling

1. Simulate server/network failure during confirmed delete.
2. Verify clear failure feedback and no false-success state.
3. Simulate stale state where workout was already completed or removed.
4. Verify authoritative not-found/conflict feedback and state reconciliation.

## 5) Validate duplicate-submit protection

1. Confirm delete and rapidly tap confirm repeatedly.
2. Verify only one deletion request is processed and final UI state is consistent.

## 6) Regression checks

1. Verify completed workout history remains visible and ordered as before.
2. Verify deleted in-progress workout is absent from active workout summary and history reads.
3. Verify existing complete-workout flow still works for a newly started workout.
