# Tasks: Reorder workout lifts

**Input**: Design documents from `/specs/010-reorder-workout-lifts/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/reorder-workout-lifts-api.yaml

**Tests**: Include backend unit tests for reorder business rules plus integration, contract, and e2e tests for reorder behavior and no-ghost failure handling.

**Organization**: Tasks are grouped by user story to keep each story independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Every task includes an exact file path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm reuse points for reorder behavior before implementation.

- [ ] T001 Review workout-lift list/add/remove API patterns in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs` and `backend/src/WeightLifting.Api/Application/Workouts/Commands/AddWorkoutLift/AddWorkoutLiftCommandHandler.cs`
- [ ] T002 Review active-workout state and list rendering integration points in `frontend/src/app/features/workouts/active-workout-page.component.ts`, `frontend/src/app/features/workouts/active-workout-page.component.html`, and `frontend/src/app/core/state/workouts-store.service.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared reorder API and command scaffolding required by all user stories.

- [ ] T003 Create reorder request/response API contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/ReorderWorkoutLiftsRequest.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Workouts/ReorderWorkoutLiftsResponse.cs`
- [ ] T004 Create reorder command models in `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsCommand.cs`, `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsOutcome.cs`, and `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsResult.cs`
- [ ] T005 [P] Add reorder endpoint client method and DTOs in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [ ] T006 [P] Add store helper for replacing ordered workout-lift entries in `frontend/src/app/core/state/workouts-store.service.ts`
- [ ] T007 Register reorder command handler in DI composition root at `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Checkpoint**: Foundation complete; user-story implementation can begin.

---

## Phase 3: User Story 1 - Reorder current workout sequence quickly (Priority: P1) 🎯 MVP

**Goal**: Allow lifter to reorder entries in active workout and persist updated sequence immediately.

**Independent Test**: In an in-progress workout with at least two entries, reorder one entry to a new position and verify immediate UI update plus persisted order after refresh/reload.

### Tests for User Story 1

- [ ] T008 [P] [US1] Add unit tests for reorder happy path and in-progress rule in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs`
- [ ] T009 [P] [US1] Add integration tests for persisted position updates and contiguous resequencing in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ReorderWorkoutLiftsIntegrationTests.cs`
- [ ] T010 [P] [US1] Add contract tests for `PUT /api/workouts/{workoutId}/lifts/reorder` success response in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [ ] T011 [P] [US1] Add e2e test for active-workout reorder happy path in `frontend/tests/e2e/workouts/reorder-workout-lifts.spec.ts`

### Implementation for User Story 1

- [ ] T012 [P] [US1] Implement reorder command handler core logic in `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandler.cs`
- [ ] T013 [US1] Add reorder endpoint and request validation mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T014 [US1] Implement reorder interaction and save orchestration in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [ ] T015 [US1] Add reorder UI affordance for workout-lift rows in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [ ] T016 [US1] Apply successful reorder response to active-workout state in `frontend/src/app/core/state/workouts-store.service.ts` and `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US1 is independently functional and testable.

---

## Phase 4: User Story 2 - Preserve entry identity while reordering duplicates (Priority: P2)

**Goal**: Ensure duplicate lift entries stay distinct and only their relative order changes.

**Independent Test**: With duplicate lift names present, reorder one specific duplicate and verify all entries remain present with unchanged identity and only position changes.

### Tests for User Story 2

- [ ] T017 [P] [US2] Extend backend unit tests for duplicate-entry instance precision and identity preservation in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs`
- [ ] T018 [P] [US2] Add integration test for duplicate-entry reorder persistence in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ReorderWorkoutLiftsIntegrationTests.cs`
- [ ] T019 [P] [US2] Add e2e test for duplicate-instance reorder behavior in `frontend/tests/e2e/workouts/reorder-workout-lifts-duplicates.spec.ts`

### Implementation for User Story 2

- [ ] T020 [US2] Enforce complete non-duplicated entry-id set validation in `backend/src/WeightLifting.Api/Application/Workouts/Commands/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandler.cs`
- [ ] T021 [US2] Ensure reorder request/response uses entry ids and stable row identity in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: US2 is independently functional and testable.

---

## Phase 5: User Story 3 - Handle failed reorder attempts clearly (Priority: P2)

**Goal**: Provide explicit failure outcomes and authoritative list reconciliation when reorder save fails.

