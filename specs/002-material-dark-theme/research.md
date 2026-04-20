# Research: Material Dark Theme

## Decision 1: Use Angular Material's modern theme system as the visual foundation

- **Decision**: Use Angular Material's current Sass `@use` theming approach with a centralized
  app-level theme definition instead of page-local styling or a prebuilt theme drop-in.
- **Rationale**: The app is already Angular 20-based, the feature is specifically about adopting
  Material styling, and a centralized theme gives the project a reusable foundation for future
  pages instead of another one-off SCSS layer.
- **Alternatives considered**:
  - Use a prebuilt Material theme: rejected because the feature needs custom brand colors and a
    future path to light mode.
  - Keep hand-authored page SCSS and only imitate Material spacing: rejected because it would not
    establish reusable Material-based foundations.

## Decision 2: Use Iowa State cardinal and gold as brand colors within a dark neutral surface system

- **Decision**: Use Iowa State's official cardinal `#C8102E` and gold `#F1BE48` as the primary
  brand references, with dark neutral or charcoal surfaces rather than pure black backgrounds.
- **Rationale**: The user explicitly wants the app to reflect Iowa State colors. A dark neutral
  base preserves the requested dark mode while allowing cardinal and gold to act as recognizable
  accents without creating a harsh or low-contrast black-and-gold look.
- **Alternatives considered**:
  - Use cardinal almost everywhere: rejected because large red surfaces would reduce hierarchy and
    make status colors harder to interpret.
  - Use pure black backgrounds with gold accents: rejected because it would feel heavier and less
    adaptable for future screens.

## Decision 3: Structure theming for future light mode without exposing a theme toggle now

- **Decision**: Build the theme so dark mode is the only user-visible mode in this feature, but
  organize the theme tokens and selectors so a future light theme can be added without redesigning
  the current surfaces.
- **Rationale**: The specification requires future readiness without adding a toggle or user-facing
  setting. A centralized theme configuration supports that path while keeping this slice scoped.
- **Alternatives considered**:
  - Dark theme only with no future structure: rejected because it would create avoidable rework.
  - Ship a visible theme toggle now: rejected because it is explicitly out of scope.

## Decision 4: Limit the first-pass scope to app shell and Settings -> Lifts

- **Decision**: Restyle only the main app shell/navigation and the `Settings -> Lifts` page in
  this phase.
- **Rationale**: Those are the only currently implemented user-facing surfaces, and the spec
  intentionally avoids styling screens that do not exist yet.
- **Alternatives considered**:
  - Define broad full-app styling for future flows: rejected because it would force speculative
    decisions about screens that are not implemented.

## Decision 5: Preserve behavior and validate styling through frontend-focused tests

- **Decision**: Treat this as a presentation-layer feature with no intended backend or persistence
  changes, and validate the work through frontend build, Angular unit tests, and mobile-oriented
  manual verification of the create-lift flow.
- **Rationale**: The constitution keeps business logic out of Angular, and this feature does not
  change business rules. The key regression risk is visual clarity and mobile usability.
- **Alternatives considered**:
  - Add backend or contract test changes: rejected because no backend contract or business-rule
    change is planned.
  - Rely only on manual verification: rejected because the current frontend test layer should still
    protect the existing create-lift interaction.
