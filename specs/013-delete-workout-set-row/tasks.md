# Tasks: Delete mistaken workout set rows

**Input**: Design documents from `/specs/013-delete-workout-set-row/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workout-set-deletes-api.yaml`

**Tests**: Include backend unit tests for delete eligibility/rule handling plus integration, contract, and e2e coverage for confirmation, cancel behavior, failure retry, and targeted regression checks.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm existing set/edit flow extension points for introducing row deletion with confirmation.

- [X] T001 Review active workout set row UI/state touchpoints in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T002 [P] Review workout set API/store conventions in `frontend/src/app/core/api/workout-lifts-api.service.ts`, `frontend/src/app/core/state/workouts-store.models.ts`, and `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T003 [P] Review backend add/update set command and endpoint patterns in `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutSet/AddWorkoutSetCommandHandler.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutSet/UpdateWorkoutSetCommandHandler.cs`, and `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared delete-set contract and scaffolding required before story-specific behavior.

**⚠️ CRITICAL**: No user story work starts until this phase is complete.

- [X] T004 Create delete-set API contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/DeleteWorkoutSetResponse.cs`
- [X] T005 Create delete-set command models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkoutSet/DeleteWorkoutSetCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkoutSet/DeleteWorkoutSetOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkoutSet/DeleteWorkoutSetResult.cs`
- [X] T006 [P] Add delete-set API DTO and client method in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T007 [P] Extend workout store models for row delete session state in `frontend/src/app/core/state/workouts-store.models.ts`
- [X] T008 [P] Add store support for targeted set-row removal in `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T009 Register delete-set command handler in DI in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Delete a mistaken set row with confirmation (Priority: P1) 🎯 MVP

**Goal**: Let users delete one persisted set row from the active workout screen only after explicit confirmation, with immediate in-place list update on success.

**Independent Test**: In an in-progress workout, delete one set row, confirm deletion, and verify only that row is removed without leaving the active workout screen.

### Tests for User Story 1

- [X] T010 [P] [US1] Add unit tests for delete success path, in-progress enforcement, and row scope checks in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkoutSet/DeleteWorkoutSetCommandHandlerTests.cs`
- [X] T011 [P] [US1] Add integration test for targeted set-row deletion persistence in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutSetIntegrationTests.cs`
- [X] T012 [P] [US1] Add contract test for `DELETE /api/workouts/{workoutId}/lifts/{workoutLiftEntryId}/sets/{setId}` success response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T013 [P] [US1] Add e2e happy-path confirmation and delete scenario in `frontend/tests/e2e/workouts/delete-workout-set.spec.ts`

### Implementation for User Story 1

- [X] T014 [US1] Implement delete-set command handler for row lookup, in-progress gating, and deletion in `backend/src/WeightLifting.Api/Application/Workouts/Commands/DeleteWorkoutSet/DeleteWorkoutSetCommandHandler.cs`
- [X] T015 [US1] Add delete-set endpoint and success response mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T016 [US1] Implement set-row delete confirmation and confirm action orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T017 [US1] Render set-row delete trigger and confirmation UI in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Keep data unchanged when deletion is canceled (Priority: P2)

**Goal**: Ensure cancellation at confirmation always preserves the set list with no mutation.

**Independent Test**: Start set-row deletion, cancel confirmation, and verify all rows remain unchanged.

### Tests for User Story 2

- [X] T018 [P] [US2] Add frontend unit tests for cancel-confirmation behavior and no-mutation state handling in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [X] T019 [P] [US2] Add e2e cancel-delete scenario in `frontend/tests/e2e/workouts/delete-workout-set-cancel.spec.ts`

### Implementation for User Story 2

- [X] T020 [US2] Implement explicit cancel-delete state transitions and cleanup in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T021 [US2] Add cancel-path UI behavior and accessibility labeling in `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Handle deletion failures clearly (Priority: P2)

**Goal**: Preserve row visibility on delete failure, show clear feedback, and support retry to successful removal.

**Independent Test**: Simulate failed delete, confirm row persists with clear error feedback, then retry and confirm successful row removal.

