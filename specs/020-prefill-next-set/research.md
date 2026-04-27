# Phase 0 Research: Prefill next set defaults

## Decision 1: Prefill source is the latest successfully saved set response

- **Decision**: Update next-set defaults only from the authoritative set returned after a successful add-set operation.
- **Rationale**: This guarantees predictable behavior and prevents draft drift when save attempts fail or are rejected.
- **Alternatives considered**:
  - Use pre-submit draft values immediately: rejected because failed saves would incorrectly change defaults.
  - Recompute from full set list on every render: rejected because it adds unnecessary complexity with no user-facing benefit.

## Decision 2: Prefill state remains scoped per workout-lift entry

- **Decision**: Keep add-set draft and prefill updates keyed by `workoutLiftEntryId` and never share values across entries.
- **Rationale**: Workouts can contain duplicate lift entries that must remain independent for trustworthy logging.
- **Alternatives considered**:
  - Global workout-level default: rejected because cross-lift leakage would violate scope requirements.
  - Lift-name keyed defaults: rejected because duplicate entries for the same lift name would collide.

## Decision 3: Blank weight is treated as an intentional value

- **Decision**: When a set is saved with no weight, preserve blank weight as the next default while still carrying reps forward.
- **Rationale**: The feature must support bodyweight or unspecified-load sets without forcing unintended numeric defaults.
- **Alternatives considered**:
  - Force zero as default weight: rejected because it changes user intent and adds correction taps.
  - Skip prefill when weight is blank: rejected because reps still benefit from reduced typing.

## Decision 4: Failure behavior preserves current user-entered draft

- **Decision**: On add-set failure outcomes, retain the current draft exactly and only show error feedback.
- **Rationale**: This avoids data loss and keeps retry flow fast under intermittent connectivity.
- **Alternatives considered**:
  - Reset fields on failure: rejected because it creates extra user effort and undermines trust.
  - Replace with last successful defaults on failure: rejected because it discards latest intended user input.

## Decision 5: Coverage emphasizes mobile repeated-set flow and error stability

- **Decision**: Add unit tests for add-set success/failure prefill behavior and targeted integration validation for repeated-set entry speed and isolation.
- **Rationale**: The highest-risk regressions are draft mutation timing, per-entry isolation, and failure-path predictability in the primary gym workflow.
- **Alternatives considered**:
  - E2E-only verification: rejected because draft-state edge cases are faster and more reliably covered in unit tests.
  - Backend-only tests: rejected because the prefill behavior is user-visible frontend state management.
