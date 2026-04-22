# Tasks: Remove lift from in-progress workout

**Input**: Design documents from `/specs/009-remove-workout-lift/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/remove-workout-lifts-api.yaml

**Tests**: Include backend unit tests for removal business rules plus integration, contract, and e2e tests for remove-lift behavior changes.

**Organization**: Tasks are grouped by user story to keep each story independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm reuse points and current remove-lift gaps before implementation.

- [X] T001 Review existing add/list workout-lift patterns in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs`
- [X] T002 Review active-workout list rendering/state integration points in `frontend/src/app/features/workouts/active-workout-page.component.ts`, `frontend/src/app/features/workouts/active-workout-page.component.html`, and `frontend/src/app/core/state/workouts-store.service.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared remove-lift contracts and backend/frontend scaffolding required by all stories.

- [X] T003 Create remove-workout-lift API contract models in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/RemoveWorkoutLiftResponse.cs`
- [X] T004 Create remove-workout-lift command and outcome models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/RemoveWorkoutLift/RemoveWorkoutLiftCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/RemoveWorkoutLift/RemoveWorkoutLiftOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/RemoveWorkoutLift/RemoveWorkoutLiftResult.cs`
- [X] T005 [P] Add remove-workout-lift API client method in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T006 [P] Add store helper for removing active workout entry by id in `frontend/src/app/core/state/workouts-store.service.ts`
- [X] T007 Register remove-workout-lift handler in dependency injection in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Remove mistaken lift entry quickly (Priority: P1) 🎯 MVP

**Goal**: Let lifter remove one selected workout-lift entry from active workout list and see immediate persisted result.

**Independent Test**: In an in-progress workout with lift entries, remove one selected entry from active workout screen and verify it disappears immediately and remains removed after refresh.

### Tests for User Story 1

- [X] T008 [P] [US1] Add unit tests for remove-workout-lift happy path in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandlerTests.cs`
- [X] T009 [P] [US1] Add integration test for successful entry removal persistence in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/RemoveWorkoutLiftIntegrationTests.cs`
- [X] T010 [P] [US1] Add contract test for `DELETE /api/workouts/{workoutId}/lifts/{workoutLiftEntryId}` success response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T011 [P] [US1] Add e2e test for active-workout remove-lift happy path in `frontend/tests/e2e/workouts/remove-workout-lift.spec.ts`

### Implementation for User Story 1

- [X] T012 [P] [US1] Implement remove-workout-lift business handler in `backend/src/WeightLifting.Api/Application/Workouts/Commands/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandler.cs`
- [X] T013 [US1] Add remove-workout-lift endpoint in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T014 [US1] Integrate remove action orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T015 [US1] Render remove controls for workout-lift rows in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T016 [US1] Apply successful remove response to store state in `frontend/src/app/core/state/workouts-store.service.ts` and `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Remove only selected duplicate instance (Priority: P2)

**Goal**: Ensure duplicate entries for same lift remain independent and only selected instance is removed.

**Independent Test**: Add the same lift multiple times, remove one specific entry, and verify only that selected entry is removed while duplicates remain.

### Tests for User Story 2

- [X] T017 [P] [US2] Add unit tests verifying remove-by-entry-id precision with duplicates in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandlerTests.cs`
- [X] T018 [P] [US2] Add integration test for duplicate-instance removal behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/RemoveWorkoutLiftIntegrationTests.cs`
- [X] T019 [P] [US2] Add e2e test for duplicate-instance removal in `frontend/tests/e2e/workouts/remove-workout-lift-duplicates.spec.ts`

### Implementation for User Story 2

- [X] T020 [US2] Ensure backend removal query filters by workout id and workout-lift-entry id in `backend/src/WeightLifting.Api/Application/Workouts/Commands/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandler.cs`
- [X] T021 [US2] Ensure UI remove action binds to entry id (not lift id) in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 remains independently functional and testable.

---

## Phase 5: User Story 3 - Show clear outcomes for failed removals (Priority: P2)

**Goal**: Provide explicit stale/conflict/network failure outcomes without ghost removals.

**Independent Test**: Simulate not-found, conflict, and server/network failures; verify explicit feedback and accurate workout list state.

### Tests for User Story 3

- [X] T022 [P] [US3] Add unit tests for not-found and conflict removal outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandlerTests.cs`
- [X] T023 [P] [US3] Add contract tests for remove-lift error payloads in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [X] T024 [P] [US3] Add e2e failure-path tests for remove-lift errors in `frontend/tests/e2e/workouts/remove-workout-lift-failures.spec.ts`

### Implementation for User Story 3

- [X] T025 [US3] Implement controller error mapping for not-found/conflict/validation outcomes in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T026 [US3] Implement frontend remove-failure feedback states with no local ghost removal in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T027 [US3] Add in-flight guard for rapid repeated remove taps in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US3 remains independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cross-story cleanup.

- [X] T028 [P] Update quickstart verification notes for remove-lift flow in `specs/009-remove-workout-lift/quickstart.md`
- [X] T029 [P] Run targeted backend unit/integration/contract suites for remove-lift behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [X] T030 [P] Run frontend build and remove-lift e2e suites via scripts in `frontend/package.json`
- [X] T031 Validate mobile viewport usability for active-workout remove flow in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [X] T032 Confirm deferred confirmation behavior is documented (no blocking modal until set logging exists) in `specs/009-remove-workout-lift/spec.md` and `specs/009-remove-workout-lift/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately
- **Phase 2 (Foundational)**: depends on Phase 1 and blocks all user stories
- **Phase 3 (US1)**: depends on Phase 2
- **Phase 4 (US2)**: depends on US1 baseline remove flow
- **Phase 5 (US3)**: depends on US1 baseline remove flow
- **Phase 6 (Polish)**: depends on all targeted stories being complete

### User Story Dependencies

- **US1 (P1)**: MVP remove-lift baseline
- **US2 (P2)**: extends US1 with duplicate-instance precision validation
- **US3 (P2)**: extends US1 with failure handling and resilience validation

### Within Each User Story

- Write required backend unit tests before implementation.
- Implement backend removal rules before frontend integration.
- Complete story-level tests before progressing to next story.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T005`, `T006`).
- In US1, backend test tasks and frontend e2e scaffolding marked `[P]` can run together.
- In US2/US3, marked `[P]` tests can run in parallel once endpoint behavior stabilizes.

---

## Parallel Example: User Story 1

```bash
Task: "T008 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/RemoveWorkoutLift/RemoveWorkoutLiftCommandHandlerTests.cs"
Task: "T009 [US1] Integration test in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/RemoveWorkoutLiftIntegrationTests.cs"
Task: "T011 [US1] E2E remove happy-path in frontend/tests/e2e/workouts/remove-workout-lift.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational phases.
2. Deliver US1 remove-lift flow end-to-end.
3. Validate mobile happy path and persisted removal behavior.
4. Demo before extending to duplicates and failure depth.

### Incremental Delivery

1. US1: baseline remove behavior.
2. US2: duplicate-instance precision.
3. US3: failure-state clarity and resilience.
4. Polish and regression validation.

### Simplicity + SOLID Guidance

- Reuse existing workout/lift slices and avoid introducing new abstraction layers without need.
- Keep remove business rules in backend command handler; keep Angular code focused on UI orchestration.
- Preserve one-class-per-file for all new production classes.
- Defer set-based confirmation behavior until set logging exists.
