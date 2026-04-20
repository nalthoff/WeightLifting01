# Tasks: Material Dark Theme

**Input**: Design documents from `/specs/002-material-dark-theme/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Preserve the current frontend safety net with Angular unit tests, Playwright coverage,
and manual mobile verification. No new backend tests are required because this feature is
presentation-only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this belongs to (e.g. `US1`, `US2`)
- Include exact file paths in descriptions

## Path Conventions

- **Frontend app**: `frontend/src/`
- **Frontend unit tests**: `frontend/tests/unit/`
- **Frontend e2e tests**: `frontend/tests/e2e/`
- **Feature docs**: `specs/002-material-dark-theme/`
- Keep production code organized so shared theming concerns stay centralized and reusable

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add the Material dependency and baseline files needed before styling work starts.

- [ ] T001 Add Angular Material to the frontend workspace with Angular CLI in `frontend/package.json` and `frontend/angular.json`
- [ ] T002 [P] Create the shared Material theme file structure in `frontend/src/styles.scss` and `frontend/src/styles/theme/_material-theme.scss`
- [ ] T003 [P] Add or update frontend font/icon and global style prerequisites in `frontend/src/index.html`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the reusable dark-theme foundation that all current surfaces will consume.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Create the centralized dark theme profile with Iowa State-inspired brand tokens in `frontend/src/styles/theme/_material-theme.scss`
- [ ] T005 [P] Create shared surface, spacing, and feedback-state helpers in `frontend/src/styles/theme/_surface-patterns.scss`
- [ ] T006 Wire the global dark theme and future light-theme-ready selector structure in `frontend/src/styles.scss`
- [ ] T007 [P] Update the app-level shell structure to support Material layout components in `frontend/src/app/app.ts` and `frontend/src/app/app.html`

**Checkpoint**: Shared Material theming foundation is ready for feature surfaces.

---

## Phase 3: User Story 1 - Use a Polished Dark Interface (Priority: P1) 🎯 MVP

**Goal**: Restyle the current app shell and `Settings -> Lifts` experience with a cohesive,
readable Material-based dark UI while preserving the existing create-lift flow.

**Independent Test**: Open the app in a mobile-sized viewport, navigate to `Settings -> Lifts`,
submit invalid and valid values, and verify the app shell plus lifts page share a consistent dark
Material presentation without breaking the create-lift workflow.

### Tests for User Story 1

- [ ] T008 [P] [US1] Update the app shell unit test expectations for the Material-styled navigation in `frontend/src/app/app.spec.ts`
- [ ] T009 [P] [US1] Update the lifts page unit tests for Material-based structure and feedback-state rendering in `frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts`
- [ ] T010 [P] [US1] Update the end-to-end coverage for the themed create-lift flow in `frontend/tests/e2e/settings-lifts/create-lift.spec.ts`

### Implementation for User Story 1

- [ ] T011 [P] [US1] Replace the app shell markup with Material navigation structure in `frontend/src/app/app.html`
- [ ] T012 [P] [US1] Replace the `Settings -> Lifts` page markup with Material form, button, card, and list structure in `frontend/src/app/features/settings/lifts/lifts-page.component.html`
- [ ] T013 [US1] Add the Angular Material imports needed by the app shell and lifts page in `frontend/src/app/app.ts` and `frontend/src/app/features/settings/lifts/lifts-page.component.ts`
- [ ] T014 [US1] Apply mobile-first shell styling that consumes the shared theme in `frontend/src/app/app.scss`
- [ ] T015 [US1] Apply mobile-first page styling for headings, form layout, list layout, and feedback states in `frontend/src/app/features/settings/lifts/lifts-page.component.scss`
- [ ] T016 [US1] Refine feedback-state presentation so loading, validation, success, and failure remain clear in the dark theme within `frontend/src/app/features/settings/lifts/lifts-page.component.html` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`

**Checkpoint**: The current visible app surfaces use a consistent dark Material theme and the create-lift flow remains independently testable.

---

## Phase 4: User Story 2 - Extend Styling Without Rework (Priority: P2)

