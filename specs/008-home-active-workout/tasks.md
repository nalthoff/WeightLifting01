# Tasks: Home active workout summary and quick completion

**Input**: Design documents from `/specs/008-home-active-workout/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks whenever the feature changes behavior. Unit tests are REQUIRED for any business-layer logic. Add integration, contract, or end-to-end tests when API, persistence, or cross-boundary behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., `US1`, `US2`, `US3`)
- Include exact file paths in descriptions

## Path Conventions

- **Backend app**: `backend/src/WeightLifting.Api/`
- **Backend tests**: `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, `backend/tests/WeightLifting.Api.ContractTests/`
- **Frontend app**: `frontend/src/app/`
- **Frontend tests**: `frontend/tests/unit/`, `frontend/tests/e2e/`
- Keep one production class per file unless an explicit exception is justified

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare minimal scaffolding for home card tests and workout-completion API surface changes.

- [X] T001 Create home-active-workout test scaffolding files in `frontend/tests/unit/home/home-active-workout-card.spec.ts` and `frontend/tests/e2e/workouts/home-active-workout.spec.ts`
- [X] T002 [P] Create backend completion test scaffolding files in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs`, `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/CompleteWorkoutIntegrationTests.cs`, and `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared completion contract and API client/store primitives that all stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 Add/extend completion and active-summary API contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/` and align `specs/008-home-active-workout/contracts/home-workout-actions-api.yaml`
- [X] T004 [P] Extend workout API client methods for active summary + completion in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T005 Extend shared workout state primitives for active-workout refresh and completion reconciliation in `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: Foundation ready; client and server contract surfaces support home summary and completion operations.

---

## Phase 3: User Story 1 - See and continue active workout from home (Priority: P1) 🎯 MVP

**Goal**: Show active workout summary card on home and allow continuation to existing workout detail route.

**Independent Test**: With an in-progress workout, home renders card with label/fallback + started time and Continue navigates to `/workouts/:workoutId`.

### Tests for User Story 1

> **NOTE: Write required tests before implementation. Business-layer unit tests are mandatory and should fail before the corresponding implementation is completed.**

- [X] T006 [P] [US1] Add frontend unit tests for home card rendering and Continue routing in `frontend/tests/unit/home/home-active-workout-card.spec.ts`
- [X] T007 [P] [US1] Add backend integration and contract coverage for active summary retrieval in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ActiveWorkoutSummaryIntegrationTests.cs` and `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`

### Implementation for User Story 1

- [X] T008 [P] [US1] Add backend endpoint/query path for active workout summary in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetActiveWorkoutSummary/`
- [X] T009 [US1] Update home page component state + continue action wiring in `frontend/src/app/features/home/home-page.component.ts`
- [X] T010 [P] [US1] Implement home active-workout card markup and styling with label fallback/start time/Continue action in `frontend/src/app/features/home/home-page.component.html` and `frontend/src/app/features/home/home-page.component.scss`

**Checkpoint**: User Story 1 is independently functional from home discovery through continue navigation.

---

## Phase 4: User Story 2 - Complete active workout directly from home (Priority: P1)

**Goal**: Let users complete active workout from home in one tap and stay on home with success feedback.

**Independent Test**: Tap Complete from home active card; workout completes, card disappears, and success feedback appears without leaving home.

### Tests for User Story 2

- [X] T011 [P] [US2] Add backend unit tests for completion lifecycle transitions and invalid-state handling in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs`
- [X] T012 [P] [US2] Add backend integration/contract tests for `POST /api/workouts/{workoutId}/complete` success path in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/CompleteWorkoutIntegrationTests.cs` and `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T013 [P] [US2] Add frontend unit tests for complete success flow (stay home, remove card, show success) in `frontend/tests/unit/home/home-active-workout-card.spec.ts`

### Implementation for User Story 2

- [X] T014 [P] [US2] Implement backend completion command/handler and lifecycle updates in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkout/` and `backend/src/WeightLifting.Api/Domain/Workouts/WorkoutSession.cs`
- [X] T015 [US2] Add completion endpoint wiring and DI registration in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T016 [US2] Implement home complete action, loading guard, and success feedback state updates in `frontend/src/app/features/home/home-page.component.ts`
- [X] T017 [P] [US2] Add complete-action button states and success feedback UI in `frontend/src/app/features/home/home-page.component.html` and `frontend/src/app/features/home/home-page.component.scss`

**Checkpoint**: User Story 2 is independently functional with one-tap home completion and correct success behavior.