### Tests for User Story 3

- [X] T022 [P] [US3] Extend unit tests for delete failure outcomes (`NotFound`, `Conflict`, `Failed`) in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkoutSet/DeleteWorkoutSetCommandHandlerTests.cs`
- [X] T023 [P] [US3] Add contract tests for delete endpoint `404` and `409` responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T024 [P] [US3] Add integration test for duplicate-entry isolation during set delete in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutSetIntegrationTests.cs`
- [X] T025 [P] [US3] Add e2e failure-and-retry delete scenario in `frontend/tests/e2e/workouts/delete-workout-set-failures.spec.ts`

### Implementation for User Story 3

- [X] T026 [US3] Map delete failure outcomes to problem responses in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T027 [US3] Implement row-level delete error and retry orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T028 [US3] Render delete failure feedback and retry affordance in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final cross-story validation, regression checks, and docs updates.

- [X] T029 [P] Update quickstart verification run notes in `specs/013-delete-workout-set-row/quickstart.md`
- [X] T030 [P] Run backend unit/integration/contract suites for delete-set behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T031 [P] Run frontend delete-set e2e suite and build in `frontend/package.json`
- [X] T032 Validate mobile viewport usability for delete confirmation/error states in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T033 Confirm regressions for add-set/edit-set/remove-lift/reorder-lift flows in `frontend/tests/e2e/workouts/add-workout-set.spec.ts`, `frontend/tests/e2e/workouts/edit-workout-set.spec.ts`, `frontend/tests/e2e/workouts/remove-workout-lift.spec.ts`, and `frontend/tests/e2e/workouts/reorder-workout-lifts.spec.ts`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP.
- **Phase 4 (US2)**: Depends on US1 delete confirmation baseline.
- **Phase 5 (US3)**: Depends on US1 delete flow and extends with failure/retry handling.
- **Phase 6 (Polish)**: Depends on all targeted stories being complete.

### User Story Dependencies

- **US1 (P1)**: Core confirmation + deletion flow for targeted set rows.
- **US2 (P2)**: Extends US1 with explicit cancel behavior guarantees.
- **US3 (P2)**: Extends US1 with failure/retry trust and isolation safeguards.

### Within Each User Story

- Write backend business-rule unit tests before implementation.
- Complete backend command/endpoint behavior before frontend integration for the same story.
- Verify story-level tests pass before moving to the next story.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T006`, `T007`, `T008`).
- In each story, marked `[P]` tests can run in parallel once test fixtures are ready (`T010`-`T013`, `T018`-`T019`, `T022`-`T025`).
- Polish validation tasks marked `[P]` can run in parallel (`T029`-`T031`).

---

## Parallel Example: User Story 1

```bash
Task: "T010 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkoutSet/DeleteWorkoutSetCommandHandlerTests.cs"
Task: "T011 [US1] Integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/DeleteWorkoutSetIntegrationTests.cs"
Task: "T013 [US1] E2E test in frontend/tests/e2e/workouts/delete-workout-set.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T018 [US2] Frontend unit test in frontend/tests/unit/workouts/active-workout-page.component.spec.ts"
Task: "T019 [US2] E2E cancel test in frontend/tests/e2e/workouts/delete-workout-set-cancel.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T022 [US3] Failure-outcome unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/DeleteWorkoutSet/DeleteWorkoutSetCommandHandlerTests.cs"
Task: "T023 [US3] Failure contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs"
Task: "T025 [US3] Failure-retry e2e tests in frontend/tests/e2e/workouts/delete-workout-set-failures.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 confirmation + delete flow end-to-end.
3. Validate targeted row removal without navigation.
4. Demo MVP before expanding to cancel/failure refinements.

### Incremental Delivery

1. US1: baseline confirmation + successful delete behavior.
2. US2: explicit cancel path with zero data mutation.
3. US3: failure visibility, retry, and scoped isolation handling.
4. Polish and regression validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. Then split by story:
   - Dev A: US1
   - Dev B: US2
   - Dev C: US3
3. Merge and run final cross-story polish/regression pass.
