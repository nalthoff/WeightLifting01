# Implementation Plan: Add per-lift set logging

**Branch**: `011-add-per-lift-sets` | **Date**: 2026-04-23 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/011-add-per-lift-sets/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/011-add-per-lift-sets/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Enable lifters to add per-entry set rows directly inside the active workout flow so completed work can be logged in minimal taps with trustworthy persistence outcomes. The design adds a workout-set entity scoped to one workout-lift entry, keeps numbering independent for duplicate lift entries, persists successful rows immediately, and guarantees explicit no-ghost feedback when add-set saves fail.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via EF Core migrations  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Primary add-set path completes in 3 interactions or fewer and successful saves are reflected immediately in the focused workout-lift section  
**Constraints**: Per-entry set-list isolation for duplicates, no optimistic ghost rows on failed saves, backend-owned validation/numbering rules, one class per production file  
**Scale/Scope**: Single-user active workout context; set logging scoped to one in-progress workout and one targeted workout-lift entry per add-set action

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
specs/[###-feature]/
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
│       │   ├── Contracts/
│       │   └── Controllers/
│       ├── Application/
│       │   └── Workouts/
│       ├── Domain/
│       │   └── Workouts/
│       └── Infrastructure/
│           ├── Persistence/
│           └── Migrations/
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

**Structure Decision**: Reuse the existing in-progress workout slice and add workout-set creation contract, backend command/validation/persistence flow, and per-lift-entry set rendering/state updates within existing frontend workout feature boundaries.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging remains primary by keeping Add Set inside each lift entry on the active workout view.
- Decision-ready history improves through persisted per-set reps/weight records that survive refresh and support next-session weight decisions.
- Angular remains presentation-focused while set numbering/validation and persistence outcomes stay in backend application/domain services.
- SOLID boundaries are preserved by separating API contracts, command handlers, domain validation, and persistence mapping responsibilities.
- One-class-per-file remains the default for all newly introduced backend production types.
- SQL persistence changes are explicitly planned via schema updates plus versioned migration artifacts for workout-set storage.
- Unit tests, integration tests, contract tests, and mobile e2e coverage are planned for all changed behaviors and failure cases.
- Azure compatibility remains unchanged by using existing API/database infrastructure assumptions.
