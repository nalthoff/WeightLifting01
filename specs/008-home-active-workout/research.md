# Phase 0 Research: Home active workout summary and quick completion

## Decision 1: Home shows active-workout card only when an in-progress workout exists

- **Decision**: Render the home summary card conditionally from authoritative active-workout state; no card when no active workout exists.
- **Rationale**: Keeps home clear and avoids presenting inactive controls when there is nothing to continue/complete.
- **Alternatives considered**:
  - Always show a placeholder card: rejected as unnecessary visual noise in the primary flow.
  - Show only a text banner: rejected because action discoverability is weaker than a focused card.

## Decision 2: Continue action reuses existing workout detail route

- **Decision**: Continue from home navigates to the existing active workout route using current workout id.
- **Rationale**: Preserves established flow and avoids duplicate detail logic.
- **Alternatives considered**:
  - Build a separate mini-detail on home: rejected for scope creep and behavior drift.
  - Open a modal summary instead of routing: rejected because it does not align with current detail flow.

## Decision 3: Completion is immediate with no confirmation

- **Decision**: Complete action triggers immediate completion request from home without confirmation dialog.
- **Rationale**: Matches explicit product input and one-tap speed objective.
- **Alternatives considered**:
  - Require confirmation prompt: rejected because it adds friction and contradicts requested behavior.
  - Long-press or gesture confirmation: rejected as less discoverable and not required.

## Decision 4: Completion source of truth stays in backend lifecycle rules

- **Decision**: Home completion calls backend-owned workout lifecycle logic; UI only reflects outcomes.
- **Rationale**: Aligns with constitution requirement that business rules live outside Angular presentation.
- **Alternatives considered**:
  - Client-side completion state toggle first: rejected due to ghost-completion risk.
  - Split completion validation across frontend/backend: rejected for consistency and testability concerns.

## Decision 5: Home state updates only from confirmed outcomes

- **Decision**: Remove active card and show success only after completion success; retain card and show error on failure.
- **Rationale**: Prevents users from believing a failed completion was saved.
- **Alternatives considered**:
  - Optimistic remove with rollback: rejected because rollback flashes degrade trust during weak connectivity.
  - Silent retries without user signal: rejected because users need immediate clarity.

## Decision 6: Race and stale-state handling resolves to authoritative status

- **Decision**: When completion/continue hits stale state, home refreshes active-workout state and surfaces recoverable feedback.
- **Rationale**: Handles real-world concurrency/network timing without leaving ambiguous UI.
- **Alternatives considered**:
  - Hard failure without refresh: rejected because UI can remain stale and confusing.
  - Force full app reload: rejected as unnecessary disruption in mobile flow.

## Decision 7: Testing focuses on home action behavior plus lifecycle boundaries

- **Decision**: Add frontend unit/e2e coverage for card visibility and action outcomes, plus backend unit/integration/contract coverage for completion behavior exposed to home.
- **Rationale**: Feature spans UI actions and lifecycle transitions; layered tests reduce regression risk.
- **Alternatives considered**:
  - Frontend-only tests: rejected because backend lifecycle is the source of truth.
  - Backend-only tests: rejected because home UX behavior and feedback timing are core requirements.
