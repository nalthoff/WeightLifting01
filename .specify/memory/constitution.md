<!--
Sync Impact Report
- Version change: 1.1.0 -> 1.1.1
- Modified principles:
  - III. Angular, C#, and SOLID Boundaries -> III. Angular, C#, and SOLID Boundaries
- Added sections:
  - None
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
  - ✅ README.md
- Follow-up TODOs:
  - None
-->
# WeightLifting01 Constitution

## Core Principles

### I. Mobile-First Logging Speed
The primary user journey MUST optimize for fast workout logging on a phone in a gym
environment. Core actions such as selecting an exercise, entering sets, and recording
working weight MUST minimize taps, avoid dense desktop-first layouts, and remain usable on
common mobile viewport widths before any larger-screen enhancement work is considered. The
rationale is simple: if logging interrupts a set, the product fails its main job.

### II. Decision-Ready Workout History
The system MUST store and present just enough recent workout history to help the lifter
choose the next weight with confidence. New features MUST preserve structured history that
supports recency, prior working sets, and quick comparison, while avoiding analytics,
reporting, or data collection that does not improve the next in-gym decision. The rationale
is to keep the product focused on immediate training usefulness rather than general fitness
tracking sprawl.

### III. Angular, C#, and SOLID Boundaries
The frontend MUST be implemented in Angular and the backend MUST be implemented in C# on a
supported .NET runtime. Presentation concerns belong in Angular; business rules for workout
logging, progression, validation, and history interpretation MUST live in the backend
application/domain layer where behavior can be tested independently of the UI. Persistent
data MUST be modeled in SQL. If a change affects the persisted data model, the same change
MUST include schema updates and versioned migrations so environments remain consistent and
deployable. New or revised code MUST follow SOLID principles so responsibilities remain
narrow, dependencies stay explicit, and behavior can be extended without fragile cross-cutting
changes. Each production class MUST live in its own file unless a language or framework
construct makes that impossible or clearly less readable. The rationale is to keep behavior
consistent across clients, preserve maintainability, and make code easy to locate.

### IV. Unit-Tested Business Logic
Any business layer logic MUST be covered by automated unit tests in the same change that
introduces or modifies that logic. This includes progression rules, logging validation,
history selection rules, and any calculation used to recommend or prefill the next weight.
Integration or contract tests SHOULD be added when API contracts, SQL mappings, or cross-
boundary flows change materially. The rationale is that workout correctness depends on rules
that are easy to regress unless they are isolated and tested.

### V. Azure-Compatible Delivery
All functionality MUST be deployable using Azure-compatible technologies, configuration, and
runtime assumptions. Features MUST avoid local-only dependencies, machine-specific file
coupling, or infrastructure choices that block deployment to services such as Azure Static
Web Apps, Azure App Service, Azure Container Apps, or Azure SQL. The rationale is to keep
the project operationally portable within the intended cloud platform from the start.

## Platform Constraints

- The product is a mobile-first web application for quickly logging strength workouts in the
  gym.
- The frontend stack MUST remain Angular-based and responsive across supported mobile
  browsers.
- The backend stack MUST remain C# and expose clear application or API boundaries that can
  evolve independently from the UI.
- Data persistence MUST use a SQL datastore with explicit schema ownership and migration
  discipline.
- Source organization MUST keep one production class per file so code is easy to find, review,
  and refactor.
- Features MUST tolerate ordinary gym connectivity issues by preventing silent data loss and
  by making save states or retry outcomes clear to the user.
- Scope expansion into broad social, nutrition, or non-strength-training domains requires an
  explicit constitution amendment or a feature specification that justifies the change
  against Principle II.

## Delivery Workflow and Quality Gates

- Every specification MUST identify the primary in-gym mobile journey, define the minimum
  history required to choose the next weight, and capture acceptance scenarios for both fast
  logging and history lookup.
- Every implementation plan MUST pass a Constitution Check that confirms Angular frontend
  ownership, C# backend ownership, SQL persistence approach, Azure deployment compatibility,
  SOLID-aligned design boundaries, one-class-per-file organization, and a unit-test strategy
  for all affected business logic.
- Every task list MUST include concrete work for backend application/domain unit tests whenever
  business rules change, plus SQL schema and migration tasks whenever persisted workout data
  changes.
- Pull requests and reviews MUST reject implementations that place business rules in Angular
  components, skip required unit tests, or add infrastructure that is not Azure-compatible
  without an approved exception. Reviews MUST also reject avoidable multi-class files or
  designs that violate SOLID without an approved exception.
- Manual verification for user-facing changes MUST include mobile viewport validation of the
  primary logging flow in addition to automated test execution.

## Governance

This constitution overrides conflicting local practices for planning, implementation, and
review. Amendments MUST be documented in this file, include an updated Sync Impact Report,
and be propagated to the affected Spec Kit templates before the amendment is considered
complete.

Versioning follows semantic versioning for governance changes: MAJOR for removing or
redefining a principle in a backward-incompatible way, MINOR for adding a principle or
materially expanding required guidance, and PATCH for clarifications that do not change team
obligations.

Compliance MUST be reviewed at three checkpoints: when drafting a specification, when
writing the implementation plan, and during pull request review. Any exception MUST be
explicitly justified in the plan's Complexity Tracking section with the simpler alternative
that was rejected.

**Version**: 1.1.1 | **Ratified**: 2026-04-20 | **Last Amended**: 2026-04-22
