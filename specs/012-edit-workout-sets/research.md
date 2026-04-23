# Phase 0 Research: Edit workout set entries

## Decision 1: Inline row editing with explicit save

- **Decision**: Existing set rows enter an inline edit state and require an explicit Save action per row.
- **Rationale**: Explicit save provides predictable state transitions and clearer trust signals under gym connectivity variability.
- **Alternatives considered**:
  - Auto-save on blur/enter: rejected because accidental saves and unclear commit timing increase user confusion.
  - Full-screen edit form: rejected because it interrupts the in-gym logging flow with unnecessary navigation.

## Decision 2: Backend-authoritative update validation and in-progress enforcement

- **Decision**: Backend application/domain logic remains the authority for validating reps/weight edits and enforcing InProgress-only updates.
- **Rationale**: Rule ownership in backend ensures consistent behavior across clients and aligns with constitution boundaries.
- **Alternatives considered**:
  - Frontend-only validation and state gating: rejected because it can drift from persisted truth and is easier to bypass.
  - Data-layer-only enforcement without application rules: rejected because business intent becomes less discoverable and testable.

## Decision 3: Preserve unsaved edits on failure with explicit retry

- **Decision**: Failed update attempts keep row inputs intact, show clear unsaved/failure state, and expose retry on the same row.
- **Rationale**: Users can recover quickly without retyping and can distinguish persisted values from local unsaved edits.
- **Alternatives considered**:
  - Revert immediately to last persisted values: rejected because user-entered corrections are lost.
  - Silent retry only: rejected because users cannot tell if data has actually saved.

## Decision 4: Last-write-wins for concurrent set updates

- **Decision**: Concurrent edits on the same set use last-write-wins as the conflict policy.
- **Rationale**: This matches explicit user direction and avoids additional merge workflows for a low-collaboration context.
- **Alternatives considered**:
  - Optimistic concurrency rejection with version checks: rejected for this slice due to extra retry friction and contract complexity.
  - Manual merge prompts: rejected because workflow overhead exceeds expected benefit for this domain.

## Decision 5: Reuse existing workout-set persistence model (no schema migration)

- **Decision**: Implement set editing using existing workout-set table/entity shape; no new schema migration in this phase.
- **Rationale**: Required editable fields (reps, weight, updated timestamp) already exist in current model and do not require expansion.
- **Alternatives considered**:
  - Introduce edit audit table now: rejected because not required by current feature scope.
  - Add soft-locking columns: rejected because last-write-wins explicitly avoids lock coordination.

## Decision 6: Test strategy prioritizes trust and regression protection

- **Decision**: Add backend unit tests for update rules, integration tests for persistence correctness, contract tests for endpoint outcomes, and e2e tests for inline edit/save/retry behavior.
- **Rationale**: This combines business-rule confidence with end-user trust guarantees and regression safety for existing workout flows.
- **Alternatives considered**:
  - Unit-only backend tests: rejected because API and UI behavior would remain unverified.
  - E2E-only coverage: rejected because failures would be harder to localize and maintain.
