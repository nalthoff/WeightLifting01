# Data Model: Edit workout set entries

## Entity: WorkoutSession

**Purpose**: Existing workout aggregate that determines whether set entries can be edited.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress` and `Completed`)

### Relevant Rules

- Set updates are allowed only while `status` is `InProgress`.
- Non-in-progress workouts reject update requests and do not mutate persisted set data.

## Entity: WorkoutLiftEntry

**Purpose**: Parent lift-entry scope that owns its set rows and prevents duplicate-lift cross-entry leakage.

### Relevant Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `liftId` (UUID, required)
- `position` (integer, required)

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `WorkoutLiftEntry` has many `WorkoutSetEntry` records.

## Entity: WorkoutSetEntry

**Purpose**: Persisted set row that supports correcting reps/weight while preserving immutable identity and numbering.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setNumber` (integer, required, immutable)
- `reps` (integer, required, editable)
- `weight` (decimal, optional, editable)
- `createdAtUtc` (datetime UTC, required)
- `updatedAtUtc` (datetime UTC, required)

### Validation Rules

- `workoutLiftEntryId` must belong to the target `workoutId`.
- `reps` must be a positive integer.
- `weight` may be null; when provided it must be non-negative and within supported precision/range.
- `setNumber` cannot be changed by update operations.

### State Transitions

- `Persisted` -> `Editing` (frontend row mode only; no persistence yet)
- `Editing` -> `Saving` (update request in flight)
- `Saving` -> `Persisted` (success; row values updated from authoritative response)
- `Saving` -> `UnsavedWithError` (failure; unsaved local values retained)
- `UnsavedWithError` -> `Saving` (retry with latest row draft)

## Entity: SetRowEditSession

**Purpose**: Per-row transient state in UI/store for managing inline draft edits and save outcomes.

### Fields

- `setId` (UUID, required)
- `draftReps` (string/number, required while editing)
- `draftWeight` (string/number/null, optional)
- `isSaving` (boolean, required)
- `errorMessage` (string | null, optional)
- `isDirty` (boolean, required)
- `isPersisted` (boolean, required)

### Rules

- Draft values remain visible after failed saves until user retries, cancels, or refresh logic clears them.
- `isPersisted` becomes true only after successful backend response.
- Row errors are scoped to one set row and do not block editing unrelated rows.

## Entity: UpdateSetAttempt

**Purpose**: A single user-triggered request to persist edits for one existing set row.

### Fields

- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setId` (UUID, required)
- `requestedReps` (integer, required)
- `requestedWeight` (decimal, optional)
- `outcome` (enum, required): `Updated`, `ValidationFailed`, `Conflict`, `NotFound`, or `Failed`

### Outcome Rules

- `Updated`: Persisted row returns latest reps/weight and timestamp.
- `ValidationFailed`: Invalid edits rejected; no persisted mutation.
- `Conflict`: Workout state or row context rejects update; no persisted mutation.
- `NotFound`: Workout/lift entry/set row missing in requested scope.
- `Failed`: Unexpected runtime/server failure; row remains unsaved locally.
