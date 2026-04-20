# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Angular 20 frontend, .NET 9 backend or NEEDS CLARIFICATION]  
**Primary Dependencies**: [e.g., Angular, ASP.NET Core, Entity Framework Core or NEEDS CLARIFICATION]  
**Storage**: [e.g., Azure SQL, SQL Server, PostgreSQL-compatible relational DB or NEEDS CLARIFICATION]  
**Testing**: [e.g., Jasmine/Karma, Playwright, xUnit/NUnit, contract tests or NEEDS CLARIFICATION]  
**Target Platform**: [e.g., mobile web browsers + Azure-hosted API or NEEDS CLARIFICATION]
**Project Type**: [mobile-first web app with Angular frontend and C# backend or NEEDS CLARIFICATION]  
**Performance Goals**: [domain-specific, e.g., fast in-gym logging, sub-second history lookup or NEEDS CLARIFICATION]  
**Constraints**: [domain-specific, e.g., mobile-first UX, Azure-compatible deployment, resilient on weak connectivity]  
**Scale/Scope**: [domain-specific, e.g., individual lifter history, number of exercises, expected concurrent users]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [ ] Mobile-first logging flow is the primary UX, with the smallest supported viewport
      treated as the design baseline.
- [ ] The feature preserves or improves "just enough history" needed to help users choose
      the next working weight.
- [ ] Angular owns presentation concerns only; business rules are assigned to C# backend or
      a dedicated business layer.
- [ ] The design follows SOLID principles with clear responsibilities and explicit
      dependency boundaries.
- [ ] Production code organization keeps one class per file unless a documented exception is
      required by the framework or language construct.
- [ ] SQL persistence changes, if any, include schema ownership and migration planning.
- [ ] All affected business-layer logic has a concrete unit-test approach.
- [ ] All proposed infrastructure and runtime assumptions are compatible with Azure hosting
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
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# Preferred Option: Mobile-first web application
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/
    ├── unit/
    ├── integration/
    └── contract/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/
    ├── unit/
    └── e2e/
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
