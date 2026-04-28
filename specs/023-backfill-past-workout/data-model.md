# Data Model: Backfill past workout

## Entity: HistoricalWorkoutDraft

**Purpose**: Captures user-entered historical workout intent before completion.

### Fields

- `workoutId` (UUID, required)
- `trainingDayLocalDate` (date, required)
- `startTimeLocal` (time, required; hour and minute precision)
- `sessionLengthMinutes` (integer, required)
- `label` (string, optional)
- `status` (enum, required: `InProgress` during draft, transitions to `Completed`)

### Validation and Rules

- `trainingDayLocalDate` must be a valid calendar day in the past relative to user intent.
- `startTimeLocal` and `sessionLengthMinutes` are required and block save when missing.
- `sessionLengthMinutes` must be greater than zero.
- Draft remains compatible with existing lift/set entry workflow.

## Entity: HistoricalWorkoutSession

**Purpose**: Completed backfilled workout shown in history and detail views.

### Fields

- `id` (UUID, required)
- `status` (enum, required: `Completed`)
- `trainingDayLocalDate` (date, required)
- `startTimeLocal` (time, required; hour and minute precision)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, required)
- `sessionLengthMinutes` (integer, required)
- `createdAtUtc` (datetime UTC, required)
- `updatedAtUtc` (datetime UTC, required)

### Validation and Rules

- Completed historical sessions must have non-null `completedAtUtc`.
- Chronology in history must honor entered historical date/time intent.
- Same-day ties use deterministic secondary ordering consistent with existing history behavior.
- Session length is required and used to derive the historical completion window.

## Entity: HistoricalWorkoutLiftEntry

**Purpose**: Exercise entry attached to a historical workout session.

### Fields

- `workoutLiftEntryId` (UUID, required)
- `workoutId` (UUID, required)
- `liftId` (UUID, required)
- `displayOrder` (integer, required)
- `sets` (array of `HistoricalWorkoutSetEntry`, required; can be empty for partial logs)

### Validation and Rules

- Uses same structural model as live workout lift entries.
- Preserves ordering semantics used in active workout entry and history detail.

## Entity: HistoricalWorkoutSetEntry

**Purpose**: Set record used for next-weight context from a backfilled session.

### Fields

- `setId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `setNumber` (integer, required; minimum 1)
- `reps` (integer, required; minimum 1)
- `weight` (number, nullable)

### Validation and Rules

- Uses same validation ranges as live workout set entry.
- Minimal or sparse set data remains valid if accepted by existing workout rules.

## Entity: ActiveWorkoutContext

**Purpose**: Existing in-progress workout state that must remain unaffected by historical backfill.

### Fields

- `activeWorkoutId` (UUID, required)
- `status` (enum, required: `InProgress`)
- `lastUpdatedAtUtc` (datetime UTC, required)

### Validation and Rules

- Backfilling a historical workout must not auto-complete, delete, or overwrite active workout state.
- User can continue active logging immediately after historical save.

## Data-structure preference

- Reuse existing persisted workout fields and lifecycle structures whenever possible.
- Introduce schema changes only if required date/time/duration behavior cannot be represented safely with current persisted fields.
