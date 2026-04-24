# Data Model: Delete in-progress workout

## Entity: WorkoutSession

**Purpose**: Existing workout aggregate that is eligible for deletion only while in progress.

### Fields

- `id` (UUID, required)
- `userId` (string, required)
- `status` (enum, required; `InProgress`, `Completed`, `Cancelled`)
- `label` (string, optional)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, optional)

### Rules

- Deletion is permitted only when `status` is `InProgress`.
- Completed workouts are not deletable by this feature.
- Delete operation is user-scoped; only the owning user workout can be deleted.

## Entity: WorkoutLiftEntry

**Purpose**: Child records under a workout that must be removed when the parent workout is deleted.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `liftId` (UUID, required)
- `position` (integer, required)
- `addedAtUtc` (datetime UTC, required)

### Relationship

- Many workout-lift entries belong to one workout session.

## Entity: WorkoutSetEntry

**Purpose**: Nested child records under each workout-lift entry that must be removed when the workout is deleted.

### Fields

- `id` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setNumber` (integer, required)
- `reps` (integer, required)
- `weight` (decimal, optional)
- `createdAtUtc` (datetime UTC, required)
- `updatedAtUtc` (datetime UTC, required)

### Relationship

- Many workout-set entries belong to one workout-lift entry.

## Entity: DeleteWorkoutAttempt

**Purpose**: A confirmed destructive request to delete one active workout.

### Fields

- `workoutId` (UUID, required)
- `requestedByUserId` (string, required)
- `confirmed` (boolean, required)
- `outcome` (enum, required): `Deleted`, `NotFound`, `Conflict`, `Failed`

### Outcome Rules

- `Deleted`: Workout and associated lift/set rows are permanently removed.
- `NotFound`: Workout does not exist in user scope; no mutation occurs.
- `Conflict`: Workout exists but is not in progress; no mutation occurs.
- `Failed`: Unexpected runtime/persistence failure; no false-success UI allowed.

## Entity: ActiveWorkoutDeleteSession (UI State)

**Purpose**: Transient client state controlling confirmation dialog and delete request feedback.

### Fields

- `isConfirmingDelete` (boolean, required)
- `isDeleting` (boolean, required)
- `errorMessage` (string | null, optional)
- `successMessage` (string | null, optional)

### Rules

- `isDeleting` blocks repeated confirms while request is pending.
- Dismissing confirmation clears confirmation state without mutation.
- Error and success feedback must reflect authoritative backend outcome.
