# Quickstart: Rename app to RackNote

## Goal

Verify users consistently see `RackNote` across in-app branding, browser metadata, and user-facing
documentation without any change to workout behavior.

## Prerequisites

- Frontend and backend can run locally.
- A browser is available with responsive viewport tools.

## 1) Verify app shell branding

1. Open the app root page on a mobile-sized viewport.
2. Confirm the visible brand label in the app shell shows `RackNote`.
3. Switch to a wider viewport.
4. Confirm the visible brand label still shows `RackNote`.

## 2) Verify browser/page metadata name

1. Open the app in a browser tab.
2. Confirm the tab title shows `RackNote`.
3. Refresh and navigate between primary routes.
4. Confirm the displayed app name remains `RackNote`.

## 3) Verify user-facing documentation references

1. Open top-level user-facing documentation.
2. Confirm product identity references use `RackNote`.
3. Confirm there are no conflicting user-facing mentions of `WeightLifting01` in scoped docs.

## 4) Regression checks for unchanged behavior

1. Start or continue a workout using existing flows.
2. Confirm logging and history behavior are unchanged.
3. Confirm no new transition labels such as "formerly WeightLifting01" are shown.
