# Implementation Plan: Workout Lifecycle Status Visibility

**Branch**: `019-workout-status-visibility` | **Date**: 2026-04-27 | **Spec**: `specs/019-workout-status-visibility/spec.md`
**Input**: Feature specification from `specs/019-workout-status-visibility/spec.md`

## Summary

Standardize workout lifecycle visibility by introducing explicit status badges on both active and completed workout detail views, while preserving existing lifecycle rules that gate history/progress to completed workouts only. Keep lifecycle business rules in existing backend/domain paths, add focused regression coverage for completion timestamp and history eligibility, and deliver mobile-first UI clarity without expanding status scope.

## Technical Context

**Language/Version**: Angular 20 frontend (TypeScript), C# on .NET 10 backend  
**Primary Dependencies**: Angular Router, Angular Material, RxJS; ASP.NET Core Web API, Entity Framework Core  
**Storage**: SQL Server / Azure SQL-compatible relational storage  
**Testing**: Frontend unit tests and e2e tests; backend unit/integration tests for workout lifecycle and history filtering  
**Target Platform**: Mobile web browsers with Azure-compatible web/API hosting
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Status state is visually identifiable within 2 seconds on detail views; no regression to history load responsiveness  
**Constraints**: Mobile-first UX, only two statuses in scope, Azure-compatible runtime/deployment, keep lifecycle business rules out of Angular  
**Scale/Scope**: Single-user workout lifecycle flows; active and history detail surfaces plus existing history/progress query path

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
├── src/WeightLifting.Api/Api/Contracts/Workouts/
├── src/WeightLifting.Api/Api/Controllers/
├── src/WeightLifting.Api/Application/Workouts/
├── src/WeightLifting.Api/Domain/Workouts/
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/app/features/workouts/
├── src/app/features/history/
├── src/app/core/api/
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Use the existing mobile-first Angular plus C# backend structure; implement badge presentation in frontend feature pages and preserve lifecycle gating logic in existing backend workout query/command paths with accompanying tests.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitution violations identified for this feature.
