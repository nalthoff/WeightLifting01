# Tasks: Add per-lift set logging

**Input**: Design documents from `/specs/011-add-per-lift-sets/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workout-sets-api.yaml`

**Tests**: Include backend unit tests for numbering/validation business rules plus integration, contract, and e2e tests for persistence, duplicate-entry isolation, and explicit failure/no-ghost behavior.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm existing workout flows and anchor extension points for per-lift set logging.

- [X] T001 Review active workout API/controller patterns in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T002 [P] Review existing workout command/result patterns in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs` and `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandler.cs`
- [X] T003 [P] Review active-workout UI/state integration points in `frontend/src/app/features/workouts/active-workout-page.component.ts`, `frontend/src/app/features/workouts/active-workout-page.component.html`, and `frontend/src/app/core/state/workouts-store.service.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared API contracts, command scaffolding, and persistence foundations required by all user stories.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [X] T004 Create add-set request/response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/CreateWorkoutSetRequest.cs`, `backend/src/WeightLifting.Api/Api/Contracts/Workouts/CreateWorkoutSetResponse.cs`, and `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutSetEntryResponse.cs`
- [X] T005 Create add-set application command models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetResult.cs`
- [X] T006 [P] Add workout-set persistence entity and mapping in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Entities/WorkoutSetEntity.cs` and `backend/src/WeightLifting.Api/Infrastructure/Persistence/Configurations/WorkoutSetEntityTypeConfiguration.cs`
- [X] T007 Add SQL schema migration for workout sets in `backend/src/WeightLifting.Api/Infrastructure/Migrations/`
- [X] T008 [P] Add API client DTOs and create-set method in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T009 [P] Extend workout state models for per-entry sets in `frontend/src/app/core/state/workouts-store.models.ts`
- [X] T010 Register add-set command handler and mapping dependencies in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Add sets directly under a lift entry (Priority: P1) 🎯 MVP

**Goal**: Enable fast add-set creation under a specific workout-lift entry with persisted success and sequential numbering.

**Independent Test**: In an in-progress workout with one lift entry and zero sets, add two sets and verify rows render under that entry as Set 1 then Set 2 and remain after refresh.

### Tests for User Story 1

- [X] T011 [P] [US1] Add unit tests for add-set happy path, in-progress requirement, and sequential numbering in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs`
- [X] T012 [P] [US1] Add integration tests for persisted workout-set rows and refresh-visible continuity in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/AddWorkoutSetIntegrationTests.cs`
- [X] T013 [P] [US1] Add contract tests for `POST /api/workouts/{workoutId}/lifts/{workoutLiftEntryId}/sets` success shape in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T014 [P] [US1] Add e2e happy-path add-set flow test in `frontend/tests/e2e/workouts/add-workout-set.spec.ts`

### Implementation for User Story 1

- [X] T015 [US1] Implement add-set command handler core rules and set-number assignment in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetCommandHandler.cs`
- [X] T016 [US1] Add add-set endpoint mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T017 [US1] Add add-set action orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T018 [US1] Render per-lift set list and Add Set UI in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T019 [US1] Persist successful set rows into active workout store in `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Keep duplicate lift entries independent (Priority: P2)

**Goal**: Ensure set lists and numbering are scoped to workout-lift entry identity, including duplicate lifts.

**Independent Test**: In a workout with duplicate lift names, add sets to each entry and verify lists stay isolated and each numbering sequence starts at Set 1.

### Tests for User Story 2

- [X] T020 [P] [US2] Extend unit tests for duplicate lift-entry identity isolation in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs`
- [X] T021 [P] [US2] Add integration tests for duplicate-entry set-list independence in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/AddWorkoutSetIntegrationTests.cs`
- [X] T022 [P] [US2] Add e2e duplicate-entry behavior test in `frontend/tests/e2e/workouts/add-workout-set-duplicates.spec.ts`

### Implementation for User Story 2

- [X] T023 [US2] Enforce workoutLiftEntry-to-workout ownership and isolation checks in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetCommandHandler.cs`
- [X] T024 [US2] Ensure UI binds set lists by workout-lift entry id and not lift id in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Show clear outcomes when add-set fails (Priority: P2)

