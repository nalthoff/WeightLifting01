# Research: Workout Lifecycle Status Visibility

## Decision 1: Reuse existing lifecycle domain states and completion flow

- **Decision**: Keep `InProgress` and `Completed` as the only lifecycle states and rely on the existing completion transition that sets a completion timestamp.
- **Rationale**: Current domain and API contracts already enforce two-state lifecycle behavior and completion timestamp semantics, so reuse avoids unnecessary migration risk.
- **Alternatives considered**:
  - Add new intermediate statuses (rejected: out of scope and no user value for this feature).
  - Create frontend-only state mapping separate from API lifecycle (rejected: increases drift risk between UI and business logic).

## Decision 2: Implement status visibility as badge presentation in workout detail UIs

- **Decision**: Add a status badge on active workout detail and completed history detail views with consistent user-facing labels.
- **Rationale**: The user problem is immediate clarity; a badge provides at-a-glance status comprehension in mobile contexts with minimal interaction cost.
- **Alternatives considered**:
  - Keep plain text status lines only (rejected: does not satisfy explicit badge requirement).
  - Show status only in list/history rows (rejected: detail views are explicitly in scope and are the decision point for users).

## Decision 3: Preserve history/progress gating by completed status plus completion timestamp

- **Decision**: Keep history/progress eligibility constrained to completed workouts with non-null completion timestamps.
- **Rationale**: Existing history query behavior already aligns with this rule and supports trustworthy progression review.
- **Alternatives considered**:
  - Include in-progress sessions in history with labels (rejected: violates requirement and adds progress noise).
  - Gate by timestamp only without status check (rejected: weaker domain intent and less explicit lifecycle correctness).

## Decision 4: Strengthen regression coverage around lifecycle visibility and gating

- **Decision**: Add focused backend tests for completion and history eligibility and frontend tests for badge rendering and status transitions.
- **Rationale**: Lifecycle regressions directly affect user trust; constitution requires unit-tested business logic changes and mobile verification.
- **Alternatives considered**:
  - Rely on manual verification only (rejected: inadequate for lifecycle rule protection).
  - Add only frontend tests (rejected: does not protect business-layer eligibility and timestamp rules).
