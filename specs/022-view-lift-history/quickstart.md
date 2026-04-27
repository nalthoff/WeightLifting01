# Quickstart: View lift history inline

## Goal

Verify lifters can open exact-lift recent history inline from active workout entry, see only the last three completed sessions, and continue logging without navigation interruptions.

## Prerequisites

- Frontend and backend are running locally.
- At least one in-progress workout exists with multiple lift entries.
- Historical completed workouts exist for at least one lift.
- Mobile-sized viewport is available for validation.

## 1) Open inline history in active workout entry

1. Open an in-progress workout in phone-sized viewport.
2. In a lift section, tap `View History`.
3. Confirm an inline expandable panel appears in that lift row.
4. Confirm the route/page does not change.

## 2) Exact-lift and completed-only filtering

1. Open inline history for Lift A.
2. Confirm all returned sessions belong to Lift A only.
3. Confirm sessions are completed instances only.

## 3) Last-three recency limit

1. Use a lift with more than three completed sessions.
2. Open `View History` for that lift.
3. Confirm only the three most recent completed sessions are shown.

## 4) Empty and partial history behavior

1. Open history for a lift with one or two completed sessions; confirm all available sessions are shown.
2. Open history for a lift with no completed sessions; confirm a clear inline empty state is shown.

## 5) Failure resilience and logging continuity

1. Simulate a history-load failure for one lift.
2. Confirm a clear inline error is shown in that lift section.
3. Confirm set entry and other lift interactions remain usable and no navigation occurs.
