# Phase 0 Research: Reorder workout lifts

## Decision 1: Reordering remains in active workout screen

- **Decision**: Keep reorder interaction within the active workout page instead of a separate management screen.
- **Rationale**: This preserves mobile logging speed and keeps sequencing corrections in the same in-gym workflow context.
- **Alternatives considered**:
  - Dedicated reorder screen: rejected because it adds navigation friction during live workout flow.
  - Settings-based reorder management: rejected because order is session-specific and not a lift-library concern.

## Decision 2: Persist order by entry-instance identity

- **Decision**: Reorder payload and backend behavior target workout-lift entry identifiers, not lift identifiers.
- **Rationale**: Duplicate lift names are valid in one workout and require instance-level precision to avoid accidental cross-entry mutations.
- **Alternatives considered**:
  - Reorder by `liftId`: rejected because duplicates cannot be disambiguated.
  - Reorder by display name or position only: rejected because identity is unstable across concurrent/stale updates.

## Decision 3: Save-on-reorder with authoritative response

- **Decision**: Persist each successful reorder action immediately and return authoritative ordered entries in the response.
- **Rationale**: Immediate persistence satisfies acceptance criteria and supports reliable UI state reconciliation.
- **Alternatives considered**:
  - Save on explicit "Done" action: rejected because it delays persistence and increases unsaved-state risk.
  - Background debounce batching without explicit success state: rejected because it can obscure whether order is actually saved.

## Decision 4: Failure behavior favors trust over optimistic UI

- **Decision**: Show explicit error/conflict feedback on failed save and reconcile UI to authoritative saved order instead of presenting failed order as persisted.
- **Rationale**: Prevents ghost state and aligns with resilient gym connectivity expectations.
- **Alternatives considered**:
  - Fully optimistic reorder with delayed rollback: rejected because temporary false success can mislead users mid-session.
  - Silent retries only: rejected because user cannot tell if sequence is actually saved.

## Decision 5: Reorder scope is current in-progress workout only

- **Decision**: Reordering is permitted only while workout is in progress and never mutates completed/past workouts.
- **Rationale**: Keeps session correction bounded and preserves historical data integrity.
- **Alternatives considered**:
  - Allow historical reorder edits: rejected because it alters historical truth and broadens scope beyond this feature.
  - Propagate order changes to templates/lift defaults: rejected because session order is contextual, not library metadata.

## Decision 6: Test strategy covers rules, persistence, contract, and UX

- **Decision**: Add backend unit tests for reorder validation and outcome rules, integration tests for persistence/order integrity, contract tests for reorder endpoint outcomes, and frontend e2e for mobile reorder success/failure/duplicate handling.
- **Rationale**: Reorder correctness depends on backend rule ownership and visible confidence in the active workout flow.
- **Alternatives considered**:
  - Frontend-only tests: rejected because backend order persistence/conflict semantics would be unverified.
  - Unit tests only: rejected because API contract and end-to-end no-ghost behavior would remain at risk.