---

## Phase 5: User Story 3 - Handle completion failures and race states clearly (Priority: P2)

**Goal**: Ensure failure/race outcomes produce explicit feedback and accurate final home state with no ghost completion.

**Independent Test**: Simulate offline/server/race failures; user sees clear errors, and home card state resolves to authoritative workout status.

### Tests for User Story 3

- [X] T018 [P] [US3] Add backend unit/integration coverage for not-found/conflict completion outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs` and `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/CompleteWorkoutIntegrationTests.cs`
- [X] T019 [P] [US3] Add frontend unit tests for completion error and stale-state reconciliation in `frontend/tests/unit/home/home-active-workout-card.spec.ts`
- [X] T020 [P] [US3] Add mobile e2e coverage for home completion failure and rapid-tap behavior in `frontend/tests/e2e/workouts/home-active-workout.spec.ts`

### Implementation for User Story 3

- [X] T021 [P] [US3] Add backend completion error mapping for not-found/conflict paths in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and related application result models
- [X] T022 [US3] Implement frontend completion failure messaging + race-state refresh logic in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T023 [US3] Add duplicate-tap guard and disabled-state handling for Complete action in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/features/home/home-page.component.html`

**Checkpoint**: User Story 3 is independently functional with resilient error handling and authoritative state reconciliation.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end validation, regression safety, and final docs alignment.

- [X] T024 [P] Run backend unit/integration/contract suites for workouts lifecycle changes from `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T025 [P] Run frontend unit and e2e suites for home active-workout behavior from `frontend/tests/unit/home/` and `frontend/tests/e2e/workouts/home-active-workout.spec.ts`
- [ ] T026 Validate manual quickstart scenarios and capture results in `specs/008-home-active-workout/quickstart.md`
- [ ] T027 Validate primary mobile logging flow regression (start workout -> continue/detail -> complete from home) using `frontend/tests/e2e/workouts/` and `specs/006-start-blank-workout/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on Foundational completion and reuses US1 summary/home primitives
- **User Story 3 (Phase 5)**: Depends on Foundational completion and extends US2 completion behavior
- **Polish (Phase 6)**: Depends on all implemented user stories

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - MVP entry point for home visibility and continue
- **User Story 2 (P1)**: Builds on home summary from US1 to add completion behavior
- **User Story 3 (P2)**: Builds on completion behavior from US2 for failure/race resilience

### Within Each User Story

- Tests first (unit/integration/contract/e2e as applicable)
- Backend business logic before API/controller wiring
- API/client/store updates before final home UI interactions
- Story must pass independent test before progressing

### Parallel Opportunities

- T002 can run in parallel with T001
- T003 and T004 can run in parallel, then merge through T005
- T006 and T007 can run in parallel
- T008 can run in parallel with T010, then T009 integrates behavior
- T011, T012, and T013 can run in parallel
- T014 can run in parallel with early UI changes, then T015/T016 integrate end-to-end
- T018, T019, and T020 can run in parallel
- T024 and T025 can run in parallel in polish phase

---

## Parallel Example: User Story 2

```bash
# Write completion tests in parallel:
Task: "Add backend completion unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CompleteWorkout/CompleteWorkoutCommandHandlerTests.cs"
Task: "Add backend completion integration/contract tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/CompleteWorkoutIntegrationTests.cs and backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs"
Task: "Add frontend complete-success tests in frontend/tests/unit/home/home-active-workout-card.spec.ts"

# Implement backend and UI pieces in parallel, then integrate:
Task: "Implement completion command/handler in backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkout/"
Task: "Add complete-action UI states in frontend/src/app/features/home/home-page.component.html and frontend/src/app/features/home/home-page.component.scss"
Task: "Integrate completion action + success feedback in frontend/src/app/features/home/home-page.component.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Confirm home card + Continue flow independently
5. Demo MVP before adding completion actions

### Incremental Delivery

1. Deliver US1 for visibility + continue
2. Add US2 one-tap completion and success feedback
3. Add US3 failure/race resilience and rapid-tap safety
4. Finish with full regression and quickstart verification

### Suggested MVP Scope

Deliver through **Phase 3 (User Story 1)** first.

---

## Notes

- Total tasks: 27
- User Story 1 tasks: 5
- User Story 2 tasks: 7
- User Story 3 tasks: 6
- Setup + Foundational + Polish tasks: 9
- All tasks follow required checklist format with checkbox, task ID, optional `[P]`, required story labels in story phases, and explicit file paths
