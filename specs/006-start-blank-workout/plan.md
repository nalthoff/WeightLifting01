# Implementation Plan: Start blank workout session

**Branch**: `006-start-blank-workout` | **Date**: 2026-04-22 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/006-start-blank-workout/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/006-start-blank-workout/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Introduce the first persisted workout-session slice so a lifter can start a blank workout from the home entry in a mobile-first flow. The implementation adds a backend-owned workout lifecycle (including one-active-session enforcement and server-authoritative start time), a SQL-backed workout model plus migration, and a minimal active-session UI that confirms session start and handles conflicts/failures without ghost success.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Router + Angular Material, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via EF Core migrations  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit tests (xUnit), integration tests, and contract tests  
**Target Platform**: Mobile web browsers first, with Azure-compatible API/database hosting  
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: User can start workout in <=3 taps and ~30 seconds on phone-sized viewport; start confirmation appears immediately after API success  
**Constraints**: One in-progress workout per user, server-authoritative UTC start time, clear failure on connectivity issues, backend-owned business rules, one class per production file  
**Scale/Scope**: Initial MVP workout start only (no set logging), single-user ownership per workout, first persisted session lifecycle slice

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
specs/006-start-blank-workout/
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
├── src/
│   └── app/
│       ├── core/
│       │   ├── api/
│       │   └── state/
│       └── features/
│           ├── home/
│           └── workouts/
└── e2e/  # Playwright-driven end-to-end coverage from frontend package scripts
```

**Structure Decision**: Use the existing mobile-first web application split (`frontend/` + `backend/`) and add a new `Workouts` vertical slice mirrored across API contracts, application handlers, domain types, persistence entities/mappings, and tests. Frontend receives a focused workouts feature area for start flow + active session shell while keeping business decisions on backend.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first start flow remains the primary UX target.
- History scope stays minimal for this slice (session start + active-session continuity only).
- Backend owns lifecycle and validation rules (single in-progress rule, label normalization, authoritative UTC start).
- SQL migration-backed persistence is required for workout sessions.
- Backend unit tests are required for all new workout lifecycle business rules.
- Proposed architecture remains Azure-compatible and preserves one-class-per-file organization.
