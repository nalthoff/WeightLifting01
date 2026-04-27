# Implementation Plan: Prefill next set defaults

**Branch**: `020-prefill-next-set` | **Date**: 2026-04-27 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/020-prefill-next-set/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/020-prefill-next-set/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

After a lifter successfully records a set for a specific workout-lift entry, the next set form for that same entry should default to the just-logged reps and weight to reduce repetitive typing in gym conditions. The implementation keeps this behavior in the existing active-workout flow, preserves per-entry isolation, and ensures failed save attempts never overwrite user drafts with misleading values.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout and set persistence)  
**Testing**: Jasmine/Karma frontend unit tests, Playwright e2e tests, xUnit backend tests (existing suites)  
**Target Platform**: Mobile web browsers first with Azure-compatible backend and SQL hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Repeated-set logging should require fewer field edits and no additional navigation, with immediate draft readiness after successful add  
**Constraints**: Prefill must be scoped per workout-lift entry, preserve blank weight behavior, and never modify drafts after failed saves  
**Scale/Scope**: Single active workout session context, multiple lift entries (including duplicates), no new persisted entities

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
└── tests/
    ├── unit/workouts/
    └── e2e/workouts/
```

**Structure Decision**: Reuse the existing active-workout frontend slice for draft/prefill behavior and existing workout-set backend API/application boundaries for success/failure response guarantees, without introducing new persistence boundaries.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging speed improves because repeated-set entry requires fewer edits on phone-sized viewports.
- Decision-ready history is preserved because the feature modifies only active set-entry defaults, not historical aggregation scope.
- Angular remains responsible for presentation and transient draft state while backend continues authoritative set-save validation and conflict outcomes.
- SOLID boundaries remain intact by extending existing API contracts/state handling without introducing cross-cutting responsibilities.
- One-class-per-file remains unchanged; no new backend production class structure exceptions are required.
- SQL schema and migration changes are not required because persisted set entities already exist and no model changes are introduced.
- Unit/integration coverage remains concrete through frontend unit tests and existing backend save-failure semantics tests.
- Azure compatibility remains unchanged because no new infrastructure/runtime dependencies are introduced.
