# Quickstart: Backfill past workout

## Goal

Verify a user can record a previously completed workout on a chosen past day, with lift/set parity and correct history placement, even while another workout is active.

## Prerequisites

- Frontend and backend are running locally.
- At least one active in-progress workout exists.
- Workout history contains multiple completed sessions for ordering checks.
- Mobile-sized viewport is available for validation.

## 1) Create and complete a historical workout

1. From workout entry flow, choose the option to log a previous workout.
2. Enter a prior training date.
3. Enter start time (hour and minute).
4. Enter duration.
5. Save the workout as completed.
6. Confirm save feedback is clear and non-blocking.

## 2) Required timing fields behavior

1. Attempt historical creation with missing date, time, or duration.
2. Confirm save is blocked with clear guidance on missing required field(s).
3. Enter all required timing fields.
4. Confirm save succeeds and history rendering is sensible.

## 3) Lift and set parity

1. In historical flow, add multiple lifts and sets.
2. Complete the workout.
3. Open the saved workout from history.
4. Confirm lifts and sets appear consistently with other completed workouts.

## 4) Chronological ordering trust

1. Backfill workouts on different past date/times and at least two workouts on the same day.
2. Open workout history.
3. Confirm sessions appear in deterministic order aligned with selected training day intent.

## 5) Catch-up while active workout exists

1. Keep an active workout open.
2. Backfill and complete a historical workout.
3. Confirm the active workout remains in progress and accessible.
4. Confirm the historical workout appears in completed history immediately after refresh.

## 6) Minimal historical entry edge case

1. Create a historical workout with required date/time/duration and minimal lift/set data accepted by current rules.
2. Complete and open it from history.
3. Confirm it remains stable and viewable in history/detail surfaces.

## Validation Run Notes

Last validated: 2026-04-28

- Backend integration ordering regression:
  - `dotnet test backend/tests/WeightLifting.Api.IntegrationTests/WeightLifting.Api.IntegrationTests.csproj --filter "FullyQualifiedName~WorkoutHistoryOrderingTests"`
  - Result: PASS (1/1)
- Frontend required-field regression:
  - `npx ng test --watch=false --browsers=ChromeHeadless --include="src/app/features/workouts/historical-workout-form.component.spec.ts"`
  - Result: PASS (8/8)
- Backend historical lifecycle suites:
  - `HistoricalWorkoutLifecycleTests`, `HistoricalWorkoutDetailsTests`, `HistoricalAndActiveWorkoutCoexistenceTests`
  - Result: PASS in targeted runs during implementation.
