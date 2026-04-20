# Data Model: Rename Lift

## Lift

- **Purpose**: Represents a user-selectable lift definition that can be renamed while preserving
  the lift's stable identity across lift-management and selection experiences.
- **Fields**:
  - `id` (GUID): Stable identifier for the lift
  - `name` (string): Current persisted display name
  - `isActive` (boolean): Indicates whether the lift is available for selection
  - `createdAtUtc` (datetime): Original creation timestamp retained across renames
- **Validation Rules**:
  - `name` is required
  - `name` must be trimmed before validation and persistence
  - `name` cannot be empty after trimming
  - `name` must not normalize to the same value as another existing lift's name
  - the lift may keep its own current normalized name without conflicting with itself
- **Relationships**:
  - Referenced by current and future workout-selection experiences through the lift's stable `id`
- **State Transitions**:
  - `Created/Active`: Lift exists and is selectable
  - `Renamed/Active`: Same lift remains selectable with an updated display name
  - Failed rename attempts do not transition persisted lift state

## RenameLiftRequest

- **Purpose**: Represents the user's request to change the name of an existing lift from
  `Settings -> Lifts`
- **Fields**:
  - `name` (string): Proposed replacement name entered by the user
- **Validation Rules**:
  - `name` must satisfy the same trimmed nonblank rules as the `Lift` entity
  - `name` must not conflict with another existing lift after normalization
  - `name` may normalize to the current lift's existing saved name without being treated as a
    conflict

## LiftRenameResult

- **Purpose**: Represents the outcome of a rename attempt for one existing lift
- **Fields**:
  - `liftId` (GUID): Target lift identity
  - `previousName` (string): Saved name before the attempt
  - `requestedName` (string): User-provided replacement name
  - `normalizedRequestedName` (string): Trimmed comparison value used for validation
  - `outcome` (enum): `renamed`, `unchanged`, `validation_failed`, `conflict`, or `not_saved`
- **Usage**:
  - Helps describe expected behavior in tests and API error handling
  - Clarifies that only confirmed successful outcomes should update shared lift consumers

## LiftListItem

- **Purpose**: Represents the lightweight lift record returned to lift-selection and settings UIs
- **Fields**:
  - `id` (GUID)
  - `name` (string)
  - `isActive` (boolean)
- **Usage**:
  - Returned from list queries
  - Stored in the shared frontend lift store used by settings and future lift-selection flows

## Behavioral Notes

- A successful rename updates the existing lift row instead of creating a replacement record.
- Duplicate-name checks compare normalized names against other lifts, not against the current lift
  itself.
- Failed rename attempts must leave the persisted name and later lift-list reads unchanged.
- The shared list refresh path should preserve stable ordering after a rename so the updated lift is
  easy to locate.
