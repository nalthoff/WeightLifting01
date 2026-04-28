# Tasks: Backfill past workout

**Input**: Design documents from `/specs/023-backfill-past-workout/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/historical-workout-api.yaml`, `quickstart.md`

**Tests**: Include test tasks whenever the feature changes behavior. Unit tests are REQUIRED for backend application/domain logic. Add integration, contract, and end-to-end tests for API, SQL persistence, and cross-boundary behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., `US1`, `US2`, `US3`)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare shared contracts and extension points used by all stories.

- [X] T001 Extend existing workout API contracts by adding only historical timing request fields and reusing standard workout response DTOs in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/`
- [X] T002 [P] Reuse existing frontend workout API models and add minimal historical timing extensions via shared interfaces in `frontend/src/app/core/api/workouts.models.ts`
- [X] T003 [P] Add route and menu-entry placeholders for historical logging access in `frontend/src/app/features/home/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core reusable backend/frontend plumbing that must be in place before story-level behavior.

- [X] T004 Implement reusable workout timing validation abstractions (base validator + historical specialization) in `backend/src/WeightLifting.Api/Application/Workouts/Commands/`
- [X] T005 [P] Wire historical workout endpoint surface in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T006 [P] Add historical workout API client methods by extending existing workout service methods (no duplicate request/response mapping) in `frontend/src/app/core/api/workouts-api.service.ts`
- [X] T007 Add shared active-workout state query/update helpers for coexistence checks in `backend/src/WeightLifting.Api/Application/Workouts/Queries/`
- [X] T008 Add shared frontend workout state hooks for historical-flow navigation and feedback by reusing existing workout state stores/selectors in `frontend/src/app/core/state/`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Log missed workout on chosen day (Priority: P1) 🎯 MVP

**Goal**: Require past date, start time, and duration, then save a completed historical workout with deterministic chronology.

**Independent Test**: Create a historical workout with past date/time/duration and verify chronological placement in history.

### Tests for User Story 1

- [X] T009 [P] [US1] Add unit tests for shared timing validator base behavior and historical validator specialization in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/CreateHistoricalWorkoutCommandHandlerTests.cs`
- [X] T010 [P] [US1] Add integration tests for historical create/complete lifecycle in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/HistoricalWorkoutLifecycleTests.cs`
- [X] T011 [P] [US1] Add contract tests for `POST /api/workouts/historical` request/response requirements in `backend/tests/WeightLifting.Api.ContractTests/Workouts/HistoricalWorkoutApiContractTests.cs`
- [X] T012 [P] [US1] Add frontend unit tests for required field validation and save gating in `frontend/src/app/features/workouts/historical-workout-form.component.spec.ts`

### Implementation for User Story 1

- [X] T013 [US1] Implement historical create command handler by deriving from shared workout command base and adding only past-date/time/duration rules in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CreateHistoricalWorkoutCommandHandler.cs`
- [X] T014 [US1] Implement completion timestamp derivation in shared completion workflow to support both live and historical sessions in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkoutCommandHandler.cs`
- [X] T015 [US1] Implement deterministic historical ordering projection for history queries in `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetWorkoutHistoryQueryHandler.cs`
- [X] T016 [US1] Implement historical create endpoint request mapping with shared response mapping reused from existing workout endpoints in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T017 [US1] Build mobile-first historical workout timing form (date, hour/minute, duration) in `frontend/src/app/features/workouts/historical-workout-form.component.ts`
- [X] T018 [US1] Wire save success/failure messaging for historical entry flow in `frontend/src/app/features/workouts/historical-workout-form.component.html`
- [X] T019 [US1] Connect historical timing form to API service and history refresh in `frontend/src/app/features/workouts/historical-workout-form.component.ts`

**Checkpoint**: User Story 1 should be fully functional and independently testable.

---

## Phase 4: User Story 2 - Keep lift and set entry parity (Priority: P1)

**Goal**: Reuse live workout lift/set entry model for historical workouts and keep history/detail parity.

**Independent Test**: Add multiple lifts/sets in historical flow, complete workout, and verify details in history/detail surfaces.

### Tests for User Story 2

- [X] T020 [P] [US2] Add backend unit tests proving historical flow reuses live lift/set persistence abstractions with no duplicate write path in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/HistoricalWorkoutLiftSetParityTests.cs`
- [X] T021 [P] [US2] Add backend integration tests for historical workout detail retrieval with lifts/sets in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/HistoricalWorkoutDetailsTests.cs`
- [X] T022 [P] [US2] Add frontend e2e test for adding lifts/sets during historical logging in `frontend/tests/e2e/historical-workout-lift-set-parity.spec.ts`

### Implementation for User Story 2

