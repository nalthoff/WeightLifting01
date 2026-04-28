# Phase 0 Research: Backfill past workout

## Decision 1: Represent historical entry as a completed workout anchored to selected training day

- **Decision**: Persist backfilled sessions as completed workouts whose chronology is derived from the user-selected training day.
- **Rationale**: Users care that history reflects when they actually trained, and downstream same-lift context depends on trustworthy ordering.
- **Alternatives considered**:
  - Save as in-progress then auto-complete later: rejected because it introduces intermediate states users did not request.
  - Save as generic note/event separate from workout sessions: rejected because it breaks parity with existing history/detail flows.

## Decision 2: Require date, time, and duration with simple mobile inputs

- **Decision**: Require historical date, start time (hour/minute), and duration for each backfilled workout, with simple entry controls optimized for phone use.
- **Rationale**: Required timing improves chronology trust while preserving ease of use through lightweight inputs.
- **Alternatives considered**:
  - Keep duration optional: rejected because explicit requirement now prioritizes complete timing data.
  - Ask for full timestamp with seconds: rejected because it adds unnecessary entry friction.

## Decision 3: Allow historical completion while another workout remains active

- **Decision**: Support creating and completing a historical workout without ending the existing in-progress workout.
- **Rationale**: Catch-up logging during active sessions is a core requested behavior and prevents forced abandonment of current workout context.
- **Alternatives considered**:
  - Block historical logging when an active workout exists: rejected as direct conflict with user requirement.
  - Auto-close active workout before historical save: rejected due to data-loss risk and surprising behavior.

## Decision 4: Preserve live-workout lift/set entry parity for backfilled sessions

- **Decision**: Use the same lift/set recording model for historical workouts as live workouts.
- **Rationale**: Reusing the familiar entry model reduces cognitive overhead and keeps decision-quality history consistent.
- **Alternatives considered**:
  - Use simplified historical summary form without sets: rejected because it weakens next-weight usefulness.
  - Introduce separate historical-only entry semantics: rejected because duplicated concepts increase confusion and maintenance cost.

## Decision 5: Keep ordering predictable for same-day and day-boundary cases

- **Decision**: Define deterministic ordering behavior when sessions share the same training day, while still honoring user-selected day intent as primary.
- **Rationale**: Predictable tie-breaking is required for trust when multiple sessions occur on one day or when local day boundaries are ambiguous.
- **Alternatives considered**:
  - Leave same-day ordering unspecified: rejected because inconsistent results would reduce confidence in history.
  - Order only by creation time: rejected because it can contradict intended training chronology for backfilled sessions.

## Decision 6: Plan for business-rule tests and no schema changes unless required

- **Decision**: Implement changes primarily in application/domain logic and query projections, explicitly preferring existing data structures and only adding schema changes if requirements cannot be met otherwise.
- **Rationale**: Existing workout persistence should be reused where possible, while timing validation and chronology behavior still require strong backend tests.
- **Alternatives considered**:
  - Frontend-only chronology behavior: rejected because lifecycle and ordering rules belong in backend per constitution.
  - Broad schema redesign up front: rejected as unnecessary before validating minimal rule extension.
