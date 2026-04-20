# Quickstart: Material Dark Theme

## Goal

Verify that the app shell and `Settings -> Lifts` use a consistent Material-styled dark theme that
matches the intended Iowa State-inspired palette while preserving the existing create-lift flow.

## Prerequisites

- Frontend application running locally
- Backend API running locally
- Mobile-sized browser viewport available for verification

## Verification Steps

1. Start the frontend and backend locally.
2. Open the app in a mobile-sized viewport.
3. Confirm the app shell/navigation uses a dark theme and feels visually consistent with the page
   content.
4. Navigate to `Settings -> Lifts`.
5. Confirm the page uses the same dark visual language as the app shell.
6. Confirm the form field, action button, card surfaces, and list styling are consistent with the
   shared theme.
7. Submit an empty or whitespace-only lift name and confirm the validation state remains readable
   and visually consistent.
8. Submit a valid lift name such as `Front Squat`.
9. Confirm the success state and updated list feel integrated into the same theme.
10. Confirm the page remains readable and usable without horizontal scrolling.
11. Confirm the default theme is dark and the current structure appears ready for a future light
    theme without exposing a user-facing theme toggle.

## Negative Checks

1. Confirm dark styling does not reduce readability for headings, body text, or action controls.
2. Confirm success and error states are distinguishable without relying only on color.
3. Confirm the update does not add any new workflow steps to create a lift.
4. Confirm the Iowa State-inspired cardinal and gold accents do not overpower the dark neutral
   surfaces.

## Automated Verification

- Frontend production build succeeds.
- Angular unit tests continue to pass.
- Existing end-to-end coverage remains aligned with the `Settings -> Lifts` flow.
