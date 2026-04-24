# Quickstart: Workout history detail flow

## Goal

Validate that users can open a completed workout from history and review read-only detail content (summary, lifts, and sets) without regressing existing history list behavior.

## Prerequisites

- Frontend and backend are running locally.
- Seed data includes at least:
  - one completed workout with lifts and sets
  - one completed workout with zero lifts
  - one lift with zero set rows
- Mobile viewport test environment is available.

## 1) Validate history-to-detail navigation

1. Navigate to Workout History.
2. Confirm completed workouts render newest-first.
3. Select a history row.
4. Confirm a completed-workout detail view opens for that exact workout.

## 2) Validate detail summary fields

1. In completed-workout detail, confirm visible summary includes:
   - completion date
   - duration
   - workout name/type when present (or label fallback behavior)
2. Confirm the view is read-only (no edit/delete controls for completed workout data).

## 3) Validate lifts and set rows

1. Confirm each lift renders with its associated set rows.
2. Confirm each set row shows recorded reps and weight.
3. For null/missing optional weight, confirm a clear fallback value is shown.
4. Confirm a lift with zero set rows still renders safely.
5. Confirm a workout with zero lifts shows a clear empty-lifts state.

## 4) Validate failure and recovery states

1. Simulate unavailable workout id during detail load and confirm clear not-found feedback.
2. Simulate network failure while opening detail and confirm explicit retry-capable error feedback.
3. Confirm users can navigate back to Workout History from error states.

## 5) Regression validation for history list behavior

1. Return to Workout History after viewing detail.
2. Re-validate completed-only filtering and newest-first ordering.
3. Re-validate existing history empty and load-error states remain unchanged.

## 6) Automated coverage targets

- Frontend unit tests for history row navigation affordance and detail rendering states.
- Frontend e2e tests for open-from-history flow and error recovery.
- Backend integration tests for workout + lift retrieval compatibility with completed detail flow.
- Backend contract tests for history, workout-by-id, and workout-lifts response shape expectations.

## Verification Notes

- 2026-04-24: Focused Angular history unit tests passed (`7/7`).
- 2026-04-24: Targeted backend integration tests for workout history/detail query behavior passed (`8/8`).
- 2026-04-24: Targeted backend contract tests for workouts and workout-lifts endpoints passed (`38/38`).