**Goal**: Leave behind reusable theming and layout foundations so future screens can adopt the same
design direction without page-specific redesign work.

**Independent Test**: Review the current shell and lifts page implementation and confirm both
consume centralized theme tokens, shared surface patterns, and future-ready theme structure instead
of relying on one-off page styling.

### Implementation for User Story 2

- [ ] T017 [P] [US2] Extract Iowa State-inspired brand color, typography, and density decisions into reusable theme tokens in `frontend/src/styles/theme/_material-theme.scss`
- [ ] T018 [P] [US2] Extract shared container, card, control, and message patterns for reuse in `frontend/src/styles/theme/_surface-patterns.scss`
- [ ] T019 [US2] Refactor shell and lifts page styles to consume shared theme helpers instead of duplicating values in `frontend/src/app/app.scss` and `frontend/src/app/features/settings/lifts/lifts-page.component.scss`
- [ ] T020 [US2] Add future light-theme-ready theme selectors and documentation comments in `frontend/src/styles.scss` and `frontend/src/styles/theme/_material-theme.scss`
- [ ] T021 [US2] Update the feature quickstart to reflect the final shared theming validation steps in `specs/002-material-dark-theme/quickstart.md`

**Checkpoint**: Shared theme foundations are reusable for future screens and the current UI no longer depends on one-off styling decisions.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup across both stories.

- [ ] T022 [P] Run the frontend production build and Angular unit tests from `frontend/`
- [ ] T023 [P] Run Playwright coverage for the themed `Settings -> Lifts` flow from `frontend/tests/e2e/`
- [ ] T024 Validate the quickstart flow manually on a mobile viewport using `specs/002-material-dark-theme/quickstart.md`
- [ ] T025 [P] Update the root project documentation for the new Material dark theme setup in `README.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies, can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on User Story 1 completion because it refactors and hardens the same surfaces into reusable foundations
- **Polish (Phase 5)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Delivers the MVP and can be demonstrated on its own after Phase 2
- **User Story 2 (P2)**: Builds on the completed dark-theme slice by consolidating it into reusable theming foundations

### Within Each User Story

- Tests update before or alongside corresponding UI changes
- Markup updates before final SCSS polish
- Shared theme wiring before page-level consumption
- Story verification before moving to the next priority

### Parallel Opportunities

- T002 and T003 can run in parallel after Angular Material is added
- T005 and T007 can run in parallel once the shared theme profile task begins
- T008 through T010 can run in parallel because they target different test layers
- T011 and T012 can run in parallel because they affect different templates
- T017 and T018 can run in parallel because they target different shared theme files
- T022 and T023 can run in parallel in the final validation phase

---

## Parallel Example: User Story 1

```bash
# Launch the story safety-net updates together:
Task: "Update app shell unit test expectations in frontend/src/app/app.spec.ts"
Task: "Update lifts page unit tests in frontend/tests/unit/settings/lifts/lifts-page.component.spec.ts"
Task: "Update themed create-lift e2e coverage in frontend/tests/e2e/settings-lifts/create-lift.spec.ts"

# Launch independent UI structure work together:
Task: "Replace app shell markup in frontend/src/app/app.html"
Task: "Replace Settings -> Lifts markup in frontend/src/app/features/settings/lifts/lifts-page.component.html"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE** using the mobile viewport quickstart flow
5. Proceed to shared-theme hardening only after the visible slice feels right

### Incremental Delivery

1. Add Angular Material and shared dark-theme wiring
2. Restyle the current visible app surfaces
3. Validate that create-lift still works cleanly on mobile
4. Consolidate the styling into reusable theme foundations
5. Finish with build, test, e2e, and manual verification

### Suggested MVP Scope

Deliver through **Phase 3** first. That provides the smallest complete slice to evaluate design
direction: the current app shell and `Settings -> Lifts` page in a dark Material-based UI.

## Notes

- Total tasks: 25
- User Story 1 tasks: 9
- User Story 2 tasks: 5
- The task set is intentionally frontend-focused because this feature does not change backend behavior
- All tasks follow the required checklist format with IDs, labels, and file paths
