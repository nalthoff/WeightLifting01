# Tasks: Workout Lifecycle Status Visibility

**Input**: Design documents from `specs/019-workout-status-visibility/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks whenever the feature changes behavior. Unit tests are REQUIRED for any backend application/domain logic. Add integration, contract, or end-to-end tests when API, SQL persistence, or cross-boundary behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Align feature scaffolding and test touchpoints for lifecycle status work.

- [x] T001 Confirm feature docs are current in `specs/019-workout-status-visibility/plan.md`, `specs/019-workout-status-visibility/research.md`, and `specs/019-workout-status-visibility/quickstart.md`
- [x] T002 Identify impacted frontend and backend test suites in `frontend/tests/unit/`, `frontend/tests/e2e/workouts/`, `backend/tests/WeightLifting.Api.UnitTests/`, and `backend/tests/WeightLifting.Api.IntegrationTests/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared lifecycle presentation primitives and API contract alignment required before story-specific implementation.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T003 Create reusable workout status badge styles in `frontend/src/app/features/workouts/active-workout-page.component.scss`
- [x] T004 [P] Add shared status-display helper mapping lifecycle enum values in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [x] T005 [P] Validate workout lifecycle summary contract expectations in `backend/src/WeightLifting.Api/Api/Contracts/Workouts/WorkoutSessionSummaryResponse.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Active Workout Status Clarity (Priority: P1) 🎯 MVP

**Goal**: Show a clear "In Progress" status badge on active workout detail with safe handling for unexpected status values.

**Independent Test**: Start an active workout and verify the detail screen renders an "In Progress" badge and does not render misleading status text for malformed status values.

### Tests for User Story 1

- [x] T006 [P] [US1] Add active badge rendering/unit-state tests in `frontend/tests/unit/workouts/active-workout-page.component.spec.ts`
- [ ] T007 [P] [US1] Add active workflow badge visibility e2e coverage in `frontend/tests/e2e/workouts/home-active-workout.spec.ts`

### Implementation for User Story 1

- [x] T008 [US1] Replace plain active status text with badge markup in `frontend/src/app/features/workouts/active-workout-page.component.html`
- [x] T009 [US1] Implement lifecycle display mapping and safe fallback behavior in `frontend/src/app/features/workouts/active-workout-page.component.ts`
- [x] T010 [US1] Apply mobile-first badge styling in `frontend/src/app/features/workouts/active-workout-page.component.scss`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - Completed Workout Status Clarity (Priority: P1)

**Goal**: Show a clear "Completed" status badge on completed workout detail in history context.

**Independent Test**: Open a completed workout from history and verify the detail screen renders a "Completed" badge consistently across refresh/navigation.

### Tests for User Story 2

- [x] T011 [P] [US2] Add completed-detail badge unit coverage in `frontend/src/app/features/history/history-workout-detail-page.component.spec.ts`
- [ ] T012 [P] [US2] Extend history-detail e2e coverage for completed badge visibility in `frontend/tests/e2e/workouts/workout-history.spec.ts`

### Implementation for User Story 2

- [x] T013 [US2] Add completed status badge markup in `frontend/src/app/features/history/history-workout-detail-page.component.html`
- [x] T014 [US2] Expose completed-status display state in `frontend/src/app/features/history/history-workout-detail-page.component.ts`
- [x] T015 [US2] Add completed badge styling for mobile detail layout in `frontend/src/app/features/history/history-workout-detail-page.component.scss`

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - History and Progress Gating (Priority: P1)

**Goal**: Ensure history/progress views include only completed workouts with valid completion timestamps and preserve completion timestamp behavior.

**Independent Test**: With one active and one completed workout, verify history/progress includes only the completed workout and completion always stamps end timestamp.

### Tests for User Story 3

- [ ] T016 [P] [US3] Add completion timestamp lifecycle unit assertions in `backend/tests/WeightLifting.Api.UnitTests/Domain/Workouts/WorkoutTests.cs`
- [x] T017 [P] [US3] Add completed-only history filtering unit coverage in `backend/tests/WeightLifting.Api.UnitTests/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelperTests.cs`
- [x] T018 [P] [US3] Add integration test coverage for completion timestamp and history gating in `backend/tests/WeightLifting.Api.IntegrationTests/Workouts/WorkoutHistoryIntegrationTests.cs`

### Implementation for User Story 3

- [x] T019 [US3] Enforce completed-and-timestamp eligibility in history query helper in `backend/src/WeightLifting.Api/Application/Workouts/Queries/ListCompletedWorkouts/ListCompletedWorkoutsQueryHelper.cs`
- [x] T020 [US3] Confirm completion command preserves completion timestamp behavior in `backend/src/WeightLifting.Api/Application/Workouts/Commands/CompleteWorkout/CompleteWorkoutCommandHandler.cs`
- [x] T021 [US3] Ensure workout summary contract remains lifecycle-consistent for history consumers in `backend/src/WeightLifting.Api/Api/Controllers/WorkoutsController.cs`

**Checkpoint**: All user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cross-story consistency.

- [ ] T022 [P] Run frontend unit and e2e validations for lifecycle badge and history flows via `frontend/tests/unit/` and `frontend/tests/e2e/workouts/`
- [x] T023 [P] Run backend unit and integration validations for lifecycle completion/history behavior via `backend/tests/WeightLifting.Api.UnitTests/` and `backend/tests/WeightLifting.Api.IntegrationTests/`
- [ ] T024 Execute quickstart verification scenarios in `specs/019-workout-status-visibility/quickstart.md`
- [ ] T025 Validate mobile viewport badge readability and action-state correctness in `frontend/src/app/features/workouts/` and `frontend/src/app/features/history/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks story implementation.
- **Phase 3-5 (User Stories)**: Depend on Phase 2 completion.
- **Phase 6 (Polish)**: Depends on completion of all targeted user stories.

