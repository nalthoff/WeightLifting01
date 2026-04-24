# Data Model: Optional Workout Name

## Entity: WorkoutSession

**Purpose**: User-owned workout lifecycle record that supports active logging, completion, and optional naming.

### Fields

- `id` (UUID, required)
- `status` (enum, required; `InProgress` or `Completed`)
- `label` (string, optional; normalized to null when empty/whitespace)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, nullable; required when status is `Completed`)

### Validation and Rules

- Name edits are permitted only while `status = InProgress`.
- Empty or whitespace-only submitted label values are normalized to no label.
- Existing length constraints for label continue to apply.
- Once `status` transitions to `Completed`, label updates are rejected.

### State Transitions

- `InProgress -> InProgress` (rename/clear label allowed)
- `InProgress -> Completed` (completion operation; label becomes read-only afterward)
- `Completed -> Completed` (no label mutation allowed)

## Entity: WorkoutNameUpdateRequest

**Purpose**: Mutation intent payload for setting, changing, or clearing the workout name on an in-progress workout.

### Fields

- `workoutId` (UUID, required)
- `label` (string, optional input; may be null/blank for clear behavior)

### Validation and Rules

- Request is invalid if `workoutId` is missing/empty.
- Blank/whitespace `label` requests are interpreted as clear-to-unnamed.
- Input exceeding existing maximum length fails validation.
- Request conflicts when target workout is not in progress.

## Entity: WorkoutHistoryItem

**Purpose**: Completed-workout history row used for post-session identification and navigation.

### Fields

- `workoutId` (UUID, required)
- `label` (string, required display value)
- `completedAtUtc` (datetime UTC, required)
- `durationDisplay` (string, required)
- `liftCount` (integer, required)

### Validation and Rules

- If persisted workout name is absent, displayed label uses fallback `"Workout"`.
- Existing completed-only and newest-first ordering behavior remains unchanged.
