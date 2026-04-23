# Implementation Plan: Workout history page and completion parity

**Branch**: `014-workout-history-page` | **Date**: 2026-04-23 | **Spec**: `c:/Users/nicka/source/repos/WeightLifting01/specs/014-workout-history-page/spec.md`
**Input**: Feature specification from `c:/Users/nicka/source/repos/WeightLifting01/specs/014-workout-history-page/spec.md`

## Summary

Deliver a dedicated Workout History page that lists completed workouts with only label and completed date, while ensuring workout completion is available from both home and active workout detail. Keep lifecycle rules and completion validity in backend C# application/domain logic, and keep Angular focused on presentation, state updates, and clear error/success feedback.

## Technical Context

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Router and Angular Material, ASP.NET Core Web API, Entity Framework Core 10  
**Storage**: SQL Server / Azure SQL-compatible relational database using existing workouts tables  
**Testing**: Frontend unit tests (Jasmine/Karma) and e2e (Playwright); backend unit/integration/contract tests (xUnit)  
**Target Platform**: Mobile web browsers first with Azure-compatible backend hosting  
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: Completion actions resolve with immediate feedback after response; history list loads quickly enough for in-gym use and renders recent completed workouts without visible lag  
**Constraints**: Mobile-first interaction design, backend-owned lifecycle rules, no completed-workout editing, no analytics/dashboard expansion, one production class per file  
**Scale/Scope**: Single-user workout history listing with concise row data (label + completed date), default ordering by most recent completion

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [ ] Mobile-first logging flow is the primary UX, with the smallest supported viewport
      treated as the design baseline.
- [ ] The feature preserves or improves "just enough history" needed to help users choose
      the next working weight.
- [ ] Angular owns presentation concerns only; business rules are assigned to the C#
      backend application/domain layer.
- [ ] The design follows SOLID principles with clear responsibilities and explicit
      dependency boundaries.
- [ ] Production code organization keeps one class per file unless a documented exception is
      required by the framework or language construct.
- [ ] SQL persistence changes, if any, include explicit schema updates and versioned
      migration planning.
- [ ] All affected backend application/domain logic has a concrete unit-test approach.
- [ ] All proposed infrastructure and runtime assumptions are compatible with Azure hosting
      and managed services.
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
specs/014-workout-history-page/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── workout-history-api.yaml
└── tasks.md
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
├── src/
│   └── app/
│       ├── core/
│       │   ├── api/
│       │   └── state/
│       └── features/
│           ├── home/
│           ├── workouts/
│           └── history/
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Extend existing workout API/application modules for completed-workout listing and reuse completion command handling; add a dedicated frontend history feature slice and route, while keeping lifecycle decisions in backend layers.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first behavior is preserved by keeping completion one-tap from existing entry points and adding a simple, scan-friendly history list.
- History remains decision-ready and bounded to the minimum scope (label plus completed date) without analytics/reporting sprawl.
- Angular remains presentation-focused; backend application/domain layers continue to own completion and lifecycle validity.
- SQL-backed persistence and Azure-compatible assumptions remain unchanged; only additive query/contract behavior is introduced.
- Planned testing includes backend unit coverage for lifecycle behaviors and UI-facing verification for completion/history flows.
