# Data Model: Delete mistaken workout set rows

## Entity: WorkoutSession

**Purpose**: Existing workout aggregate that determines whether set rows are eligible for deletion.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress` and `Completed`)

### Relevant Rules

- Set-row deletion is allowed only while `status` is `InProgress`.
- Non-in-progress workouts reject deletion and do not mutate persisted set data.

## Entity: WorkoutLiftEntry

**Purpose**: Parent scope that owns set rows and ensures deletion targets the intended entry when duplicate lifts exist.

### Relevant Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `liftId` (UUID, required)
- `position` (integer, required)

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `WorkoutLiftEntry` has many `WorkoutSetEntry` records.

## Entity: WorkoutSetEntry

**Purpose**: Persisted set row eligible for targeted deletion during an active workout.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setNumber` (integer, required)
- `reps` (integer, required)
- `weight` (decimal, optional)
- `createdAtUtc` (datetime UTC, required)
- `updatedAtUtc` (datetime UTC, required)

### Validation Rules

- `workoutLiftEntryId` must belong to the requested `workoutId`.
- The row must exist and be associated with the requested workout lift entry.
- Row deletion is rejected if workout state is not in progress.

### State Transitions

- `Persisted` -> `PendingDeleteConfirmation` (UI-only state before mutation)
- `PendingDeleteConfirmation` -> `Persisted` (user cancels confirmation)
- `PendingDeleteConfirmation` -> `Deleting` (user confirms delete and request starts)
- `Deleting` -> `Removed` (delete succeeds; row no longer returned in list)
- `Deleting` -> `PersistedWithDeleteError` (delete fails; row remains visible with error feedback)

## Entity: SetRowDeleteSession

**Purpose**: Per-row transient UI/store state for confirmation, in-flight deletion, and failure feedback.

### Fields

- `setId` (UUID, required)
- `isConfirmingDelete` (boolean, required)
- `isDeleting` (boolean, required)
- `errorMessage` (string | null, optional)

### Rules

- Delete confirmation must be explicit before a request can be sent.
- Duplicate delete submits for the same row are blocked while `isDeleting` is true.
- Errors are scoped to one row and do not block other non-conflicting row actions.

## Entity: DeleteSetAttempt

**Purpose**: A single user-confirmed request to remove one existing set row.

### Fields

- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setId` (UUID, required)
- `outcome` (enum, required): `Deleted`, `NotFound`, `Conflict`, or `Failed`

### Outcome Rules

- `Deleted`: Row is removed and no longer appears in the returned set list for that entry.
- `NotFound`: Workout/lift entry/set row is missing in the requested scope; no mutation occurs.
- `Conflict`: Workout state rejects deletion (for example, not in progress); no mutation occurs.
- `Failed`: Unexpected runtime/server failure; row stays visible and retry remains available.
