# Quickstart: Optional Workout Name

## Goal

Validate that workout naming remains optional, editable during active sessions, blocked after completion, and consistently rendered with history fallback behavior.

## Prerequisites

- Frontend and backend are running locally.
- Seed data includes at least:
  - one in-progress workout
  - one completed workout without stored name
  - one completed workout with stored name
- Mobile viewport test environment is available.

## 1) Validate add/edit/clear while in progress

1. Open an in-progress workout.
2. Enter a workout name and save.
3. Confirm updated name is shown in active workout summary.
4. Edit the name to a new value and save.
5. Confirm latest value is shown.
6. Clear the name (blank or whitespace-only input) and save.
7. Confirm workout remains in progress and is treated as unnamed.

## 2) Validate optional flow does not block completion

1. With an unnamed in-progress workout, complete the workout.
2. Confirm completion succeeds without requiring a name.
3. Repeat completion flow with a named workout to confirm both paths succeed.

## 3) Validate post-completion rename protection

1. Open a completed workout.
2. Attempt to change workout name using any available path.
3. Confirm mutation is rejected and completed workout remains unchanged.

## 4) Validate history fallback rendering

1. Navigate to workout history.
2. Locate completed workout with no stored name.
3. Confirm displayed label is `"Workout"`.
4. Locate completed workout with stored name and confirm named display is preserved.

## 5) Validate failure and validation outcomes

1. Submit an over-limit workout name value.
2. Confirm validation failure is shown and existing name value is unchanged.
3. Simulate network failure during active rename.
4. Confirm user sees a clear save failure state and no misleading success UI.

## 6) Automated coverage targets

- Backend unit tests for name normalization and lifecycle guard checks.
- Backend integration tests for in-progress rename success, completed rename conflict, and validation failure.
- Backend contract tests for request/response shape and status code behavior of name update endpoint.
- Frontend unit tests for active-workout naming state and post-completion read-only behavior.
- Frontend e2e tests for optional naming across add/edit/clear/complete/history fallback paths.

## Verification Notes

- 2026-04-24: Backend unit tests for rename handler passed (`5/5`) via `dotnet test --filter UpdateWorkoutLabelCommandHandlerTests`.
- 2026-04-24: Backend integration tests for rename lifecycle and completion compatibility passed (`3/3`) via `dotnet test --filter UpdateWorkoutLabelIntegrationTests`.
- 2026-04-24: Backend contract tests for workout label endpoint passed (`4/4`) via `dotnet test --filter WorkoutLabelApiContractTests`.
- 2026-04-24: Frontend Angular unit suite passed (`33/33`) via `npx ng test --watch=false --browsers=ChromeHeadless`.
- 2026-04-24: New Playwright rename scenarios passed (`3/3`) via targeted `npx playwright test` commands.
