# UI Contract: Material Dark Theme

## Covered Surfaces

- App shell / top-level navigation
- `Settings -> Lifts` page

## Visual Contract

### Theme Behavior

- The application presents a dark theme by default.
- The dark theme uses Iowa State-inspired brand colors:
  - cardinal as the primary brand color
  - gold as the accent color
- The feature does not expose a user-facing theme toggle.

### Shared Surface Expectations

- Navigation and page content use the same visual system.
- Surfaces, spacing, and controls feel like parts of one Material-based application.
- Mobile layout remains the default presentation baseline.

### Settings -> Lifts Expectations

- The lift name field is presented as a Material-styled input control.
- The submit action is presented as a Material-styled button.
- Loading, validation, success, and error states follow the same visual language as the page.
- The lift list is visually grouped and readable within the dark theme.

### Future Compatibility Expectations

- Theme setup supports adding a light theme later without replacing the full styling strategy.
- New screens should be able to adopt the same visual foundations instead of introducing
  unrelated page-specific styling rules.
