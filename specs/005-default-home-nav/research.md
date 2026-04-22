# Research: Default home landing and Settings navigation

## Decision 1: Replace root redirect with an explicit home route

- **Decision**: The root entry should resolve to a dedicated home route/view rather than redirect
  straight to Lift management.
- **Rationale**: The feature's main value is a stable starting point for future product growth and
  clearer information architecture from first load.
- **Alternatives considered**:
  - Keep redirect to `/settings/lifts`: rejected because it prevents a neutral landing experience.
  - Redirect to `/settings`: rejected because a settings-first entry still violates the requested
    default home behavior.

## Decision 2: Keep home intentionally empty with accessibility-safe minimum structure

- **Decision**: Home will contain no product feature content, with only minimal structure needed for
  semantic layout/accessibility (for example, a heading or landmark if necessary).
- **Rationale**: Matches scope boundaries and avoids committing to unfinished product messaging or
  workflow on the landing screen.
- **Alternatives considered**:
  - Add "coming soon" cards or feature teasers: rejected as out of scope.
  - Reuse the lifts page as pseudo-home: rejected because it removes clear separation between home
    and settings.

## Decision 3: Present a clearly labeled Settings navigation path to existing Lifts

- **Decision**: Update app-shell navigation so users can discover a "Settings" area and reach
  existing lift management from it, using the smallest change that fits current shell patterns.
- **Rationale**: Maintains continuity with existing management functionality while improving app
  structure for expansion.
- **Alternatives considered**:
  - Keep a single "Settings / Lifts" link with no Settings framing: partially acceptable but less
    clear for growth to future settings items.
  - Build a full settings hub now: rejected as unnecessary scope expansion.

## Decision 4: Preserve deep-link behavior for `/settings/lifts`

- **Decision**: Keep direct navigation to `/settings/lifts` functioning exactly as before.
- **Rationale**: Protects bookmarks, shared links, and existing user habits while changing default
  entry behavior.
- **Alternatives considered**:
  - Force all entries through home first: rejected because it breaks direct access expectations.
  - Change Lift management URL to nest under a new parent path: rejected due to migration overhead
    and lack of user value in this slice.

## Decision 5: Keep this feature frontend-only with no contract or persistence changes

- **Decision**: Limit changes to Angular routing, shell structure, and navigation labels/links.
- **Rationale**: The requested behavior is presentational and does not require backend logic or data
  evolution.
- **Alternatives considered**:
  - Add backend endpoint to signal "home readiness": rejected as unnecessary coupling.
  - Add analytics/event contracts as required scope: rejected; optional future enhancement only.

## Decision 6: Validate with route smoke tests and mobile nav usability checks

- **Decision**: Cover root landing, Settings->Lifts navigation, and direct `/settings/lifts`
  opening in automated or scripted smoke tests, plus narrow-viewport interaction checks.
- **Rationale**: The highest risk is routing regression and discoverability, not business logic.
- **Alternatives considered**:
  - Rely only on manual ad-hoc testing: rejected for release confidence.
  - Add extensive backend tests: rejected because backend behavior is unchanged.
