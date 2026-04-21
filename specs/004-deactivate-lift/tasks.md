# Tasks: Deactivate Lift

**Input**: Design documents from `/specs/004-deactivate-lift/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks whenever the feature changes behavior. Unit tests are REQUIRED for any business-layer logic. Add integration, contract, or end-to-end tests when API, persistence, or cross-boundary behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this belongs to (e.g. `US1`, `US2`)
- Include exact file paths in descriptions

## Path Conventions

- **Backend app**: `backend/src/WeightLifting.Api/`
- **Backend tests**: `backend/tests/WeightLifting.Api.UnitTests/`, `backend/tests/WeightLifting.Api.IntegrationTests/`, `backend/tests/WeightLifting.Api.ContractTests/`
- **Frontend app**: `frontend/src/app/`
- **Frontend tests**: `frontend/tests/unit/`, `frontend/tests/e2e/`
- Prefer extending existing lift objects, handlers, shared state, and the current `Settings -> Lifts` page before adding new abstractions
- Keep one production class per file unless a documented exception is required

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the minimum scaffolding needed to add lift deactivation without introducing new application boundaries.

- [ ] T001 Create the deactivate command and backend test directory scaffolding in `backend/src/WeightLifting.Api/Application/Lifts/Commands/DeactivateLift/`, `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/DeactivateLift/`, and `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/`
- [ ] T002 [P] Add frontend deactivate/filter test scaffolding in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`, `frontend/tests/unit/core/state/lifts-store.service.spec.ts`, and `frontend/tests/e2e/settings-lifts/deactivate-lift.spec.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared contracts and command primitives that MUST exist before the user stories are implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Create the backend deactivate response contract in `backend/src/WeightLifting.Api/Api/Contracts/Lifts/DeactivateLiftResponse.cs`
- [ ] T004 [P] Extend the shared frontend lifts API client with deactivate response typing and a `deactivateLift` method in `frontend/src/app/core/api/lifts-api.service.ts`
- [ ] T005 Create the deactivate command model in `backend/src/WeightLifting.Api/Application/Lifts/Commands/DeactivateLift/DeactivateLiftCommand.cs`

**Checkpoint**: Foundation ready. The repo has the minimal deactivate contract surface needed to extend the existing lift stack.

---

## Phase 3: User Story 1 - Deactivate an Existing Lift (Priority: P1) 🎯 MVP

**Goal**: Let a user deactivate an existing lift from `Settings -> Lifts` with explicit confirmation so the lift no longer appears in default active-only lift lists.

**Independent Test**: Open `Settings -> Lifts`, choose an active lift, confirm deactivation, and verify the lift disappears from the default active-only list while failed or cancelled attempts leave it active.

### Tests for User Story 1

> **NOTE: Write required tests before implementation. Business-layer unit tests are mandatory and should fail before the corresponding implementation is completed.**

- [ ] T006 [P] [US1] Add backend unit tests for deactivate state transitions, missing-lift handling, and no-op behavior in `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/DeactivateLift/DeactivateLiftCommandHandlerTests.cs`
- [ ] T007 [P] [US1] Add backend integration tests for deactivate persistence and default active-only list exclusion in `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/DeactivateLiftIntegrationTests.cs`
- [ ] T008 [P] [US1] Add contract tests for `PUT /api/lifts/{liftId}/deactivate` success and not-found responses in `backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs`
- [ ] T009 [P] [US1] Add Angular unit tests for deactivate confirmation state and save feedback in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`
- [ ] T010 [P] [US1] Add a mobile e2e test for successful, cancelled, and failed deactivate flows in `frontend/tests/e2e/settings-lifts/deactivate-lift.spec.ts`

### Implementation for User Story 1

- [ ] T011 [P] [US1] Extend the existing lift domain object with deactivate behavior in `backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs`
- [ ] T012 [US1] Implement the deactivate command handler in `backend/src/WeightLifting.Api/Application/Lifts/Commands/DeactivateLift/DeactivateLiftCommandHandler.cs`
- [ ] T013 [US1] Register the deactivate handler and add the deactivate endpoint in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs` and `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [ ] T014 [US1] Extend the existing lifts facade with deactivate confirmation, submit, cancel, and failure handling in `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`
- [ ] T015 [P] [US1] Update the current settings-lifts page markup and styling for deactivate controls and confirmation UI in `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - View Inactive Lifts in Management (Priority: P2)

**Goal**: Let a user switch the `Settings -> Lifts` view between active-only and include-inactive results while keeping default selection reads active-only.

**Independent Test**: Deactivate a lift, switch the `Settings -> Lifts` filter from active-only to include inactive lifts, and verify the inactive lift becomes visible while default active-only views still hide it.

### Tests for User Story 2

- [ ] T016 [P] [US2] Add backend integration and contract coverage for `activeOnly=true` versus `activeOnly=false` lift-list reads after deactivation in `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/DeactivateLiftIntegrationTests.cs` and `backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs`
- [ ] T017 [P] [US2] Add frontend unit tests for filter toggling, inactive lift rendering, and selected-filter refresh behavior in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts` and `frontend/tests/unit/core/state/lifts-store.service.spec.ts`
- [ ] T018 [P] [US2] Add mobile e2e coverage for the include-inactive filter behavior in `frontend/tests/e2e/settings-lifts/deactivate-lift.spec.ts`

