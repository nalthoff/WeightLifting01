# Phase 0 Research: Remove lift from in-progress workout

## Decision 1: Remove entry from active workout screen

- **Decision**: Keep lift removal initiated directly from the active workout list rather than routing to a separate management screen.
- **Rationale**: Preserves mobile logging speed and minimizes interruption during in-gym use.
- **Alternatives considered**:
  - Dedicated remove-management route: rejected because it adds navigation overhead and context switching.
  - Remove from Settings lift library: rejected because this feature targets workout-session entries, not library definitions.

## Decision 2: Removal targets workout-lift entry identity, not lift identity

- **Decision**: Backend and frontend removal operations identify the target by workout-lift entry id.
- **Rationale**: Duplicate lifts are allowed in a workout, so only entry-level identity guarantees precise removal.
- **Alternatives considered**:
  - Remove by `liftId`: rejected because it can remove the wrong duplicate instance.
  - Remove all matching duplicates in one action: rejected because it conflicts with explicit scope.

## Decision 3: No blocking confirmation modal in this slice

- **Decision**: Execute removal directly without a blocking confirmation modal for this release.
- **Rationale**: Set logging does not yet exist, so conditional confirmation cannot be meaningfully evaluated and would add unnecessary friction.
- **Alternatives considered**:
  - Always require confirmation: rejected because it slows primary flow without current user-value signal.
  - Add conditional confirmation placeholder UI now: rejected to avoid speculative behavior tied to non-existent set data.

## Decision 4: Failure handling prioritizes explicit feedback and no ghost removals

- **Decision**: Update client list only after successful remove response; on failure show explicit error and keep list consistent with saved data.
- **Rationale**: Prevents user confusion under flaky connectivity and aligns with no-ghost-state requirement.
- **Alternatives considered**:
  - Optimistic remove with rollback: rejected due to potential momentary incorrect state during live logging.
  - Silent retry without user feedback: rejected due to poor observability and trust.

## Decision 5: Reuse existing workout-lift persistence model

- **Decision**: Use existing persisted workout-lift entry model and add remove behavior via application/domain and API layers, without introducing a new persistence model.
- **Rationale**: Existing data structure already represents removable entry instances and supports session scoping.
- **Alternatives considered**:
  - Soft-delete flag for workout-lift entries: rejected because hard removal better matches current session-correction intent and keeps queries simple.
  - Frontend-only filtering of removed entries: rejected because persistence and reload consistency would break.

## Decision 6: Testing strategy emphasizes backend rules plus end-to-end user confidence

- **Decision**: Add backend unit tests for remove rules, integration tests for persistence behavior, contract tests for remove endpoint outcomes, and frontend e2e coverage for remove success/failure/duplicate-instance flows.
- **Rationale**: Business rules are backend-owned and user trust depends on clear behavior at boundaries.
- **Alternatives considered**:
  - UI-only testing: rejected because it misses backend state and conflict logic.
  - Unit tests only: rejected because API and persistence integration risks would remain unverified.
