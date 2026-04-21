# Research: Deactivate Lift

## Decision 1: Reuse the existing `Settings -> Lifts` page for deactivation and filtering

- **Decision**: The frontend will extend the current `Settings -> Lifts` management page with an
  explicit deactivate confirmation flow and a local filter state for active-only versus
  include-inactive views rather than introducing a separate route or standalone management screen.
- **Rationale**: The current product already centers lift management in `Settings -> Lifts`, and
  keeping the action in-page preserves a fast mobile workflow with fewer taps and less navigation.
- **Alternatives considered**:
  - Add a separate lift-details page: rejected because it adds routing and tap overhead to a small
    management task.
  - Add a separate inactive-lifts screen: rejected because the feature only needs a visibility
    filter, not a new management area.

## Decision 2: Use the existing `isActive` state as the canonical soft-delete mechanism

- **Decision**: Deactivation will be implemented by setting the existing lift record's `isActive`
  state to `false`, with no hard-delete path and no workout-history checks in this feature.
- **Rationale**: The feature explicitly avoids permanent deletion and avoids introducing
  workout-history structures. The current data model already contains an availability flag, so
  reusing it satisfies the user need without expanding scope.
- **Alternatives considered**:
  - Add a separate deleted/deactivated timestamp model: rejected because it adds schema complexity
    without improving the current user outcome.
  - Conditionally hard-delete lifts with no usage history: rejected because the user chose a single
    consistent soft-delete rule and wants to avoid history detection work.

## Decision 3: Keep deactivate rules and active-list semantics in backend application/domain layers

- **Decision**: Angular will own confirmation, filter state, and feedback messaging, while the
  backend application/domain layers will own the canonical deactivate behavior and the default
  active-only list semantics.
- **Rationale**: The constitution requires business rules to live outside Angular and be covered by
  unit tests. Whether a lift is active and which lifts appear in default selection reads are shared
  business rules that must remain consistent across consumers.
- **Alternatives considered**:
  - Filter inactive lifts only in Angular: rejected because it would allow other consumers to drift
    from the same availability rules.
  - Put deactivate logic directly in the controller: rejected because it would mix transport and
    business behavior.

## Decision 4: Add a dedicated deactivate endpoint instead of a generic mutable status API

- **Decision**: The backend will expose a dedicated endpoint for deactivating an existing lift, and
  the response will return the updated lift so the client can reconcile state after confirmed
  success.
- **Rationale**: A dedicated endpoint keeps the intent explicit, avoids prematurely designing a
  broader status-management API, and matches the current feature scope centered on deactivation.
- **Alternatives considered**:
  - Add a generic update endpoint for all lift fields: rejected because it expands the surface area
    beyond this slice.
  - Reuse the rename endpoint with a mixed-purpose payload: rejected because it blurs unrelated
    behaviors and weakens clarity.

## Decision 5: Update shared state only after confirmed success, then reconcile with a filtered list refresh

- **Decision**: The frontend will not optimistically hide a lift before confirmation. After a
  successful deactivate response, the shared lift store will be updated from the confirmed backend
  result and then reconciled with a refresh of the currently selected filter view.
- **Rationale**: The spec requires failed deactivation attempts to leave the lift visibly active.
  Confirmed-only state updates preserve a trustworthy source of truth and match the existing lift
  management pattern.
- **Alternatives considered**:
  - Optimistically hide the lift before the backend responds: rejected because failures would make
    the UI appear to have saved a deactivate that never persisted.
  - Force a full page reload after deactivation: rejected because it slows the mobile management
    flow and is unnecessary with the existing shared store.

## Decision 6: Test deactivation across business rules, API contract, filter behavior, and mobile UX

- **Decision**: Add backend unit tests for deactivate business rules, backend integration/contract
  tests for the new endpoint plus list filtering behavior, focused Angular tests for confirmation,
  filter state, and store reconciliation, and a mobile-viewport e2e test for successful and failed
  deactivation flows.
- **Rationale**: Deactivation crosses validation, persistence, API, shared state, and user feedback
  boundaries. Layered coverage is the lowest-risk way to prove that inactive lifts disappear from
  default reads while remaining inspectable in management.
- **Alternatives considered**:
  - E2E-only coverage: rejected because rule regressions would be harder to isolate.
  - Backend-only coverage: rejected because it would not prove confirmation UX or filtered list
    behavior on mobile.
