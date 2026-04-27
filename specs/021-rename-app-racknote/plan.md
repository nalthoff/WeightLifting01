# Implementation Plan: Rename app to RackNote

**Branch**: `021-rename-app-racknote` | **Date**: 2026-04-27 | **Spec**: `C:/Users/nicka/source/repos/WeightLifting01/specs/021-rename-app-racknote/spec.md`
**Input**: Feature specification from `C:/Users/nicka/source/repos/WeightLifting01/specs/021-rename-app-racknote/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Deliver a focused product rename so users consistently see `RackNote` across visible app identity
surfaces and user-facing documentation, while preserving all workout behavior. Implementation is
limited to user-facing name strings plus related automated tests, with no workflow, data model, or
infrastructure changes.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: Angular 20 (TypeScript) frontend, C# on .NET 10 backend  
**Primary Dependencies**: Angular Material + Angular Router, ASP.NET Core, Entity Framework Core 10  
**Storage**: Existing SQL Server / Azure SQL-compatible relational storage (unchanged)  
**Testing**: Jasmine/Karma frontend unit tests, Playwright e2e tests, existing backend xUnit suites (regression only)  
**Target Platform**: Mobile-first web app in browser with Azure-compatible backend/runtime
**Project Type**: Mobile-first web application with Angular frontend and C# backend  
**Performance Goals**: No regression to current in-gym flow speed; rename content remains immediately visible on page load/navigation  
**Constraints**: Rename-only scope, no transition label text, no workout behavior change, maintain viewport consistency  
**Scale/Scope**: User-visible app identity strings in app shell/metadata + user-facing top-level documentation

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
└── tests/
    ├── WeightLifting.Api.UnitTests/
    ├── WeightLifting.Api.IntegrationTests/
    └── WeightLifting.Api.ContractTests/

frontend/
├── src/
│   ├── app/
│   │   └── app.html
│   └── index.html
└── tests/
    ├── e2e/
    │   └── navigation/
    └── unit/

README.md
```

**Structure Decision**: Keep all changes in existing frontend shell/metadata files and
user-facing documentation, with test updates in current frontend unit/e2e suites.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

## Post-Design Constitution Re-Check

- Mobile-first logging flow remains the baseline and receives only display-text changes.
- Decision-ready workout history behavior is unchanged because no history logic is modified.
- Angular remains responsible for visible branding text; backend business rules remain untouched.
- SOLID boundaries are preserved because no new service/domain responsibility is introduced.
- One-class-per-file principle is unaffected by this change set.
- SQL persistence and migration scope are unchanged because no data model updates are introduced.
- Backend unit-test requirement is unaffected because no backend business logic changes occur.
- Azure compatibility remains unchanged; no new infrastructure assumptions are introduced.
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
