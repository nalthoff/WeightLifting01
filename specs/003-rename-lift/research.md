# Research: Rename Lift

## Decision 1: Add rename interaction to the existing `Settings -> Lifts` page

- **Decision**: The frontend will extend the current `Settings -> Lifts` management page with an
  explicit edit state for an existing lift rather than introducing a separate route or a detached
  management screen.
- **Rationale**: The current product already centers lift management in `Settings -> Lifts`, and a
  local edit state keeps the rename flow fast on mobile by avoiding extra navigation.
- **Alternatives considered**:
  - Add a separate lift-details page: rejected because it adds taps and routing complexity to a
    small management task.
  - Use a modal-only flow: rejected because it adds extra UI complexity without a clear benefit
    over an in-page edit state on a small feature slice.

## Decision 2: Keep canonical rename validation in backend application/domain layers

- **Decision**: Angular will own selection, edit-state, and feedback messaging, while the backend
  application/domain layers will own the canonical rename rules: trimmed required names, same-lift
  no-op handling, and normalized duplicate-name conflict detection against other existing lifts.
- **Rationale**: The constitution requires business rules to live outside Angular and be covered by
  unit tests. Rename validation affects correctness across all lift consumers, so it should be
  enforced at the shared backend boundary.
- **Alternatives considered**:
  - Validate conflicts only in Angular: rejected because it can be bypassed and would duplicate
    business rules.
  - Put rename rules directly in the controller: rejected because it would mix transport and
    business behavior.

## Decision 3: Reuse the existing `Lifts` table without adding a global uniqueness migration

- **Decision**: The feature will update the existing lift row in the current `Lifts` table and
  enforce duplicate-name prevention only for rename attempts by comparing the normalized target name
  against other persisted lifts. This slice will not add a database-level unique constraint or
  attempt to clean up previously created duplicates.
- **Rationale**: The earlier create-lift feature explicitly left global uniqueness out of scope, so
  introducing a unique index now would create migration and data-remediation work that exceeds the
  rename feature's scope. Rename-only conflict checks satisfy the user need without forcing broader
  catalog cleanup.
- **Alternatives considered**:
  - Add a unique index on normalized lift name: rejected because existing duplicate data may already
    exist and would require remediation planning not covered by this feature.
  - Continue allowing conflicting rename targets: rejected because it would leave users unable to
    trust lift names in later selection flows.

## Decision 4: Use a dedicated update endpoint and non-optimistic shared-store refresh

- **Decision**: The backend will expose a dedicated update endpoint for an existing lift, and the
  frontend will update the shared lift store only after a confirmed success response, followed by a
  list refresh reconciliation.
- **Rationale**: A dedicated update contract makes the rename behavior explicit, preserves the
  lift's stable identity, and avoids showing an unsaved name during connectivity or server failures.
- **Alternatives considered**:
  - Optimistically rename the lift in the store before confirmation: rejected because failures would
    leave the unsaved name looking canonical.
  - Force a full page reload after rename: rejected because it slows the mobile management flow and
    is unnecessary when the shared store already exists.

## Decision 5: Treat unchanged names as harmless no-op requests, not conflicts

- **Decision**: Rename attempts that normalize to the lift's current saved name will not be treated
  as duplicate-name conflicts. The frontend should avoid presenting them as meaningful saves, and
  the backend should tolerate them without changing lift identity.
- **Rationale**: The spec requires users to be able to keep the current name without self-conflict.
  This approach preserves predictable behavior while keeping the duplicate-name rule focused on other
  lifts.
- **Alternatives considered**:
  - Reject unchanged names as validation failures: rejected because it treats the current value as
    invalid and creates unnecessary friction.
  - Treat unchanged names as duplicate conflicts: rejected because a lift must never conflict with
    itself.

## Decision 6: Test rename behavior across business rules, API contract, shared state, and mobile UX

- **Decision**: Add backend unit tests for normalized rename validation and conflict detection,
  backend integration/contract tests for the update endpoint plus list consistency, focused Angular
  tests for edit-state and messaging, and a mobile-viewport e2e test for successful and failed
  rename behavior.
- **Rationale**: The rename flow crosses validation, persistence, API, shared state, and UI
  feedback boundaries, so a layered test strategy is the lowest-risk way to catch regressions.
- **Alternatives considered**:
  - E2E-only coverage: rejected because it makes validation regressions harder to isolate.
  - Backend-only coverage: rejected because it would not prove the mobile rename UX or shared-store
    update behavior.
