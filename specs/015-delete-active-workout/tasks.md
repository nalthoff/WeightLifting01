# Tasks: Delete in-progress workout

**Input**: Design documents from `/specs/015-delete-active-workout/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/delete-workout-api.yaml`

**Tests**: Backend unit tests are required for workout-delete business rules. Add integration, contract, frontend unit, and e2e coverage for destructive confirmation, success, and failure paths.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm current active-workout lifecycle touchpoints before introducing destructive workout deletion.

- [ ] T001 Review active workout completion/state handling in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [ ] T002 [P] Review workout session API contracts and client usage in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/`, `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`, and `frontend/src/app/core/api/workouts-api.service.ts`
- [ ] T003 [P] Review active workout persistence and query patterns in `backend/src/WeightLifting.Api/Infrastructure/Persistence/`, `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetActiveWorkoutSummary/`, and `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetWorkoutById/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared delete-workout command/contracts plumbing that all user stories depend on.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [ ] T004 Create delete-workout API response contract in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/DeleteWorkoutResponse.cs`
- [ ] T005 Create delete-workout command models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkout/DeleteWorkoutCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkout/DeleteWorkoutOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkout/DeleteWorkoutResult.cs`
- [ ] T006 [P] Register delete-workout command handler dependency in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T007 [P] Add delete-workout API client method in `frontend/src/app/core/api/workouts-api.service.ts`
- [ ] T008 [P] Extend active workout store for post-delete clear/reconcile flow in `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Confirm before deleting active workout (Priority: P1) 🎯 MVP

**Goal**: Let users trigger Delete Workout and require explicit confirmation before any destructive action.

**Independent Test**: From active workout page with an in-progress workout, open delete confirmation and cancel it, verifying no mutation occurs.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add frontend unit tests for delete confirmation open/cancel behavior in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`
- [ ] T010 [P] [US1] Add e2e confirmation-cancel scenario in `frontend/tests/e2e/workouts/delete-workout-cancel.spec.ts`

### Implementation for User Story 1

- [ ] T011 [US1] Add active workout delete confirmation UI state model in `frontend/src/app/core/state/workouts-store.models.ts`
- [ ] T012 [US1] Implement delete confirmation state transitions in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T013 [US1] Render Delete Workout trigger and confirmation actions in `frontend/src/app/features/workouts/active-workout-page.component.html`
- [ ] T014 [US1] Add delete confirmation visual treatment and mobile layout support in `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Permanently delete confirmed in-progress workout (Priority: P1)

**Goal**: Permanently delete confirmed in-progress workout data and clear active state with success guidance.

**Independent Test**: Confirm delete on active workout and verify workout aggregate cannot be read as active/history afterward.

### Tests for User Story 2

- [ ] T015 [P] [US2] Add unit tests for delete-workout success and in-progress enforcement in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkout/DeleteWorkoutCommandHandlerTests.cs`
- [ ] T016 [P] [US2] Add integration test for hard-delete of workout aggregate and child rows in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutIntegrationTests.cs`
- [ ] T017 [P] [US2] Add contract test for `DELETE /api/workouts/{workoutId}` success response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [ ] T018 [P] [US2] Add e2e happy-path delete workout scenario in `frontend/tests/e2e/workouts/delete-workout.spec.ts`

### Implementation for User Story 2

- [ ] T019 [US2] Implement delete-workout command handler with in-progress gating and aggregate removal in `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkout/DeleteWorkoutCommandHandler.cs`
- [ ] T020 [US2] Add delete-workout endpoint and success mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T021 [US2] Implement confirmed delete orchestration and success messaging in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T022 [US2] Wire post-delete active-state refresh behavior in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Handle delete failures and race conditions safely (Priority: P2)

**Goal**: Provide clear recoverable feedback for delete failures and stale-state conflicts while preventing false-success states.

**Independent Test**: Simulate server failure and stale-state conflict during delete and verify clear feedback with authoritative state reconciliation.

### Tests for User Story 3

- [ ] T023 [P] [US3] Extend backend unit tests for `NotFound`, `Conflict`, and failure outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkout/DeleteWorkoutCommandHandlerTests.cs`
- [ ] T024 [P] [US3] Add contract tests for delete-workout `404` and `409` responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [ ] T025 [P] [US3] Add integration test for stale-state delete behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutIntegrationTests.cs`
- [ ] T026 [P] [US3] Add e2e failure-and-retry delete workout scenario in `frontend/tests/e2e/workouts/delete-workout-failures.spec.ts`

### Implementation for User Story 3

- [ ] T027 [US3] Map delete-workout conflict/not-found/problem outcomes in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T028 [US3] Implement delete failure feedback and race-state reconciliation in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T029 [US3] Render delete error states and retry affordance in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cross-story validation, regression checks, and documentation updates.

- [ ] T030 [P] Update verification instructions and run log in `specs/015-delete-active-workout/quickstart.md`
- [ ] T031 [P] Run backend unit/integration/contract suites for delete-workout behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [ ] T032 [P] Run frontend unit/e2e suites for delete workout flows in `frontend/tests/unit/` and `frontend/tests/e2e/workouts/`
- [ ] T033 Validate mobile viewport behavior for delete confirmation/success/failure in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [ ] T034 Verify completed workout history remains unchanged after deletions in `frontend/src/app/features/history/history-page.component.ts` and `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines safe confirmation MVP.
- **Phase 4 (US2)**: Depends on US1 confirmation baseline and foundational backend plumbing.
- **Phase 5 (US3)**: Depends on US2 delete path to extend failure/race handling.
- **Phase 6 (Polish)**: Depends on all targeted stories being complete.

