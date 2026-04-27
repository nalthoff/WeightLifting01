# Data Model: Workout Lifecycle Status Visibility

## Entity: WorkoutSession

**Purpose**: Canonical workout lifecycle record used by active workout flows, completion transitions, and history/progress eligibility.

### Fields

- `id` (UUID, required)
- `status` (enum, required; `InProgress` or `Completed`)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, nullable; required when status is `Completed`)
- `label` (string, optional)

### Validation and Rules

- Only two lifecycle states are valid: `InProgress`, `Completed`.
- `completedAtUtc` MUST be null for `InProgress` workouts.
- `completedAtUtc` MUST be non-null for `Completed` workouts.
- Completion timestamp cannot be earlier than start timestamp.

### State Transitions

- `InProgress -> InProgress` (active editing/logging behavior)
- `InProgress -> Completed` (completion action stamps `completedAtUtc`)
- `Completed -> Completed` (read-only lifecycle state in this feature scope)

## Entity: WorkoutStatusBadgeView

**Purpose**: User-facing status indicator model rendered on workout detail surfaces.

### Fields

- `displayText` (string, required; "In Progress" or "Completed")
- `semanticState` (enum-like value, required; maps from lifecycle status)
- `isVisible` (boolean, required)

### Validation and Rules

- Active workout detail always shows `displayText = "In Progress"`.
- Completed workout detail always shows `displayText = "Completed"`.
- Unknown status input must fail safely (no misleading lifecycle text rendered).

## Entity: HistoryProgressRecord

**Purpose**: Completed workout projection used by history-based progress views.

### Fields

- `workoutId` (UUID, required)
- `status` (must be `Completed`)
- `completedAtUtc` (datetime UTC, required)
- `label` (string, required display value via existing fallback rules)
- `durationDisplay` (string, required)
- `liftCount` (integer, required)

### Validation and Rules

- Only workouts where `status = Completed` and `completedAtUtc` is present are eligible.
- In-progress workouts are always excluded from history/progress views.
- Malformed rows (for example missing completion timestamp) are excluded from history/progress output.
