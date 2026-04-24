# Implementation Plan: Optional Workout Name

**Branch**: `018-name-workout` | **Date**: 2026-04-24 | **Spec**: `/specs/018-name-workout/spec.md`
**Input**: Feature specification from `/specs/018-name-workout/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Allow lifters to add, edit, or clear an optional workout name while a workout is in progress, without adding mandatory steps to mobile logging. The plan extends existing workout lifecycle contracts with an explicit in-progress rename path, preserves blank/whitespace normalization to unnamed behavior, and keeps history fallback rendering ("Workout") unchanged when no name exists.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend; C# on .NET 10 backend  
**Primary Dependencies**: Angular, Angular Router, Angular Material, ASP.NET Core Web API, Entity Framework Core  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workouts table and migrations)  
**Testing**: Frontend unit tests (Jasmine/Karma), frontend e2e tests (Playwright), backend unit/integration/contract tests (xUnit)  
**Target Platform**: Mobile web browsers with Azure-compatible backend hosting
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Keep active-workout naming non-blocking so start/log/complete flow remains fast on phone-sized viewports  
**Constraints**: Name-only scope (no type field), edit allowed only while workout is In Progress, preserve existing history fallback label behavior  
**Scale/Scope**: Per-user workout sessions, single optional name value per workout, no expansion into analytics/reporting workflows

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Mobile-first logging flow is the primary UX, with the smallest supported viewport
      treated as the design baseline.
- [x] The feature preserves or improves "just enough history" needed to help users choose
      the next working weight.
- [x] Angular owns presentation concerns only; business rules are assigned to the C#
      backend application/domain layer.
- [x] The design follows SOLID principles with clear responsibilities and explicit
      dependency boundaries.
- [x] Production code organization keeps one class per file unless a documented exception is
      required by the framework or language construct.
- [x] SQL persistence changes, if any, include explicit schema updates and versioned
      migration planning.
- [x] All affected backend application/domain logic has a concrete unit-test approach.
- [x] All proposed infrastructure and runtime assumptions are compatible with Azure hosting
      and managed services.

## Project Structure

### Documentation (this feature)

```text
specs/018-name-workout/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
```text
backend/
├── src/
│   └── WeightLifting.Api/
│       ├── Api/Contracts/Workouts/
│       ├── Api/Controllers/
│       ├── Application/Workouts/Commands/
│       ├── Domain/Workouts/
│       └── Infrastructure/Persistence/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/app/
│   ├── core/api/
│   ├── core/state/
│   └── features/workouts/
└── tests/
    ├── unit/workouts/
    └── e2e/workouts/
```

**Structure Decision**: Extend existing workout contracts/commands plus active-workout frontend paths; keep all naming business rules in backend workout command handling and reuse current history rendering paths for fallback label behavior.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
