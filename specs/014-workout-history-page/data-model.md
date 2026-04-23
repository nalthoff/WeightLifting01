# Data Model: Workout history page and completion parity

## Entity: WorkoutSession

**Purpose**: Authoritative persisted workout record that transitions from active logging to completed history.

### Fields

- `id` (UUID, required)
- `status` (enum, required: `InProgress` or `Completed`)
- `label` (string, nullable)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, nullable until completion)
- `updatedAtUtc` (datetime UTC, required)

### Validation and Rules

- Completion is valid only when current status is `InProgress`.
- A `Completed` workout must have a non-null `completedAtUtc`.
- `completedAtUtc` cannot be earlier than `startedAtUtc`.
- Completed workouts are excluded from active-workout surfaces.

## Entity: CompletedWorkoutHistoryItem

**Purpose**: Minimal user-facing projection for Workout History page list rows.

### Fields

- `workoutId` (UUID, required)
- `label` (string, nullable in source)
- `displayLabel` (string, required; derived fallback to `"Workout"` when `label` is null/empty)
- `completedAtUtc` (datetime UTC, required)
- `completedDate` (localized display date, derived from `completedAtUtc`)

### Validation and Display Rules

- `displayLabel` must never be blank in rendered history.
- Items without `completedAtUtc` are not included in completed history.
- List ordering defaults to `completedAtUtc` descending.

## Entity: WorkoutHistoryList

**Purpose**: API and UI container for pageless history list in this slice.

### Fields

- `items` (array of `CompletedWorkoutHistoryItem`, required)
- `isEmpty` (boolean, derived in UI when `items` length is zero)

### Behavior Rules

- Empty list state is explicit and user-guiding.
- Newly completed workouts appear in list after successful completion persistence and refresh.

## Entity: CompletionAttempt

**Purpose**: User action model for completing from home or active workout detail.

### Fields

- `workoutId` (UUID, required)
- `source` (enum: `home` or `active_workout_detail`)
- `requestedAtUtc` (datetime UTC, required)
- `outcome` (enum: `completed`, `not_found`, `conflict`, `failed`)
- `feedbackMessage` (string, required for non-success outcomes)

### Behavior Rules

- Duplicate rapid attempts should converge to one authoritative persisted outcome.
- Failure outcomes must not mark workout as completed in UI.
- Stale/race outcomes trigger state reconciliation before final render.

## State Transitions

- `InProgress -> Completed`: occurs on successful completion command.
- `InProgress -> InProgress`: retained when completion fails or is rejected.
- `Completed -> Completed`: idempotent read state; additional completion attempts return conflict/not-found behavior based on API rules.
