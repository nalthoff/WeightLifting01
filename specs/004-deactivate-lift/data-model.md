# Data Model: Deactivate Lift

## Lift

- **Purpose**: Represents a user-selectable lift definition whose availability can be toggled
  between active and inactive while preserving the lift's stable identity.
- **Fields**:
  - `id` (GUID): Stable identifier for the lift
  - `name` (string): Persisted display name
  - `isActive` (boolean): Whether the lift is included in default selection lists
  - `createdAtUtc` (datetime): Original creation timestamp retained across availability changes
- **Validation Rules**:
  - `name` remains required and normalized according to the existing lift rules
  - `isActive = true` means the lift appears in default active-only list reads
  - `isActive = false` means the lift is hidden from default active-only list reads but can still
    be returned when inactive lifts are included
- **Relationships**:
  - Referenced by lift-management and current/future lift-selection experiences through stable `id`
- **State Transitions**:
  - `Created/Active`: Lift exists and appears in default lists
  - `Deactivated/Inactive`: Same lift remains persisted but is excluded from default active-only
    list reads
  - Failed or cancelled deactivate attempts do not transition persisted lift state

## DeactivateLiftRequest

- **Purpose**: Represents the user's confirmed request to deactivate an existing lift from
  `Settings -> Lifts`
- **Fields**:
  - No user-entered fields are required beyond the targeted `liftId`
- **Validation Rules**:
  - The target lift must exist
  - The request must operate on an existing lift identity rather than a name match

## DeactivateLiftResult

- **Purpose**: Represents the outcome of a deactivate attempt for one existing lift
- **Fields**:
  - `liftId` (GUID): Target lift identity
  - `previousIsActive` (boolean): Saved availability before the attempt
  - `currentIsActive` (boolean): Availability after processing
  - `outcome` (enum): `deactivated`, `unchanged`, `not_found`, or `not_saved`
- **Usage**:
  - Clarifies that only confirmed successful outcomes should update shared lift consumers
  - Helps describe expected behavior in tests and API handling

## LiftListView

- **Purpose**: Represents the lift list returned to settings and selection UIs under a chosen
  visibility mode
- **Fields**:
  - `items` (array of `LiftListItem`)
  - `activeOnly` (boolean): Whether inactive lifts were excluded from the query
  - `lastSyncedAtUtc` (datetime, optional): Timestamp used for client reconciliation metadata
- **Behavior Rules**:
  - When `activeOnly = true`, only active lifts are returned
  - When `activeOnly = false`, both active and inactive lifts may be returned

## LiftListItem

- **Purpose**: Represents the lightweight lift record returned to settings and future
  lift-selection UIs
- **Fields**:
  - `id` (GUID)
  - `name` (string)
  - `isActive` (boolean)
- **Usage**:
  - Stored in the shared frontend lift store
  - Drives both default active-only displays and include-inactive management views

## Behavioral Notes

- Deactivation updates the existing lift row instead of creating, deleting, or renaming a record.
- The same persisted lift can be shown or hidden depending on whether the current list view
  includes inactive lifts.
- Default selection reads remain active-only so inactive lifts disappear from ordinary workout
  setup flows after a successful deactivate.
- The model intentionally preserves reactivation potential for a future feature without requiring
  lift re-creation.
