# Quickstart: Add per-lift set logging

## Goal

Validate that a lifter can quickly add persisted set rows under each workout-lift entry in the active workout screen, with independent duplicate-entry behavior, sequential numbering, and explicit no-ghost failure handling.

## Prerequisites

- Existing active workout flow is running locally.
- At least one in-progress workout exists with one or more workout-lift entries.
- Local backend, frontend, and SQL-compatible database are running.

## 1) Validate backend behavior

1. Run unit tests covering add-set rules (in-progress-only, entry ownership checks, sequential numbering, reps/weight validation).
2. Run integration tests for workout-set persistence, per-entry numbering sequence, and duplicate-lift-entry isolation.
3. Run contract tests for add-set endpoint success and failure payloads.

## 2) Validate mobile add-set happy path

1. Open app on a phone-sized viewport.
2. Continue an in-progress workout where one lift entry has zero sets.
3. Tap Add Set under that specific lift entry and provide reps (optionally weight).
4. Confirm the set row appears under that same entry with Set 1.
5. Add another set to the same entry and confirm Set 2 appears in order.
6. Refresh or navigate away/back and confirm saved rows remain visible.

## 3) Validate duplicate-lift entry independence

1. Ensure workout includes two entries with the same lift name.
2. Add a set under the first entry.
3. Confirm only the first entry's list changes.
4. Add a set under the second entry and confirm its numbering starts at Set 1 independently.

## 4) Validate failure handling and trust outcomes

1. Simulate connectivity/server/conflict failure during Add Set.
2. Confirm explicit failure feedback is shown to the user.
3. Confirm no failed attempt appears as a persisted set row.
4. Confirm visible numbering and list state remain consistent after failure.

## 5) Regression checks

1. Confirm existing add/remove/reorder workout-lift behaviors still work.
2. Confirm completed-workout views remain unchanged by this feature.
3. Confirm lift-library management flows remain unchanged.

## 6) Mobile speed check

1. Verify the primary add-set path can be completed in 3 interactions or fewer after focusing a lift entry.
2. Verify controls and feedback remain clear on the smallest supported mobile viewport.

## Verification Run (2026-04-23)

- `dotnet test backend/tests/WeightLifting.Api.UnitTests/WeightLifting.Api.UnitTests.csproj --filter AddWorkoutSet /p:UseSharedCompilation=false -m:1` (Passed: 5)
- `dotnet test backend/tests/WeightLifting.Api.IntegrationTests/WeightLifting.Api.IntegrationTests.csproj --filter AddWorkoutSet /p:UseSharedCompilation=false -m:1` (Passed: 2)
- `dotnet test backend/tests/WeightLifting.Api.ContractTests/WeightLifting.Api.ContractTests.csproj --filter "PostWorkoutSet|AddWorkoutSet" /p:UseSharedCompilation=false -m:1` (Passed: 3)
- `npm run build` in `frontend` (Passed)
- `npx playwright test tests/e2e/workouts/add-workout-set.spec.ts tests/e2e/workouts/add-workout-set-duplicates.spec.ts tests/e2e/workouts/add-workout-set-failures.spec.ts` in `frontend` (Passed: 5)
- `npx playwright test tests/e2e/workouts/add-workout-lift.spec.ts tests/e2e/workouts/remove-workout-lift.spec.ts tests/e2e/workouts/reorder-workout-lifts.spec.ts` in `frontend` (Passed: 3)
