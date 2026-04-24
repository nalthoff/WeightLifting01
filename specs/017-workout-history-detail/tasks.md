# Tasks: Workout history detail flow

**Input**: Design documents from `/specs/017-workout-history-detail/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Include test tasks because this feature changes user-visible behavior and API usage across frontend/backend boundaries.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes exact file path(s)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared route/component scaffolding and test baselines.

- [ ] T001 Add feature route placeholder for completed workout detail in `frontend/src/app/features/history/history.routes.ts`
- [ ] T002 [P] Create completed workout detail component files in `frontend/src/app/features/history/history-workout-detail-page.component.ts`, `frontend/src/app/features/history/history-workout-detail-page.component.html`, and `frontend/src/app/features/history/history-workout-detail-page.component.scss`
- [ ] T003 [P] Add reusable display helpers for completed detail formatting in `frontend/src/app/features/history/history-workout-detail-page.component.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish backend/frontend retrieval foundations required by all user stories.

**CRITICAL**: User story work starts after this phase.

- [ ] T004 Add completed-workout read guard for workout detail retrieval in `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetWorkoutById/GetWorkoutByIdQueryHelper.cs`
- [ ] T005 Add completed-workout read guard for lift/set retrieval in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListWorkoutLifts/ListWorkoutLiftsQueryHelper.cs`
- [ ] T006 Update workout detail endpoint error handling for unavailable/non-completed history workout access in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T007 [P] Extend frontend API service typing for completed detail load and not-found handling in `frontend/src/app/core/api/workouts-api.service.ts` and `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [ ] T008 [P] Add backend integration coverage for completed-only detail retrieval behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`

**Checkpoint**: Detail foundations are ready; user stories can be implemented.

---

## Phase 3: User Story 1 - Open completed workout detail from history (Priority: P1) 🎯 MVP

**Goal**: Open a completed workout detail view from a selected history row.

**Independent Test**: From Workout History, selecting one completed row opens detail for the selected workout in one tap/click.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add history-row navigation unit coverage in `frontend/src/app/features/history/history-page.component.spec.ts`
- [ ] T010 [P] [US1] Add open-from-history e2e flow coverage in `frontend/tests/e2e/workouts/workout-history.spec.ts`

### Implementation for User Story 1

- [ ] T011 [US1] Convert history list rows into detail navigation targets in `frontend/src/app/features/history/history-page.component.html`
- [ ] T012 [US1] Add route parameter mapping and navigation wiring for completed detail in `frontend/src/app/features/history/history.routes.ts`
- [ ] T013 [US1] Implement completed workout bootstrap load by workout id in `frontend/src/app/features/history/history-workout-detail-page.component.ts`
- [ ] T014 [US1] Render completed workout detail shell with back-to-history navigation in `frontend/src/app/features/history/history-workout-detail-page.component.html`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Review completed workout structure and values (Priority: P1)

**Goal**: Show summary, lifts, and set rows with recorded values in read-only detail.

**Independent Test**: Open completed detail and verify date, duration, name/type when present, lift entries, and set rows with reps/weight.

### Tests for User Story 2

- [ ] T015 [P] [US2] Add completed detail summary-and-values unit tests in `frontend/tests/unit/history/history-workout-detail-page.component.spec.ts`
- [ ] T016 [P] [US2] Add backend contract assertions for workout and lift payload compatibility in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutHistoryContractTests.cs`
- [ ] T017 [P] [US2] Extend e2e assertions for lift/set row rendering in `frontend/tests/e2e/workouts/workout-history.spec.ts`

### Implementation for User Story 2

- [ ] T018 [US2] Implement summary section rendering (date/duration/name-type fallback) in `frontend/src/app/features/history/history-workout-detail-page.component.html`
- [ ] T019 [US2] Implement read-only lift and set row rendering with position ordering in `frontend/src/app/features/history/history-workout-detail-page.component.ts` and `frontend/src/app/features/history/history-workout-detail-page.component.html`
- [ ] T020 [US2] Implement safe optional weight fallback display formatting in `frontend/src/app/features/history/history-workout-detail-page.component.ts`
- [ ] T021 [US2] Remove or disable editing affordances for completed workout detail context in `frontend/src/app/features/history/history-workout-detail-page.component.html`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Preserve history list behavior and resilient loading (Priority: P2)

**Goal**: Keep existing history list behavior while adding robust detail loading and recovery states.

**Independent Test**: Validate completed-only newest-first history list still works and detail not-found/network errors are explicit and actionable.

