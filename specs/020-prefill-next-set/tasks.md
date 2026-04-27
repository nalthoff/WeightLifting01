# Tasks: Prefill next set defaults

**Input**: Design documents from `/specs/020-prefill-next-set/`  
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Include behavior-focused frontend unit tests for add-set success/failure prefill behavior and regression checks for per-entry isolation.

**Organization**: Tasks are grouped by user story so each story remains independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency collisions)
- **[Story]**: User story label (`[US1]`, `[US2]`)
- Every task includes an exact file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm feature docs and test harness targets before code changes.

- [X] T001 Confirm active feature context and docs in `.specify/feature.json` and `specs/020-prefill-next-set/plan.md`
- [X] T002 Capture current add-set behavior baseline in `frontend/src/app/features/workouts/active-workout-page.component.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Prepare shared test scaffolding for add-set behavior validation.

- [X] T003 Add add-set success/failure fixture helpers in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T004 [P] Add per-entry draft-state assertion helpers in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`

**Checkpoint**: Foundational test scaffolding is ready for story implementation.

---

## Phase 3: User Story 1 - Faster Repeated Set Logging (Priority: P1) 🎯 MVP

**Goal**: After a successful add-set action, the same lift entry's next set draft is prefilled from the saved values.

**Independent Test**: Save a set in one lift entry and verify the same entry's draft keeps reps/weight while other entries remain unchanged.

### Tests for User Story 1

- [X] T005 [US1] Add unit test for success-prefill behavior in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T006 [US1] Add unit test for per-entry isolation behavior in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T007 [US1] Add unit test for blank-weight prefill behavior in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`

### Implementation for User Story 1

- [X] T008 [US1] Update add-set success draft handling in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T009 [US1] Preserve editable prefilled values and existing validations in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: User Story 1 works independently with passing unit tests.

---

## Phase 4: User Story 2 - Reliable Prefill During Logging Errors (Priority: P2)

**Goal**: Failed add-set attempts never overwrite or clear user-entered drafts.

**Independent Test**: Trigger add-set failure and verify the current draft remains unchanged, then retry successfully and confirm defaults update only on success.

### Tests for User Story 2

- [X] T010 [US2] Add unit test for failure-preserves-draft behavior in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T011 [US2] Add unit test for failed-then-successful-retry behavior in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`

### Implementation for User Story 2

- [X] T012 [US2] Ensure add-set error paths keep existing draft untouched in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: User Story 2 works independently with passing unit tests.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate full feature behavior and documentation alignment.

- [X] T013 [P] Run targeted frontend unit tests for active workout set flow in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T014 [P] Verify quickstart scenarios and update notes in `specs/020-prefill-next-set/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1**: No dependencies.
- **Phase 2**: Depends on Phase 1; blocks story work.
- **Phase 3 (US1)**: Depends on Phase 2 completion.
- **Phase 4 (US2)**: Depends on US1 behavior in Phase 3.
- **Phase 5**: Depends on completion of US1 and US2.

### User Story Dependencies

- **US1 (P1)**: Can start once foundational scaffolding is complete.
- **US2 (P2)**: Depends on US1 success-path behavior to validate retry flow.

### Parallel Opportunities

- T003 and T004 can be executed together once setup is complete.
- T013 and T014 can be executed together in polish phase.

---

## Parallel Example: User Story 1

```bash
Task: "T005 Add unit test for success-prefill behavior in frontend/tests/unit/workouts/active-workout-page.component.spec.ts"
Task: "T008 Update add-set success draft handling in frontend/src/app/features/workouts/active-workout-page.component.ts"
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Setup + Foundational phases.
2. Implement US1 tests and code changes.
3. Validate targeted unit tests.

### Incremental Delivery

1. Deliver US1 success-path prefill and isolation.
2. Add US2 failure/retry safety behavior validation.
3. Complete polish test run and quickstart verification.

### Notes

- Maintain per-entry draft boundaries keyed by workout-lift entry id.
- Keep production changes limited to active workout component behavior.
- Mark each task complete (`[X]`) as implementation proceeds.
