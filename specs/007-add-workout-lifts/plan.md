# Implementation Plan: Add lifts to in-progress workout

**Branch**: `007-add-workout-lifts` | **Date**: 2026-04-22 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/007-add-workout-lifts/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/007-add-workout-lifts/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a mobile-first Add Lift flow on the active workout screen so lifters can select active lifts from the existing lift library and attach one or more lifts to the current in-progress workout. The feature extends workout-session persistence with workout-lift association data, allows duplicates intentionally for this release, and enforces clear failure handling with backend-owned rules.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Router + Angular Material, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via EF Core migrations  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests with xUnit  
**Target Platform**: Mobile web browsers first, with Azure-compatible API/database hosting  
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Add-lift happy path completed in about 3 taps on phone-sized viewport; added lift visible immediately in workout flow after success  
**Constraints**: Active-lifts-only picker, duplicate lifts allowed, backend-owned validation, explicit failure messaging with no ghost additions, one class per production file  
**Scale/Scope**: Single-user mode for now; focused on workout-lift association only (no remove-lift and no set/rep/weight expansion)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Mobile-first logging flow is the primary UX, with the smallest supported viewport
      treated as the design baseline.
- [x] The feature preserves or improves "just enough history" needed to help users choose
      the next working weight.
- [x] Angular owns presentation concerns only; business rules are assigned to C# backend or
      a dedicated business layer.
- [x] The design follows SOLID principles with clear responsibilities and explicit
      dependency boundaries.
- [x] Production code organization keeps one class per file unless a documented exception is
      required by the framework or language construct.
- [x] SQL persistence changes, if any, include schema ownership and migration planning.
- [x] All affected business-layer logic has a concrete unit-test approach.
- [x] All proposed infrastructure and runtime assumptions are compatible with Azure hosting
      and managed services.

## Project Structure

### Documentation (this feature)

```text
specs/007-add-workout-lifts/
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
│       │   ├── Workouts/
│       │   └── Lifts/
│       └── Infrastructure/
│           └── Persistence/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/
│   └── app/
│       ├── core/
│       │   ├── api/
│       │   └── state/
│       └── features/
│           ├── home/
│           ├── settings/lifts/
│           └── workouts/
└── e2e/
```

**Structure Decision**: Reuse the existing workouts and lifts vertical slices. Extend workout backend domain/application/persistence with workout-lift association behavior, and extend the existing active-workout frontend surface with an add-lift picker and immediate list rendering instead of introducing new cross-cutting modules.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first add-lift journey stays primary and starts from active workout screen.
- History scope remains minimal and focused on immediate workout progression relevance.
- Backend owns business rules for workout-lift association and duplicate policy.
- SQL migrations remain the persistence change mechanism.
- Unit/integration/contract testing strategy is explicit for changed backend behavior.
- Azure-compatible deployment assumptions remain unchanged.
