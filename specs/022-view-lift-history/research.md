# Phase 0 Research: View lift history inline

## Decision 1: Use a dedicated exact-lift recent-history query endpoint

- **Decision**: Add a focused endpoint that returns only completed sessions for one exact lift, already limited to the three most recent instances.
- **Rationale**: The active workout screen needs a small, reliable payload for one-tap inline rendering without client-side overfetching and filtering.
- **Alternatives considered**:
  - Reuse full workout history endpoint and filter client-side: rejected because it overfetches and risks inconsistency in exact-lift rules.
  - Reuse workout-detail endpoint repeatedly per completed workout: rejected because it requires multiple calls and adds latency in a mobile flow.

## Decision 2: Keep exact-lift matching on immutable lift identity

- **Decision**: Scope history by lift identity id, not by display name text.
- **Rationale**: Name-based matching can mix similarly named lifts or renamed lifts and break trust in decision guidance.
- **Alternatives considered**:
  - Match by lift display name: rejected because names are mutable and not unique.
  - Match by workout-lift entry id: rejected because entry ids are per-workout and cannot represent cross-session history.

## Decision 3: Keep inline panel state per active workout lift entry

- **Decision**: Track inline history UI state by workout-lift entry id in the active workout page state.
- **Rationale**: A workout can contain repeated entries for the same lift and each row needs independent expand/load/error behavior.
- **Alternatives considered**:
  - Single global panel state: rejected because it cannot safely represent multiple row interactions.
  - Lift-id-only state: rejected because duplicate lift rows in one workout would collide.

## Decision 4: Preserve active entry continuity under failure

- **Decision**: Handle loading, empty, and error feedback inline in each lift section without redirecting or blocking set-entry actions.
- **Rationale**: The primary requirement is uninterrupted workout entry in variable connectivity conditions.
- **Alternatives considered**:
  - Route to dedicated history page on failure: rejected because it violates non-navigation requirement.
  - Show global blocking error banner: rejected because one lift’s history issue should not block entire entry flow.

## Decision 5: No SQL schema migration required

- **Decision**: Implement through query and projection changes only, with no schema updates.
- **Rationale**: Required fields (lift identity, workout completion status/timestamp, set details) already exist in persisted tables.
- **Alternatives considered**:
  - Add denormalized lift-history table: rejected due to complexity and unnecessary write-path impact.
  - Add materialized analytics projections: rejected as out of scope for “just enough history.”
