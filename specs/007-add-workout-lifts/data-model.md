# Data Model: Add lifts to in-progress workout

## Entity: WorkoutSession

**Purpose**: Existing in-progress workout aggregate that receives added lifts.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress`)
- `startedAtUtc` (datetime UTC, required)

### Relevant Rules

- Add-lift operations target in-progress workouts only.

## Entity: Lift

**Purpose**: Existing lift-library item selectable in picker.

### Relevant Fields

- `id` (UUID, required)
- `name` (string, required)
- `isActive` (boolean, required)

### Relevant Rules

- Picker source includes active lifts only (`isActive = true`).

## Entity: WorkoutLiftEntry

**Purpose**: Persisted association representing one lift added to one workout.

### Fields

- `id` (UUID, required): Unique workout-lift entry identifier.
- `workoutId` (UUID, required): Parent workout session reference.
- `liftId` (UUID, required): Selected lift reference from lift library.
- `displayName` (string, required): Lift name snapshot used in current workout flow display.
- `addedAtUtc` (datetime UTC, required): Server-assigned add timestamp.
- `position` (integer, required): Ordering field within workout flow for deterministic rendering.

### Validation Rules

- `workoutId` must reference an existing in-progress workout.
- `liftId` must reference an existing active lift at add time.
- Duplicate `(workoutId, liftId)` entries are allowed in this release.
- `position` must be unique per workout and strictly increasing for insertion order.

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `Lift` can appear in many `WorkoutLiftEntry` records.

## Entity: AddWorkoutLiftRequest

**Purpose**: API input for adding one lift to a workout.

### Fields

- `liftId` (UUID, required)

### Validation Rules

- Missing/invalid `liftId` results in validation failure.

## Entity: AddWorkoutLiftResult

**Purpose**: API output for successful add operation.

### Fields

- `workoutLift` (WorkoutLiftEntrySummary, required)

## Entity: WorkoutLiftEntrySummary

**Purpose**: Minimal response shape for immediate frontend update.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `liftId` (UUID, required)
- `displayName` (string, required)
- `addedAtUtc` (datetime UTC, required)
- `position` (integer, required)

## State/Flow Notes

- Add action does not modify workout lifecycle status.
- Each successful add creates a new `WorkoutLiftEntry`, including duplicates.
- Remove behavior is intentionally deferred.
