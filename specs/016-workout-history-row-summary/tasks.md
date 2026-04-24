# Tasks: Workout history row summary details

**Input**: Design documents from `/specs/016-workout-history-row-summary/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workout-history-summary-api.yaml`, `quickstart.md`

**Tests**: Include backend unit tests for duration/lift-count business logic plus integration, contract, frontend unit, and e2e coverage for history row rendering and regression states.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm current history feature touchpoints and test surfaces before implementation.

- [x] T001 Review existing history query and contract touchpoints in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`, `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutHistoryItemResponse.cs`, and `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [x] T002 [P] Review history frontend rendering and API client usage in `frontend/src/app/features/history/history-page.component.ts`, `frontend/src/app/features/history/history-page.component.html`, and `frontend/src/app/core/api/workouts-api.service.ts`
- [x] T003 [P] Review existing automated coverage for history and completion flow in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`, `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`, `frontend/tests/unit/history/history-page.component.spec.ts`, and `frontend/tests/e2e/workouts/workout-history.spec.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared contract/model scaffolding required by both stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T004 Extend backend history contract DTO with duration and lift count fields in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutHistoryItemResponse.cs`
- [x] T005 [P] Extend backend query projection model for row summary fields in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/CompletedWorkoutHistoryItem.cs`
- [x] T006 [P] Extend frontend history API DTO for duration and lift count fields in `frontend/src/app/core/api/workouts-api.service.ts`
- [x] T007 [P] Update history API contract documentation in `specs/016-workout-history-row-summary/contracts/workout-history-summary-api.yaml`

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Scan enriched completed workout history rows (Priority: P1) 🎯 MVP

**Goal**: Show each completed workout row with label, completion date, duration (`HH:MM`), and lift count while preserving recency ordering.

**Independent Test**: Open history with completed workouts and verify each row includes all required fields, shows lift count for zero-lift workouts, and remains ordered most recent first.

### Tests for User Story 1

- [x] T008 [P] [US1] Add backend unit tests for duration formatting, invalid timestamp fallback, and lift-count derivation in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelperTests.cs`
- [x] T009 [P] [US1] Add backend integration tests for completed-only recency ordering and summary fields in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`
- [x] T010 [P] [US1] Add backend contract tests for enriched `GET /api/workouts/history` response shape in `backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs`
- [x] T011 [P] [US1] Add frontend unit tests for duration/lift-count row rendering in `frontend/tests/unit/history/history-page.component.spec.ts`
- [x] T012 [P] [US1] Add e2e coverage for enriched history rows in `frontend/tests/e2e/workouts/workout-history.spec.ts`

### Implementation for User Story 1

- [x] T013 [US1] Implement history query logic for duration calculation, safe fallback, and lift count in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`
- [x] T014 [US1] Map enriched history query fields in API response mapping in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [x] T015 [US1] Update frontend history page state handling for duration and lift count fields in `frontend/src/app/features/history/history-page.component.ts`
- [x] T016 [US1] Render duration and lift-count metadata in each history row in `frontend/src/app/features/history/history-page.component.html` and `frontend/src/app/features/history/history-page.component.scss`

**Checkpoint**: US1 is fully functional and testable independently.

---

## Phase 4: User Story 2 - Preserve existing history behavior and regression states (Priority: P1)

**Goal**: Keep existing route/navigation, completed-only behavior, ordering, and empty/error states stable while new row metadata is present.

**Independent Test**: Validate history route entry, completed-only list behavior, and existing empty/error states still work with the enhanced row model.

### Tests for User Story 2

- [x] T017 [P] [US2] Add frontend unit regression tests for empty and load-error states with enhanced row model in `frontend/tests/unit/history/history-page.component.spec.ts`
- [x] T018 [P] [US2] Add e2e regression checks for history route and state behavior in `frontend/tests/e2e/workouts/workout-history.spec.ts`
- [x] T019 [P] [US2] Add backend integration regression assertions for completed-only filtering and stable ordering in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`

### Implementation for User Story 2

- [x] T020 [US2] Preserve and verify history route wiring with no navigation regressions in `frontend/src/app/app.routes.ts`, `frontend/src/app/app.html`, and `frontend/src/app/features/history/history.routes.ts`
- [x] T021 [US2] Preserve existing history page loading/empty/error behavior while integrating enriched rows in `frontend/src/app/features/history/history-page.component.ts` and `frontend/src/app/features/history/history-page.component.html`
- [x] T022 [US2] Preserve completed-only and recency query semantics while adding enriched projection fields in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`

**Checkpoint**: US1 and US2 are independently functional and regression-safe.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cross-story hardening.

- [x] T023 [P] Run backend unit/integration/contract suites for history summary behavior in `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, and `backend/tests/WeightLifting.Api.ContractTests/`
- [x] T024 [P] Run frontend unit and e2e suites for history summary and regression flows in `frontend/tests/unit/history/` and `frontend/tests/e2e/workouts/`
- [x] T025 Validate quickstart walkthrough and update verification notes in `specs/016-workout-history-row-summary/quickstart.md`
- [x] T026 Validate mobile viewport readability for history row summary metadata in `frontend/src/app/features/history/history-page.component.html` and `frontend/src/app/features/history/history-page.component.scss`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks user stories.
- **Phase 3 (US1)**: Depends on Phase 2; defines MVP behavior.
- **Phase 4 (US2)**: Depends on Phase 2 and can run after US1 or in parallel with late US1 verification.
- **Phase 5 (Polish)**: Depends on all targeted user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Independent MVP slice for enriched history rows.
- **US2 (P1)**: Regression/stability slice that builds on shared foundational contract/model updates.

### Within Each User Story

- Backend business-rule unit tests should be written before corresponding backend implementation.
- API/contract and integration tests should be updated before finalizing implementation.
- Frontend rendering updates should follow API model updates for stable integration.

### Parallel Opportunities

- Setup review tasks marked `[P]` can run in parallel (`T002`, `T003`).
- Foundational model/contract tasks marked `[P]` can run in parallel (`T005`, `T006`, `T007`).
- US1 test tasks marked `[P]` can run in parallel (`T008`-`T012`).
- US2 test tasks marked `[P]` can run in parallel (`T017`, `T018`, `T019`).
- Polish validation tasks marked `[P]` can run in parallel (`T023`, `T024`).

---

## Parallel Example: User Story 1

```bash
Task: "T008 [US1] Unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelperTests.cs"
Task: "T010 [US1] Contract tests in backend/tests/WeightLifting.Api.ContractTests/Workouts/WorkoutsApiContractTests.cs"
Task: "T011 [US1] Frontend unit tests in frontend/tests/unit/history/history-page.component.spec.ts"
```

## Parallel Example: User Story 2

```bash
Task: "T017 [US2] Frontend unit regression tests in frontend/tests/unit/history/history-page.component.spec.ts"
Task: "T018 [US2] E2E regression checks in frontend/tests/e2e/workouts/workout-history.spec.ts"
Task: "T019 [US2] Backend integration regression assertions in backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Deliver US1 end-to-end (backend query + API + frontend rows + tests).
3. Validate US1 independently before adding regression hardening work.

### Incremental Delivery

1. Setup + Foundational
2. US1 enriched row summary (MVP)
3. US2 regression and behavior-preservation pass
4. Polish and full-suite verification

### Parallel Team Strategy

1. Team completes Setup + Foundational.
2. Then split:
   - Developer A: US1 backend and contract updates
   - Developer B: US1 frontend rendering/tests
3. After US1 merge, run US2 regression hardening in parallel across backend/frontend tests.