### Tests for User Story 3

- [ ] T022 [P] [US3] Add unit tests for completed detail loading/error/retry state transitions in `frontend/tests/unit/history/history-workout-detail-page.component.spec.ts`
- [ ] T023 [P] [US3] Add e2e coverage for not-found and connectivity failure recovery in `frontend/tests/e2e/workouts/workout-history.spec.ts`
- [ ] T024 [P] [US3] Add backend integration assertions for unavailable workout responses in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`

### Implementation for User Story 3

- [ ] T025 [US3] Preserve existing completed-only newest-first history list behavior while adding row click affordance in `frontend/src/app/features/history/history-page.component.ts` and `frontend/src/app/features/history/history-page.component.html`
- [ ] T026 [US3] Implement explicit loading, not-found, and retry-capable error states in `frontend/src/app/features/history/history-workout-detail-page.component.ts`
- [ ] T027 [US3] Implement resilient error-state UI with actionable return path in `frontend/src/app/features/history/history-workout-detail-page.component.html`
- [ ] T028 [US3] Align API controller problem responses with detail recovery UX expectations in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality checks and documentation alignment across all stories.

- [ ] T029 [P] Update or add quick-reference API contract notes for implementation deltas in `specs/017-workout-history-detail/contracts/workout-history-detail-api.yaml`
- [ ] T030 Run quickstart validation checklist from `specs/017-workout-history-detail/quickstart.md` and capture verification notes in `specs/017-workout-history-detail/quickstart.md`
- [ ] T031 Validate mobile viewport behavior for history and completed detail flows in `frontend/src/app/features/history/history-page.component.scss` and `frontend/src/app/features/history/history-workout-detail-page.component.scss`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: no dependencies
- **Phase 2 (Foundational)**: depends on Phase 1 and blocks all user stories
- **Phase 3 (US1)**: depends on Phase 2
- **Phase 4 (US2)**: depends on Phase 2 and integrates US1 route/component scaffolding
- **Phase 5 (US3)**: depends on Phase 2 and validates no regressions from US1/US2
- **Phase 6 (Polish)**: depends on completion of selected user stories

### User Story Dependencies

- **US1**: first deliverable and MVP slice
- **US2**: depends on US1 navigation entry and detail shell
- **US3**: verifies cross-story resilience and regression protection

### Within Each User Story

- Tests for each story run before or alongside implementation and must pass before story sign-off.
- Backend behavior alignment tasks complete before frontend error-state assumptions are finalized.
- UI rendering tasks complete before e2e assertions are finalized.

### Parallel Opportunities

- T002 and T003 can run in parallel after T001.
- T007 and T008 can run in parallel with T004-T006 once foundational approach is set.
- US1 test tasks T009 and T010 run in parallel.
- US2 test tasks T015-T017 run in parallel.
- US3 test tasks T022-T024 run in parallel.
- Polish tasks T029 and T031 can run in parallel before T030 final validation.

---

## Parallel Example: User Story 2

```bash
# Run US2 verification tasks together:
Task: "T015 [US2] Add completed detail summary-and-values unit tests in frontend/tests/unit/history/history-workout-detail-page.component.spec.ts"
Task: "T017 [US2] Extend e2e assertions for lift/set row rendering in frontend/tests/e2e/workouts/workout-history.spec.ts"

# Build read-only rendering in parallel files:
Task: "T018 [US2] Implement summary section rendering in frontend/src/app/features/history/history-workout-detail-page.component.html"
Task: "T020 [US2] Implement safe optional weight fallback display formatting in frontend/src/app/features/history/history-workout-detail-page.component.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) for one-tap open-from-history.
3. Validate US1 independently via T009-T014 before expanding scope.

### Incremental Delivery

1. Add US1 navigation and detail shell.
2. Add US2 content completeness (summary, lifts, sets, fallback display).
3. Add US3 resilience and regression protections.
4. Finish with polish and quickstart validation.

### Parallel Team Strategy

1. One developer completes backend foundational tasks (T004-T006, T008).
2. One developer completes frontend routing and shell tasks (T001-T003, T011-T014).
3. One developer drives test coverage in parallel across US phases (T009-T010, T015-T017, T022-T024).

---

## Notes

- `[P]` tasks are chosen for separate files and low coupling.
- All tasks follow the required checklist format with task ID and file path.
- Backend unit/integration/contract coverage is included where backend behavior/contracts are touched.
- Completed workout detail remains read-only by design in all phases.
