# Phase 0 Research: Add per-lift set logging

## Decision 1: Add Set is scoped to one workout-lift entry

- **Decision**: Add-set actions target a specific workout-lift entry id and never a workout-level global set collection.
- **Rationale**: The feature requires duplicate lift entries in one workout to maintain independent set lists and numbering.
- **Alternatives considered**:
  - Workout-global set list: rejected because duplicate entries would share state incorrectly.
  - Lift-id-scoped set list: rejected because two instances of the same lift in one workout cannot be safely isolated.

## Decision 2: Numbering is backend-authoritative and sequential per entry

- **Decision**: Set number assignment is generated in backend application/domain logic from persisted state per workout-lift entry.
- **Rationale**: Backend ownership avoids race-condition drift and keeps numbering consistent across refresh/navigation and multi-request scenarios.
- **Alternatives considered**:
  - Frontend-only numbering: rejected because stale client state can produce duplicate/skipped numbers.
  - Client-provided set numbers: rejected because validation complexity and trust issues increase.

## Decision 3: Save-first behavior without optimistic ghost rows

- **Decision**: A new row is displayed as persisted only after successful save response; failures show explicit feedback and preserve prior visible state.
- **Rationale**: The spec requires clear trust boundaries in weak connectivity and explicitly forbids ghost saved rows.
- **Alternatives considered**:
  - Fully optimistic UI with rollback: rejected because temporary false-success rows reduce trust mid-workout.
  - Silent background retries only: rejected because users cannot determine whether work was actually recorded.

## Decision 4: Data model introduces workout-set persistence with migration

- **Decision**: Persist workout set entries in SQL with explicit schema changes and versioned migration artifacts.
- **Rationale**: Feature scope includes durable set logging and constitution requires migration discipline for persisted model changes.
- **Alternatives considered**:
  - In-memory/session-only sets: rejected because data loss on refresh/navigation violates requirements.
  - Reusing unrelated existing tables without explicit set entity: rejected because entity boundaries and validation clarity degrade.

## Decision 5: Failure outcomes map to explicit API responses

- **Decision**: Add-set endpoint returns clear outcome classes (success, validation failure, state conflict, not found, server error) with user-facing error messaging support.
- **Rationale**: Explicit outcomes are needed to provide trustworthy failure feedback and keep list/numbering state consistent.
- **Alternatives considered**:
  - Generic 500 for all failures: rejected because it prevents actionable feedback.
  - Silent no-op on invalid requests: rejected because users would assume saves succeeded.

## Decision 6: Test strategy covers rule correctness and mobile UX trust

- **Decision**: Add backend unit tests for numbering and validation rules, integration tests for persistence/order integrity, contract tests for endpoint outcomes, and mobile e2e tests for happy/failure/duplicate-entry flows.
- **Rationale**: Correctness depends on backend rule ownership and visible no-ghost behavior in the active workout UI.
- **Alternatives considered**:
  - Frontend-only testing: rejected because persistence and business rules would be unverified.
  - Unit-only backend tests: rejected because API contracts and end-to-end user trust behaviors remain unproven.
