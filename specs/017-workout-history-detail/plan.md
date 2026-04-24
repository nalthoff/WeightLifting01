# Implementation Plan: Workout history detail flow

**Branch**: `017-workout-history-detail` | **Date**: 2026-04-24 | **Spec**: `/specs/017-workout-history-detail/spec.md`
**Input**: Feature specification from `/specs/017-workout-history-detail/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Add a read-only completed-workout detail flow from Workout History so users can open a past session and review summary metadata, lifts, and sets (weight/reps). The design reuses existing backend workout and workout-lift endpoints, adds history-to-detail navigation in Angular, and preserves current history list behavior (completed-only, newest-first, existing empty/error states).

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend; C# on .NET 10 backend  
**Primary Dependencies**: Angular, Angular Router, Angular Material, ASP.NET Core Web API, Entity Framework Core  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout tables)  
**Testing**: Frontend unit tests (Jasmine/Karma), frontend e2e tests (Playwright), backend unit/integration/contract tests (xUnit)  
**Target Platform**: Mobile web browsers with Azure-compatible backend hosting
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Open detail from history with a single tap and show usable loading/error feedback under ordinary gym connectivity  
**Constraints**: Preserve completed-only newest-first history behavior; completed workout detail is read-only; no analytics/filter/suggested progression additions in this slice  
**Scale/Scope**: Per-user completed workout inspection across existing history volumes; detail includes workout summary, lift list, and set rows only

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
specs/017-workout-history-detail/
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
│       ├── Api/Contracts/Workouts/
│       ├── Api/Controllers/
│       └── Application/Workouts/Queries/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/app/
│   ├── core/api/
│   ├── core/state/
│   └── features/
│       ├── history/
│       └── workouts/
└── tests/
    ├── unit/history/
    └── e2e/workouts/
```

**Structure Decision**: Keep the existing frontend/backend split and extend current workout-history navigation plus existing workout detail query/contract paths, avoiding new modules or persistence layers.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
