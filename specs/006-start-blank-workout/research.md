# Phase 0 Research: Start blank workout session

## Decision 1: Enforce one active workout per user in backend command flow

- **Decision**: The start-workout command checks for an existing `InProgress` workout for the user before create; if found, return a structured conflict response containing the existing workout reference for continue flow.
- **Rationale**: Prevents duplicate in-progress sessions from double taps, retries, or multi-device usage while keeping lifecycle rules centralized and testable.
- **Alternatives considered**:
  - Allow multiple in-progress workouts and let UI resolve: rejected due to rule ambiguity and higher user confusion.
  - Enforce only in frontend state: rejected because it breaks under refresh/multi-device and violates backend rule ownership.

## Decision 2: Label normalization rule for optional workout name

- **Decision**: Accept optional label input; trim whitespace server-side and persist `null` when empty after trim.
- **Rationale**: Matches stakeholder intent for optional labeling while avoiding validation noise during in-gym start flow.
- **Alternatives considered**:
  - Reject whitespace-only labels with validation error: rejected because it adds friction and does not improve session correctness.
  - Persist raw whitespace text: rejected because it creates low-quality noisy data.

## Decision 3: Failure semantics and ghost-prevention behavior

- **Decision**: UI only transitions to active-session view after successful API create response; all network/server failures keep user on start surface with explicit error message and retry option.
- **Rationale**: Satisfies requirement for no silent fake success under flaky connectivity.
- **Alternatives considered**:
  - Optimistic local session creation with later sync: rejected for this slice because it can display unsaved sessions as real.
  - Silent retry loop without clear status: rejected due to unclear user feedback and longer perceived latency.

## Decision 4: Minimum in-session MVP scope

- **Decision**: Provide a minimal active-session screen that confirms session status (`In Progress`), start time, and optional label; defer lift/set logging to future features.
- **Rationale**: Delivers an end-to-end session-start slice without scope creep.
- **Alternatives considered**:
  - Include lift selection/set entry now: rejected as explicitly out-of-scope.
  - Redirect back to home with toast only: rejected because user needs clear in-session state after successful start.

## Decision 5: API contract shape for start and continue conflict

- **Decision**: Add dedicated workouts API contract with:
  - `POST /api/workouts` for start requests
  - `201 Created` for new session created
  - `409 Conflict` when an in-progress session already exists, including minimal existing-session summary for continue prompt
- **Rationale**: Aligns with existing REST-style patterns already used in the codebase and supports explicit continue decision.
- **Alternatives considered**:
  - Return `200 OK` for both create and existing: rejected because callers cannot reliably distinguish start-vs-continue branch.
  - Use non-standard status codes: rejected for interoperability and clarity.

## Decision 6: Persistence and migration strategy

- **Decision**: Introduce a new persisted workouts table through EF Core migration, with UTC start timestamp required and index strategy to support fast active-session lookup by user and status.
- **Rationale**: Meets SQL migration discipline and enables deterministic active-session checks.
- **Alternatives considered**:
  - Store session state in frontend/local storage: rejected as non-authoritative and not shareable across devices.
  - Reuse lifts table for session data: rejected due to incorrect aggregate boundary and poor maintainability.

## Decision 7: Test strategy for new business rules

- **Decision**: Add backend unit tests for start command handler rules (label normalization, one-active-session enforcement, timestamp assignment pathway assumptions), plus integration/contract coverage for API success/failure/conflict shapes.
- **Rationale**: Constitution requires unit tests for business logic and benefits from API boundary checks.
- **Alternatives considered**:
  - Integration tests only: rejected because rule-level regressions become harder to isolate.
  - UI tests only: rejected because core lifecycle correctness belongs to backend.

## Decision 8: Auth/user identity assumption for this slice

- **Decision**: Use existing authenticated user context infrastructure and scope active-session checks by current user identity; do not introduce new auth mechanisms.
- **Rationale**: Keeps this feature focused and consistent with existing architecture.
- **Alternatives considered**:
  - Anonymous workout starts: rejected because per-user active-session constraint cannot be enforced safely.
  - New auth provider integration: rejected as unrelated to MVP session-start slice.
