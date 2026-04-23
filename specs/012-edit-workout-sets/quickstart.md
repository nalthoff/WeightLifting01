# Quickstart: Edit workout set entries

## Goal

Validate that lifters can correct existing set rows inline during an in-progress workout, save in place, and clearly distinguish saved versus unsaved edits when failures occur.

## Prerequisites

- Local frontend, backend, and SQL-compatible database are running.
- An in-progress workout exists with at least one lift entry and one existing set row.
- A non-in-progress workout exists for access-control verification.

## 1) Validate backend update behavior

1. Run unit tests for update rules: in-progress gating, reps/weight validation, immutable set-number enforcement, and entry ownership checks.
2. Run integration tests to confirm successful updates persist and are returned correctly for subsequent reads.
3. Run contract tests for update endpoint success and failure response shapes (404/409/422/500).

## 2) Validate mobile inline edit happy path

1. Open active workout screen at a phone-sized viewport.
2. Locate an existing set row and enter inline edit mode.
3. Change reps and/or weight and tap Save on that row.
4. Confirm the row updates immediately without leaving the workout screen.
5. Confirm set number remains unchanged.

## 3) Validate failure and retry trust behavior

1. Simulate network/server failure for a set-row save.
2. Confirm edited draft values remain visible in the row.
3. Confirm row-level error is shown and the row is clearly not saved.
4. Retry save from the same row and confirm successful transition back to persisted state.

## 4) Validate boundaries and isolation

1. Verify edit controls are unavailable for non-in-progress workouts.
2. In a workout with duplicate lift entries, edit a set in one entry and confirm no set rows in other entries are changed.
3. Confirm last-write-wins outcome by issuing near-concurrent updates and verifying latest successful write is reflected.

## 5) Regression checks

1. Confirm existing add-set behavior still works for the same workout.
2. Confirm existing remove-lift and reorder-lift flows remain functional.
3. Confirm active workout loading and refresh behaviors remain stable.

## Verification Run (2026-04-23)

- `dotnet test backend/tests/WeightLifting.Api.UnitTests/WeightLifting.Api.UnitTests.csproj --filter "FullyQualifiedName~UpdateWorkoutSetCommandHandlerTests" /p:UseSharedCompilation=false -m:1` (Passed: 5)
- `dotnet test backend/tests/WeightLifting.Api.IntegrationTests/WeightLifting.Api.IntegrationTests.csproj --filter "FullyQualifiedName~UpdateWorkoutSetIntegrationTests" /p:UseSharedCompilation=false -m:1` (Passed: 2)
- `dotnet test backend/tests/WeightLifting.Api.ContractTests/WeightLifting.Api.ContractTests.csproj --filter "FullyQualifiedName~WorkoutLiftsApiContractTests.PutWorkoutSet" /p:UseSharedCompilation=false -m:1` (Passed: 2)
- `npm run build` in `frontend` (Passed)
- `npx playwright test tests/e2e/workouts/edit-workout-set.spec.ts tests/e2e/workouts/edit-workout-set-failures.spec.ts tests/e2e/workouts/edit-workout-set-constraints.spec.ts tests/e2e/workouts/add-workout-set.spec.ts tests/e2e/workouts/remove-workout-lift.spec.ts tests/e2e/workouts/reorder-workout-lifts.spec.ts` in `frontend` (Passed: 7)
- Mobile viewport inline edit/save/retry usability validated via Playwright Mobile Chrome coverage in edit-set and regression suites.
