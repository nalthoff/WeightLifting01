# Tasks: Default home landing and Settings navigation

**Input**: Design documents from `/specs/005-default-home-nav/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks whenever the feature changes behavior. This feature changes route and navigation behavior, so focused frontend unit/e2e coverage is included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this belongs to (e.g. `US1`, `US2`)
- Include exact file paths in descriptions

## Path Conventions

- **Frontend app**: `frontend/src/app/`
- **Frontend unit tests**: `frontend/src/app/` and `frontend/tests/unit/`
- **Frontend e2e tests**: `frontend/tests/e2e/`
- **Docs for this feature**: `specs/005-default-home-nav/`
- Keep changes frontend-only; no backend/API/data model updates for this feature

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the smallest scaffolding needed for a dedicated home route and navigation updates.

- [X] T001 Create home feature folder and component files in `frontend/src/app/features/home/home-page.component.ts` and `frontend/src/app/features/home/home-page.component.html`
- [X] T002 [P] Create test file scaffolding for routing/navigation behavior in `frontend/tests/unit/app/app.routes.spec.ts` and `frontend/tests/e2e/navigation/home-navigation.spec.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish core route wiring before story-level behavior is built.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 Update top-level route definitions in `frontend/src/app/app.routes.ts` to support a dedicated home route and preserve direct `settings/lifts` access
- [X] T004 [P] Add a minimal home route component template contract in `frontend/src/app/features/home/home-page.component.html` with only allowed semantic structure (no feature content)

**Checkpoint**: Foundation ready; root can resolve to home and `settings/lifts` route remains addressable.

---

## Phase 3: User Story 1 - Land on an empty home (Priority: P1) 🎯 MVP

**Goal**: Root entry lands on a dedicated home surface that is intentionally minimal and feature-empty.

**Independent Test**: Open the root URL and verify app shell renders with a home main area containing no lift management or workout logging content.

### Tests for User Story 1

- [X] T005 [P] [US1] Add unit tests for root-to-home route behavior in `frontend/tests/unit/app/app.routes.spec.ts`
- [X] T006 [P] [US1] Extend app shell unit assertions for home-state rendering expectations in `frontend/src/app/app.spec.ts`

### Implementation for User Story 1

- [X] T007 [US1] Implement minimal standalone home component logic in `frontend/src/app/features/home/home-page.component.ts`
- [X] T008 [US1] Ensure home main content remains intentionally empty/minimal in `frontend/src/app/features/home/home-page.component.html`

**Checkpoint**: User Story 1 is complete and testable independently from root URL behavior alone.

---

## Phase 4: User Story 2 - Open Lift management from Settings (Priority: P1)

**Goal**: Navigation clearly exposes a Settings path that takes users to existing Lift management.

**Independent Test**: From home, use only primary navigation to reach `settings/lifts` without manual URL entry.

### Tests for User Story 2

- [X] T009 [P] [US2] Add unit tests for Settings navigation visibility and destination in `frontend/src/app/app.spec.ts`

### Implementation for User Story 2

- [X] T010 [US2] Update app shell navigation labels/links to expose a clear Settings path in `frontend/src/app/app.html`
- [X] T011 [P] [US2] Update active-link styling selectors for revised navigation labels/structure in `frontend/src/app/app.scss`

**Checkpoint**: User Story 2 is complete and users can reach lifts via the Settings path.

---

## Phase 5: User Story 3 - Deep link to Lift management still works (Priority: P2)

**Goal**: Existing direct URL access to Lift management remains intact after home-route changes.

**Independent Test**: Open `/settings/lifts` directly and verify the page loads correctly without forced redirect to home.

### Tests for User Story 3

- [X] T012 [P] [US3] Add e2e deep-link smoke test for `settings/lifts` in `frontend/tests/e2e/navigation/home-navigation.spec.ts`
- [X] T013 [P] [US3] Add unit-level route guard/redirect regression test for direct `settings/lifts` access in `frontend/tests/unit/app/app.routes.spec.ts`

### Implementation for User Story 3

- [X] T014 [US3] Verify and adjust route ordering or matching rules in `frontend/src/app/app.routes.ts` so deep links are not redirected away from `settings/lifts`

**Checkpoint**: User Story 3 is complete and bookmarked direct links keep working.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation alignment with minimal scope.

- [ ] T015 [P] Run frontend unit and e2e suites covering app shell and navigation from `frontend/src/app/` and `frontend/tests/e2e/navigation/`
- [ ] T016 Validate manual quickstart checks and record completion notes in `specs/005-default-home-nav/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup; blocks user stories
- **User Story 1 (Phase 3)**: Depends on Foundational
- **User Story 2 (Phase 4)**: Depends on Foundational and can proceed after US1 route baseline is in place
- **User Story 3 (Phase 5)**: Depends on Foundational; validate after US2 nav updates
- **Polish (Phase 6)**: Depends on selected user stories being complete

### User Story Dependencies

- **US1 (P1)**: MVP baseline; required first for default home behavior
- **US2 (P1)**: Depends on US1 home baseline to validate navigation from home
- **US3 (P2)**: Can be validated independently but safest after US2 final route/nav updates

### Within Each User Story

- Add tests for changed behavior first
- Implement route/nav updates
- Re-run story-specific tests before moving on

### Parallel Opportunities

- T002 can run in parallel with T001
- T005 and T006 can run in parallel
- T011 can run in parallel with T010
- T012 and T013 can run in parallel
- T015 can run in parallel with manual validation prep for T016

---

## Parallel Example: User Story 1

```bash
# Write route + shell tests in parallel:
Task: "Add root-to-home route tests in frontend/tests/unit/app/app.routes.spec.ts"
Task: "Extend shell assertions in frontend/src/app/app.spec.ts"

# Then complete home component files:
Task: "Implement home component in frontend/src/app/features/home/home-page.component.ts"
Task: "Finalize minimal home template in frontend/src/app/features/home/home-page.component.html"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2
2. Complete Phase 3 (US1) only
3. Validate root lands on empty home
4. Demo and confirm direction before adding nav/deep-link refinements

### Incremental Delivery

1. Ship root-to-home baseline (US1)
2. Add Settings-path navigation clarity (US2)
3. Lock in deep-link regression coverage (US3)
4. Finish with quickstart and smoke verification

### Suggested MVP Scope

Complete through **Phase 3 (US1)** for the smallest valuable increment.

## Notes

- Total tasks: 16
- User Story 1 tasks: 4
- User Story 2 tasks: 3
- User Story 3 tasks: 3
- Parallelizable tasks: 9 (`T002`, `T004`, `T005`, `T006`, `T009`, `T011`, `T012`, `T013`, `T015`), with execution still gated by phase dependencies
- All tasks follow required checklist format: checkbox, task ID, optional `[P]`, story label for story phases, and explicit file paths
