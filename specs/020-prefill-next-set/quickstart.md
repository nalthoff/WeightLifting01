# Quickstart: Prefill next set defaults

## Goal

Verify that repeated set logging becomes faster by automatically carrying forward the last successfully logged reps/weight for the same lift entry, while preserving user input when saves fail.

## Prerequisites

- Frontend and backend are running locally.
- An in-progress workout exists with at least two lift entries.
- Mobile-sized viewport is available for manual verification.

## 1) Happy path prefill

1. Open an in-progress workout on a phone-sized viewport.
2. In one lift entry, enter reps and weight, then tap `+`.
3. Confirm the set is saved and listed.
4. Confirm the same lift entry input now shows the same reps and weight as defaults.
5. Tap `+` again without editing and confirm second set logs successfully.

## 2) Entry isolation

1. With two lift entries visible, add a set under Entry A.
2. Confirm only Entry A defaults update.
3. Confirm Entry B input values remain unchanged.

## 3) Blank weight behavior

1. Enter reps and clear weight for one lift entry.
2. Save the set.
3. Confirm next default keeps reps and leaves weight blank.

## 4) Failure behavior

1. Enter reps/weight for a lift entry.
2. Simulate an add-set failure (validation/state/conflict/network path).
3. Confirm current draft values remain unchanged after failure.
4. Correct/retry and confirm successful save updates next defaults.

## 5) Regression checks

1. Confirm existing add-set validation messages still appear when inputs are invalid.
2. Confirm set editing/deleting behavior remains unchanged.
3. Confirm no cross-lift value leakage in repeated usage.
