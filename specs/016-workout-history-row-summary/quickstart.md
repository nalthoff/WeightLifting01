# Quickstart: Workout history row summary details

## Goal

Validate that Workout History keeps existing behavior and now shows label, completion date, duration (`HH:MM`), and lift count for each completed workout row.

## Prerequisites

- Backend and frontend are running locally.
- At least one completed workout exists.
- Mobile viewport testing is available (device or browser emulation).

## 1) Validate existing behavior is preserved

1. Navigate to Workout History from current app navigation.
2. Confirm only completed workouts are listed.
3. Confirm ordering is most recent completion first.

## 2) Validate row summary fields

1. Open Workout History with completed workouts available.
2. Confirm each row displays:
   - workout label (existing fallback behavior preserved)
   - completion date
   - duration in `HH:MM`
   - lift count (number of lifts, not sets)
3. Confirm a workout with zero lifts displays `0`.

## 3) Validate duration behavior and fallback

1. Validate a normal workout duration is calculated from start and completion timestamps.
2. Confirm formatted output always appears as `HH:MM`.
3. Simulate missing/invalid timestamp data and verify the row still renders with a safe fallback duration display.

## 4) Validate regression states

1. Confirm existing empty-history state still appears when no completed workouts exist.
2. Confirm existing load-error feedback still appears when history request fails.
3. Confirm page remains usable on mobile viewport widths.

## 5) Automated coverage targets

- Backend unit tests for duration formatting/fallback and lift-count derivation.
- Backend integration tests for completed-only filtering and recency ordering with new summary fields.
- Backend contract tests for updated `GET /api/workouts/history` response schema.
- Frontend unit tests for row field rendering and fallback handling.
- Frontend e2e tests for history row summaries and regression states.
