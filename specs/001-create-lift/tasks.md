# Tasks: Create Lift

**Input**: Design documents from `/specs/001-create-lift/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include backend unit tests for business rules, backend integration/contract tests for
create/list endpoints, frontend unit tests for page behavior, and one e2e test for the mobile
flow from `Settings -> Lifts` to workout lift selection.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g. `US1`)
- Include exact file paths in descriptions

## Path Conventions

- **Backend app**: `backend/src/WeightLifting.Api/`
- **Backend tests**: `backend/tests/WeightLifting.Api.UnitTests/`,
  `backend/tests/WeightLifting.Api.IntegrationTests/`,
  `backend/tests/WeightLifting.Api.ContractTests/`
- **Frontend app**: `frontend/src/app/`
- **Frontend tests**: `frontend/tests/unit/`, `frontend/tests/e2e/`
- Keep one production class per file unless a documented exception is required
- Prefer CLI/tool-generated output for project scaffolding, Angular workspace artifacts, and
  EF Core migrations instead of hand-authoring those files

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the minimum project scaffolding needed to start building and validating the feature.

- [ ] T001 Create the backend and frontend directory skeleton with command-line setup in `backend/` and `frontend/`
- [ ] T002 Initialize the .NET 10 backend solution and projects with `dotnet new` and `dotnet sln` in `backend/src/WeightLifting.Api/WeightLifting.Api.csproj`, `backend/tests/WeightLifting.Api.UnitTests/WeightLifting.Api.UnitTests.csproj`, `backend/tests/WeightLifting.Api.IntegrationTests/WeightLifting.Api.IntegrationTests.csproj`, and `backend/tests/WeightLifting.Api.ContractTests/WeightLifting.Api.ContractTests.csproj`
- [ ] T003 Initialize the Angular frontend workspace and app shell with Angular CLI in `frontend/angular.json`, `frontend/package.json`, and `frontend/src/app/app.routes.ts`
- [ ] T004 [P] Configure baseline formatting and test runner settings in `backend/Directory.Build.props`, `frontend/eslint.config.js`, and `frontend/playwright.config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared backend, persistence, API, and frontend state foundations before building the user-facing lift flow.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Create the SQL persistence baseline and EF Core database context in `backend/src/WeightLifting.Api/Infrastructure/Persistence/WeightLiftingDbContext.cs`
- [ ] T006 [P] Create the initial lift persistence model in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Lifts/LiftEntity.cs`
- [ ] T007 Create the initial SQL migration with `dotnet ef migrations add` in `backend/src/WeightLifting.Api/Infrastructure/Persistence/Migrations/`
- [ ] T008 [P] Add shared API error handling and problem details setup in `backend/src/WeightLifting.Api/Api/ProblemDetails/ProblemDetailsConfiguration.cs`
- [ ] T009 [P] Add backend dependency registration for API, application, and persistence layers in `backend/src/WeightLifting.Api/Api/DependencyInjection/ServiceCollectionExtensions.cs`
- [ ] T010 [P] Create the shared frontend lift API client in `frontend/src/app/core/api/lifts-api.service.ts`
- [ ] T011 Create the shared frontend lift store used by settings and workouts in `frontend/src/app/core/state/lifts-store.service.ts`
- [ ] T012 [P] Generate the `Settings -> Lifts` route shell with Angular CLI and wire it in `frontend/src/app/features/settings/lifts/lifts.routes.ts`

**Checkpoint**: Foundation ready. You should be able to validate app wiring, routing entry points, and persistence scaffolding before feature behavior is added.

---

## Phase 3: User Story 1 - Create Lift From Settings Lifts Page (Priority: P1) 🎯 MVP

**Goal**: Let a user open `Settings -> Lifts`, create a lift with a required name, and see it become selectable in workouts immediately after a confirmed save.

**Independent Test**: Open `Settings -> Lifts`, submit a valid lift name, confirm success on the page, then open workout lift selection and verify the new lift is immediately available without a manual reload.

### Tests for User Story 1

> **NOTE: Write required tests before implementation. Business-layer unit tests are mandatory and should fail before the corresponding implementation is completed.**

- [ ] T013 [P] [US1] Add backend unit tests for trimmed required-name validation in `backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/CreateLift/CreateLiftCommandHandlerTests.cs`
- [ ] T014 [P] [US1] Add backend integration tests for create-and-list behavior in `backend/tests/WeightLifting.Api.IntegrationTests/Lifts/CreateLiftIntegrationTests.cs`
- [ ] T015 [P] [US1] Add contract tests for `POST /api/lifts` and `GET /api/lifts` in `backend/tests/WeightLifting.Api.ContractTests/Lifts/LiftsApiContractTests.cs`
- [ ] T016 [P] [US1] Add Angular unit tests for the `Settings -> Lifts` page behavior in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`
- [ ] T017 [P] [US1] Add a mobile e2e test for create-and-select flow in `frontend/tests/e2e/settings-lifts/create-lift.spec.ts`

### Implementation for User Story 1

