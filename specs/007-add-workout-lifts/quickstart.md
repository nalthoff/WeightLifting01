# Quickstart: Add lifts to in-progress workout

## Goal

Validate that a lifter can add one or more active lifts from the active workout screen and immediately see persisted workout-lift entries, including duplicate additions.

## Prerequisites

- Existing start-workout flow available and verified.
- Lift library includes at least one active lift.
- Backend and frontend dependencies installed.
- Local SQL-compatible database available for migration and test runs.

## 1) Apply persistence changes

1. Add migration(s) for workout-lift association persistence.
2. Apply migrations and confirm new workout-lift table(s) and required indexes/constraints exist.

## 2) Verify backend tests

1. Run unit tests for add-lift business rules (active-only validation, duplicate-allowed policy, ordering assignment).
2. Run integration tests for add-lift persistence and list behavior.
3. Run contract tests for list/add workout-lift API responses and failure payloads.

## 3) Verify frontend flow on mobile viewport

1. Start frontend and backend locally.
2. Open app on a phone-sized viewport.
3. Start or continue a workout and open the active workout screen.
4. Tap Add Lift and verify picker opens with active lifts only.
5. Add one lift and verify it appears immediately in workout flow.

## 4) Verify duplicate-allowed behavior

1. Add the same lift multiple times in the same workout.
2. Confirm each successful add appears as a separate entry and remains after refresh.

## 5) Verify edge and failure behavior

1. Ensure picker empty state appears clearly when no active lifts are available.
2. Simulate offline/timeout/server error while adding a lift.
3. Confirm clear failure feedback and no ghost workout-lift entry in UI.

## 6) Regression checks

1. Confirm existing lift library management in Settings remains unchanged.
2. Confirm start-workout and active-workout baseline behavior remains intact.
3. Confirm remove-lift behavior is not introduced in this slice.
