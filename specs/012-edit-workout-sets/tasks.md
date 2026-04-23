# Tasks: Edit workout set entries

**Input**: Design documents from `/specs/012-edit-workout-sets/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workout-set-updates-api.yaml`

**Tests**: Include backend unit tests for update validation/state rules plus integration, contract, and e2e coverage for in-place save behavior, failure-retry trust signals, and workout-status gating.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm existing workout-set creation patterns and identify extension points for set updates.

- [ ] T001 Review active workout set rendering/edit touchpoints in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [ ] T002 [P] Review workout set DTO and API client conventions in `frontend/src/app/core/api/workout-lifts-api.service.ts` and `frontend/src/app/core/state/workouts-store.models.ts`
- [ ] T003 [P] Review backend add-set command/endpoint patterns in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetCommandHandler.cs` and `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared update-set contracts and command scaffolding required by all user stories.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [ ] T004 Create update-set API contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/UpdateWorkoutSetRequest.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Workouts/UpdateWorkoutSetResponse.cs`
- [ ] T005 Create update-set application command models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetResult.cs`
- [ ] T006 [P] Add update-set API DTOs and client method in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [ ] T007 [P] Extend workout store models for row edit session state in `frontend/src/app/core/state/workouts-store.models.ts`
- [ ] T008 Register update-set command handler dependencies in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Edit a logged set inline (Priority: P1) 🎯 MVP

**Goal**: Let users edit reps/weight inline for an existing set row and save in place with immediate visible updates.

**Independent Test**: In an in-progress workout with existing sets, edit one set row and save; verify only that row updates immediately without leaving the screen and set number remains unchanged.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add unit tests for update validation, immutable set number, and in-progress requirement in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs`
- [ ] T010 [P] [US1] Add integration tests for persisted set updates and lift-entry scoped row targeting in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutSetIntegrationTests.cs`
- [ ] T011 [P] [US1] Add contract tests for `PUT /api/workouts/{workoutId}/lifts/{workoutLiftEntryId}/sets/{setId}` success shape in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [ ] T012 [P] [US1] Add e2e happy-path inline-edit flow in `frontend/tests/e2e/workouts/edit-workout-set.spec.ts`

### Implementation for User Story 1

- [ ] T013 [US1] Implement update-set command handler for reps/weight persistence and last-write-wins updates in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetCommandHandler.cs`
- [ ] T014 [US1] Add update-set endpoint mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T015 [US1] Add store support for applying updated set rows in `frontend/src/app/core/state/workouts-store.service.ts`
- [ ] T016 [US1] Implement inline edit mode, row draft tracking, and save action orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T017 [US1] Render row-level inline edit controls and save affordances in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Keep row state clear during save failures (Priority: P2)

**Goal**: Preserve unsaved row drafts on failed save, show clear error state, and support retry without leaving the row.

**Independent Test**: Simulate failed update saves and verify row-level draft persistence, explicit unsaved/error feedback, and successful retry path.

### Tests for User Story 2

- [ ] T018 [P] [US2] Extend unit tests for update failure outcomes (`NotFound`, `Conflict`, `ValidationFailed`) in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs`
- [ ] T019 [P] [US2] Add contract tests for update-set endpoint 404/409/422 responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [ ] T020 [P] [US2] Add e2e failure-retry scenarios for inline edit rows in `frontend/tests/e2e/workouts/edit-workout-set-failures.spec.ts`

### Implementation for User Story 2

- [ ] T021 [US2] Map update-set failure outcomes to problem responses in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T022 [US2] Implement frontend row-level unsaved/error state handling and retry orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T023 [US2] Render explicit row-level unsaved/failure messaging and retry UI in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Enforce editing boundaries for active sessions (Priority: P2)

**Goal**: Restrict row editing to in-progress workouts and keep duplicate lift-entry rows isolated when edits are applied.