- [ ] T018 [P] [US1] Create the lift domain model in `backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs`
- [ ] T019 [P] [US1] Create the create-lift request contract in `backend/src/WeightLifting.Api/Api/Contracts/Lifts/CreateLiftRequest.cs`
- [ ] T020 [P] [US1] Create the lift response contracts in `backend/src/WeightLifting.Api/Api/Contracts/Lifts/CreateLiftResponse.cs` and `backend/src/WeightLifting.Api/Api/Contracts/Lifts/LiftListResponse.cs`
- [ ] T021 [US1] Implement create-lift business logic in `backend/src/WeightLifting.Api/Application/Lifts/Commands/CreateLift/CreateLiftCommandHandler.cs`
- [ ] T022 [US1] Implement the lift query service for selectable lifts in `backend/src/WeightLifting.Api/Application/Lifts/Queries/GetLifts/GetLiftsQueryHandler.cs`
- [ ] T023 [US1] Implement the lifts API controller in `backend/src/WeightLifting.Api/Api/Controllers/LiftsController.cs`
- [ ] T024 [P] [US1] Generate the `Settings -> Lifts` page component with Angular CLI and implement it in `frontend/src/app/features/settings/lifts/lifts-page.component.ts`
- [ ] T025 [P] [US1] Implement the `Settings -> Lifts` page template and mobile-first styling in `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`
- [ ] T026 [US1] Implement the settings-lifts page facade and submit flow in `frontend/src/app/features/settings/lifts/lifts-page.facade.ts`
- [ ] T027 [US1] Integrate successful create responses into the shared lift store and workout selector reads in `frontend/src/app/core/state/lifts-store.service.ts` and `frontend/src/app/features/workouts/shared/workout-lift-selector.service.ts`
- [ ] T028 [US1] Add failure messaging, retry-safe save-state handling, and no-false-success behavior in `frontend/src/app/features/settings/lifts/lifts-page.component.ts`
- [ ] T029 [US1] Create a manual testing guide for User Story 1 in `specs/001-create-lift/manual-tests/us1-create-lift.md`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup after the MVP feature works end-to-end.

- [ ] T030 [P] Update feature documentation and local run notes in `specs/001-create-lift/quickstart.md` and `README.md`
- [ ] T031 Run the full backend, frontend, contract, and e2e test suite from `backend/tests/` and `frontend/tests/`
- [ ] T032 Validate the quickstart flow manually on a mobile viewport and capture any follow-up fixes in `specs/001-create-lift/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies, start immediately
- **Foundational (Phase 2)**: Depends on Setup completion and blocks story work
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **Polish (Phase 4)**: Depends on User Story 1 completion

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational and is the MVP for this feature

### Within User Story 1

- Tests must be written before implementation and should fail first
- Backend contracts/models before handlers and controllers
- Backend create/list endpoints before frontend integration
- Frontend page before shared-store integration and final failure-handling polish

### Parallel Opportunities

- T004 can run in parallel with other setup tasks after project creation starts
- T006, T008, T009, T010, and T012 can run in parallel after T005 starts the shared foundation
- T013 through T017 can run in parallel because they target different test layers
- T018 through T020 and T024 through T025 can run in parallel because they target separate files

---

## Parallel Example: User Story 1

```bash
# Launch test-writing tasks together:
Task: "Add backend unit tests in backend/tests/WeightLifting.Api.UnitTests/Application/Lifts/CreateLift/CreateLiftCommandHandlerTests.cs"
Task: "Add backend integration tests in backend/tests/WeightLifting.Api.IntegrationTests/Lifts/CreateLiftIntegrationTests.cs"
Task: "Add Angular unit tests in frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts"

# Launch independent scaffolding tasks together:
Task: "Create lift domain model in backend/src/WeightLifting.Api/Domain/Lifts/Lift.cs"
Task: "Create API contracts in backend/src/WeightLifting.Api/Api/Contracts/Lifts/"
Task: "Implement Settings -> Lifts page files in frontend/src/app/features/settings/lifts/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE** using the independent test and `quickstart.md`
5. Complete Phase 4 only after the MVP flow is correct

### Incremental Delivery

1. Get the project skeleton compiling
2. Generate standard workspace, project, route, and migration artifacts with CLI tooling
3. Get persistence, API wiring, routing, and shared lift state in place
4. Add backend rules and endpoints for lift creation/listing
5. Add the `Settings -> Lifts` UI and connect it to the backend
6. Validate immediate lift availability in workout selection
7. Finish with full test runs and manual mobile verification

### Suggested MVP Scope

Deliver through **Phase 3** first. That gives you the smallest complete slice to judge direction:
dedicated settings navigation, create-lift validation, persistence, and immediate workout list visibility.

## Notes

- Total tasks: 32
- Use CLI tooling for scaffolding and migrations whenever practical instead of hand-authoring generated artifacts
- User Story 1 tasks: 17
- All tasks follow the required checklist format with IDs, labels, and file paths
- The easiest validation points are after Phase 2 foundation setup, after test writing in Phase 3, and after the full User Story 1 checkpoint
