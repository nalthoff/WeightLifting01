# Implementation Plan: Home active workout summary and quick completion

**Branch**: `008-home-active-workout` | **Date**: 2026-04-22 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/008-home-active-workout/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/008-home-active-workout/spec.md`

## Summary

Add a home-screen active-workout summary card that shows the current in-progress workout and allows users to either continue or complete it without opening workout detail first. The design keeps completion rules backend-owned, updates home state immediately after completion outcomes, and preserves existing workout-detail behavior while reducing taps in the primary mobile logging flow.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Router + Angular Material, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database via existing workout persistence  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit, integration, and contract tests (xUnit)  
**Target Platform**: Mobile web browsers first, with Azure-compatible API/database hosting  
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Users complete or continue active workout from home in one tap; completion feedback and home-card state update are immediate after API response  
**Constraints**: No confirmation step for completion in this release; no workout-detail redesign; explicit failure feedback with no ghost completion; backend-owned lifecycle rules; one class per production file  
**Scale/Scope**: Single-user mode assumptions; focused on home card visibility, continue action, and home-triggered completion only

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
specs/008-home-active-workout/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── home-workout-actions-api.yaml
└── tasks.md
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
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Reuse existing home/workouts slices and extend current workout lifecycle APIs with an explicit completion operation consumable from home. Keep the home page responsible for rendering summary/actions, while backend application/domain layers own completion validity and lifecycle transitions.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Home workflow stays mobile-first and reduces in-gym taps for current-session handling.
- Changes preserve focus on immediate workout usefulness rather than broad analytics/reporting expansion.
- Completion and lifecycle validation remain backend-owned, with Angular handling presentation and feedback.
- Existing SQL-backed workout persistence is reused; no non-Azure infrastructure assumptions introduced.
- Unit/integration/contract coverage is planned for any changed backend lifecycle behavior.
