# Implementation Plan: Edit workout set entries

**Branch**: `012-edit-workout-sets` | **Date**: 2026-04-23 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/012-edit-workout-sets/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/012-edit-workout-sets/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Enable lifters to correct previously logged set rows directly in the active workout screen using inline, explicit save actions per row. The design adds a row edit workflow with clear saved-versus-unsaved visibility, keeps business validation and in-progress gating in backend workout logic, and preserves existing add/remove/reorder lift behavior while supporting last-write-wins concurrency.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout set persistence)  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Edit-save flow completes without screen transition and reflects successful row updates immediately in the visible entry list  
**Constraints**: Inline save per row, clear unsaved state on failure, edits only for in-progress workouts, last-write-wins conflicts, one class per backend production file  
**Scale/Scope**: Single-user active workout context; update existing set rows only, scoped per workout-lift entry identity

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

**Structure Decision**: Reuse existing workout feature boundaries, adding set-update API contract and backend command flow under `Application/Workouts` plus inline set-edit state and UI updates in the active-workout frontend slice.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging remains primary by keeping set editing inside each row on the active workout view.
- Decision-ready history improves because corrections make logged reps/weight reliable for next-weight decisions.
- Angular remains presentation-focused while in-progress gating, validation, and persistence conflict handling stay in backend business logic.
- SOLID boundaries are preserved by separating API contracts, command handlers, domain validation, and state orchestration responsibilities.
- One-class-per-file remains the default for all new backend production types introduced for update-set behavior.
- SQL schema changes are not required because workout sets already persist; existing schema is reused.
- Unit tests plus integration/contract/e2e coverage are planned for changed business rules and user-visible flows.
- Azure compatibility remains unchanged by using existing API and SQL hosting assumptions.
