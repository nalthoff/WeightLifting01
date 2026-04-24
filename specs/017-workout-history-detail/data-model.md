# Data Model: Workout history detail flow

## Entity: CompletedWorkoutHistoryItem

**Purpose**: History-list summary row for a completed workout and source of navigation into the detail view.

### Fields

- `workoutId` (UUID, required)
- `label` (string, required display value with existing fallback behavior)
- `completedAtUtc` (datetime UTC, required)
- `durationDisplay` (string, required)
- `liftCount` (integer, required, minimum `0`)

### Validation and Rules

- Includes completed workouts only.
- Ordered newest-first by completion timestamp in history list.
- Selecting a row routes to completed-workout detail for that `workoutId`.

## Entity: CompletedWorkoutDetail

**Purpose**: Read-only detail representation of a completed workout for post-session review.

### Fields

- `workoutId` (UUID, required)
- `label` (string, optional)
- `status` (enum, required; expected `Completed` in this flow)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, required for completed workouts)
- `durationDisplay` (string, required for display)
- `lifts` (array of `CompletedWorkoutLiftEntry`, required; may be empty)

### Validation and Rules

- Detail route is intended for completed workouts opened from history.
- Detail page is read-only; no mutation controls in scope.
- Missing optional display values (for example absent label) show safe fallback text.

## Entity: CompletedWorkoutLiftEntry

**Purpose**: A performed lift inside a completed workout detail view.

### Fields

- `workoutLiftEntryId` (UUID, required)
- `displayName` (string, required)
- `position` (integer, required)
- `sets` (array of `CompletedWorkoutSetRow`, required; may be empty)

### Validation and Rules

- Lifts render in stored order.
- Lift entries with zero sets still render as valid content.

## Entity: CompletedWorkoutSetRow

**Purpose**: Recorded set values for a lift in completed-workout detail.

### Fields

- `setId` (UUID, required)
- `setNumber` (integer, required, positive)
- `reps` (integer, required, positive)
- `weight` (decimal, optional)

### Validation and Rules

- Reps and weight values reflect recorded data exactly as persisted.
- Null or missing optional weight values render with a non-breaking fallback display.

## Entity: CompletedWorkoutDetailViewState

**Purpose**: UI state container for detail loading lifecycle.

### Fields

- `isLoading` (boolean, required)
- `loadError` (string, nullable)
- `detail` (`CompletedWorkoutDetail`, nullable)

### Validation and Rules

- While `isLoading=true`, a clear loading state is shown.
- Not-found and connectivity failures surface actionable error messaging.
- Error state includes a clear path back to Workout History.
