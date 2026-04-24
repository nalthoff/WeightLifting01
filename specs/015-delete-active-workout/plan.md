# Implementation Plan: Delete in-progress workout

**Branch**: `015-delete-active-workout` | **Date**: 2026-04-23 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/015-delete-active-workout/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/015-delete-active-workout/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a destructive but confirmation-gated Delete Workout action to the active workout page so lifters can permanently discard an unwanted in-progress session. The Angular UI owns confirmation and feedback states, while the C# backend owns in-progress eligibility, deletion orchestration, and authoritative outcomes (success/not-found/conflict/failure), preserving completed history behavior and mobile-first flow speed.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database using existing workout tables  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Confirmed delete resolves in-place without navigation regressions, with immediate success/failure clarity and no false-success states  
**Constraints**: Active-workout-page scope only, explicit confirmation required, in-progress-only deletion, irreversible hard-delete, duplicate-submit protection, one class per backend production file  
**Scale/Scope**: Single active workout per user, delete one workout aggregate with associated in-progress rows, preserve completed workout history behavior

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
specs/015-delete-active-workout/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── WeightLifting.Api/
│       ├── Api/
│       │   ├── Contracts/
│       │   └── Controllers/
│       ├── Application/
│       │   └── Workouts/
│       ├── Domain/
│       │   └── Workouts/
│       └── Infrastructure/
│           └── Persistence/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/app/
│   ├── core/
│   │   ├── api/
│   │   └── state/
│   └── features/workouts/
└── tests/e2e/workouts/
```

**Structure Decision**: Reuse the existing workout slice in both frontend and backend, adding a workout-delete command path and API contract in backend plus confirmation/deleting UI state in active workout page and frontend workout state services.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging remains primary by keeping delete action, confirmation, and feedback inline on active workout.
- Decision-ready history remains focused because only discarded in-progress sessions are removed; completed history behavior is unchanged.
- Angular remains presentation-focused while eligibility and deletion rules remain backend business logic.
- SOLID boundaries remain explicit with dedicated API contract, application command flow, and frontend state orchestration responsibilities.
- One-class-per-file remains the default for any new backend production types.
- SQL schema changes are not required because feature uses hard-delete over existing workout aggregate data.
- Unit, integration, contract, and e2e tests are planned to cover business rules and confirmation/failure UX.
- Azure compatibility remains unchanged by using existing API and SQL stack.
