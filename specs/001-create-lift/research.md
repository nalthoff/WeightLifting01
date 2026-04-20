# Research: Create Lift

## Decision 1: Use a dedicated `Settings -> Lifts` page with non-optimistic creation

- **Decision**: The frontend will expose a dedicated `Settings -> Lifts` page. Lift creation
  will be non-optimistic: the UI only adds the lift to the shared selectable list after the
  backend confirms a successful save.
- **Rationale**: This satisfies the requirement that lifts appear immediately after save while
  also preventing false positives during flaky mobile connectivity or save failures.
- **Alternatives considered**:
  - Optimistically insert the lift before save confirmation: rejected because it can make a
    failed create appear selectable.
  - Keep lift creation inline on the general Settings page: rejected because the spec now
    requires a dedicated lift-management page.

## Decision 2: Keep business validation in the backend application/domain layers

- **Decision**: Angular will own navigation, form state, and UX validation, while the backend
  application/domain layers will own the canonical create-lift validation and persistence
  orchestration.
- **Rationale**: The constitution requires business logic outside Angular, unit-tested, and
  organized with SOLID boundaries. The create-lift rule set is small but business-critical:
  required name, whitespace rejection, and only surfacing confirmed records.
- **Alternatives considered**:
  - Validate only in Angular: rejected because it duplicates rules and violates the
    constitution's backend-owned business logic requirement.
  - Let controllers persist directly without an application layer: rejected because it mixes
    transport, business validation, and persistence concerns.

## Decision 3: Use SQL-backed lift persistence with explicit migrations

- **Decision**: Persist lifts in a `Lifts` table managed through Entity Framework Core
  migrations against SQL Server / Azure SQL-compatible storage.
- **Rationale**: The project constitution explicitly requires SQL persistence and Azure
  compatibility. Code-first migrations keep schema evolution reviewable and reproducible.
- **Alternatives considered**:
  - Browser-local storage: rejected because the feature must survive beyond a single client and
    be immediately available to workout flows from the canonical backend source of truth.
  - Hand-authored SQL only: rejected because it weakens repeatability for a greenfield codebase.

## Decision 4: Share lift state across settings and workout selection

- **Decision**: The frontend will maintain a shared lift query/store service consumed by both
  the `Settings -> Lifts` page and workout lift selection UI. After `POST /api/lifts`
  succeeds, the returned DTO is inserted into the shared store and then reconciled with a
  background `GET /api/lifts` refresh.
- **Rationale**: This gives immediate visibility to the new lift without a manual reload and
  keeps multiple screens aligned to the same source of truth.
- **Alternatives considered**:
  - Force a full page reload after create: rejected because it breaks the mobile-first speed
    requirement.
  - Keep separate list states for settings and workouts: rejected because it increases
    synchronization bugs and rework risk.

## Decision 5: Test strategy emphasizes backend unit rules plus end-to-end confirmation

- **Decision**: Add backend unit tests for create-lift business rules, backend integration and
  contract tests for create/list endpoints, focused Angular tests for page behavior, and one
  mobile-viewport end-to-end test that proves the new lift becomes selectable in workouts.
- **Rationale**: This covers the constitution's unit-test requirement while also verifying the
  feature's user-facing acceptance path.
- **Alternatives considered**:
  - E2E-only coverage: rejected because it leaves business rules under-specified and hard to
    diagnose.
  - Unit-only backend coverage: rejected because it would not prove immediate selector updates
    across frontend/backend boundaries.

## Open choices intentionally deferred

- Lift-name uniqueness remains out of scope for this feature and should not be enforced unless
  a later specification introduces it.
- Multi-user ownership is deferred until authentication and profile boundaries are specified.