**Independent Test**: Simulate stale/conflict/network failure paths and verify explicit error feedback with no failed order shown as persisted.

### Tests for User Story 3

- [ ] T022 [P] [US3] Add backend unit tests for `NotFound`, `Conflict`, and `ValidationFailed` reorder outcomes in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs`
- [ ] T023 [P] [US3] Add contract tests for reorder endpoint 404/409/422 responses in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs`
- [ ] T024 [P] [US3] Add e2e tests for reorder failure feedback and reconciliation in `frontend/tests/e2e/workouts/reorder-workout-lifts-failures.spec.ts`

### Implementation for User Story 3

- [ ] T025 [US3] Map reorder failure outcomes to API problem responses in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [ ] T026 [US3] Implement frontend reorder error feedback and no-ghost handling in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `frontend/src/app/features/workouts/active-workout-page.component.html`
- [ ] T027 [US3] Add in-flight reorder guard and stale-state reload/reconcile path in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: US3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cross-story cleanup.

- [ ] T028 [P] Update reorder verification notes in `specs/010-reorder-workout-lifts/quickstart.md`
- [ ] T029 [P] Run targeted backend reorder unit/integration/contract suites in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [ ] T030 [P] Run frontend build and reorder e2e suites via scripts in `frontend/package.json`
- [ ] T031 Validate mobile viewport usability of reorder flow in `frontend/src/app/features/workouts/active-workout-page.component.html` and `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [ ] T032 Confirm regression coverage for unaffected historical workouts in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ReorderWorkoutLiftsIntegrationTests.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately
- **Phase 2 (Foundational)**: depends on Phase 1 and blocks all user stories
- **Phase 3 (US1)**: depends on Phase 2
- **Phase 4 (US2)**: depends on US1 reorder baseline
- **Phase 5 (US3)**: depends on US1 reorder baseline
- **Phase 6 (Polish)**: depends on all targeted stories being complete

### User Story Dependencies

- **US1 (P1)**: MVP reorder baseline with persistence and immediate UI update
- **US2 (P2)**: extends US1 with duplicate-instance identity guarantees
- **US3 (P2)**: extends US1 with failure handling and state reconciliation

### Within Each User Story

- Write required backend business-rule unit tests before implementation.
- Implement backend reorder rules before frontend integration.
- Complete story-level tests before progressing to next story.

### Parallel Opportunities

- Foundational tasks marked `[P]` can run in parallel (`T005`, `T006`).
- In US1, backend test work and frontend e2e scaffolding marked `[P]` can run together (`T008`-`T011`).
- In US2 and US3, marked `[P]` tests can run in parallel once endpoint contract is stable (`T017`-`T019`, `T022`-`T024`).

---

## Parallel Example: User Story 1

```bash
Task: "T008 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs"
Task: "T009 [US1] Integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ReorderWorkoutLiftsIntegrationTests.cs"
Task: "T011 [US1] E2E happy path in frontend/tests/e2e/workouts/reorder-workout-lifts.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T017 [US2] Duplicate-instance unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs"
Task: "T018 [US2] Duplicate-instance integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/ReorderWorkoutLiftsIntegrationTests.cs"
Task: "T019 [US2] Duplicate-instance e2e test in frontend/tests/e2e/workouts/reorder-workout-lifts-duplicates.spec.ts"
```

## Parallel Example: User Story 3

```bash
Task: "T022 [US3] Failure-outcome unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/ReorderWorkoutLifts/ReorderWorkoutLiftsCommandHandlerTests.cs"
Task: "T023 [US3] Failure contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutLiftsApiContractTests.cs"
Task: "T024 [US3] Failure e2e test in frontend/tests/e2e/workouts/reorder-workout-lifts-failures.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational phases.
2. Deliver US1 reorder flow end-to-end.
3. Validate immediate save plus persisted order after refresh.
4. Demo before extending to duplicate and failure depth.

### Incremental Delivery

1. US1: baseline reorder persistence and UI update.
2. US2: duplicate-entry identity-safe reorder behavior.
3. US3: explicit failure feedback and authoritative reconciliation.
4. Polish and regression verification.

### Simplicity + SOLID Guidance

- Keep reorder business rules in backend command handler and keep Angular focused on UI orchestration.
- Reuse existing workout-lift data structures rather than introducing a new domain aggregate.
- Preserve one-class-per-file for all new backend production classes.
