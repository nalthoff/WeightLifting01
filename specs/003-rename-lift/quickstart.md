# Quickstart: Rename Lift

## Goal

Verify that a user can open `Settings -> Lifts`, rename an existing lift with a valid replacement
name, and see that updated name reflected in later lift-selection reads after a confirmed save.

## Prerequisites

- Frontend application running locally
- Backend API running locally
- SQL database available with the latest migrations applied
- At least two existing lifts available for rename and duplicate-conflict checks

## Verification Steps

1. Open the app in a mobile-sized viewport.
2. Navigate to `Settings`.
3. Open the `Lifts` page.
4. Confirm the page shows the current list of existing lifts.
5. Select an existing lift to rename.
6. Confirm the current saved lift name is shown in the edit flow.
7. Enter a valid replacement name such as changing `Front Squat` to `Paused Front Squat`.
8. Save the rename.
9. Confirm the UI shows a successful result and the updated name appears on `Settings -> Lifts`
   without creating a second visible lift entry.
10. Open a later flow that reads from the current lift list.
11. Confirm that same lift is shown by its updated name.

## Negative Checks

1. Attempt to rename a lift to an empty value.
2. Attempt to rename a lift to only spaces.
3. Confirm the UI blocks the save and clearly states that a valid name is required.
4. Attempt to rename a lift to the name of another existing lift.
5. Confirm the UI clearly states that the name is already in use.
6. Confirm the original saved lift name remains the visible source of truth after the rejected
   attempt.
7. Simulate or force a failed save attempt for a valid rename.
8. Confirm the UI states that the rename did not complete.
9. Confirm later lift-list reads still show the previously saved name rather than the unsaved name.

## Automated Coverage Targets

- Backend unit tests cover trimmed required-name validation, unchanged-name handling, and normalized
  duplicate-name conflict detection.
- Backend integration and contract tests cover `PUT /api/lifts/{liftId}` plus list consistency
  after a successful rename.
- Frontend unit tests cover entering edit state, save-state feedback, and shared-store updates after
  confirmed rename responses.
- One mobile-viewport end-to-end test proves a successful rename is reflected in later lift-list
  reads without leaving stale or duplicate names visible.
