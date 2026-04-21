# Quickstart: Deactivate Lift

## Goal

Verify that a user can deactivate an existing lift from `Settings -> Lifts`, keep the record
persisted as inactive, and see that inactive lifts are hidden from default selection reads while
still viewable in management when the broader filter is enabled.

## Prerequisites

- Frontend application running locally
- Backend API running locally
- SQL database available with the latest migrations applied
- At least two active lifts available so list filtering and post-deactivate visibility can be
  checked

## Verification Steps

1. Open the app in a mobile-sized viewport.
2. Navigate to `Settings`.
3. Open the `Lifts` page.
4. Confirm the page initially shows only active lifts.
5. Choose one active lift to deactivate.
6. Confirm the UI presents a clear confirmation step using deactivate/inactive wording.
7. Confirm the deactivation action.
8. Confirm the UI shows a successful result.
9. Confirm the deactivated lift no longer appears in the default active-only list.
10. Change the list filter to include inactive lifts.
11. Confirm the same lift now appears in the list and is visibly inactive.
12. Open a later flow that reads the default lift list.
13. Confirm the deactivated lift is not shown in that default selection flow.

## Negative Checks

1. Start the deactivate flow for an active lift.
2. Cancel at the confirmation step.
3. Confirm the lift remains visible in the default active-only list.
4. Start the deactivate flow again and simulate or force a failed save.
5. Confirm the UI states that the deactivate action did not complete.
6. Confirm the lift still appears as active in the default list after the failed attempt.
7. Enable the filter to include inactive lifts when no inactive lifts exist.
8. Confirm the page remains understandable and does not imply lifts were deleted.

## Automated Coverage Targets

- Backend unit tests cover deactivate business rules and active/inactive state transitions.
- Backend integration and contract tests cover `PUT /api/lifts/{liftId}/deactivate` plus list
  filtering behavior for `activeOnly=true` and `activeOnly=false`.
- Frontend unit tests cover confirmation state, success/failure feedback, filter toggling, and
  shared-store reconciliation after confirmed deactivation.
- One mobile-viewport end-to-end test proves successful deactivation hides the lift from default
  views while a broader filter still reveals the inactive record.
