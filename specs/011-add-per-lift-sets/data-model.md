# Data Model: Add per-lift set logging

## Entity: WorkoutSession

**Purpose**: Existing workout aggregate that owns lift entries and constrains set logging to in-progress sessions.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress` and `Completed`)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, optional)

### Relevant Rules

- Add-set creation is allowed only when workout status is `InProgress`.
- Set logging effects are scoped to one workout session and do not mutate historical completed sessions.

## Entity: WorkoutLiftEntry

**Purpose**: One lift instance inside a workout that owns an independent ordered set list.

### Fields

- `id` (UUID, required): Stable identity used to scope set creation.
- `workoutId` (UUID, required): Parent workout reference.
- `liftId` (UUID, required): Lift-library reference.
- `position` (integer, required): Entry position in workout flow.
- `addedAtUtc` (datetime UTC, required)

### Validation Rules

- `workoutId` must reference an existing in-progress workout for add-set operations.
- Duplicate lift entries are valid and are distinguished by `WorkoutLiftEntry.id`.
- Newly added workout-lift entries start with zero associated set rows.

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `WorkoutLiftEntry` belongs to exactly one `WorkoutSession`.
- One `WorkoutLiftEntry` has many `WorkoutSetEntry` records.

## Entity: WorkoutSetEntry

**Purpose**: One persisted completed set tied to exactly one workout-lift entry.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required): Denormalized parent workout reference for query efficiency and integrity checks.
- `workoutLiftEntryId` (UUID, required): Parent lift-entry reference.
- `setNumber` (integer, required): Sequential number within one workout-lift entry.
- `reps` (integer, required): Completed reps.
- `weight` (decimal, optional): Optional logged load value.
- `createdAtUtc` (datetime UTC, required)
- `updatedAtUtc` (datetime UTC, required)

### Validation Rules

- `workoutLiftEntryId` must belong to the target `workoutId`.
- `setNumber` must be unique within (`workoutLiftEntryId`) and assigned sequentially without gaps for persisted rows.
- `reps` must be a positive integer.
- `weight` may be null/blank; when provided it must be non-negative and within supported precision/range.

### Relationships

- One `WorkoutLiftEntry` has many `WorkoutSetEntry` rows ordered by `setNumber`.
- One `WorkoutSetEntry` belongs to exactly one `WorkoutLiftEntry`.

## Entity: AddSetAttempt

**Purpose**: Represents one user-triggered attempt to create a set row for a specific workout-lift entry.

### Fields

- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `requestedReps` (integer, required)
- `requestedWeight` (decimal, optional)
- `outcome` (enum, required): `Created`, `ValidationFailed`, `Conflict`, `NotFound`, or `Failed`.

### Outcome Rules

- `Created`: Set row persisted with authoritative `setNumber` returned.
- `ValidationFailed`: Invalid reps/weight or malformed payload; no row persisted.
- `Conflict`: Workout not in progress or state conflict; no row persisted.
- `NotFound`: Workout or lift entry not found within scope; no row persisted.
- `Failed`: Unexpected server/runtime failure; no row persisted.

## API Contract Shapes

### CreateWorkoutSetRequest

- `reps` (integer, required)
- `weight` (number, optional, nullable)

### CreateWorkoutSetResponse

- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `set` (`WorkoutSetEntryView`, required)

### WorkoutSetEntryView

- `id` (UUID, required)
- `setNumber` (integer, required)
- `reps` (integer, required)
- `weight` (number, nullable)
- `createdAtUtc` (datetime UTC, required)

### Problem / ValidationProblem

- `title` (string, optional)
- `status` (integer, optional)
- `detail` (string, optional)
- `errors` (map<string, string[]>, optional)

## State/Flow Notes

- Successful add-set returns authoritative row content used to append the visible list.
- Failed add-set returns explicit error outcome and leaves the last known persisted list unchanged.
- Set numbering is independent per workout-lift entry and supports duplicate lift names in one workout.
