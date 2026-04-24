# Phase 0 Research: Optional Workout Name

## Decision 1: Introduce a dedicated in-progress rename operation

- **Decision**: Add a focused workout-name update operation that applies only to workouts in `InProgress` status.
- **Rationale**: The feature requires editing after start, while preserving post-completion immutability and explicit lifecycle enforcement.
- **Alternatives considered**:
  - Reuse workout start endpoint for renames: rejected because it does not model post-start edits.
  - Allow generic workout patch including completion-state edits: rejected due to broader scope and higher regression risk.

## Decision 2: Keep naming optional with normalization to unlabeled

- **Decision**: Treat empty or whitespace-only input as no name, and persist unlabeled state without blocking workflow.
- **Rationale**: Preserves fast logging and matches existing user expectation that workout naming is optional.
- **Alternatives considered**:
  - Require non-empty names once field is touched: rejected because it adds friction and contradicts acceptance criteria.
  - Store literal whitespace: rejected because it creates inconsistent history labeling behavior.

## Decision 3: Preserve history fallback display behavior as-is

- **Decision**: Continue rendering fallback `"Workout"` for completed sessions with no stored name.
- **Rationale**: Existing history affordance is already known and avoids introducing null/empty display regressions.
- **Alternatives considered**:
  - Show blank labels in history: rejected for poor scanability.
  - Introduce a new fallback term in this slice: rejected as unnecessary UX churn.

## Decision 4: Enforce lifecycle rules in backend business layer

- **Decision**: Keep rename eligibility checks (in-progress only, validation bounds, normalization) in C# application/domain command handling.
- **Rationale**: Constitution requires backend ownership of business rules and makes behavior consistent across clients.
- **Alternatives considered**:
  - Frontend-only enforcement: rejected because it is bypassable and inconsistent.
  - Persistence-layer-only checks: rejected because domain error semantics become less explicit.

## Decision 5: No schema expansion required for this slice

- **Decision**: Reuse existing optional workout label persistence model; no new workout-type field and no additional entities.
- **Rationale**: Scope is name-only and current workout data model already supports optional label semantics.
- **Alternatives considered**:
  - Add a separate workout type column: rejected as explicitly out of scope.
  - Add derived history naming projection tables: rejected as unnecessary complexity.

## Decision 6: Add targeted regression tests across lifecycle and fallback behavior

- **Decision**: Add backend unit/integration/contract tests for rename lifecycle and validation outcomes, plus frontend unit/e2e tests for optional naming UX and post-completion read-only behavior.
- **Rationale**: Main risks are lifecycle regressions and fallback rendering mismatches across active and history flows.
- **Alternatives considered**:
  - Backend-only tests: rejected because user-facing flow and messaging are critical.
  - Frontend-only tests: rejected because lifecycle guarantees must be enforced server-side.
