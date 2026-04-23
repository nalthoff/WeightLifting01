# Quickstart: Workout history page and completion parity

## Goal

Validate that users can complete workouts from both home and active workout detail, and that completed workouts appear on a dedicated Workout History page with label and completed date.

## Prerequisites

- Backend and frontend apps run locally.
- A test user can start and complete workouts.
- Mobile viewport emulation (or physical device) is available.
- Database includes workouts table with lifecycle timestamps.

## 1) Validate completion from home

1. Start one workout and confirm it appears as active on home.
2. Tap `Complete Workout` on the home active-workout card.
3. Confirm success feedback appears and no active workout remains.
4. Confirm persisted workout status is `Completed` with non-null completion timestamp.

## 2) Validate completion from active workout detail

1. Start a new workout and navigate to active workout detail.
2. Tap `Complete Workout` in active workout detail.
3. Confirm success feedback/state update and that workout is no longer active.
4. Confirm persisted workout status is `Completed` with non-null completion timestamp.

## 3) Validate dedicated Workout History page

1. Navigate to Workout History route/page.
2. Confirm completed workouts are listed in most-recent-first order.
3. Confirm each row shows:
   - workout label (or `"Workout"` fallback)
   - completed date
4. Confirm no extra fields beyond this slice are required to complete history scanning.

## 4) Validate edge cases

1. Trigger completion failure (offline or forced server error) from each entry point.
2. Confirm clear failure feedback and no false-completed state.
3. Simulate stale/race completion (already completed elsewhere) and verify state reconciliation.
4. Verify empty-history state when no completed workouts exist.
5. Rapid-tap completion and confirm final state remains authoritative and unambiguous.

## 5) Automated coverage targets

- Backend unit tests for completion state transition and invalid-state rejection.
- Backend integration tests for persisted completion timestamp and history query behavior.
- Backend contract tests for `POST /api/workouts/{workoutId}/complete` and `GET /api/workouts/history`.
- Frontend unit tests for both completion entry points and history list/fallback rendering.
- Frontend e2e tests for end-to-end completion-to-history journey on mobile viewport.
