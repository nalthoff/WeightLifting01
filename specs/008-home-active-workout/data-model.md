# Data Model: Home active workout summary and quick completion

## Entity: WorkoutSession

**Purpose**: Authoritative persisted workout lifecycle record used to determine whether home should show an active-workout card.

### Relevant Fields

- `id` (UUID, required)
- `status` (enum, required; includes at least `InProgress` and `Completed`)
- `label` (string, optional)
- `startedAtUtc` (datetime UTC, required)
- `completedAtUtc` (datetime UTC, optional; set when completion succeeds)

### Relevant Rules

- Home active-workout card renders only when status is `InProgress`.
- Successful completion transitions status from `InProgress` to `Completed`.
- Completion is invalid for workouts already completed or unavailable.

## Entity: ActiveWorkoutSummary

**Purpose**: Home-facing projection of the current in-progress workout with minimal fields for fast decision-making.

### Fields

- `workoutId` (UUID, required)
- `displayLabel` (string, required; fallback "Workout" when workout label is null/empty)
- `startedAtUtc` (datetime UTC, required)
- `canContinue` (boolean, derived)
- `canComplete` (boolean, derived)

### Validation/Display Rules

- `displayLabel` must never be blank in rendered home card.
- Summary is omitted when no active workout exists.

## Entity: HomeCompletionAttempt

**Purpose**: Represents user action to complete an in-progress workout from home and resulting feedback state.

### Fields

- `workoutId` (UUID, required)
- `attemptedAtUtc` (datetime UTC, required)
- `outcome` (enum: `completed`, `not_found`, `invalid_state`, `not_saved`)
- `message` (string, required user-facing feedback)

### Behavior Rules

- Success outcome removes active-workout card and shows success feedback on home.
- Failure outcomes preserve accurate state and show explicit error feedback.
- Repeated rapid attempts should not create conflicting final state.

## State Transition Notes

- `InProgress -> Completed`: allowed through completion action when lifecycle rules pass.
- `InProgress -> InProgress`: retained when completion fails or cannot be applied.
- Stale/race responses must resolve by refreshing to authoritative persisted state before final home render.
