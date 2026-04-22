# UI Contract: Home and Settings Navigation

## Covered Surfaces

- App shell header/navigation
- Home route surface
- `Settings -> Lifts` route access path

## Route Contract

- Root entry address resolves to the home surface.
- Home surface is intentionally minimal and contains no lift management or workout logging feature
  content.
- Direct navigation to `settings/lifts` remains supported.

## Navigation Contract

- The primary navigation clearly exposes a Settings area.
- Users can reach Lift management from that Settings path without manual URL entry.
- Active-state styling/indication remains clear enough for users to orient where they are.

## Content Contract

- Home main content must remain empty/minimal for this feature.
- Optional semantic elements (for example, heading or landmark) are allowed strictly for
  accessibility and structure.
- No "coming soon," promotional blocks, or feature cards are introduced on home.

## Mobile Usability Contract

- Navigation controls remain discoverable and tappable at narrow phone viewport widths.
- Interaction with Settings navigation should not require precision taps or hidden desktop-only UI.
- Route transitions do not trap users; browser Back/Forward continues to work predictably between
  home and `settings/lifts`.

## Non-Goals

- No new backend or API contract changes.
- No settings hub content beyond what is needed to expose Lift management.
- No workout logging flow changes.
