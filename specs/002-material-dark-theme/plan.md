# Implementation Plan: Material Dark Theme

**Branch**: `[002-material-dark-theme]` | **Date**: 2026-04-20 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-material-dark-theme/spec.md`

## Summary

Adopt Angular Material as the styling foundation for the current frontend, restyling the app shell
and `Settings -> Lifts` page with a mobile-first dark theme that uses Iowa State-inspired cardinal
and gold accents. Keep the feature scoped to presentation only while leaving behind reusable
theme foundations for future screens and a clean path to a future light theme.

## Technical Context

**Language/Version**: Angular 20 frontend, C# with .NET 10 backend  
**Primary Dependencies**: Angular, Angular Material, RxJS, ASP.NET Core Web API, Entity Framework Core  
**Storage**: SQL Server / Azure SQL-compatible relational storage already used by the app; no storage changes planned  
**Testing**: Angular unit tests with Karma/Jasmine, Playwright end-to-end coverage, existing backend xUnit suites remain unchanged unless integration breaks  
**Target Platform**: Mobile web browsers with Azure-compatible backend hosting assumptions  
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Preserve the current create-lift flow speed and keep the styling update from adding interaction friction or extra steps  
**Constraints**: Dark theme is the only user-visible theme in this slice; styling applies only to the app shell and `Settings -> Lifts`; Iowa State-inspired cardinal and gold palette; no business-logic or persistence changes; mobile-first readability and contrast remain mandatory  
**Scale/Scope**: One existing app shell plus one existing feature page; reusable theme foundation for future frontend surfaces

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Mobile-first logging flow is the primary UX, with the smallest supported viewport treated as the design baseline.
- [x] The feature preserves or improves "just enough history" needed to help users choose the next working weight.
- [x] Angular owns presentation concerns only; business rules are assigned to C# backend or a dedicated business layer.
- [x] The design follows SOLID principles with clear responsibilities and explicit dependency boundaries.
- [x] Production code organization keeps one class per file unless a documented exception is required by the framework or language construct.
- [x] SQL persistence changes, if any, include schema ownership and migration planning.
- [x] All affected business-layer logic has a concrete unit-test approach.
- [x] All proposed infrastructure and runtime assumptions are compatible with Azure hosting and managed services.

## Project Structure

### Documentation (this feature)

```text
specs/002-material-dark-theme/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── theme-contract.md
└── tasks.md
```

### Source Code (repository root)

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
│   │   ├── app.html
│   │   ├── app.scss
│   │   └── features/
│   │       └── settings/
│   │           └── lifts/
│   │               ├── lifts-page.component.html
│   │               └── lifts-page.component.scss
│   └── styles.scss
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: Keep the existing split Angular frontend and .NET backend structure. The
implementation work is concentrated in frontend styling, shared theme setup, and frontend test
verification while preserving the current backend and persistence layers.

## Phase 0: Research Output

Research resolved the planning unknowns with the following decisions:

- Use Angular Material's centralized theming system rather than page-local SCSS as the long-term
  foundation.
- Use Iowa State cardinal `#C8102E` and gold `#F1BE48` as the brand references, applied within a
  dark neutral surface system.
- Keep dark mode as the only visible mode in this feature, but structure theme setup to support a
  future light theme.
- Limit first-pass styling to the current app shell and `Settings -> Lifts`.
- Validate the feature through frontend-focused build, unit, and manual mobile verification.

## Phase 1: Design & Contracts Output

### Data Model

`data-model.md` defines the non-persistent design concepts for this feature:

- `Theme Profile`
- `Surface Pattern`
- `Feedback State Pattern`

### Contract

`contracts/theme-contract.md` captures the UI contract for:

- the themed app shell
- the themed `Settings -> Lifts` page
- future-compatible theme behavior without a visible toggle

### Manual Verification

`quickstart.md` defines the mobile-first verification flow for validating:

- shared dark styling across the current surfaces
- readable loading, validation, success, and error states
- no regression in the create-lift interaction

## Post-Design Constitution Check

- [x] Mobile-first baseline remains explicit in quickstart and scope.
- [x] Existing workout-history behavior is preserved because this feature is styling-only.
- [x] Presentation stays in Angular with no business-rule migration into the UI.
- [x] Shared theming and surface patterns keep responsibilities clear and reusable.
- [x] One-class-per-file constraints remain unaffected by the planned work.
- [x] No SQL or migration work is introduced.
- [x] No new business-layer logic is planned, so existing backend unit-test obligations remain unchanged.
- [x] The design remains Azure-compatible because it changes only frontend presentation assets and dependencies.

## Complexity Tracking

No constitution violations are currently expected for this feature.
