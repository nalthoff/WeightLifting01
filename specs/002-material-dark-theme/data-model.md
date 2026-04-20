# Data Model: Material Dark Theme

This feature does not introduce new persisted business data. The "entities" below represent the
design concepts that the implementation needs to keep consistent.

## Theme Profile

- **Purpose**: Defines the app-wide visual identity for the current release.
- **Fields**:
  - `mode`: dark-only for this release
  - `brandPrimary`: Iowa State cardinal
  - `brandAccent`: Iowa State gold
  - `surfaceBase`: dark neutral surface color family
  - `contentContrastRules`: text/icon contrast expectations for dark surfaces
- **Validation rules**:
  - Must preserve readable contrast for navigation, form controls, messages, and list items
  - Must support a future light-mode variant without redefining every component from scratch

## Surface Pattern

- **Purpose**: Establishes shared visual rules for currently implemented screens.
- **Fields**:
  - `containerSpacing`
  - `sectionHierarchy`
  - `controlShape`
  - `elevationOrOutlineTreatment`
  - `interactiveStateTreatment`
- **Relationships**:
  - Derived from `Theme Profile`
  - Applied to app shell and `Settings -> Lifts`
- **Validation rules**:
  - Must stay usable on mobile-sized screens
  - Must feel consistent between the app shell and lifts page

## Feedback State Pattern

- **Purpose**: Normalizes loading, validation, success, and failure presentation in the dark theme.
- **Fields**:
  - `stateType`: loading, validation, success, failure
  - `foregroundTreatment`
  - `backgroundOrContainerTreatment`
  - `iconOrEmphasisUsage`
  - `readabilityExpectation`
- **Relationships**:
  - Consumed by `Settings -> Lifts`
  - Governed by `Theme Profile`
- **Validation rules**:
  - State meaning must remain clear without relying only on color
  - Success and error treatments must remain visually integrated with the shared theme
