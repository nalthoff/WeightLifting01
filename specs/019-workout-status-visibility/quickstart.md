# Quickstart: Workout Lifecycle Status Visibility

## Goal

Verify that workout detail screens always show a clear status badge, completion stamps an end timestamp, and history/progress views include only completed workouts.

## Prerequisites

- Frontend and backend running locally.
- Test data includes at least:
  - one in-progress workout
  - one completed workout
- Mobile viewport available for manual UX verification.

## 1) Active workout badge

1. Start or open an in-progress workout.
2. Navigate to workout detail.
3. Confirm a visible status badge reads **In Progress**.

## 2) Completion transition behavior

1. From in-progress workout detail, complete the workout.
2. Confirm completion succeeds.
3. Confirm workout detail now indicates **Completed** status.
4. Confirm completed workout has a non-null completion timestamp in API response.

## 3) Completed history detail badge

1. Open a completed workout from history.
2. Confirm a visible status badge reads **Completed**.
3. Refresh and re-open the same workout to confirm badge stability.

## 4) History/progress gating

1. Ensure at least one workout remains in progress.
2. Open history/progress views.
3. Confirm in-progress workouts do not appear.
4. Confirm completed workouts appear as expected.

## 5) Failure safety check

1. Simulate or inject an unknown status payload in frontend test coverage.
2. Confirm UI avoids misleading status display and fails safely.

## Automated Coverage Targets

- Backend integration tests for completion timestamp and completed-only history filtering.
- Backend unit tests around lifecycle state validity and completion invariants.
- Frontend unit tests for active/completed badge rendering and transition refresh behavior.
- Frontend e2e tests for end-to-end detail visibility and history gating.