**Goal**: Provide explicit failure feedback and prevent ghost saved rows while keeping visible list/numbering consistent.

**Independent Test**: Simulate offline/server/conflict failures for add-set and verify explicit error feedback with no saved-row illusion or numbering corruption.

### Tests for User Story 3

- [X] T025 [P] [US3] Add unit tests for `NotFound`, `Conflict`, and `ValidationFailed` add-set outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs`
- [X] T026 [P] [US3] Add contract tests for add-set endpoint 404/409/422 responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T027 [P] [US3] Add e2e failure-path add-set tests in `frontend/tests/e2e/workouts/add-workout-set-failures.spec.ts`

### Implementation for User Story 3

- [X] T028 [US3] Map add-set outcomes to problem responses in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T029 [US3] Implement frontend add-set error feedback and no-ghost handling in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T030 [US3] Add in-flight submit guard and reconciliation path for failed add-set attempts in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cross-story validation and regression checks.

- [X] T031 [P] Update quickstart verification notes in `specs/011-add-per-lift-sets/quickstart.md`
- [X] T032 [P] Run targeted backend unit/integration/contract suites in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T033 [P] Run frontend add-set e2e suite and build via scripts in `frontend/package.json`
- [X] T034 Validate mobile viewport usability and low-tap flow in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T035 Confirm regressions for existing add/remove/reorder flows in `frontend/tests/e2e/workouts/add-workout-lift.spec.ts`, `frontend/tests/e2e/workouts/remove-workout-lift.spec.ts`, and `frontend/tests/e2e/workouts/reorder-workout-lifts.spec.ts`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately.
- **Phase 2 (Foundational)**: depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: depends on US1 add-set baseline.
- **Phase 5 (US3)**: depends on US1 endpoint and UI flow baseline.
- **Phase 6 (Polish)**: depends on all targeted stories being complete.

### User Story Dependencies

- **US1 (P1)**: core add-set path with persistence and numbering.
- **US2 (P2)**: extends US1 for duplicate-entry isolation guarantees.
- **US3 (P2)**: extends US1 for explicit failure/no-ghost guarantees.

### Within Each User Story

- Write backend business-rule unit tests before implementation.
- Complete backend command/endpoint behavior before frontend state integration.
- Verify story-level tests pass before moving to the next story.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T006`, `T008`, `T009`).
- In each story, marked `[P]` tests can run in parallel once fixtures are ready (`T011`-`T014`, `T020`-`T022`, `T025`-`T027`).
- Polish validation tasks marked `[P]` can run in parallel (`T031`-`T033`).

---

## Parallel Example: User Story 1

```bash
Task: "T011 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs"
Task: "T012 [US1] Integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/AddWorkoutSetIntegrationTests.cs"
Task: "T014 [US1] E2E test in frontend/tests/e2e/workouts/add-workout-set.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T020 [US2] Duplicate-isolation unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs"
Task: "T021 [US2] Duplicate-isolation integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/AddWorkoutSetIntegrationTests.cs"
Task: "T022 [US2] Duplicate-isolation e2e test in frontend/tests/e2e/workouts/add-workout-set-duplicates.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T025 [US3] Failure-outcome unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/AddWorkoutSet/AddWorkoutSetCommandHandlerTests.cs"
Task: "T026 [US3] Failure contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs"
Task: "T027 [US3] Failure e2e test in frontend/tests/e2e/workouts/add-workout-set-failures.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 add-set flow end-to-end.
3. Validate save persistence and sequential numbering after refresh.
4. Demo MVP before expanding scope.

### Incremental Delivery

1. US1: baseline add-set persistence and rendering.
2. US2: duplicate-entry isolation guarantees.
3. US3: explicit failures and no-ghost trust behaviors.
4. Polish and regression verification.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Then split by story:
   - Dev A: US1
   - Dev B: US2
   - Dev C: US3
3. Merge into final polish/regression pass.
