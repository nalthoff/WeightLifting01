# Data Model: Reorder workout lifts

## Entity: WorkoutSession

**Purpose**: Existing workout aggregate that owns the ordered sequence of workout-lift entries for one session.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes `InProgress` and `Completed`)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, optional)

### Relevant Rules

- Reorder is allowed only when workout status is `InProgress`.
- Reorder effects are scoped to one workout session and must not mutate historical sessions.

## Entity: WorkoutLiftEntry

**Purpose**: One lift instance in a workout list, including duplicates, with stable identity and explicit sequence position.

### Fields

- `id` (UUID, required): Stable entry identity used for reorder operations.
- `workoutId` (UUID, required): Parent workout reference.
- `liftId` (UUID, required): Lift-library reference.
- `displayName` (string, required): Render label.
- `addedAtUtc` (datetime UTC, required): Original add timestamp.
- `position` (integer, required): Ordered position within workout.

### Validation Rules

- Every entry id in a reorder request must belong to the target workout.
- Reorder request must represent a complete, non-duplicated ordered set of current entry ids.
- Post-reorder positions must be contiguous and deterministic.
- Entry identities remain unchanged; only relative order/position changes.

### Relationships

- One `WorkoutSession` has many `WorkoutLiftEntry` records.
- One `WorkoutLiftEntry` belongs to exactly one `WorkoutSession`.

## Entity: WorkoutLiftOrder

**Purpose**: Authoritative ordered sequence for lift entries in one in-progress workout.

### Fields

- `workoutId` (UUID, required)
- `entryIds` (array<UUID>, required): Ordered list of `WorkoutLiftEntry.id` values.
- `updatedAtUtc` (datetime UTC, required): Timestamp for latest saved order.

### Rules

- Must include each current entry id exactly once.
- Must persist immediately on successful reorder.
- Must be returned to client as authoritative order after save.

## Entity: ReorderWorkoutLiftsAttempt

**Purpose**: Represents one user-initiated reorder save attempt and outcome.

### Fields

- `workoutId` (UUID, required): Workout scope from route.
- `orderedEntryIds` (array<UUID>, required): Requested sequence.
- `outcome` (enum, required): `Reordered`, `NotFound`, `Conflict`, or `ValidationFailed`.

### Outcome Rules

- `Reordered`: Order persisted and authoritative ordered entries returned.
- `NotFound`: Workout or one/more entries are missing; no partial reorder persisted.
- `Conflict`: Workout is no longer reorderable (for example, not in progress).
- `ValidationFailed`: Request shape/order set is invalid; no changes persisted.

## API Contract Shapes

### ReorderWorkoutLiftsRequest

- `orderedWorkoutLiftEntryIds` (array<UUID>, required): Full ordered sequence after user reorder.

### ReorderWorkoutLiftsResponse

- `workoutId` (UUID, required)
- `items` (array<WorkoutLiftEntry>, required): Authoritative ordered entries after save.

### Problem / ValidationProblem

- `title` (string, optional)
- `status` (integer, optional)
- `errors` (map<string, string[]>, optional)

## State/Flow Notes

- Reorder preserves workout lifecycle status.
- Failed reorder must not be shown as persisted in UI.
- Duplicates remain valid because identity is entry-based, not lift-name based.
