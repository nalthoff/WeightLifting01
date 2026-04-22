# Implementation Plan: Remove lift from in-progress workout

**Branch**: `009-remove-workout-lift` | **Date**: 2026-04-22 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/009-remove-workout-lift/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/009-remove-workout-lift/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Allow lifters to remove mistaken lift entries from the active in-progress workout list with entry-level precision, including duplicate-instance targeting, while preserving fast mobile flow and explicit failure outcomes. The feature extends existing workout-lift APIs and active-workout state handling so removals are persisted, scoped to the current workout only, and never shown as successful when backend removal fails.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Router + Angular Material, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via EF Core migrations  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API/database hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Remove-lift happy path completes in 2 taps or fewer on phone viewport with immediate in-list update after success  
**Constraints**: Entry-level removal only, duplicate instances preserved unless explicitly selected, no blocking confirmation modal in this slice, backend-owned business rules, one class per production file  
**Scale/Scope**: Single-user mode for now; focused on removing workout-lift entries from in-progress workouts only

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
specs/009-remove-workout-lift/
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

**Structure Decision**: Reuse existing workout-lift vertical slices across backend and frontend. Add remove-entry command/endpoint behavior within existing workouts boundaries and extend active-workout UI/state orchestration rather than introducing new modules.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first flow remains the baseline by keeping removal on the active workout screen.
- History remains decision-ready by removing mistaken entries without altering historical data outside the current session.
- Business rules stay in backend application/domain handlers; Angular handles orchestration and feedback.
- SOLID boundaries remain intact by adding focused command/query/controller changes per responsibility.
- One-class-per-file remains the default for all new backend production classes.
- SQL schema changes are expected to be unnecessary if existing workout-lift persistence already supports delete by entry id; if schema changes emerge they require migration artifacts.
- Unit tests will cover backend removal business rules, with integration/contract/e2e coverage for boundary behavior.
- Azure compatibility assumptions remain unchanged.