**Independent Test**: Verify edit controls are unavailable for non-in-progress workouts and updates never leak between duplicate lift-entry contexts.

### Tests for User Story 3

- [ ] T024 [P] [US3] Add unit tests for non-in-progress workout gating and lift-entry ownership checks in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs`
- [ ] T025 [P] [US3] Add integration tests for duplicate-lift-entry isolation during set updates in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutSetIntegrationTests.cs`
- [ ] T026 [P] [US3] Add e2e tests for in-progress-only edit controls and duplicate-entry isolation in `frontend/tests/e2e/workouts/edit-workout-set-constraints.spec.ts`

### Implementation for User Story 3

- [ ] T027 [US3] Enforce in-progress-only update-set rule and scoped row ownership checks in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetCommandHandler.cs`
- [ ] T028 [US3] Gate row edit controls by workout status in active workout UI in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [ ] T029 [US3] Ensure set-row updates are applied only to the targeted workout-lift entry in `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cross-story validation and regression checks.

- [ ] T030 [P] Update quickstart verification notes in `specs/012-edit-workout-sets/quickstart.md`
- [ ] T031 [P] Run backend unit/integration/contract suites for update-set behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [ ] T032 [P] Run frontend edit-set e2e suite and build in `frontend/package.json`
- [ ] T033 Validate mobile viewport usability for inline edit/save/retry controls in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [ ] T034 Confirm regressions for add-set/remove-lift/reorder-lift flows in `frontend/tests/e2e/workouts/add-workout-set.spec.ts`, `frontend/tests/e2e/workouts/remove-workout-lift.spec.ts`, and `frontend/tests/e2e/workouts/reorder-workout-lifts.spec.ts`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: Depends on US1 baseline update flow.
- **Phase 5 (US3)**: Depends on US1 baseline update flow.
- **Phase 6 (Polish)**: Depends on all targeted stories being complete.

### User Story Dependencies

- **US1 (P1)**: Core inline edit-save path for existing set rows.
- **US2 (P2)**: Extends US1 with failure trust and retry behavior.
- **US3 (P2)**: Extends US1 with workout-status gating and scoped isolation guarantees.

### Within Each User Story

- Write backend business-rule unit tests before implementation.
- Complete backend command/endpoint behavior before frontend integration.
- Verify story-level tests pass before moving to the next story.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T006`, `T007`).
- In each story, marked `[P]` tests can run in parallel once fixtures are ready (`T009`-`T012`, `T018`-`T020`, `T024`-`T026`).
- Polish validation tasks marked `[P]` can run in parallel (`T030`-`T032`).

---

## Parallel Example: User Story 1

```bash
Task: "T009 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs"
Task: "T010 [US1] Integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutSetIntegrationTests.cs"
Task: "T012 [US1] E2E test in frontend/tests/e2e/workouts/edit-workout-set.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T018 [US2] Failure-outcome unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs"
Task: "T019 [US2] Failure contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs"
Task: "T020 [US2] Failure-retry e2e tests in frontend/tests/e2e/workouts/edit-workout-set-failures.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T024 [US3] In-progress gating unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutSet/UpdateWorkoutSetCommandHandlerTests.cs"
Task: "T025 [US3] Duplicate-entry isolation integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutSetIntegrationTests.cs"
Task: "T026 [US3] Constraints e2e tests in frontend/tests/e2e/workouts/edit-workout-set-constraints.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 inline edit and save flow end-to-end.
3. Validate immediate row updates without screen navigation.
4. Demo MVP before expanding to failure and boundary stories.

### Incremental Delivery

1. US1: baseline inline edit + save behavior.
2. US2: unsaved-on-failure visibility and retry behavior.
3. US3: in-progress-only gating and duplicate-entry isolation.
4. Polish and regression validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Then split by story:
   - Dev A: US1
   - Dev B: US2
   - Dev C: US3
3. Merge and run final cross-story polish/regression pass.