### Implementation for User Story 2

- [ ] T019 [US2] Extend the existing lifts facade to track the selected active/inactive filter and reload lifts with the current `activeOnly` value in `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`
- [ ] T020 [P] [US2] Update the current settings-lifts page markup and styling to expose the filter and visually distinguish inactive lifts in `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`
- [ ] T021 [US2] Adjust shared lift-store reconciliation so confirmed deactivation and filtered list refreshes preserve the selected view without introducing duplicate state models in `frontend/src/app/core/state/lifts-store.service.ts` and `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently, with inactive lifts hidden by default and viewable on demand.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup after deactivation and filtering work end-to-end.

- [ ] T022 Run the backend unit, integration, contract, frontend unit, and e2e suites from `backend/tests/` and `frontend/tests/`
- [ ] T023 Validate the deactivate quickstart manually on a mobile viewport using `specs/004-deactivate-lift/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies, start immediately
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user-story work
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on Foundational completion and builds on deactivation support introduced in User Story 1
- **Polish (Phase 5)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational and is the MVP for this feature
- **User Story 2 (P2)**: Builds on the deactivate flow from US1 and extends the current settings page with filter behavior

### Within Each User Story

- Required business-layer unit tests MUST be written and fail before implementation
- Contracts and shared API client updates before controller and facade wiring
- Domain behavior before application command handling
- Backend deactivate behavior before frontend reconciliation
- Story complete before moving to the next priority when working sequentially

### Parallel Opportunities

- T002 can run in parallel with T001 once the feature scope is fixed
- T003 and T004 can run in parallel after Phase 1 starts, while T005 stays sequential with backend command work
- T006 through T010 can run in parallel because they target different test layers
- T011 and T015 can run in parallel because they touch different layers and files
- T016 through T018 can run in parallel because they target different test files
- T020 can run in parallel with T019 once the filter semantics are defined

---

## Parallel Example: User Story 1

```bash
# Launch deactivate test-writing tasks together:
Task: "Add backend unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/DeactivateLift/DeactivateLiftCommandHandlerTests.cs"
Task: "Add backend integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Lifts/DeactivateLiftIntegrationTests.cs"
Task: "Add Angular unit tests in frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts"
Task: "Add mobile e2e deactivate coverage in frontend/tests/e2e/settings-lifts/deactivate-lift.spec.ts"

# Launch backend and frontend work together after tests exist:
Task: "Extend Lift deactivate behavior in backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs"
Task: "Update settings-lifts page markup and styling in frontend/src/app/features/settings/lifts/lifts-page.component.html and frontend/src/app/features/settings/lifts/lifts-page.component.scss"
```

---

## Parallel Example: User Story 2

```bash
# Launch filter-focused tests together:
Task: "Add backend list-filter coverage in backend/tests/WeightLifting.Api.IntegrationTests/Lifts/DeactivateLiftIntegrationTests.cs and backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs"
Task: "Add frontend filter tests in frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts and frontend/tests/unit/core/state/lifts-store.service.spec.ts"
Task: "Add e2e include-inactive coverage in frontend/tests/e2e/settings-lifts/deactivate-lift.spec.ts"

# Launch filter state and UI work together:
Task: "Extend lifts-page facade filter state in frontend/src/app/features/settings/lifts/lifts-page.facade.ts"
Task: "Update filter controls and inactive styling in frontend/src/app/features/settings/lifts/lifts-page.component.html and frontend/src/app/features/settings/lifts/lifts-page.component.scss"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE** using the independent test and `quickstart.md`
5. Complete Phase 4 only after the core deactivate flow is correct

### Incremental Delivery

1. Reuse the existing lift stack and add the minimal deactivate contract surface
2. Add the in-page deactivate flow with backend confirmation and failure-safe reconciliation
3. Validate successful deactivation, cancellation, and failed-save messaging
4. Add the include-inactive filter without introducing a second lift state model
5. Finish with full automated coverage and manual mobile verification

### Suggested MVP Scope

Deliver through **Phase 3** first. That gives the smallest complete slice to judge direction:
deactivate an existing lift from `Settings -> Lifts` with explicit confirmation, confirmed
persistence, and default active-only exclusion.

## Notes

- Total tasks: 23
- User Story 1 tasks: 10
- User Story 2 tasks: 6
- The task list intentionally extends existing `Lift`, `GetLifts`, `LiftsController`, `LiftsApiService`, `LiftsStoreService`, and `LiftsPageFacade` flows instead of introducing a generic status-management abstraction
- All tasks follow the required checklist format with IDs, labels, and file paths
- Keep one production class per file unless an explicit exception is justified
