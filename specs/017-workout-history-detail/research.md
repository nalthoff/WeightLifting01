# Phase 0 Research: Workout history detail flow

## Decision 1: Reuse existing workout and lift endpoints for completed detail

- **Decision**: Build completed-workout detail using existing `GET /api/workouts/{workoutId}` for workout metadata and `GET /api/workouts/{workoutId}/lifts` for lift/set rows.
- **Rationale**: Current contracts already return required fields (date/timestamps/label plus lift sets with weight/reps), minimizing risk and preserving backend rule ownership.
- **Alternatives considered**:
  - Add a new combined detail endpoint: rejected for this slice because it adds contract surface and duplicate mapping logic without clear user-facing benefit.
  - Load detail from history payload only: rejected because history summary payload intentionally remains lightweight and does not include per-lift set rows.

## Decision 2: Add history-row navigation into a read-only detail route

- **Decision**: Extend history row UI to navigate into a dedicated completed-workout detail view keyed by selected workout id.
- **Rationale**: Meets one-tap open requirement while keeping existing history listing behavior and enabling clear back-to-history flow.
- **Alternatives considered**:
  - Expand history row inline: rejected because dense mobile layout and nested set rendering hurt scanability.
  - Reuse active workout page directly: rejected because that page includes in-progress editing workflows out of scope for read-only completed history review.

## Decision 3: Keep completed-workout detail strictly read-only

- **Decision**: Completed detail view renders summary, lifts, and sets but excludes edit/delete/reorder interactions.
- **Rationale**: Directly matches scope boundaries and avoids accidental cross-over into active-workout modification behaviors.
- **Alternatives considered**:
  - Allow limited set edits for completed sessions: rejected as scope expansion and potential rule conflict.
  - Add completion lifecycle controls in detail view: rejected because this flow is for historical review.

## Decision 4: Preserve existing history list constraints and states

- **Decision**: Maintain completed-only and newest-first history list behavior, including existing empty and load-error states, while adding row click affordance.
- **Rationale**: Prevents regression in already-delivered history outcomes and aligns with explicit acceptance criteria.
- **Alternatives considered**:
  - Add filters/sorting in the same release: rejected as non-essential to the requested outcome.
  - Rework history query semantics: rejected due to regression risk for low value.

## Decision 5: Use resilient loading/error handling for detail open flow

- **Decision**: Show explicit loading state while fetching detail and actionable error states for not-found/connectivity failures with a clear path back to history.
- **Rationale**: Supports mobile gym usage where connectivity may be unreliable and prevents silent failure.
- **Alternatives considered**:
  - Generic error toast only: rejected as insufficiently actionable for recovery.
  - Hard navigation failure with blank page: rejected because it degrades trust and usability.

## Decision 6: No schema migration in this phase

- **Decision**: Do not modify SQL schema or add migrations for this feature.
- **Rationale**: Required detail fields are already persisted and exposed by existing query models.
- **Alternatives considered**:
  - Add precomputed detail projection columns: rejected as unnecessary complexity.
  - Introduce caching tables for history detail: rejected due to no demonstrated performance need.

## Decision 7: Test strategy covers navigation, display correctness, and regressions

- **Decision**: Add frontend unit/e2e coverage for history-to-detail navigation and rendering states, plus backend integration/contract checks only where contract behavior is tightened (not expanded).
- **Rationale**: Primary risk is user-facing navigation/rendering regression, while backend contracts are largely reused.
- **Alternatives considered**:
  - Frontend-only testing: rejected because endpoint contract assumptions should remain verifiable.
  - Backend-only testing: rejected because acceptance criteria are route and UI behavior centric.
