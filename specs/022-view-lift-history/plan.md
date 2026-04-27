# Implementation Plan: View lift history inline

**Branch**: `022-view-lift-history` | **Date**: 2026-04-27 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/022-view-lift-history/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/022-view-lift-history/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a one-tap inline history panel to each lift section in the active workout entry screen so lifters can view the last three completed sessions for the exact lift without leaving current logging flow. The design reuses existing workout/lift data boundaries, adds a focused history query for exact-lift recency, and keeps error/empty states inline to preserve mobile-first workout-entry continuity.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material, Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout, workout-lift, and workout-set tables)  
**Testing**: Jasmine/Karma frontend unit tests, Playwright e2e coverage, xUnit backend unit/integration/contract tests  
**Target Platform**: Mobile web browsers first with Azure-compatible backend/API hosting
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Same-lift history opens inline without route change and keeps decision-making fast in active entry flow  
**Constraints**: Exact-lift scoping only, completed sessions only, max-three recency limit, no analytics expansion, resilient inline error states under weak connectivity  
**Scale/Scope**: Per-user workout history lookups in active workout context, multiple lift entries per workout including repeated lifts

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
specs/022-view-lift-history/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
backend/
├── src/
│   └── WeightLifting.Api/
│       ├── Api/
│       │   ├── Contracts/Workouts/
│       │   └── Controllers/
│       ├── Application/Workouts/
│       │   └── Queries/
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

**Structure Decision**: Extend existing active-workout frontend module for inline panel state and rendering, add focused backend query/contract support under current workouts boundaries, and avoid introducing new persistence modules or schema.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging speed remains primary because history is shown inline with no route change and minimal extra taps.
- Decision-ready history is improved by limiting data to exact-lift, completed-only, last-three sessions for immediate next-weight decisions.
- Angular handles panel state and rendering while backend owns exact-lift filtering, completion gating, and recency selection rules.
- SOLID boundaries remain intact by adding focused query/helper and API contract extensions rather than broad cross-layer coupling.
- One-class-per-file organization is preserved for any new backend query or contract DTO classes.
- SQL schema and migration work is not required because existing workout/workout-lift/workout-set persistence already supports this read slice.
- Unit/integration strategy is concrete through backend query tests and frontend component/state tests for inline panel behavior.
- Azure compatibility remains unchanged because no new runtime or infrastructure dependency is introduced.
