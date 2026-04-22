# Quickstart: Start blank workout session

## Goal

Validate that a user can start a blank workout from home, receive backend-authoritative session creation, and enter a minimal active-session experience with robust failure handling.

## Prerequisites

- Backend and frontend dependencies installed.
- Local database available for migrations/tests.
- Auth context available for a test user.

## 1) Apply database migration

1. Create and apply the workout-session migration in backend persistence.
2. Verify the workouts table exists with required fields including status and `startedAtUtc`.

## 2) Run backend tests

1. Run unit tests for workout start business rules.
2. Run integration tests for start endpoint success/failure/conflict paths.
3. Run contract tests to verify API response shapes for `201` and `409`.

## 3) Run frontend and verify mobile-first flow

1. Start frontend and backend locally.
2. Open app in a phone-sized viewport.
3. Confirm home page shows prominent Start Workout action.
4. Start workout with no label; verify transition into active-session screen.
5. Start workout with label; verify label displays in active-session screen.

## 4) Verify conflict handling

1. Ensure one in-progress workout exists for test user.
2. Trigger Start Workout again from home.
3. Confirm user sees continue-existing-session prompt and no duplicate in-progress session is created.

## 5) Verify failure handling (no ghost sessions)

1. Simulate offline or force backend failure.
2. Trigger Start Workout.
3. Confirm explicit error feedback appears and UI does not show an active session as saved.

## 6) Regression smoke checks

1. Confirm existing Settings -> Lifts flows remain reachable.
2. Confirm no set/lift logging screens were introduced in this slice.
