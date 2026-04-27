# Tasks: View lift history inline

**Input**: Design documents from `/specs/022-view-lift-history/`  
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Include backend unit/contract coverage for exact-lift completed-session selection and frontend unit coverage for inline panel behavior and failure handling.

**Organization**: Tasks are grouped by user story to keep implementation independently testable and incrementally deliverable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency collisions)
- **[Story]**: User story label (`[US1]`, `[US2]`, `[US3]`)
- Every task includes an exact file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align feature docs and current code boundaries before behavior changes.

- [X] T001 Confirm feature context in `.specify/feature.json` and planning docs under `specs/022-view-lift-history/`
- [X] T002 Capture active workout baseline touchpoints in `frontend/src/app/features/workouts/active-workout-page.component.ts` and `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared API and model scaffolding used by all user stories.

- [X] T003 Add inline-history API response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/`
- [X] T004 [P] Add frontend API models and client method in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T005 [P] Add per-entry inline-history state models in `frontend/src/app/core/state/workouts-store.models.ts`

**Checkpoint**: Shared contract/state scaffolding is ready for story implementation.

---

## Phase 3: User Story 1 - Expand same-lift history inline during entry (Priority: P1) 🎯 MVP

**Goal**: Users can open an inline panel from each lift row and stay on the active workout page.

**Independent Test**: On active workout page, tap View History on a lift and verify an inline panel opens in-row without navigation.

### Tests for User Story 1

- [X] T006 [US1] Add frontend unit test for View History inline panel toggle in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`
- [X] T007 [US1] Add frontend unit test that opening history does not invoke router navigation in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`

### Implementation for User Story 1

- [X] T008 [US1] Add inline history panel signals and actions in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T009 [US1] Add View History action and inline panel markup in `frontend/src/app/features/workouts/active-workout-page.component.html`
- [X] T010 [US1] Add inline history panel styles for mobile readability in `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: User Story 1 is functional and testable independently.

---

## Phase 4: User Story 2 - See only the most recent completed same-lift sessions (Priority: P1)

**Goal**: Inline panel shows exact-lift completed-only history limited to three recent sessions.

**Independent Test**: For a lift with >3 completed sessions, open View History and verify only the three most-recent completed sessions for that exact lift are shown.

### Tests for User Story 2

- [X] T011 [US2] Add backend unit tests for exact-lift completed-only recency selection in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/Queries/`
- [X] T012 [US2] Add backend contract/integration test for inline history endpoint payload/limit in `backend/tests/WeightLifting.Api.ContractTests/Workouts/`
- [X] T013 [US2] Add frontend unit test for last-three and exact-lift rendering in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`

### Implementation for User Story 2

- [X] T014 [US2] Implement completed exact-lift history query helper in `backend/src/WeightLifting.Api/Application/Workouts/Queries/`
- [X] T015 [US2] Add inline lift history GET endpoint in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`
- [X] T016 [US2] Wire inline history API call and data mapping in `frontend/src/app/core/api/workout-lifts-api.service.ts`
- [X] T017 [US2] Bind inline history session/set rendering to panel state in `frontend/src/app/features/workouts/active-workout-page.component.ts`

**Checkpoint**: User Story 2 is functional and testable independently.

---

## Phase 5: User Story 3 - Maintain confidence during loading and failure states (Priority: P2)

**Goal**: Inline history loading, empty, and failure states are clear and non-disruptive.

**Independent Test**: Simulate inline history load failure and verify inline feedback appears while set-entry actions remain available.

### Tests for User Story 3

- [X] T018 [US3] Add frontend unit test for loading, empty, and failure panel states in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`
- [X] T019 [US3] Add frontend unit test ensuring other lift actions remain available after history-load failure in `frontend/src/app/features/workouts/active-workout-page.component.spec.ts`

### Implementation for User Story 3

- [X] T020 [US3] Implement per-entry loading/error/empty transitions for inline history in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [X] T021 [US3] Add user-facing inline loading/empty/error copy in `frontend/src/app/features/workouts/active-workout-page.component.html`

**Checkpoint**: User Story 3 is functional and testable independently.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate end-to-end expectations and documentation alignment.

- [X] T022 [P] Run targeted frontend and backend tests covering inline history behavior
- [X] T023 [P] Verify and adjust `specs/022-view-lift-history/quickstart.md` based on implemented behavior

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1**: No dependencies.
- **Phase 2**: Depends on Phase 1; blocks story work.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2 and should follow US1 UI scaffolding.
- **Phase 5 (US3)**: Depends on US1 + US2 data and panel plumbing.
- **Phase 6**: Depends on completion of US1, US2, and US3.

### User Story Dependencies

- **US1 (P1)**: Can start immediately after foundational setup.
- **US2 (P1)**: Depends on foundational work; integrates with US1 panel actions.
- **US3 (P2)**: Depends on US1/US2 panel and API behavior.

### Parallel Opportunities

- T004 and T005 can run in parallel in Phase 2.
- T022 and T023 can run in parallel in Phase 6.

---

## Parallel Example: User Story 2

```bash
Task: "T011 Add backend unit tests for exact-lift completed-only recency selection in backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/Queries/"
Task: "T013 Add frontend unit test for last-three and exact-lift rendering in frontend/src/app/features/workouts/active-workout-page.component.spec.ts"
```

---

## Implementation Strategy

### MVP First (US1)

1. Complete Phase 1 and Phase 2.
2. Implement US1 panel open/close behavior and no-navigation validation.
3. Validate US1 tests before expanding data behavior.

### Incremental Delivery

1. Ship US1 inline interaction shell.
2. Add US2 backend+frontend exact-lift recent-history data flow.
3. Add US3 resilient loading/error/empty states.
4. Run polish validations and quickstart verification.

### Notes

- Keep business filtering rules in backend query helper, not Angular UI code.
- Preserve existing active-workout set-entry and lift-management behavior while adding inline history.
- Mark each task complete (`[X]`) as execution progresses.
