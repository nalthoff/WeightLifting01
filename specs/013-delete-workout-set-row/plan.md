# Implementation Plan: Delete mistaken workout set rows

**Branch**: `013-delete-workout-set-row` | **Date**: 2026-04-23 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/013-delete-workout-set-row/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/013-delete-workout-set-row/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Allow lifters to remove mistakenly logged set rows directly from the active workout screen, with a mandatory confirmation step before mutation. The design keeps confirmation and loading/error feedback in the Angular UI while enforcing in-progress eligibility, row scoping, and delete behavior in backend application/domain logic, preserving history integrity and existing add/edit/reorder/remove workout flows.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database (reuse existing workout set persistence)  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Set-row delete workflow completes in-place without navigation and shows clear success/failure state per attempt  
**Constraints**: Explicit confirmation before delete, in-progress-only deletion, no cross-entry side effects, no duplicate submissions while deleting, one class per backend production file  
**Scale/Scope**: Single-user active workout flows; delete existing persisted set rows only, scoped to one workout lift entry row at a time

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

**Structure Decision**: Reuse existing workout set boundaries by adding a set-delete command flow and API contract in the backend workout slice plus confirmation/deleting-row UI state in the active-workout frontend slice.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging remains primary by keeping set-row delete controls and confirmation within the active workout screen.
- Decision-ready history improves because mistaken set rows can be removed before they mislead next-weight decisions.
- Angular remains presentation-focused while delete eligibility, row ownership, and persistence mutation remain backend business logic.
- SOLID boundaries stay explicit by separating API contracts, command handlers, domain validation, and state orchestration.
- One-class-per-file remains the default for all new backend production types introduced for delete behavior.
- SQL schema changes are not required because deleting existing set rows reuses current persistence shape.
- Unit, integration, contract, and e2e tests are planned for delete rules and user-visible confirmation/failure behaviors.
- Azure compatibility remains unchanged by continuing existing API and SQL hosting patterns.
