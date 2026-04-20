# User Story 1 Manual Test Guide

## Goal

Verify that a user can create a lift from `Settings -> Lifts` and immediately see it in the lift list on that page.

## Preconditions

- Backend API is running.
- Frontend app is running.
- The database is reachable.

## Test Steps

1. Open the app and navigate to `Settings / Lifts`.
2. Confirm the page loads and shows the lift-management form.
3. Submit the form with an empty or whitespace-only lift name.
4. Confirm the page shows an error and does not report success.
5. Enter `Front Squat` as the lift name and submit the form.
6. Confirm the page shows a success message.
7. Confirm `Front Squat` appears in the current lift list without refreshing the page.

## Expected Results

- Blank names are rejected.
- Successful saves show a success message only after the API confirms the create request.
- The new lift appears in the shared list on the settings page.
