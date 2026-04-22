# Tasks: Start blank workout session

**Input**: Design documents from `/specs/006-start-blank-workout/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/workouts-api.yaml

**Tests**: Unit tests are required for new backend business rules. Add integration, contract, and e2e checks where behavior crosses API/UI boundaries.

**Organization**: Tasks are grouped by user story so each story stays independently buildable and testable.

## Format: `[ID] [P?] [Story] Description`

- `[P]` = parallelizable (different files, no dependency)
- `[Story]` = user story label (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Keep implementation simple by reusing established lifts patterns and test harnesses.

- [X] T001 Review and align new workout API naming with existing lift patterns in `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [X] T002 Review frontend route/home composition for reuse points in `frontend/src/app/app.routes.ts` and `frontend/src/app/features/home/home-page.component.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core workout session foundation required before user stories.

- [X] T003 Create workout status enum in `backend/src/WeightLifting.Api/Domain/Workouts/WorkoutStatus.cs`
- [X] T004 Create workout domain model in `backend/src/WeightLifting.Api/Domain/Workouts/Workout.cs`
- [X] T005 Create workout persistence entity in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Workouts/WorkoutEntity.cs`
- [X] T006 Update DB context mapping for workouts in `backend/src/WeightLifting.Api/Infrastructure/Persistence/WeightLiftingDbContext.cs`
- [X] T007 Add EF migration for workouts table and indexes in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Migrations/`
- [X] T008 Create base API contract models for workouts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/`
- [X] T009 Register workout handlers/services in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T010 Enable automatic EF migrations during API startup in `backend/src/WeightLifting.Api/Program.cs`

**Checkpoint**: Workout persistence and wiring are in place; story work can begin.

---

## Phase 3: User Story 1 - Start a workout from home in the gym (Priority: P1) 🎯 MVP

**Goal**: Start an in-progress workout from home and land in a minimal active-session screen with server-authoritative start time.

**Independent Test**: From home on phone viewport, tap Start Workout and verify persisted `InProgress` session with non-null UTC start timestamp.

### Tests for User Story 1

- [X] T011 [P] [US1] Add unit tests for start command happy path in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/StartWorkout/StartWorkoutCommandHandlerTests.cs`
- [X] T012 [P] [US1] Add integration test for POST start success in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/StartWorkoutIntegrationTests.cs`
- [X] T013 [P] [US1] Add contract test for `201 Created` response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T014 [P] [US1] Add frontend e2e happy-path start test in `frontend/e2e/workouts/start-workout.spec.ts`

### Implementation for User Story 1

- [X] T015 [P] [US1] Implement start command and result models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/StartWorkout/`
- [X] T016 [US1] Implement start command handler (create + UTC start assignment) in `backend/src/WeightLifting.Api/Application/Workouts/Commands/StartWorkout/StartWorkoutCommandHandler.cs`
- [X] T017 [US1] Implement workouts start endpoint in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T018 [P] [US1] Add frontend workouts API client methods in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T019 [P] [US1] Add workout session state store in `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T020 [US1] Add Start Workout entry to home page in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/features/home/home-page.component.html`
- [X] T021 [US1] Create minimal active-session screen in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T022 [US1] Add workouts routes and navigation in `frontend/src/app/features/workouts/workouts.routes.ts` and `frontend/src/app/app.routes.ts`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Name the workout at start (Priority: P2)

**Goal**: Support optional session label while preserving fast start.

**Independent Test**: Start with label and with whitespace-only input; both succeed, whitespace persists as no label.

### Tests for User Story 2

- [X] T023 [P] [US2] Add unit tests for label normalization rules in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/StartWorkout/StartWorkoutCommandHandlerTests.cs`
- [X] T024 [P] [US2] Add integration test for optional label behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/StartWorkoutIntegrationTests.cs`
- [X] T025 [P] [US2] Add frontend e2e label entry test in `frontend/e2e/workouts/start-workout-label.spec.ts`

### Implementation for User Story 2

- [X] T026 [US2] Add backend label validation/normalization in `backend/src/WeightLifting.Api/Application/Workouts/Commands/StartWorkout/StartWorkoutCommandHandler.cs`
- [X] T027 [US2] Add optional label input on home start flow in `frontend/src/app/features/home/home-page.component.html` and `frontend/src/app/features/home/home-page.component.ts`
- [X] T028 [US2] Show saved label on active-session screen in `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Handle active workout conflicts safely (Priority: P2)

