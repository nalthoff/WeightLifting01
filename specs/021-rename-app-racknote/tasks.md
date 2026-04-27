# Tasks: Rename app to RackNote

**Input**: Design documents from `specs/021-rename-app-racknote/`
**Prerequisites**: `plan.md` (required), `spec.md` (required), `research.md`, `data-model.md`, `contracts/`

**Tests**: Include test tasks whenever the feature changes behavior. This feature changes user-facing identity text, so frontend unit and e2e expectations are updated.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm feature artifacts and implementation baseline are in place

- [x] T001 Verify feature prerequisites and artifact presence using `.specify/scripts/powershell/check-prerequisites.ps1` from repository root

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared rename scope before user story execution

- [x] T002 Document in-scope identity surfaces and acceptance references in `specs/021-rename-app-racknote/contracts/app-identity-surfaces.yaml`

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - See RackNote Across Primary App Entry Points (Priority: P1) 🎯 MVP

**Goal**: Ensure all primary in-app and browser identity surfaces display RackNote

**Independent Test**: Load the app on mobile and wider viewports and confirm app shell brand + tab title show RackNote with no old-name remnants.

### Tests for User Story 1

- [x] T003 [US1] Update app shell unit expectation in `frontend/src/app/app.spec.ts` to assert `RackNote`
- [x] T004 [US1] Update navigation e2e expectations in `frontend/tests/e2e/navigation/home-navigation.spec.ts` to assert `RackNote`
- [x] T005 [US1] Update settings-lifts e2e expectation in `frontend/tests/e2e/settings-lifts/create-lift.spec.ts` to assert `RackNote`

### Implementation for User Story 1

- [x] T006 [US1] Update visible app-shell brand label to `RackNote` in `frontend/src/app/app.html`
- [x] T007 [US1] Update browser title metadata to `RackNote` in `frontend/src/index.html`

**Checkpoint**: User Story 1 should be independently functional and testable

---

## Phase 4: User Story 2 - See RackNote in User-Facing Product Documentation (Priority: P2)

**Goal**: Align user-facing product documentation with the renamed identity

**Independent Test**: Read top-level user-facing documentation and confirm product identity references use RackNote consistently.

### Implementation for User Story 2

- [x] T008 [US2] Update user-facing product identity references to `RackNote` in `README.md` while preserving technical identifier references that are not app-brand display text

**Checkpoint**: User Stories 1 and 2 both work independently

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate end-to-end consistency and prevent regressions

- [x] T009 Run targeted frontend unit/e2e tests covering brand-name assertions
- [x] T010 Run repository-wide search validation to confirm no residual user-facing `WeightLifting01` remains in scoped surfaces (`frontend/src/app/app.html`, `frontend/src/index.html`, `frontend/src/app/app.spec.ts`, `frontend/tests/e2e/navigation/home-navigation.spec.ts`, `frontend/tests/e2e/settings-lifts/create-lift.spec.ts`, `README.md`)
- [x] T011 Validate quickstart acceptance checks in `specs/021-rename-app-racknote/quickstart.md` against completed implementation and record outcomes in work summary

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies
- **Phase 2 (Foundational)**: Depends on Phase 1 completion
- **Phase 3 (US1)**: Depends on Phase 2 completion
- **Phase 4 (US2)**: Depends on Phase 2 completion and can run after US1 MVP validation
- **Phase 5 (Polish)**: Depends on completion of US1 and US2

### User Story Dependencies

- **US1 (P1)**: Starts after foundational scope confirmation
- **US2 (P2)**: Independent from workout behavior and can proceed once rename scope is established

### Within Each User Story

- Update tests and implementation artifacts for the same story in one cohesive slice
- Validate story outcomes before moving to final polish

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup + Foundational phases
2. Complete US1 test/implementation updates
3. Validate US1 independently via targeted checks

### Incremental Delivery

1. Deliver US1 (primary app entry points)
2. Deliver US2 (documentation alignment)
3. Run polish validations and quickstart checks

---

## Notes

- Keep scope to user-facing identity text only.
- Do not modify workout behavior, persistence, or backend business rules.
