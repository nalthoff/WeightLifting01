# Data Model: Workout history row summary details

## Entity: WorkoutSession

**Purpose**: Persisted workout aggregate that provides source timestamps and associated lifts for completed-history summaries.

### Fields

- `id` (UUID, required)
- `status` (enum, required: `InProgress` | `Completed`)
- `label` (string, nullable)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, nullable until completion)

### Validation and Rules

- Only `Completed` workouts with non-null `completedAtUtc` are eligible for history list output.
- `completedAtUtc` earlier than `startedAtUtc` is treated as invalid for duration calculation and triggers fallback duration display.

## Entity: CompletedWorkoutHistoryItem

**Purpose**: API/UI row projection used by Workout History page.

### Fields

- `workoutId` (UUID, required)
- `label` (string, required display value; source label with existing fallback behavior)
- `completedAtUtc` (datetime UTC, required)
- `durationDisplay` (string, required; formatted `HH:MM`)
- `liftCount` (integer, required; count of workout lifts, minimum `0`)

### Validation and Rules

- `durationDisplay` is derived from `completedAtUtc - startedAtUtc` when both timestamps are valid.
- Invalid or missing timestamp inputs must map to a safe fallback `durationDisplay` value and not remove the row.
- `liftCount` counts workout lifts (exercises), never set rows.
- `liftCount` must return `0` for workouts with no lifts.

## Entity: WorkoutHistoryResponse

**Purpose**: API container for completed workout list displayed on history page.

### Fields

- `items` (array of `CompletedWorkoutHistoryItem`, required)

### Behavior Rules

- `items` are ordered by `completedAtUtc` descending (most recent first).
- Response remains compatible with existing history route/page flow while carrying additional row summary fields.
