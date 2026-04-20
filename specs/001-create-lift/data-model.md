# Data Model: Create Lift

## Lift

- **Purpose**: Represents a user-selectable lift definition that can be chosen during workout
  entry.
- **Fields**:
  - `id` (GUID): Stable identifier for the lift
  - `name` (string): Required display name entered by the user
  - `isActive` (boolean): Indicates whether the lift is available for selection; defaults to
    `true`
  - `createdAtUtc` (datetime): Timestamp of successful creation
- **Validation Rules**:
  - `name` is required
  - `name` must be trimmed before validation and persistence
  - `name` cannot be empty after trimming
  - lift-name uniqueness is not enforced in this feature
- **Relationships**:
  - Referenced by future workout-entry records when the user selects a lift
- **State Transitions**:
  - `Created/Active`: Lift has been successfully saved and is selectable in workout flows
  - Failed submissions do not create or transition a persisted lift record

## CreateLiftRequest

- **Purpose**: Represents the user request to create a new lift from `Settings -> Lifts`
- **Fields**:
  - `name` (string): Raw user-entered lift name
  - `clientRequestId` (string, optional): Optional idempotency or reconciliation token for
    flaky network scenarios
- **Validation Rules**:
  - `name` must satisfy the same trimmed nonblank rules as the `Lift` entity

## LiftListItem

- **Purpose**: Represents the lightweight lift record returned to selection UIs
- **Fields**:
  - `id` (GUID)
  - `name` (string)
  - `isActive` (boolean)
- **Usage**:
  - Returned from list queries
  - Stored in the shared frontend lift store used by settings and workout selection

## Behavioral Notes

- A lift becomes visible to workout selection only after the backend confirms a successful
  create operation.
- Save failures must not produce a selectable `LiftListItem`.
- The shared list refresh path should preserve a stable sort order so the newly created lift is
  easy to locate.
