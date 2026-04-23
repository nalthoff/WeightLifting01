# Phase 0 Research: Workout history page and completion parity

## Decision 1: Reuse existing completion lifecycle flow for both entry points

- **Decision**: Keep completion state transition logic in the existing backend completion command/handler and expose it to both home and active workout detail UI surfaces.
- **Rationale**: Lifecycle state validity already exists and should remain single-source to prevent rule drift across screens.
- **Alternatives considered**:
  - Add frontend-only completion toggles per page: rejected due to ghost-state risk and constitution violation.
  - Create separate completion endpoints for each UI surface: rejected because behavior duplication increases regression risk.

## Decision 2: Add a dedicated completed-workout listing endpoint

- **Decision**: Introduce a dedicated API read operation for completed workouts to drive the Workout History page.
- **Rationale**: The existing API supports active and by-id reads, but not a list of completed sessions; a focused endpoint keeps contracts explicit and efficient.
- **Alternatives considered**:
  - Fetch all workouts and filter client-side: rejected due to unnecessary payload and unclear API intent.
  - Overload active-workout endpoint: rejected because active and history use distinct query semantics.

## Decision 3: Keep history row payload minimal (label + completed date)

- **Decision**: History list contract returns only the fields needed for this slice, including fallback-safe label support and completion timestamp/date.
- **Rationale**: Matches scope constraints and principle of just-enough history for immediate value.
- **Alternatives considered**:
  - Include full workout details and sets in history rows: rejected as out of scope and likely to trigger broader redesign.
  - Include analytics-derived fields: rejected because feature excludes analytics expansion.

## Decision 4: Default completed-workout ordering to most recent first

- **Decision**: Sort completed history by completion timestamp descending.
- **Rationale**: Recency-first supports fast recall of latest training without adding filtering controls.
- **Alternatives considered**:
  - Ascending order: rejected because older sessions are less useful in immediate gym decisions.
  - User-configurable sort: rejected as scope expansion beyond this slice.

## Decision 5: Surface completion from active workout detail without adding new rules

- **Decision**: Add/enable a Complete Workout action in active workout detail that reuses the same completion operation used on home.
- **Rationale**: Meets acceptance criteria while preserving consistent lifecycle semantics.
- **Alternatives considered**:
  - Keep completion only on home: rejected because user explicitly requested both entry points.
  - Add confirmation dialog only in detail: rejected for inconsistent behavior and added tap friction.

## Decision 6: Failure and race handling should reconcile from authoritative backend state

- **Decision**: On 404/409 or stale outcomes, refresh relevant workout state and show explicit feedback rather than optimistic state assumptions.
- **Rationale**: Ensures UI never claims completion that did not persist, especially under weak connectivity.
- **Alternatives considered**:
  - Optimistic completion with rollback: rejected due to confusing flashes and trust erosion.
  - Silent retry loop without user feedback: rejected because users need immediate clarity.

## Decision 7: Testing strategy covers API contracts, lifecycle rules, and mobile UX

- **Decision**: Expand tests across backend unit/integration/contract layers and frontend unit/e2e for completion entry points and history rendering.
- **Rationale**: Feature spans domain lifecycle behavior and user-visible flows, requiring layered verification.
- **Alternatives considered**:
  - API-only testing: rejected because UI routing/rendering is a primary acceptance target.
  - UI-only testing: rejected because lifecycle correctness depends on backend rule coverage.
