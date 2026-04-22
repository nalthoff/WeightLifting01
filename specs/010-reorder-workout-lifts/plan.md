# Implementation Plan: Reorder workout lifts

**Branch**: `010-reorder-workout-lifts` | **Date**: 2026-04-22 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/010-reorder-workout-lifts/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/010-reorder-workout-lifts/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Enable lifters to reorder lift entries in the active in-progress workout so the list mirrors real execution sequence, with immediate persistence, immediate UI reflection, and explicit no-ghost failure handling. The feature extends existing workout-lift ordering behavior using entry-instance identity (including duplicates), keeps reordering scoped to the current workout only, and preserves historical workout data unchanged.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via EF Core  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Reorder happy path completes in 3 interactions or fewer on phone viewport with immediate list update after successful save  
**Constraints**: Entry-instance precision for duplicates, no impact to completed/history workouts, backend-owned reorder business rules, clear save failure feedback, one class per production file  
**Scale/Scope**: Single-user context; reorder operations limited to one active in-progress workout and its current lift entries

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
specs/010-reorder-workout-lifts/
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

**Structure Decision**: Reuse existing workout-lift slices and active-workout orchestration. Add reorder-specific API contract, command handling, persistence ordering updates, and frontend list interaction/state updates inside current feature boundaries.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging remains the baseline by keeping reorder action in the active workout flow.
- Decision-ready history remains intact by changing only current workout sequence and leaving historical workouts unchanged.
- Business rules remain in backend application/domain handlers, while Angular handles rendering and interaction state.
- SOLID boundaries remain intact through focused command/controller/state responsibilities.
- One-class-per-file remains the default for any new production classes.
- SQL migration impact is likely unnecessary if existing ordered entry persistence supports reorder updates; if schema changes are required they must ship with migration artifacts.
- Unit tests will cover reorder business rules with integration/contract/e2e coverage for persistence and user-facing behavior.
- Azure compatibility assumptions remain unchanged.
