# Data Model: Remove lift from in-progress workout

## Entity: WorkoutSession

**Purpose**: Existing in-progress workout aggregate that contains removable workout-lift entries.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress`)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, optional)

### Relevant Rules

- Lift entry removal is allowed only while workout is in progress.
- Removal affects only the current workout session context.

## Entity: Lift

**Purpose**: Existing lift-library definition referenced by workout-lift entries.

### Relevant Fields

- `id` (UUID, required)
- `name` (string, required)
- `isActive` (boolean, required)

### Relevant Rules

- Removing a workout-lift entry never deletes or deactivates the lift-library definition.

## Entity: WorkoutLiftEntry

**Purpose**: Persisted association representing one lift instance within one workout session.

### Fields

- `id` (UUID, required): Unique entry identifier used as removal target.
- `workoutId` (UUID, required): Parent workout reference.
- `liftId` (UUID, required): Referenced lift-library item.
- `displayName` (string, required): Lift name snapshot for workout-list rendering.
- `addedAtUtc` (datetime UTC, required): Timestamp when entry was added.
- `position` (integer, required): Ordering index within workout list.

### Validation Rules

- Entry must exist at remove time or removal fails with not-found outcome.
- Parent workout must be in progress to permit removal.
- Duplicate `liftId` entries in a workout remain valid; removal may delete exactly one selected entry.

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `Lift` can appear in many `WorkoutLiftEntry` records.

## Entity: RemoveWorkoutLiftAttempt

**Purpose**: Represents a request to remove one workout-lift entry and its resulting outcome.

### Fields

- `workoutId` (UUID, required): Workout scope provided in route.
- `workoutLiftEntryId` (UUID, required): Selected entry instance to remove.
- `outcome` (enum, required): `Removed`, `NotFound`, or `Conflict`.

### Outcome Rules

- `Removed`: selected entry is deleted and should disappear from active workout list.
- `NotFound`: workout or entry is missing/stale; list must remain accurate and user sees recoverable feedback.
- `Conflict`: workout is not in a removable state (for example no longer in progress); list remains accurate with conflict feedback.

## API Contract Shapes

### RemoveWorkoutLiftResponse

- `removedWorkoutLiftEntryId` (UUID, required): Identifier of removed entry.
- `workoutId` (UUID, required): Workout scope for confirmation.

### Problem / ValidationProblem

- `title` (string, optional)
- `status` (integer, optional)
- `errors` (map<string, string[]>, optional for validation/conflict detail)

## State/Flow Notes

- Removal does not modify workout lifecycle status.
- UI list mutation is driven by successful API response only.
- Conditional confirmation based on logged sets is deferred until set logging exists.