### User Story Dependencies

- **US1 (P1)**: No dependency on other stories after foundational phase.
- **US2 (P1)**: Uses US1 confirmation interaction and adds destructive backend + success flow.
- **US3 (P2)**: Extends US2 with negative-path outcomes and reconciliation.

### Within Each User Story

- Tests should be authored before implementation and fail first when feasible.
- Backend business logic is implemented before frontend integration for each story.
- Story-specific checkpoint must pass before moving to next priority.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run together (`T006`, `T007`, `T008`).
- US2 tests marked `[P]` can run in parallel (`T015`-`T018`) after foundational setup.
- US3 tests marked `[P]` can run in parallel (`T023`-`T026`) after US2 core behavior lands.
- Polish validation tasks marked `[P]` can run in parallel (`T030`-`T032`).

---

## Parallel Example: User Story 1

```bash
Task: "T009 [US1] Frontend unit tests in frontend/src/app/features/workouts/active-workout-page.component.spec.ts"
Task: "T010 [US1] E2E cancel scenario in frontend/tests/e2e/workouts/delete-workout-cancel.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T015 [US2] Backend unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkout/DeleteWorkoutCommandHandlerTests.cs"
Task: "T016 [US2] Backend integration test in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutIntegrationTests.cs"
Task: "T018 [US2] Frontend e2e happy path in frontend/tests/e2e/workouts/delete-workout.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T024 [US3] Contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs"
Task: "T025 [US3] Integration stale-state test in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutIntegrationTests.cs"
Task: "T026 [US3] Frontend e2e failures in frontend/tests/e2e/workouts/delete-workout-failures.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 confirmation-only safety flow.
3. Validate that cancellation leaves data unchanged.
4. Demo confirmation UX before destructive behavior is enabled.

### Incremental Delivery

1. US1: Add confirmation and safe cancel behavior.
2. US2: Add hard-delete backend + successful delete flow.
3. US3: Add failure/race-condition feedback and reconciliation.
4. Polish with regressions and quickstart validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Then split by story sequence:
   - Dev A: US1 then US2 frontend integration
   - Dev B: US2 backend delete command and endpoint
   - Dev C: US3 failure-path tests and reconciliation behavior
3. Rejoin for final polish and regression execution.
