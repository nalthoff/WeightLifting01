# Quickstart: Reorder workout lifts

## Goal

Validate that a lifter can reorder lift entries in an active in-progress workout so sequence matches real execution order, with immediate persistence, duplicate-instance safety, and explicit no-ghost failure handling.

## Prerequisites

- Existing start-workout, add-lift, and list-lifts flows are working locally.
- At least one in-progress workout exists with at least two workout-lift entries.
- Local backend, frontend, and SQL-compatible database are running.

## 1) Validate backend behavior

1. Run unit tests covering reorder business rules (in-progress-only, complete ordered set validation, duplicate-id rejection, no partial save).
2. Run integration tests for persistence updates and position re-sequencing behavior.
3. Run contract tests for reorder endpoint success and failure payloads.

## 2) Validate mobile reorder happy path

1. Open app on a phone-sized viewport.
2. Start or continue an in-progress workout with multiple lift entries.
3. Reorder one lift entry to a new position.
4. Confirm list updates immediately in current view.
5. Refresh or return to the page and confirm order remains persisted.

## 3) Validate duplicate-instance behavior

1. Add the same lift multiple times so duplicates exist.
2. Reorder one duplicate instance between others.
3. Confirm all duplicate entries remain present and only ordering changes.

## 4) Validate failure handling

1. Simulate connectivity/server failure during reorder save.
2. Simulate stale/conflict reorder request (workout state changed or stale entry set).
3. Confirm explicit user feedback appears and failed order is not shown as persisted.
4. Confirm list reconciles to authoritative saved order after failure.

## 5) Regression checks

1. Confirm add/remove workout-lift flows remain unchanged.
2. Confirm completed and historical workouts are unaffected by current-workout reorder operations.
3. Confirm lift library management (create/rename/deactivate) remains unchanged.

## 6) Mobile speed check

1. Verify primary reorder flow completes in 3 interactions or fewer.
2. Confirm feedback and interaction affordances remain clear on smallest supported mobile viewport.
