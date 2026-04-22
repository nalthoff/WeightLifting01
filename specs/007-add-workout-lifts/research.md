# Phase 0 Research: Add lifts to in-progress workout

## Decision 1: Add-lift entry point stays on active workout screen

- **Decision**: The primary Add Lift action is placed directly on the active workout screen and opens a picker from there.
- **Rationale**: Minimizes navigation/taps during in-gym logging and aligns with mobile-first flow requirements.
- **Alternatives considered**:
  - Route users to a separate management screen: rejected due to additional taps and context switching.
  - Place add action under Settings: rejected because it breaks workout-flow continuity.

## Decision 2: Picker source uses active lift library only

- **Decision**: Lift picker lists active lifts only, sourced from existing lift library state.
- **Rationale**: Matches product constraint and reduces confusion from retired/deactivated lifts.
- **Alternatives considered**:
  - Show all lifts with filtering: rejected as unnecessary complexity for this slice.
  - Allow inactive-lift adds: rejected because inactive status implies non-default usage.

## Decision 3: Duplicate lift additions are intentionally allowed

- **Decision**: The same lift may be added multiple times within a single in-progress workout in this release.
- **Rationale**: Explicitly requested product behavior for now and avoids blocking user intent.
- **Alternatives considered**:
  - Strict duplicate prevention: rejected because it conflicts with stated acceptance.
  - Duplicate confirmation dialog: rejected as extra friction not requested for MVP.

## Decision 4: Persist workout-lift association as a dedicated data model

- **Decision**: Introduce explicit persisted workout-lift entries associated to workout sessions and library lifts.
- **Rationale**: Provides durable association and immediate reload/deep-link consistency for workout flow.
- **Alternatives considered**:
  - Frontend-only in-memory association: rejected due to loss on refresh and ghost-state risk.
  - Embed lift list as serialized workout blob: rejected for queryability and migration discipline.

## Decision 5: Failure behavior forbids ghost additions

- **Decision**: UI updates workout-lift list only after add API success; failures show clear messages and do not append local-only entries.
- **Rationale**: Directly satisfies no-ghost requirement under flaky connectivity.
- **Alternatives considered**:
  - Optimistic add with eventual rollback: rejected because transient failure could appear as saved.
  - Silent retry loops without user feedback: rejected for poor observability.

## Decision 6: Single-user scoping remains explicit for this phase

- **Decision**: Maintain current single-user mode assumptions and scope workout-lift operations to the same user context used in workouts.
- **Rationale**: Avoids introducing authentication scope changes unrelated to this feature goal.
- **Alternatives considered**:
  - Introduce multi-user authorization now: rejected as out of scope.
  - Ignore user scoping entirely in data model: rejected due to future incompatibility risk.

## Decision 7: Test strategy emphasizes backend rule correctness and boundary behavior

- **Decision**: Add unit tests for add-lift business rules, integration tests for persistence/association behavior, and contract tests for add/fetch API payloads; add frontend e2e for mobile add flow and error paths.
- **Rationale**: Constitution requires unit coverage for business logic and supports resilient user-facing behavior.
- **Alternatives considered**:
  - UI-only testing: rejected because core rules live in backend.
  - Contract tests only: rejected because domain-rule regressions are harder to isolate.