**Goal**: Enforce one in-progress workout and route users to continue existing session.

**Independent Test**: With one in-progress session already present, Start Workout prompts continue behavior and does not create a duplicate.

### Tests for User Story 3

- [X] T029 [P] [US3] Add unit tests for one-active-workout rule in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/StartWorkout/StartWorkoutCommandHandlerTests.cs`
- [X] T030 [P] [US3] Add integration test for conflict response in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/StartWorkoutIntegrationTests.cs`
- [X] T031 [P] [US3] Add contract test for `409 Conflict` response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T032 [P] [US3] Add frontend e2e continue-existing prompt test in `frontend/e2e/workouts/start-workout-conflict.spec.ts`

### Implementation for User Story 3

- [X] T033 [US3] Implement active-workout lookup and conflict outcome in `backend/src/WeightLifting.Api/Application/Workouts/Commands/StartWorkout/StartWorkoutCommandHandler.cs`
- [X] T034 [US3] Return structured `409` continue payload in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T035 [US3] Add continue-existing-session prompt handling in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/features/home/home-page.component.html`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Keep UX clear on flaky connectivity and finalize verification.

- [X] T036 [P] Add backend integration test coverage for timeout/error semantics in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/StartWorkoutIntegrationTests.cs`
- [X] T037 [P] Add explicit frontend error-state messaging for failed starts in `frontend/src/app/features/home/home-page.component.html` and `frontend/src/app/features/home/home-page.component.ts`
- [X] T038 Run and document quickstart verification in `specs/006-start-blank-workout/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 -> no dependencies
- Phase 2 -> depends on Phase 1
- Phase 3 (US1) -> depends on Phase 2
- Phase 4 (US2) -> depends on Phase 3 foundation, but can run in parallel with later US3 tests if capacity allows
- Phase 5 (US3) -> depends on Phase 3 foundation
- Phase 6 -> depends on completed target user stories

### User Story Dependencies

- **US1 (P1)**: No user-story dependency; this is MVP.
- **US2 (P2)**: Reuses US1 start flow and extends with optional label input.
- **US3 (P2)**: Reuses US1 start endpoint and adds conflict branch.

### Within Each User Story

- Prefer test-first for backend business rules.
- Implement backend rule/endpoint before frontend wiring.
- Finish story-level tests before moving on.

### Parallel Opportunities

- Foundational data + API contract tasks can run in parallel where file boundaries differ (`T003`-`T010`).
- In each story, backend and frontend scaffolding tasks marked `[P]` can run together.
- Contract/integration/e2e tests marked `[P]` can be authored in parallel once API contracts stabilize.

---

## Parallel Example: User Story 1

```bash
Task: "T011 [US1] unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/StartWorkout/StartWorkoutCommandHandlerTests.cs"
Task: "T018 [US1] workouts API client in frontend/src/app/core/api/workouts-api.service.ts"
Task: "T019 [US1] workouts store in frontend/src/app/core/state/workouts-store.service.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate mobile happy path + persisted workout record.
4. Demo/review before adding optional flows.

### Incremental Delivery

1. Add US1 (core start flow).
2. Add US2 (optional label, no extra complexity).
3. Add US3 (conflict handling and continue flow).
4. Finish polish/failure UX and regression checks.

### Keep It Simple / Reuse First

- Reuse existing controller/contract/test patterns from lifts rather than introducing new architectural layers.
- Extend existing home route and app state style instead of adding redundant feature shells.
- Keep active-session MVP minimal (status, start time, optional label) and defer logging details.
