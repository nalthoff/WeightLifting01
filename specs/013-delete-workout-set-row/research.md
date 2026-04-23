# Phase 0 Research: Delete mistaken workout set rows

## Decision 1: Keep set-row deletion inline with explicit confirmation

- **Decision**: Deletion starts from each set row in the active workout screen and requires explicit user confirmation before the delete request is sent.
- **Rationale**: This preserves fast in-gym flow while preventing accidental single-tap data loss.
- **Alternatives considered**:
  - Immediate delete without confirmation: rejected because it violates acceptance criteria and increases accidental deletions.
  - Separate full-screen delete flow: rejected because additional navigation interrupts mobile logging speed.

## Decision 2: Backend-authoritative delete eligibility and scope checks

- **Decision**: Backend application/domain logic remains authoritative for in-progress gating, row ownership, and not-found/conflict outcomes.
- **Rationale**: Centralized business-rule enforcement ensures consistent behavior across clients and aligns with constitution boundaries.
- **Alternatives considered**:
  - Frontend-only gating and scoping checks: rejected because client checks can drift from persisted truth.
  - Data-layer-only enforcement with minimal application rules: rejected due to weaker rule discoverability and testability.

## Decision 3: Reuse existing workout-set persistence model with no schema migration

- **Decision**: Use existing workout set entities/tables and add delete behavior through current command/controller patterns.
- **Rationale**: The feature mutates existing rows (removal) without requiring new persisted fields.
- **Alternatives considered**:
  - Add soft-delete columns now: rejected because current scope does not require retained deleted-row history.
  - Add audit/event tables in this slice: rejected because out of scope for simple mistaken-row correction.

## Decision 4: Failure behavior keeps row visible with clear retry path

- **Decision**: If delete fails, the targeted row remains visible, row/list-level feedback is shown, and user can retry.
- **Rationale**: This creates clear save-state trust under gym connectivity issues and prevents silent data ambiguity.
- **Alternatives considered**:
  - Optimistic remove with silent background retry: rejected because users can misinterpret unsaved deletions as final.
  - Remove row and force full list reload on any error: rejected because unstable network would create confusing list flicker.

## Decision 5: Concurrency handling prevents duplicate delete submissions

- **Decision**: While one row delete is pending, block duplicate submissions for that same row and present pending state.
- **Rationale**: This avoids accidental repeated requests and inconsistent feedback.
- **Alternatives considered**:
  - Allow repeated taps and rely on backend idempotency: rejected because it creates noisy user feedback and race-prone UX.
  - Block all set actions globally during one delete: rejected because it unnecessarily slows the workflow.

## Decision 6: Test strategy emphasizes confirmation safety and regressions

- **Decision**: Add backend unit tests for delete rules, integration tests for persistence mutation, contract tests for endpoint outcomes, and e2e tests for confirm/cancel/failure flows plus targeted regressions.
- **Rationale**: Coverage ensures both business correctness and user-trust behavior on mobile.
- **Alternatives considered**:
  - Unit-only coverage: rejected because API and UI behavior would remain unverified.
  - E2E-only coverage: rejected due to slower feedback and harder failure localization.