- [X] T023 [US2] Reuse live workout lift/set command path by introducing a shared base workout entry service and historical specialization only where required in `backend/src/WeightLifting.Api/Application/Workouts/Commands/`
- [X] T024 [US2] Ensure historical workout detail query returns lift/set payload parity in `backend/src/WeightLifting.Api/Application/Workouts/Queries/GetWorkoutDetailQueryHandler.cs`
- [X] T025 [US2] Adapt workout entry UI to launch historical mode with shared lift/set editors in `frontend/src/app/features/workouts/workout-entry.component.ts`
- [X] T026 [US2] Ensure history detail screen renders historical lifts/sets consistently in `frontend/src/app/features/history/workout-history-detail.component.ts`

**Checkpoint**: User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Catch up while a live workout exists (Priority: P2)

**Goal**: Allow completing historical workouts without interrupting or mutating active in-progress workout context.

**Independent Test**: Keep an active workout running, complete historical workout, verify active workout still available and unchanged.

### Tests for User Story 3

- [X] T027 [P] [US3] Add backend unit tests for active-workout continuity guard behavior using shared lifecycle guard abstractions in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/HistoricalWorkoutActiveContextTests.cs`
- [X] T028 [P] [US3] Add backend integration tests for active + historical coexistence lifecycle in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/HistoricalAndActiveWorkoutCoexistenceTests.cs`
- [X] T029 [P] [US3] Add frontend e2e catch-up scenario test with active workout preserved in `frontend/tests/e2e/historical-workout-active-context.spec.ts`

### Implementation for User Story 3

- [X] T030 [US3] Implement lifecycle rules in shared workout lifecycle service, with historical-specific override only for coexistence checks, in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkoutCommandHandler.cs`
- [X] T031 [US3] Implement conflict handling and explicit problem responses for lifecycle violations in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T032 [US3] Implement frontend catch-up flow routing that returns user to active workout context in `frontend/src/app/features/workouts/workout-entry.component.ts`

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, chronology edge-case hardening, and documentation updates.

- [X] T033 [P] Add backend regression tests for same-day ordering tie-break behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryOrderingTests.cs`
- [X] T034 [P] Add frontend regression test for required-field mobile UX feedback in `frontend/src/app/features/workouts/historical-workout-form.component.spec.ts`
- [X] T035 Run quickstart validation scenarios and document observed results in `specs/023-backfill-past-workout/quickstart.md`
- [X] T036 Update feature documentation and API notes for historical workflow behavior in `README.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; can start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; forms MVP.
- **Phase 4 (US2)**: Depends on Phase 2 and integrates with US1 data/flows.
- **Phase 5 (US3)**: Depends on Phase 2; validates coexistence behavior across US1/US2 flow.
- **Phase 6 (Polish)**: Depends on completion of all target user stories.

### User Story Dependencies

- **US1 (P1)**: Independent after foundational plumbing.
- **US2 (P1)**: Independent after foundational plumbing; reuses US1 timing/completion surfaces.
- **US3 (P2)**: Independent after foundational plumbing; verifies coexistence with active-workout context and historical flow.

### Within Each User Story

- Tests first (unit/contract/integration/e2e), and expected to fail before implementation.
- Reuse existing contracts/services first; add specialization through base/derived classes or shared interfaces only when behavior truly differs.
- Backend command/query behavior before controller wiring where possible.
- API integration before final frontend UX polish.
- Complete each story to independent acceptance before moving to next.

### Parallel Opportunities

- `T002` and `T003` can run in parallel during setup.
- `T005`, `T006`, and `T008` can run in parallel after `T004`.
- Story test tasks marked `[P]` can run in parallel per story.
- Backend and frontend implementation tasks touching different files can run in parallel once contracts are stable.

---

## Parallel Example: User Story 1

```bash
# Run US1 tests in parallel
Task: "T009 [US1] backend unit tests for historical timing validation"
Task: "T010 [US1] backend integration tests for historical create/complete lifecycle"
Task: "T011 [US1] contract tests for POST /api/workouts/historical"
Task: "T012 [US1] frontend unit tests for required historical fields"

# Build backend and frontend pieces in parallel after tests exist
Task: "T013 [US1] create backend historical command handler"
Task: "T017 [US1] build frontend historical timing form"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver Phase 3 (US1) end-to-end.
3. Validate US1 independently via tests and quickstart scenario 1 + 2.
4. Demo/deploy MVP if acceptable.

### Incremental Delivery

1. Ship US1 (historical timing + chronology).
2. Ship US2 (lift/set parity and detail-view parity).
3. Ship US3 (active-workout coexistence).
4. Finish Phase 6 hardening and docs.

### Suggested MVP Scope

- **MVP**: Phase 1 + Phase 2 + Phase 3 (`US1`) only.
- **Post-MVP**: `US2` and `US3` in separate increments.
