# Implementation Plan: Workout history row summary details

**Branch**: `016-workout-history-row-summary` | **Date**: 2026-04-24 | **Spec**: `/specs/016-workout-history-row-summary/spec.md`
**Input**: Feature specification from `/specs/016-workout-history-row-summary/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Extend the existing completed-workout history list so each row includes completion date, workout duration in `HH:MM`, and number of lifts while preserving current behavior (completed-only data, recency-first ordering, label display, and existing empty/error states). The plan keeps presentation in Angular and adds history summary calculation/query behavior in the C# backend application/query layer.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend; C# on .NET 10 backend  
**Primary Dependencies**: Angular, Angular Material, ASP.NET Core Web API, Entity Framework Core  
**Storage**: SQL Server / Azure SQL-compatible relational database (existing workout tables)  
**Testing**: Frontend unit tests (Jasmine/Karma), frontend e2e (Playwright), backend unit/integration/contract tests (xUnit)  
**Target Platform**: Mobile web browsers with Azure-compatible backend hosting  
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: History page remains quick to scan on mobile; summary fields render without adding additional navigation steps  
**Constraints**: Preserve existing history behavior; keep analytics/filtering out of scope; keep domain rules in backend; tolerate missing timestamp data safely  
**Scale/Scope**: Single-user history list of completed workouts with lightweight row metadata only (label, date, HH:MM duration, lift count)

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
specs/016-workout-history-row-summary/
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
│   └── features/history/
└── tests/
    ├── unit/history/
    └── e2e/workouts/
```

**Structure Decision**: Use the existing mobile-first web application split (`frontend/` + `backend/`) and extend current history query/contract/UI paths without introducing new modules.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
