# Tasks: Add lifts to in-progress workout

**Input**: Design documents from `/specs/007-add-workout-lifts/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/workout-lifts-api.yaml

**Tests**: Include unit, integration, contract, and e2e coverage for changed workout-lift behavior.

**Organization**: Tasks are grouped by user story to keep delivery independently testable and simple.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Reuse existing workout/lift patterns and keep architecture simple.

- [X] T001 Review reuse points in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [X] T002 Review active-workout UI integration points in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared workout-lift persistence and core backend wiring before story work.

- [X] T003 Create workout-lift persistence entity in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Workouts/WorkoutLiftEntryEntity.cs`
- [X] T004 Update `WeightLiftingDbContext` mapping for workout-lift entries in `backend/src/WeightLifting.Api/Infrastructure/Persistence/WeightLiftingDbContext.cs`
- [X] T005 Add EF migration for workout-lift association schema in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Migrations/`
- [X] T006 Add workout-lift API contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/`
- [X] T007 Create query/command support models for workout-lift entries in `backend/src/WeightLifting.Api/Application/Workouts/`
- [X] T008 Register workout-lift query/command services in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user stories can proceed.

---

## Phase 3: User Story 1 - Add lifts from active workout screen (Priority: P1) 🎯 MVP

**Goal**: Add lifts from active workout screen via active-lift picker and show entries immediately.

**Independent Test**: On active workout screen, open Add Lift picker, add active lift(s), and verify immediate in-flow visibility without manual refresh.

### Tests for User Story 1

- [X] T009 [P] [US1] Add backend unit tests for add-workout-lift happy path in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutLift/AddWorkoutLiftCommandHandlerTests.cs`
- [X] T010 [P] [US1] Add backend integration tests for add/list workout-lifts in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutLiftsIntegrationTests.cs`
- [X] T011 [P] [US1] Add contract tests for `GET/POST /api/workouts/{workoutId}/lifts` success responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T012 [P] [US1] Add mobile e2e test for Add Lift happy path in `frontend/e2e/workouts/add-workout-lift.spec.ts`

### Implementation for User Story 1

- [X] T013 [P] [US1] Implement add-workout-lift command and result models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/`
- [X] T014 [P] [US1] Implement list-workout-lifts query helper in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListWorkoutLifts/ListWorkoutLiftsQueryHelper.cs`
- [X] T015 [US1] Implement add-workout-lift business handler in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs`
- [X] T016 [US1] Add list/add workout-lifts endpoints in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T017 [P] [US1] Add workout-lifts API client methods in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T018 [P] [US1] Extend workout state for lift entries in `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T019 [US1] Add Add Lift primary action and picker UI in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T020 [US1] Integrate successful add response into immediate active-workout rendering in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US1 independently functional and testable.

---

## Phase 4: User Story 2 - Handle duplicate lift additions intentionally (Priority: P2)

**Goal**: Allow the same lift to be added multiple times in one workout.

**Independent Test**: Add same active lift repeatedly and verify each add persists and appears as separate entry.

### Tests for User Story 2

- [X] T021 [P] [US2] Add unit tests confirming duplicate add is allowed in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutLift/AddWorkoutLiftCommandHandlerTests.cs`
- [X] T022 [P] [US2] Add integration tests for duplicate workout-lift entries in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutLiftsIntegrationTests.cs`
- [X] T023 [P] [US2] Add e2e duplicate-add behavior test in `frontend/e2e/workouts/add-workout-lift-duplicates.spec.ts`

### Implementation for User Story 2

- [X] T024 [US2] Ensure backend add-lift handler does not enforce duplicate blocking in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs`
- [X] T025 [US2] Ensure workout-lift persistence constraints permit duplicates in `backend/src/WeightLifting.Api/Infrastructure/Persistence/WeightLiftingDbContext.cs` and migration files in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Migrations/`
- [X] T026 [US2] Render duplicate workout-lift entries distinctly in active workout UI in `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 independently functional and testable.

---

## Phase 5: User Story 3 - Stay resilient under picker and network edge cases (Priority: P2)

**Goal**: Provide clear empty/error states and prevent ghost additions under failure.

**Independent Test**: Validate empty picker, add failure, and rapid tap behavior with explicit UI outcomes and no unsaved entries shown.

### Tests for User Story 3

- [X] T027 [P] [US3] Add unit tests for active-lift-only and failure-path validation in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutLift/AddWorkoutLiftCommandHandlerTests.cs`
- [X] T028 [P] [US3] Add contract tests for validation/not-found/error payloads in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T029 [P] [US3] Add e2e tests for empty picker and failed add behavior in `frontend/e2e/workouts/add-workout-lift-failures.spec.ts`

### Implementation for User Story 3

- [X] T030 [US3] Enforce active-lift-only selection rule in backend handler in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs`
- [X] T031 [US3] Return clear API error responses for add-lift failure scenarios in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T032 [US3] Implement empty-state guidance for no active lifts in `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T033 [US3] Implement explicit add-failure messaging and prevent ghost list updates in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T034 [US3] Add simple add-action in-flight guard for rapid taps in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US3 independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, simplicity cleanup, and workflow validation.

- [X] T035 [P] Refactor to keep business rules out of Angular components and preserve one-class-per-file organization in `backend/src/WeightLifting.Api/Application/Workouts/` and `frontend/src/app/features/workouts/`
- [X] T036 [P] Run full targeted backend tests for workout and workout-lift flows in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T037 [P] Run frontend build and e2e suites for workout flows from `frontend/package.json` scripts
- [X] T038 Update verification notes in `specs/007-add-workout-lifts/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately
- **Phase 2 (Foundational)**: depends on Phase 1; blocks all stories
- **Phase 3 (US1)**: depends on Phase 2
- **Phase 4 (US2)**: depends on US1 backend add/list baseline
- **Phase 5 (US3)**: depends on US1 baseline; can run after Phase 3
- **Phase 6 (Polish)**: depends on all desired stories complete

### User Story Dependencies

- **US1 (P1)**: MVP and baseline for subsequent stories
- **US2 (P2)**: extends US1 add behavior for duplicates
- **US3 (P2)**: extends US1 flow with resilience/edge handling

### Within Each User Story

- Write backend tests before business-rule implementation.
- Implement backend rules/endpoints before frontend integration.
- Complete story tests before moving to next story.

### Parallel Opportunities

- Foundational tasks touching different files can run in parallel (`T003`, `T006`, `T007`).
- In US1, backend and frontend scaffolding tasks marked `[P]` can run together.
- Contract/integration/e2e tasks marked `[P]` can run in parallel once API contract stabilizes.

---

## Parallel Example: User Story 1

```bash
Task: "T009 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutLift/AddWorkoutLiftCommandHandlerTests.cs"
Task: "T017 [US1] Workout-lifts API client in frontend/src/app/core/api/workout-lifts-api.service.ts"
Task: "T018 [US1] Workout lift state extension in frontend/src/app/core/state/workouts-store.service.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational phases.
2. Deliver US1 add-lift flow end-to-end.
3. Validate mobile happy path and persisted workout-lift visibility.
4. Demo before extending behavior.

### Incremental Delivery

1. US1: add and display workout lifts.
2. US2: duplicate-add acceptance behavior.
3. US3: edge/failure resilience.
4. Polish and full regression checks.

### Simplicity + SOLID Guidance

- Reuse existing workout and lift vertical slices before adding abstractions.
- Keep backend handlers focused on single responsibilities (validation, add, list).
- Keep Angular components focused on presentation and orchestration only.
- Avoid introducing remove/edit workflows in this slice.
