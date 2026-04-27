# Data Model: Prefill next set defaults

## Entity: SetEntryDraft

**Purpose**: Represents editable next-set input state for a specific workout-lift entry in the active workout flow.

### Fields

- `workoutLiftEntryId` (UUID/string, required)
- `reps` (string input state, required for submission)
- `weight` (string input state, optional)
- `lastUpdatedAt` (client timestamp, optional)

### Validation and Rules

- `reps` must parse to a positive integer before save.
- `weight`, when provided, must parse to a non-negative number.
- Draft state is isolated per `workoutLiftEntryId`.

## Entity: LoggedSet

**Purpose**: Authoritative persisted set returned from successful add-set operations.

### Fields

- `id` (UUID, required)
- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setNumber` (integer, required)
- `reps` (integer, required)
- `weight` (number, nullable)
- `createdAtUtc` (datetime UTC, required)

### Validation and Rules

- Only successful add-set outcomes produce a `LoggedSet` used for prefill updates.
- `weight = null` is valid and must map to blank draft weight.

## Entity: AddSetAttemptOutcome

**Purpose**: Models whether an add-set action should update defaults or preserve existing draft values.

### Fields

- `workoutLiftEntryId` (UUID, required)
- `status` (enum, required): `Success`, `ValidationFailed`, `Conflict`, `NotFound`, `Failed`
- `returnedSet` (`LoggedSet`, optional; present only for `Success`)
- `errorMessage` (string, optional)

### State Transition Rules

- On `Success`: update `SetEntryDraft` for the matching entry using `returnedSet` values.
- On non-success statuses: preserve current `SetEntryDraft` for the matching entry unchanged.
- Outcomes for one entry must not mutate drafts for other entries.
