# Phase 0 Research: Rename app to RackNote

## Decision 1: Treat app shell brand text and document title as primary identity surfaces

- **Decision**: Update the visible app-shell brand label and browser/page title to `RackNote`.
- **Rationale**: These are the highest-visibility identity points users see first and repeatedly.
- **Alternatives considered**:
  - Update only in-app label: rejected because browser metadata would still show old identity.
  - Update only metadata title: rejected because in-app branding would remain inconsistent.

## Decision 2: Restrict rename scope to user-facing surfaces only

- **Decision**: Keep internal identifiers (repository folder names, solution names, DB names,
  internal namespaces) out of this feature unless they are directly user-facing.
- **Rationale**: The specification explicitly asks for a focused product rename without broader
  workflow or system refactoring.
- **Alternatives considered**:
  - Rename all internal project identifiers: rejected because it expands scope and risk without
    user-facing value for this feature.

## Decision 3: Update user-facing top-level documentation references

- **Decision**: Replace product-name mentions in README sections that describe app identity or user
  usage context with `RackNote`.
- **Rationale**: README is the primary user/contributor-facing guidance entry and should align with
  the visible app identity.
- **Alternatives considered**:
  - Leave docs unchanged: rejected because mixed naming creates avoidable confusion.
  - Rename every technical token (e.g., solution file names) in docs: rejected because many tokens
    are internal identifiers rather than app-brand references.

## Decision 4: Preserve behavior by updating only string literals and expectations

- **Decision**: Implement the feature with display-string updates plus unit/e2e expectation updates.
- **Rationale**: This meets rename requirements while preventing accidental behavior changes in
  workout logging or history flows.
- **Alternatives considered**:
  - Include workflow/UI restructuring while renaming: rejected as out of scope.
  - Skip automated test updates: rejected because existing tests intentionally assert brand text.

## Decision 5: Keep contract surface minimal and user-centric

- **Decision**: Capture a lightweight UI identity contract listing required user-facing surfaces that
  must present `RackNote`.
- **Rationale**: The feature has no API/data contract changes, but explicit identity-surface coverage
  improves acceptance clarity and reduces missed references.
- **Alternatives considered**:
  - No contract artifact: rejected because explicit acceptance mapping is useful for this rename.
  - Full API contract update: rejected because no external API behavior changes are required.
