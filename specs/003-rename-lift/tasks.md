# Tasks: Rename Lift

**Input**: Design documents from `/specs/003-rename-lift/`
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
- Keep one production class per file unless a documented exception is required

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the minimum feature scaffolding needed to start implementing and validating lift rename behavior.

- [X] T001 Create the rename command and test directory scaffolding in `backend/src/WeightLifting.Api/Application/Lifts/Commands/RenameLift/`, `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/RenameLift/`, and `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/`
- [X] T002 [P] Add the frontend rename test scaffolding in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts` and `frontend/tests/e2e/settings-lifts/rename-lift.spec.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared contracts and client/store primitives that MUST be complete before user-story behavior is implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Create the backend rename request and response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Lifts/RenameLiftRequest.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Lifts/RenameLiftResponse.cs`
- [X] T004 [P] Extend the shared frontend lifts API client with rename request/response types and a `renameLift` method in `frontend/src/app/core/api/lifts-api.service.ts`
- [X] T005 [P] Add a shared confirmed-rename replacement helper to `frontend/src/app/core/state/lifts-store.service.ts`
- [X] T006 Create the rename command model in `backend/src/WeightLifting.Api/Application/Lifts/Commands/RenameLift/RenameLiftCommand.cs`

**Checkpoint**: Foundation ready. The repo has the rename contract surface, shared API client support, and store primitives needed for story implementation.

---

## Phase 3: User Story 1 - Rename an Existing Lift (Priority: P1) 🎯 MVP

**Goal**: Let a user open an existing lift from `Settings -> Lifts`, change its name, and save the updated value without creating a second lift.

**Independent Test**: Open `Settings -> Lifts`, choose an existing lift, rename it with a valid name, save it, and verify the updated name appears on the page without creating a second lift.

### Tests for User Story 1

> **NOTE: Write required tests before implementation. Business-layer unit tests are mandatory and should fail before the corresponding implementation is completed.**

- [X] T007 [P] [US1] Add backend unit tests for required-name trimming and unchanged-name handling in `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/RenameLift/RenameLiftCommandHandlerTests.cs`
- [X] T008 [P] [US1] Add backend integration tests for successful rename persistence and failed-save behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/RenameLiftIntegrationTests.cs`
- [X] T009 [P] [US1] Add contract tests for `PUT /api/lifts/{liftId}` basic rename responses in `backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs`
- [X] T010 [P] [US1] Add Angular unit tests for entering edit mode and rename save feedback in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`
- [X] T011 [P] [US1] Add a mobile e2e test for rename success and failed-save messaging in `frontend/tests/e2e/settings-lifts/rename-lift.spec.ts`

### Implementation for User Story 1

- [X] T012 [P] [US1] Update lift rename behavior in `backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs`
- [X] T013 [US1] Implement the rename command handler in `backend/src/WeightLifting.Api/Application/Lifts/Commands/RenameLift/RenameLiftCommandHandler.cs`
- [X] T014 [US1] Implement the rename endpoint in `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [X] T015 [US1] Extend rename edit-state, submit, cancel, and save-state handling in `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`
- [X] T016 [P] [US1] Update the settings-lifts page markup and styling for rename entry/edit states in `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Keep Lift Names Consistent Across Usage (Priority: P2)

**Goal**: Prevent conflicting rename targets and ensure later lift-list reads show the canonical updated name for the same lift.

**Independent Test**: Rename a lift in `Settings -> Lifts`, then perform later lift-list reads and verify the updated name is shown for that same lift while conflicting rename targets are blocked.

### Tests for User Story 2

- [ ] T017 [P] [US2] Add backend unit tests for normalized duplicate-name conflict detection in `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/RenameLift/RenameLiftCommandHandlerTests.cs`
- [ ] T018 [P] [US2] Add integration and contract coverage for conflicting rename targets and canonical list reads in `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/RenameLiftIntegrationTests.cs` and `backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs`
- [ ] T019 [P] [US2] Add frontend unit tests for shared-store rename reconciliation in `frontend/tests/unit/core/state/lifts-store.service.spec.ts` and `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`

### Implementation for User Story 2

- [ ] T020 [US2] Add normalized duplicate-name checks against persisted lifts in `backend/src/WeightLifting.Api/Application/Lifts/Commands/RenameLift/RenameLiftCommandHandler.cs`
- [ ] T021 [US2] Update rename conflict and missing-lift HTTP mappings in `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [ ] T022 [US2] Update canonical shared-list reconciliation for renamed lifts in `frontend/src/app/core/state/lifts-store.service.ts` and `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`
- [ ] T023 [US2] Surface duplicate-name conflicts while preserving the previously saved lift name in `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently, with duplicate-name conflicts and later list reads covered.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup after the rename feature works end-to-end.

- [ ] T024 Run the backend unit, integration, contract, frontend unit, and e2e suites from `backend/tests/` and `frontend/tests/`
- [ ] T025 Validate the rename quickstart manually on a mobile viewport using `specs/003-rename-lift/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies, start immediately
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user-story work
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on Foundational completion and extends the rename path introduced in User Story 1
- **Polish (Phase 5)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational and is the MVP for this feature
- **User Story 2 (P2)**: Builds on the rename endpoint and UI introduced in US1, but remains independently testable as a follow-on slice

