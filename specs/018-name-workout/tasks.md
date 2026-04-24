# Tasks: Optional Workout Name

**Input**: Design documents from `/specs/018-name-workout/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks whenever the feature changes behavior. Unit tests are REQUIRED
for any backend application/domain logic. Add integration, contract, or end-to-end tests
when API, SQL persistence, or cross-boundary behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align feature scaffolding and contracts with current branch artifacts

- [X] T001 Align optional workout-name contract details in `specs/018-name-workout/contracts/workout-name-api.yaml`
- [X] T002 Create implementation task checklist baseline in `specs/018-name-workout/tasks.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core API/application plumbing required before any user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add workout-name update request/response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/UpdateWorkoutLabelRequest.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Workouts/UpdateWorkoutLabelResponse.cs`
- [X] T004 [P] Add application command models for rename flow in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutLabel/UpdateWorkoutLabelCommand.cs`, `UpdateWorkoutLabelOutcome.cs`, and `UpdateWorkoutLabelResult.cs`
- [X] T005 Implement rename command handler and registration in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandler.cs` and `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`
- [X] T006 Wire rename endpoint and shared validation response mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T007 [P] Add frontend API client method for rename endpoint in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T008 [P] Extend workout state update pathway for rename responses in `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Name an active workout (Priority: P1) 🎯 MVP

**Goal**: Users can add and edit workout name while a workout is in progress.

**Independent Test**: Start a workout, set a name, edit it again, and confirm active summary shows the latest saved value.

### Tests for User Story 1

- [X] T009 [P] [US1] Add backend unit tests for in-progress rename success in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandlerTests.cs`
- [X] T010 [P] [US1] Add backend integration tests for rename endpoint success path in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutLabelIntegrationTests.cs`
- [X] T011 [P] [US1] Add backend contract tests for rename endpoint response shape in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLabelApiContractTests.cs`
- [X] T012 [P] [US1] Add frontend unit coverage for active workout name save/edit behavior in `frontend/tests/unit/workouts/active-workout-name.spec.ts`
- [X] T013 [P] [US1] Add frontend e2e coverage for rename happy path in `frontend/tests/e2e/workouts/edit-workout-name.spec.ts`

### Implementation for User Story 1

- [X] T014 [US1] Add workout-name UI controls and save interactions in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T015 [P] [US1] Add workout-name input layout and affordances in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T016 [US1] Connect active workout rename operation to API/state flow in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/core/state/workouts-store.service.ts`

**Checkpoint**: User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Keep naming optional (Priority: P2)

**Goal**: Blank/whitespace names remain valid, normalize to unnamed, and do not block completion.

**Independent Test**: Complete in-progress workouts with explicit name, blank value, and whitespace-only value; verify all complete and unnamed normalization is preserved.

### Tests for User Story 2

- [X] T017 [P] [US2] Add backend unit tests for blank/whitespace normalization and max-length validation in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandlerTests.cs`
- [X] T018 [P] [US2] Add backend integration tests for blank/whitespace rename and completion compatibility in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutLabelIntegrationTests.cs`
- [X] T019 [P] [US2] Add frontend unit tests for optional input behavior and validation messaging in `frontend/tests/unit/workouts/active-workout-name.spec.ts`
- [X] T020 [P] [US2] Add frontend e2e coverage for optional naming completion flow in `frontend/tests/e2e/workouts/edit-workout-name-optional.spec.ts`

### Implementation for User Story 2

- [X] T021 [US2] Apply backend normalization and validation semantics in `backend/src/WeightLifting.Api/Application/Workouts/Commands/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandler.cs`
- [X] T022 [US2] Ensure active workout UI treats blank/whitespace as clear-to-unnamed without blocking completion in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: User Stories 1 and 2 should work independently

---

## Phase 5: User Story 3 - Preserve clear history labels (Priority: P3)

**Goal**: History continues to show fallback label when name is absent, and post-completion rename is blocked.

**Independent Test**: Complete an unnamed workout, verify history shows `"Workout"`, and verify rename attempts after completion are rejected.

### Tests for User Story 3

- [X] T023 [P] [US3] Add backend unit tests for completed-workout rename conflict in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandlerTests.cs`
- [X] T024 [P] [US3] Add backend integration tests for completed rename rejection and history fallback label in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs` and `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutLabelIntegrationTests.cs`
- [X] T025 [P] [US3] Add backend contract tests for rename conflict and history fallback contract stability in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLabelApiContractTests.cs` and `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [X] T026 [P] [US3] Add frontend e2e test for completed-workout rename rejection and history fallback display in `frontend/tests/e2e/workouts/edit-workout-name-completed.spec.ts`

### Implementation for User Story 3

- [X] T027 [US3] Enforce completed-workout rename conflict response in controller mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T028 [US3] Ensure UI disables or blocks rename controls for completed workouts while preserving history fallback rendering in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/history/history-page.component.ts`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate end-to-end behavior and complete documentation updates

- [X] T029 [P] Run targeted backend unit/integration/contract suites for workout naming in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T030 [P] Run targeted frontend unit/e2e suites for workout naming in `frontend/tests/unit/workouts/` and `frontend/tests/e2e/workouts/`
- [X] T031 Execute quickstart validation workflow in `specs/018-name-workout/quickstart.md`
- [X] T032 Mark completed tasks and capture verification notes in `specs/018-name-workout/tasks.md` and `specs/018-name-workout/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Phase 6)**: Depends on all user stories completion

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2 and provides MVP value independently.
- **User Story 2 (P2)**: Starts after Phase 2; builds on same rename flow but remains independently testable.
- **User Story 3 (P3)**: Starts after Phase 2; validates lifecycle guard and history fallback outcomes.

### Within Each User Story

- Tests first, then implementation changes.
- Backend lifecycle and validation rules before frontend behavior polish.
- Story must pass independent test criteria before advancing.

### Parallel Opportunities

- T004, T007, and T008 can run in parallel in Phase 2.
- Within each user story, backend/frontend tests marked `[P]` can run concurrently.
- UI markup/styles tasks and backend test tasks can run in parallel when files do not overlap.

---

## Parallel Example: User Story 1

```bash
# Run backend and frontend test authoring in parallel:
Task: "T009 backend unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/UpdateWorkoutLabel/UpdateWorkoutLabelCommandHandlerTests.cs"
Task: "T012 frontend unit tests in frontend/tests/unit/workouts/active-workout-page.component.spec.ts"

# Run UI and service integration updates in parallel where safe:
Task: "T015 UI markup/styles in frontend/src/app/features/workouts/active-workout-page.component.html and .scss"
Task: "T010 integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/UpdateWorkoutLabelIntegrationTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete User Story 1 test + implementation tasks.
3. Validate the US1 independent test.
4. Demo/deploy MVP if needed.

### Incremental Delivery

1. Deliver US1 (rename while in progress).
2. Deliver US2 (optional blank/whitespace behavior).
3. Deliver US3 (post-completion lock + history fallback validation).
4. Finish with Phase 6 verification and documentation updates.

### Parallel Team Strategy

1. One engineer handles backend command/controller/contracts tasks.
2. One engineer handles frontend active-workout UI and tests.
3. One engineer handles integration/contract/e2e verification.

---

## Notes

- `[P]` tasks indicate no file overlap and safe concurrency.
- Backend business-rule unit tests are required by constitution and included.
- Each production-class addition should remain one class per file.
