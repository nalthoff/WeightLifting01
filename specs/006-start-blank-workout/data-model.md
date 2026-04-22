# Data Model: Start blank workout session

## Entity: WorkoutSession

**Purpose**: Represents a persisted workout that has been officially started by a user.

### Fields

- `id` (UUID, required): Unique workout session identifier.
- `userId` (string/UUID, required): Owning user identity.
- `status` (enum, required): Lifecycle status for this slice; includes `InProgress` and future-ready terminal statuses.
- `label` (string, optional): User-provided session label after server-side trimming; `null` when omitted or whitespace-only.
- `startedAtUtc` (datetime UTC, required): Authoritative server-assigned start timestamp.
- `createdAtUtc` (datetime UTC, required): Persistence creation timestamp.
- `updatedAtUtc` (datetime UTC, required): Last mutation timestamp.

### Validation Rules

- `status` must be a valid known lifecycle value.
- `startedAtUtc` is always set by backend at create time and cannot be null.
- `label` length is bounded (exact max to be finalized during implementation based on existing naming conventions).
- Whitespace-only `label` normalizes to `null`.
- User can have at most one `InProgress` session at any time.

### Relationships

- One `WorkoutSession` belongs to one user.
- For this feature slice, no child set/lift rows are required.

## Entity: StartWorkoutRequest

**Purpose**: Input model used by the start API command.

### Fields

- `label` (string, optional): Optional free text session label.

### Validation Rules

- If omitted: valid.
- If provided: trimmed; if empty after trim, treated as absent.

## Entity: StartWorkoutResult

**Purpose**: Command result envelope that distinguishes create success from active-session conflict.

### Fields

- `outcome` (enum, required): `Created` or `AlreadyInProgress`.
- `workout` (WorkoutSessionSummary, required): Session summary for created or existing active session.

## Entity: WorkoutSessionSummary

**Purpose**: Minimal session representation needed by home + active-session UI.

### Fields

- `id` (UUID, required)
- `status` (enum, required)
- `label` (string, optional)
- `startedAtUtc` (datetime UTC, required)

## State Transitions

- `None -> InProgress`: Triggered by successful start request when no active session exists.
- `InProgress -> InProgress` (no-op conflict): Triggered when a start request arrives and an active session already exists; returns conflict/continue payload without creating a new row.
- Future transitions (not in scope here): `InProgress -> Completed`, `InProgress -> Cancelled`.

## Persistence Notes

- Add a new workouts table via EF Core migration.
- Add index support for efficient lookup by `userId` + `status`.
- Ensure migration remains compatible with SQL Server / Azure SQL deployment assumptions.
