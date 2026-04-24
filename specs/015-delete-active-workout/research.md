# Phase 0 Research: Delete in-progress workout

## Decision 1: Require explicit confirmation before hard delete

- **Decision**: The delete action on the active workout page opens a confirmation step, and no deletion request is sent until the user explicitly confirms.
- **Rationale**: Confirmation satisfies destructive-action safety requirements while keeping the mobile flow short.
- **Alternatives considered**:
  - Single-tap immediate delete: rejected due to high accidental-loss risk.
  - Separate full-screen delete flow: rejected due to extra navigation friction during in-gym use.

## Decision 2: Keep workout lifecycle and delete eligibility backend-authoritative

- **Decision**: Backend application/domain logic decides whether a workout can be deleted (in-progress only) and returns authoritative outcomes for not-found/conflict/success.
- **Rationale**: Centralized rules prevent client drift and align with constitution boundaries.
- **Alternatives considered**:
  - Frontend-only eligibility checks: rejected because stale client state can produce incorrect destructive behavior.
  - Persistence-only checks without application-level command handling: rejected because it weakens testability and rule discoverability.

## Decision 3: Hard-delete workout aggregate and associated in-progress data

- **Decision**: Confirmed deletion permanently removes the workout and associated workout-lift and workout-set rows.
- **Rationale**: User requirement explicitly calls for irreversible discard with no archive/recovery path.
- **Alternatives considered**:
  - Soft-cancel (`Cancelled` status) with hidden history: rejected because user requested hard-delete.
  - Trash/recycle-bin retention: rejected because out of scope.

## Decision 4: Preserve completed-history behavior by design

- **Decision**: History endpoint behavior remains focused on completed workouts; deleted in-progress workouts naturally disappear because they no longer exist.
- **Rationale**: Meets requirement to avoid history regressions and aligns with decision-ready history principle.
- **Alternatives considered**:
  - Adding history filters for deleted sessions: rejected because deleted records are not retained and filter UI is out of scope.
  - Broader history redesign: rejected as unnecessary scope expansion.

## Decision 5: Concurrency handling prevents duplicate destructive requests

- **Decision**: Frontend blocks repeated confirm submissions while deletion is in-flight; backend still handles stale or duplicate timing safely via authoritative outcomes.
- **Rationale**: Avoids conflicting UI states and improves user trust.
- **Alternatives considered**:
  - Allow repeated taps and rely only on backend behavior: rejected due to noisy/confusing feedback.
  - Lock the entire screen during delete: rejected because it over-constrains the logging experience.

## Decision 6: Testing strategy covers destructive safety and state trust

- **Decision**: Add backend unit tests for delete rules, integration tests for aggregate removal semantics, contract tests for endpoint outcomes, and frontend unit/e2e tests for confirmation, cancel, success, and failure paths.
- **Rationale**: The feature risk is destructive state change, so cross-layer verification is required.
- **Alternatives considered**:
  - Unit-only backend coverage: rejected because it misses user-visible confirmation and feedback behavior.
  - E2E-only coverage: rejected because it slows diagnosis and misses contract guarantees.
