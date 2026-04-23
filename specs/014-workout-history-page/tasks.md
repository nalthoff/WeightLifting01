# Tasks: Workout history page and completion parity

**Input**: Design documents from `/specs/014-workout-history-page/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workout-history-api.yaml`

**Tests**: Include backend unit tests for completion/history business rules plus integration, contract, frontend unit, and e2e coverage for completion entry points, history rendering, and failure-state reconciliation.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm current completion and navigation extension points before implementation.

- [X] T001 Review completion flow touchpoints in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T002 [P] Review workouts API/store contracts in `frontend/src/app/core/api/workouts-api.service.ts`, `frontend/src/app/core/state/workouts-store.models.ts`, and `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T003 [P] Review backend completion/query patterns in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkout/CompleteWorkoutCommandHandler.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetActiveWorkoutSummary/GetActiveWorkoutSummaryQueryHelper.cs`, and `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared contracts, query scaffolding, and routing foundations needed by all stories.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [X] T004 Create history API response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutHistoryResponse.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutHistoryItemResponse.cs`
- [X] T005 Create completed-workout query model in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/CompletedWorkoutHistoryItem.cs`
- [X] T006 [P] Add completed-workout query helper skeleton in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`
- [X] T007 [P] Add history API client DTOs and method in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T008 [P] Add history route shell in `frontend/src/app/features/history/history-page.component.ts`, `frontend/src/app/features/history/history-page.component.html`, and `frontend/src/app/features/history/history-page.component.scss`
- [X] T009 Register history route navigation entry in `frontend/src/app/app.routes.ts` and `frontend/src/app/app.html`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Complete workout from home and active detail (Priority: P1) 🎯 MVP

**Goal**: Allow users to complete in-progress workouts from both home and active workout detail while persisting completed state and timestamp.

**Independent Test**: Start two separate workouts and complete one from home and one from active detail; both save Completed status, non-null completion timestamp, and remove active state.

### Tests for User Story 1

- [X] T010 [P] [US1] Add backend unit tests for completion lifecycle and timestamp persistence in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs`
- [X] T011 [P] [US1] Add backend integration tests for completion persistence and active-state removal in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/CompleteWorkoutIntegrationTests.cs`
- [X] T012 [P] [US1] Add contract tests for `POST /api/workouts/{workoutId}/complete` success and conflict outcomes in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T013 [P] [US1] Add frontend unit tests for completion from both entry points in `frontend/tests/unit/workouts/active-workout-completion.spec.ts` and `frontend/tests/unit/home/home-active-workout-card.spec.ts`
- [X] T014 [P] [US1] Add e2e completion parity scenario across home and active detail in `frontend/tests/e2e/workouts/complete-workout-entry-points.spec.ts`

### Implementation for User Story 1

- [X] T015 [US1] Ensure completion command handler enforces in-progress-only transitions and persisted completion timestamp in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkout/CompleteWorkoutCommandHandler.cs`
- [X] T016 [US1] Ensure completion endpoint returns authoritative completion outcomes in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T017 [US1] Add complete-workout action and state orchestration to active workout detail in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T018 [US1] Render complete-workout action and loading/feedback states in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T019 [US1] Align home completion reconciliation messaging and active-state refresh in `frontend/src/app/features/home/home-page.component.ts`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - View completed workouts on dedicated history page (Priority: P1)

**Goal**: Provide a dedicated Workout History page listing completed workouts with label and completed date only.

**Independent Test**: Complete a workout, navigate to Workout History, and verify list row shows label (or fallback) and completed date in most-recent-first order.

### Tests for User Story 2

- [X] T020 [P] [US2] Add backend integration tests for completed-workout history query ordering and fields in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`
- [X] T021 [P] [US2] Add backend contract tests for `GET /api/workouts/history` in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T022 [P] [US2] Add frontend unit tests for history list rendering, label fallback, and empty state in `frontend/tests/unit/history/history-page.component.spec.ts`
- [X] T023 [P] [US2] Add e2e completion-to-history journey test in `frontend/tests/e2e/workouts/workout-history.spec.ts`

### Implementation for User Story 2

