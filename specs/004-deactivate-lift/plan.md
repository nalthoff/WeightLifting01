# Implementation Plan: Deactivate Lift

**Branch**: `[004-deactivate-lift]` | **Date**: 2026-04-20 | **Spec**: [`specs/004-deactivate-lift/spec.md`](./spec.md)
**Input**: Feature specification from `/specs/004-deactivate-lift/spec.md`

## Summary

Add a mobile-friendly deactivate flow to the existing `Settings -> Lifts` page so a user can hide
an existing lift from default selection lists without permanently removing its record. The
implementation will reuse the current Angular lift-management page and shared lift store, keep the
existing `isActive` persistence model and `activeOnly` list-query behavior as the canonical source
of availability, add a backend deactivate command and API contract on the existing ASP.NET Core +
EF Core stack, and extend the settings page with explicit confirmation plus an active-only versus
include-inactive filter. Successful deactivations will update shared state only after confirmed
backend success, followed by list reconciliation so later lift-list reads exclude inactive lifts by
default.

## Technical Context

**Language/Version**: Angular 20 frontend, C# with .NET 10 backend  
**Primary Dependencies**: Angular, Angular Material, RxJS, ASP.NET Core Web API, Entity Framework Core, SQL Server provider  
**Storage**: Azure SQL / SQL Server-compatible relational storage using the existing `Lifts` table and `IsActive` column  
**Testing**: Angular unit tests, Playwright e2e, xUnit unit tests, ASP.NET integration tests, contract tests  
**Target Platform**: Mobile web browsers with Azure-hosted API and Azure SQL  
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Deactivate flow completes in <=30 seconds on mobile; the active/inactive filter updates the visible list within the current page flow; successful deactivation is reflected in later default lift-list reads immediately after save  
**Constraints**: Mobile-first UX, Azure-compatible deployment, resilient handling of flaky connectivity, explicit confirmation before destructive-seeming actions, backend-owned business rules, SOLID boundaries, one production class per file, no hard delete behavior, and no introduction of workout-history or logging data structures in this slice  
**Scale/Scope**: Single feature slice for deactivating existing lifts in Settings and viewing inactive lifts for an individual lifter's exercise catalog

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Mobile-first logging flow is the primary UX, with the smallest supported viewport
      treated as the design baseline.
- [x] The feature preserves or improves "just enough history" needed to help users choose
      the next working weight.
- [x] Angular owns presentation concerns only; business rules are assigned to C# backend or
      a dedicated business layer.
- [x] The design follows SOLID principles with clear responsibilities and explicit
      dependency boundaries.
- [x] Production code organization keeps one class per file unless a documented exception is
      required by the framework or language construct.
- [x] SQL persistence changes, if any, include schema ownership and migration planning.
- [x] All affected business-layer logic has a concrete unit-test approach.
- [x] All proposed infrastructure and runtime assumptions are compatible with Azure hosting
      and managed services.

Post-design review: Pass. The selected design keeps Angular focused on confirmation, filter state,
and feedback UX, keeps availability rules in backend application/domain layers, reuses the existing
SQL-backed lift table and `IsActive` state without introducing non-Azure dependencies, and adds a
concrete unit/integration/contract/UI/e2e test strategy for deactivation and filtered list
behavior.

## Project Structure

### Documentation (this feature)

```text
specs/004-deactivate-lift/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── lifts.openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── WeightLifting.Api/
│       ├── Api/
│       │   ├── Controllers/
│       │   └── Contracts/
│       │       └── Lifts/
│       ├── Application/
│       │   └── Lifts/
│       │       ├── Commands/
│       │       │   ├── CreateLift/
│       │       │   ├── DeactivateLift/
│       │       │   └── RenameLift/
│       │       └── Queries/
│       │           └── GetLifts/
│       ├── Domain/
│       │   └── Lifts/
│       └── Infrastructure/
│           └── Persistence/
│               └── Lifts/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/
│   └── app/
│       ├── core/
│       │   ├── api/
│       │   └── state/
│       └── features/
│           └── settings/
│               └── lifts/
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Reuse the existing two-application structure: a single ASP.NET Core backend
organized by API/Application/Domain/Infrastructure boundaries and a separate Angular frontend with
shared API/state services plus feature-specific lift-management UI. This feature adds a
deactivation command path under the existing lifts slice and extends the current settings-lifts
page with confirmation and filter controls rather than introducing a new page or application
boundary.

## Complexity Tracking

No constitution violations are expected, and no exception requires justification at this time.