### User Story Dependencies

- **US1 (P1)**: Can start immediately after Foundational phase.
- **US2 (P1)**: Can start after Foundational phase; functionally independent of US1.
- **US3 (P1)**: Can start after Foundational phase; backend-focused and independent of UI stories.

### Within Each User Story

- Tests should be authored before or alongside implementation and validated before story completion.
- UI markup/state/styling tasks on the same component proceed sequentially.
- Backend query/command updates follow corresponding test updates.

### Parallel Opportunities

- Foundational tasks `T004` and `T005` can run in parallel.
- US1 test tasks `T006` and `T007` can run in parallel.
- US2 test tasks `T011` and `T012` can run in parallel.
- US3 test tasks `T016`, `T017`, and `T018` can run in parallel.
- Polish validation tasks `T022` and `T023` can run in parallel.

---

## Parallel Example: User Story 3

```bash
# Launch US3 tests together:
Task: "T016 [US3] completion timestamp lifecycle unit assertions"
Task: "T017 [US3] completed-only history filtering unit coverage"
Task: "T018 [US3] completion timestamp and history gating integration coverage"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Implement US1 badge clarity on active workout detail.
3. Validate US1 unit/e2e tests.
4. Demo/verify active status clarity on mobile viewport.

### Incremental Delivery

1. Deliver US1 active badge clarity.
2. Deliver US2 completed badge clarity.
3. Deliver US3 history/progress gating validation.
4. Perform polish validation and quickstart checks.

### Parallel Team Strategy

1. Complete Setup + Foundational together.
2. Assign frontend owner to US1 and US2.
3. Assign backend owner to US3.
4. Merge and run cross-cutting validation in Phase 6.

---

## Notes

- `[P]` tasks indicate independent files and safe parallel execution.
- Keep one production class per file for backend changes.
- Do not introduce new workout statuses in this feature.
- Maintain explicit mobile-first readability for all new status badges.
