# Quickstart: Remove lift from in-progress workout

## Goal

Validate that a lifter can remove a mistaken lift entry from the active workout list, including duplicate-instance precision and clear failure outcomes, without affecting lift library data or other workouts.

## Prerequisites

- Existing start-workout and add-workout-lift flows are working locally.
- At least one in-progress workout exists with one or more workout-lift entries.
- Local backend, frontend, and SQL-compatible database are running.

## 1) Validate backend behavior

1. Run unit tests covering remove-workout-lift business rules.
2. Run integration tests for persistence removal behavior and workout-state conflicts.
3. Run contract tests for remove endpoint success and failure payloads.

## 2) Validate mobile remove happy path

1. Open app on a phone-sized viewport.
2. Start or continue a workout and ensure lift entries are visible.
3. Remove one selected lift entry.
4. Confirm the entry disappears immediately and remaining entries stay intact.

## 3) Validate duplicate-instance precision

1. Add the same lift multiple times in one workout.
2. Remove one specific duplicate entry.
3. Confirm only the selected instance is removed and other duplicates remain.

## 4) Validate failure handling

1. Simulate stale-entry removal (entry already removed elsewhere) and confirm recoverable feedback.
2. Simulate workout-state conflict (workout no longer in progress) and confirm conflict feedback.
3. Simulate connectivity/server failure and confirm explicit error with no ghost removal.

## 5) Regression checks

1. Confirm add/list workout-lifts flow remains unchanged.
2. Confirm lift library management (create/rename/deactivate) remains unchanged.
3. Confirm only current workout entries are affected; completed/past workout data is untouched.

## 6) Confirmation deferral check

1. Verify no blocking confirmation modal is required in this slice.
2. Record conditional confirmation as deferred behavior pending set-logging availability.

## Verification Run (2026-04-22)

- `dotnet test backend/tests/WeightLifting.Api.UnitTests/WeightLifting.Api.UnitTests.csproj --filter RemoveWorkoutLift` (Passed: 5)
- `dotnet test backend/tests/WeightLifting.Api.IntegrationTests/WeightLifting.Api.IntegrationTests.csproj --filter RemoveWorkoutLift` (Passed: 4)
- `dotnet test backend/tests/WeightLifting.Api.ContractTests/WeightLifting.Api.ContractTests.csproj --filter WorkoutLiftsApiContractTests` (Passed: 10)
- `npm run build` in `frontend` (Passed)
- `npm run e2e -- tests/e2e/workouts/remove-workout-lift.spec.ts tests/e2e/workouts/remove-workout-lift-duplicates.spec.ts tests/e2e/workouts/remove-workout-lift-failures.spec.ts` in `frontend` (Passed: 5)
