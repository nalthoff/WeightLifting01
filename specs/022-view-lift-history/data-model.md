# Data Model: View lift history inline

## Entity: InlineLiftHistoryRequest

**Purpose**: Represents the user request context for opening history inline for a lift in an active workout.

### Fields

- `workoutId` (UUID, required)
- `workoutLiftEntryId` (UUID, required)
- `liftId` (UUID, required)
- `maxCompletedSessions` (integer, required; fixed to 3 for this feature)

### Validation and Rules

- `workoutId`, `workoutLiftEntryId`, and `liftId` must be non-empty ids.
- `maxCompletedSessions` is capped at 3 for this feature slice.
- Request does not mutate persisted workout data.

## Entity: LiftHistorySessionSummary

**Purpose**: One completed session row shown in the inline panel for an exact lift.

### Fields

- `workoutId` (UUID, required)
- `completedAtUtc` (datetime UTC, required)
- `workoutLabel` (string, optional)
- `sets` (array of `LiftHistorySetSummary`, required; can be empty)

### Validation and Rules

- Rows include completed workouts only.
- Rows are ordered most-recent-first by completion timestamp.
- Maximum returned rows is 3.

## Entity: LiftHistorySetSummary

**Purpose**: Set-level data for the selected exact lift inside one completed session row.

### Fields

- `setNumber` (integer, required; minimum 1)
- `reps` (integer, required; minimum 1)
- `weight` (number, nullable)

### Validation and Rules

- Set rows belong only to the selected exact lift.
- `weight = null` is valid and displayed as blank/placeholder in UI.

## Entity: InlineLiftHistoryPanelState

**Purpose**: UI state for each active workout lift row panel.

### Fields

- `workoutLiftEntryId` (UUID/string, required)
- `isExpanded` (boolean, required)
- `isLoading` (boolean, required)
- `errorMessage` (string, nullable)
- `items` (array of `LiftHistorySessionSummary`, required)

### State Transition Rules

- Closed -> Loading when user taps View History.
- Loading -> Loaded when request succeeds with one or more items.
- Loading -> Empty when request succeeds with zero items.
- Loading -> Error when request fails.
- Error/Loaded/Empty -> Closed when user collapses panel.
