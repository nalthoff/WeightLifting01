# Quickstart: Home active workout summary and quick completion

## Goal

Validate that home shows the current in-progress workout summary and lets users continue or complete it directly, with clear success/failure feedback and no state ambiguity.

## Prerequisites

- Start-workout flow and active-workout route are already working.
- Backend and frontend services run locally.
- Test data includes one user with and without an in-progress workout.
- Mobile viewport emulation or physical phone is available.

## 1) Validate home summary card rendering

1. Open home with no in-progress workout and confirm no active-workout card appears.
2. Start or seed one in-progress workout.
3. Return to home and confirm card appears with:
   - workout label or fallback "Workout"
   - started time
   - Continue and Complete actions

## 2) Validate Continue behavior

1. From active-workout card, tap Continue.
2. Confirm navigation to the existing active workout detail route.
3. Confirm workout detail behavior remains unchanged.

## 3) Validate Complete from home behavior

1. From active-workout card, tap Complete once.
2. Confirm completion succeeds without confirmation prompt.
3. Confirm user remains on home.
4. Confirm active-workout card is removed.
5. Confirm success feedback is displayed.

## 4) Validate failure and race behavior

1. Simulate completion failure (offline/timeout/server error).
2. Tap Complete and confirm explicit error feedback.
3. Confirm workout is not shown as completed incorrectly and card state remains accurate.
4. Simulate stale/race state where workout is already completed elsewhere.
5. Confirm UI reconciles to authoritative state with recoverable messaging.

## 5) Validate rapid-tap protection

1. Tap Complete rapidly multiple times.
2. Confirm duplicate conflicting completion attempts are prevented.
3. Confirm final state is unambiguous (single completion outcome reflected).

## 6) Automated coverage targets

- Backend unit tests for completion lifecycle rules and invalid-state handling.
- Backend integration tests for active-summary retrieval and completion persistence.
- Backend contract tests for `/api/workouts/active` and `/api/workouts/{workoutId}/complete`.
- Frontend unit tests for home card rendering, continue navigation, and completion feedback states.
- Frontend e2e tests for mobile happy path and failure path.