### Within Each User Story

- Required business-layer unit tests MUST be written and fail before implementation
- Contracts and request/response types before controller wiring
- Domain behavior before application command handling
- Backend rename behavior before frontend shared-store reconciliation
- Story complete before moving to the next priority when working sequentially

### Parallel Opportunities

- T002 can run in parallel with T001 once the feature scope is fixed
- T004 and T005 can run in parallel after T003 starts the rename contract surface
- T007 through T011 can run in parallel because they target different test layers
- T012 and T016 can run in parallel because they touch different files and layers
- T017 through T019 can run in parallel because they target different test files

---

## Parallel Example: User Story 1

```bash
# Launch rename test-writing tasks together:
Task: "Add backend unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/RenameLift/RenameLiftCommandHandlerTests.cs"
Task: "Add backend integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Lifts/RenameLiftIntegrationTests.cs"
Task: "Add Angular unit tests in frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts"
Task: "Add mobile e2e rename coverage in frontend/tests/e2e/settings-lifts/rename-lift.spec.ts"

# Launch backend and frontend implementation work together after tests exist:
Task: "Update lift rename behavior in backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs"
Task: "Update settings-lifts page markup and styling in frontend/src/app/features/settings/lifts/lifts-page.component.html and frontend/src/app/features/settings/lifts/lifts-page.component.scss"
```

---

## Parallel Example: User Story 2

```bash
# Launch conflict and consistency tests together:
Task: "Add duplicate-conflict unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/RenameLift/RenameLiftCommandHandlerTests.cs"
Task: "Add integration and contract coverage in backend/tests/WeightLifting.Api.IntegrationTests/Lifts/RenameLiftIntegrationTests.cs and backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs"
Task: "Add shared-store reconciliation tests in frontend/tests/unit/core/state/lifts-store.service.spec.ts"

# Launch backend and frontend consistency work together:
Task: "Add normalized duplicate-name checks in backend/src/WeightLifting.Api/Application/Lifts/Commands/RenameLift/RenameLiftCommandHandler.cs"
Task: "Update canonical shared-list reconciliation in frontend/src/app/core/state/lifts-store.service.ts and frontend/src/app/features/settings/lifts/lifts-page.facade.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE** using the independent test and `quickstart.md`
5. Complete Phase 4 only after the MVP rename flow is correct

### Incremental Delivery

1. Get the rename contract surface, API client support, and store helpers in place
2. Add the end-to-end `Settings -> Lifts` rename flow
3. Validate successful rename, blank-name blocking, unchanged-name handling, and save-failure messaging
4. Add duplicate-name conflict handling and canonical later-list consistency
5. Finish with full automated coverage and manual mobile verification

### Suggested MVP Scope

Deliver through **Phase 3** first. That gives the smallest complete slice to judge direction:
rename an existing lift from `Settings -> Lifts` with backend validation, confirmed persistence, and mobile feedback.

## Notes

- Total tasks: 25
- User Story 1 tasks: 10
- User Story 2 tasks: 7
- All tasks follow the required checklist format with IDs, labels, and file paths
- Keep one production class per file unless an explicit exception is justified
