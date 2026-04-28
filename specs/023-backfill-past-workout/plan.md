# Implementation Plan: Backfill past workout

**Branch**: `023-backfill-past-workout` | **Date**: 2026-04-28 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/023-backfill-past-workout/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/023-backfill-past-workout/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Enable lifters to backfill already-completed sessions using required historical date, start time (hour/minute), and duration inputs, while preserving active workout context and keeping chronology trustworthy for next-weight decisions. The plan extends existing workout creation/completion and history listing flows with explicit historical timing behavior, avoids data-structure changes unless strictly required, and keeps mobile lift/set entry parity during catch-up.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material, Angular Router, RxJS, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout, workout-lift, and workout-set persistence with migration support)  
**Testing**: Jasmine/Karma frontend unit tests, Playwright e2e coverage, xUnit backend unit/integration/contract tests  
**Target Platform**: Mobile web browsers first with Azure-compatible backend/API hosting
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Keep historical backfill flow comparable to standard workout start flow despite required timing fields and preserve quick, predictable workout history lookup  
**Constraints**: Mobile-first low-tap flow with required date/time/duration entry, active workout continuity during backfill, no data-structure changes unless strictly needed, clear save feedback under ordinary connectivity variability, no analytics-scope expansion  
**Scale/Scope**: Single-user workout timeline integrity, multiple completed sessions per day, and catch-up entries alongside one active in-progress workout

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
specs/023-backfill-past-workout/
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
│       │   ├── Contracts/Workouts/
│       │   └── Controllers/
│       ├── Application/Workouts/
│       │   ├── Commands/
│       │   └── Queries/
│       ├── Domain/Workouts/
│       └── Infrastructure/Persistence/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
└── src/app/
    ├── core/api/
    ├── core/state/
    ├── features/home/
    ├── features/history/
    └── features/workouts/
```

**Structure Decision**: Extend existing workout command/query and history listing boundaries in backend, and add historical-workout entry affordances within existing frontend workout flows, avoiding any new top-level modules.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first speed remains primary by keeping historical logging in the same lightweight workout-entry interaction model used in gym flows.
- Decision-ready history is preserved by anchoring chronology to required historical date/time and keeping completed sessions reliable for next-weight decisions.
- Angular remains responsible for user interaction and feedback, while workout lifecycle and ordering rules stay in C# backend application/domain layers.
- SOLID boundaries are maintained by focused command/query updates and explicit DTO/domain responsibilities.
- One-class-per-file remains intact for all new or changed production backend classes.
- SQL persistence remains explicit and migration-ready, but design preference is to reuse existing structures unless a strict requirement forces schema adjustment.
- Backend rule changes (historical completion, active-workout coexistence, chronology selection) include concrete unit-test coverage.
- Azure compatibility is unchanged because no non-Azure runtime dependencies are introduced.
