# Phase 0 Research: Workout history row summary details

## Decision 1: Calculate duration in backend history projection

- **Decision**: Compute workout duration from `startedAtUtc` and `completedAtUtc` in backend history query/projection and return a display-ready `HH:MM` value in the history API item.
- **Rationale**: Duration is a business-facing history rule that should be consistent across clients and testable in backend unit/integration tests.
- **Alternatives considered**:
  - Calculate duration in Angular only: rejected because it duplicates business logic and risks format drift.
  - Return raw timestamps only and let each client derive duration: rejected due to cross-client inconsistency risk.

## Decision 2: Include lift count in history endpoint payload

- **Decision**: Extend completed-workout history item payload with `liftCount` derived from number of workout lifts for each completed workout.
- **Rationale**: The row requires number of lifts and this value belongs in the history summary contract so UI remains presentation-only.
- **Alternatives considered**:
  - Fetch lift counts per row with additional requests: rejected due to unnecessary network overhead and complex UI orchestration.
  - Count set rows instead of lifts: rejected because the feature explicitly defines exercise count as lift count.

## Decision 3: Keep ordering and completed-only filtering unchanged

- **Decision**: Preserve existing completed-workout filtering and `completedAtUtc DESC` ordering while adding new summary fields.
- **Rationale**: This avoids regressions in already-shipped behavior and matches accepted scope boundaries.
- **Alternatives considered**:
  - Add alternate sort/filter controls now: rejected as scope expansion.
  - Rework history query structure broadly: rejected as unnecessary risk for a targeted enhancement.

## Decision 4: Safe fallback for invalid or missing timestamps

- **Decision**: If duration cannot be computed safely due to invalid or missing timestamps, return a stable fallback duration display value and continue rendering the row.
- **Rationale**: The spec requires resilience and no page crashes when history data quality is imperfect.
- **Alternatives considered**:
  - Drop invalid rows from history: rejected because users could lose visibility of completed sessions.
  - Surface hard error and block page render: rejected due to poor gym usability and excessive disruption.

## Decision 5: Testing strategy prioritizes business rule correctness plus UI regression

- **Decision**: Add backend unit/integration/contract tests for duration/lift count/ordering and frontend unit/e2e tests for row rendering and legacy empty/error behavior.
- **Rationale**: The enhancement changes API shape and visible UI fields, so layered tests are needed to prevent regressions.
- **Alternatives considered**:
  - UI-only tests: rejected because duration/count rules originate in backend logic.
  - Backend-only tests: rejected because acceptance criteria require row-level UI validation.