- [X] T024 [US2] Implement completed-workout history query with recency ordering in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`
- [X] T025 [US2] Add workout history endpoint response mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T026 [US2] Extend workouts API service with history retrieval method and DTO mapping in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T027 [US2] Implement dedicated history page data loading, fallback label handling, and empty state in `frontend/src/app/features/history/history-page.component.ts`
- [X] T028 [US2] Render history list rows with label and completed date only in `frontend/src/app/features/history/history-page.component.html` and `frontend/src/app/features/history/history-page.component.scss`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Clear failure feedback and stale-state reconciliation (Priority: P2)

**Goal**: Ensure completion failures and race conditions are surfaced clearly with authoritative state reconciliation and no false-completed UI.

**Independent Test**: Simulate network/server/stale completion failures from both entry points and verify explicit error feedback, no false completion, and correct refreshed state.

### Tests for User Story 3

- [X] T029 [P] [US3] Add backend unit tests for not-found and conflict completion outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs`
- [X] T030 [P] [US3] Add backend contract tests for completion `404` and `409` responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T031 [P] [US3] Add frontend unit tests for failure feedback and stale-state reconciliation in `frontend/tests/unit/workouts/active-workout-completion.spec.ts` and `frontend/tests/unit/home/home-active-workout-card.spec.ts`
- [X] T032 [P] [US3] Add e2e failure-and-retry completion scenario in `frontend/tests/e2e/workouts/complete-workout-failures.spec.ts`

### Implementation for User Story 3

- [X] T033 [US3] Refine completion error payload mapping for conflict and not-found outcomes in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T034 [US3] Implement active workout detail failure messaging and authoritative state refresh in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T035 [US3] Implement home failure messaging and authoritative reconciliation on stale outcomes in `frontend/src/app/features/home/home-page.component.ts`
- [X] T036 [US3] Add rapid-tap protection and consistent disabled-state behavior for completion actions in `frontend/src/app/features/home/home-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, regression checks, and documentation polish across stories.

- [X] T037 [P] Run backend unit/integration/contract suites for completion/history behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T038 [P] Run frontend unit and e2e suites for completion/history flows in `frontend/tests/unit/` and `frontend/tests/e2e/workouts/`
- [X] T039 [P] Validate quickstart end-to-end verification notes in `specs/014-workout-history-page/quickstart.md`
- [X] T040 Validate mobile viewport usability for completion actions and history list scanning in `frontend/src/app/features/home/home-page.component.html`, `frontend/src/app/features/workouts/active-workout-page.component.html`, and `frontend/src/app/features/history/history-page.component.html`
- [X] T041 Confirm no regressions to start/continue/add-set flows in `frontend/tests/e2e/workouts/start-workout-label.spec.ts`, `frontend/tests/e2e/workouts/home-active-workout.spec.ts`, and `frontend/tests/e2e/workouts/add-workout-set.spec.ts`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP completion parity.
- **Phase 4 (US2)**: Depends on Phase 2 and can run after foundational API/routing scaffolding; functionally independent from US1.
- **Phase 5 (US3)**: Depends on US1 completion entry points and extends failure handling.
- **Phase 6 (Polish)**: Depends on all targeted stories being complete.

### User Story Dependencies

- **US1 (P1)**: Core completion behavior from both entry points with persisted lifecycle transition.
- **US2 (P1)**: Dedicated completed-workout history view; can be built once foundational history scaffolding exists.
- **US3 (P2)**: Builds on US1 completion flow to harden failure and stale-state handling.

### Within Each User Story

- Write required backend business-rule unit tests before implementation.
- Implement backend behavior and API mapping before frontend integration for that story.
- Complete story-level automated tests before marking story done.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T006`, `T007`, `T008`).
- Within US1, tests marked `[P]` can run in parallel (`T010`-`T014`).
- Within US2, tests marked `[P]` can run in parallel (`T020`-`T023`).
- Within US3, tests marked `[P]` can run in parallel (`T029`-`T032`).
- Polish tasks marked `[P]` can run in parallel (`T037`-`T039`).

---

## Parallel Example: User Story 1

```bash
Task: "T010 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs"
Task: "T012 [US1] Contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs"
Task: "T014 [US1] E2E test in frontend/tests/e2e/workouts/complete-workout-entry-points.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T020 [US2] Integration test in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs"
Task: "T022 [US2] Frontend unit test in frontend/tests/unit/history/history-page.component.spec.ts"
Task: "T023 [US2] E2E test in frontend/tests/e2e/workouts/workout-history-page.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T030 [US3] Contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs"
Task: "T031 [US3] Frontend unit tests in frontend/tests/unit/workouts/active-workout-completion.spec.ts"
Task: "T032 [US3] E2E failure test in frontend/tests/e2e/workouts/complete-workout-failures.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 completion parity end-to-end.
3. Validate independent completion from home and active detail.
4. Demo MVP before layering history and hardened failure behavior.

### Incremental Delivery

1. US1: completion parity and persisted lifecycle transition.
2. US2: dedicated history page with minimal row fields.
3. US3: explicit failure handling and stale-state reconciliation.
4. Polish and regression validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. After Phase 2:
   - Developer A: US1 completion parity
   - Developer B: US2 history page and endpoint
3. After US1 lands:
   - Developer C: US3 failure-state hardening
4. Finish with shared polish/regression pass.
