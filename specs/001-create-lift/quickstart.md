# Quickstart: Create Lift

## Goal

Verify that a user can open `Settings -> Lifts`, create a lift with a valid name, and use it
in workout selection immediately after a confirmed save.

## Prerequisites

- Frontend application running locally
- Backend API running locally
- SQL database available with the latest migrations applied

## Verification Steps

1. Open the app in a mobile-sized viewport.
2. Navigate to `Settings`.
3. Open the `Lifts` page.
4. Confirm the page presents lift management rather than general settings content.
5. Enter a valid lift name such as `Front Squat`.
6. Submit the form.
7. Confirm the UI shows a successful result and the new lift appears on the `Settings -> Lifts`
   page without a manual reload.
8. Navigate to a workout flow that allows lift selection.
9. Confirm the newly created lift appears in the selectable lift list immediately.

## Negative Checks

1. Attempt to submit an empty value.
2. Attempt to submit only spaces.
3. Confirm the UI blocks submission and clearly states that a name is required.
4. Simulate or force a failed save attempt.
5. Confirm the UI states that the lift was not created.
6. Confirm the failed lift does not appear in the selectable workout lift list.

## Automated Coverage Targets

- Backend unit tests cover required-name and whitespace-only validation rules.
- Backend integration/contract tests cover `POST /api/lifts` and `GET /api/lifts`.
- Frontend tests cover page navigation, form validation, and shared-store updates.
- One end-to-end mobile-viewport test proves successful creation and immediate workout
  selection visibility.
