# Implementation Plan: Create Lift

**Branch**: `[001-create-lift]` | **Date**: 2026-04-20 | **Spec**: [`specs/001-create-lift/spec.md`](./spec.md)
**Input**: Feature specification from `/specs/001-create-lift/spec.md`

## Summary

Add a dedicated mobile-first `Settings -> Lifts` management page where users can create a
lift with a required manually entered name. The implementation will use an Angular settings
page and shared lift store on the frontend, plus an ASP.NET Core create/query API backed by
SQL persistence so newly created lifts become selectable in workout flows immediately after a
confirmed save.

## Technical Context

**Language/Version**: Angular 20 frontend, C# with .NET 10 backend  
**Primary Dependencies**: Angular, ASP.NET Core Web API, Entity Framework Core, SQL Server provider  
**Storage**: Azure SQL / SQL Server-compatible relational storage  
**Testing**: Angular unit tests, Playwright e2e, xUnit unit tests, ASP.NET integration tests, contract tests  
**Target Platform**: Mobile web browsers with Azure-hosted API and Azure SQL  
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Lift creation flow completes in <=30 seconds on mobile; new lift visible in selection immediately after successful save  
**Constraints**: Mobile-first UX, Azure-compatible deployment, resilient handling of flaky connectivity, SOLID boundaries, one production class per file, backend-owned business validation  
**Scale/Scope**: Single feature slice for lift management in Settings, supporting individual lifter setup and immediate workout selection reuse

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

Post-design review: Pass. The selected design keeps Angular focused on navigation/form state,
keeps validation and persistence orchestration in the backend application layer, uses SQL
storage plus migrations, and preserves immediate-but-confirmed list updates via a shared
frontend lift store refreshed from successful API responses.

## Project Structure

### Documentation (this feature)

```text
specs/001-create-lift/
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
│       ├── Application/
│       │   └── Lifts/
│       │       ├── Commands/
│       │       │   └── CreateLift/
│       │       └── Queries/
│       │           └── GetLifts/
│       ├── Domain/
│       │   └── Lifts/
│       └── Infrastructure/
│           └── Persistence/
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
│       ├── features/
│       │   ├── settings/
│       │   │   └── lifts/
│       │   └── workouts/
│       │       └── shared/
│       └── shell/
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Use a two-application structure with a single backend project organized
into API/Application/Domain/Infrastructure folders and a separate Angular frontend. This keeps
the feature simple enough for a greenfield repo while still honoring SOLID boundaries and
one-class-per-file discoverability.

## Complexity Tracking

No constitution violations are expected, and no exception requires justification at this time.
