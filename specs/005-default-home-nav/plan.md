# Implementation Plan: Default home landing and Settings navigation

**Branch**: `[005-default-home-nav]` | **Date**: 2026-04-22 | **Spec**: [`specs/005-default-home-nav/spec.md`](./spec.md)
**Input**: Feature specification from `/specs/005-default-home-nav/spec.md`

## Summary

Introduce a dedicated default home route so root entry loads an intentionally empty home surface,
then keep the existing Lift management experience reachable through a clearly labeled Settings
navigation path. This slice is frontend-only and focuses on route structure, app-shell navigation,
and deep-link preservation for `/settings/lifts` without altering backend contracts, persistence, or
lift business behavior.

## Technical Context

**Language/Version**: Angular 20 frontend, C# .NET 10 backend (unchanged)  
**Primary Dependencies**: Angular Router, Angular Material toolbar/button/nav primitives, existing lifts feature routes  
**Storage**: Existing SQL-backed backend remains unchanged (no data changes in this feature)  
**Testing**: Angular unit tests, frontend integration/smoke checks, Playwright e2e smoke for root and deep-link routing  
**Target Platform**: Mobile-first web browsers with Azure-compatible frontend/backend hosting assumptions  
**Project Type**: Mobile-first web app with Angular frontend and C# backend  
**Performance Goals**: Root navigation to home and Settings->Lifts navigation should feel immediate in normal client-side routing (<1 second local navigation expectation)  
**Constraints**: No new backend/API/data model changes; home content intentionally minimal; preserve direct `/settings/lifts` access; keep tap targets usable on narrow mobile viewports; keep business rules outside Angular  
**Scale/Scope**: One route addition (home), one route-behavior change (remove root redirect to lifts), and one shell navigation update (clear Settings path to existing lifts)

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

Post-design review: Pass. The chosen design adds only Angular routing/navigation presentation
structure, preserves existing backend-owned lift behavior and SQL model, requires no infrastructure
deviation from Azure-compatible assumptions, and keeps mobile navigation usability explicit in
acceptance and quickstart validation.

## Project Structure

### Documentation (this feature)

```text
specs/005-default-home-nav/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── navigation-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
frontend/
└── src/
    └── app/
        ├── app.html
        ├── app.routes.ts
        ├── features/
        │   ├── home/
        │   │   ├── home-page.component.ts
        │   │   └── home-page.component.html
        │   └── settings/
        │       └── lifts/
        │           └── lifts.routes.ts
        └── shared/
            └── shell styling/templates (existing)

backend/
└── src/WeightLifting.Api/
    └── (no changes expected for this feature)
```

**Structure Decision**: Reuse the existing Angular app shell and route tree, adding a dedicated
home feature surface and adjusting top-level routes/navigation only. Keep current settings/lifts
feature location and lazy loading semantics. Backend structure remains untouched.

## Complexity Tracking

No constitution violations are expected, and no exception requires justification for this feature.
