# Quickstart: Default home landing and Settings navigation

## Goal

Verify that the app opens to a dedicated, minimal home surface and that users can still reach the
existing Lift management page through a clearly labeled Settings navigation path, including direct
deep-link access.

## Prerequisites

- Frontend app running locally.
- Backend/API availability optional for routing checks, but recommended for full Lift management
  smoke verification.
- Browser with responsive viewport emulation or a physical phone.

## Verification Steps

1. Open the app using the root entry URL.
2. Confirm the app shell appears and main content is home.
3. Confirm home main area does not display lift lists, workout logging controls, or "coming soon"
   promotional content.
4. Confirm primary navigation shows a clearly identifiable Settings path.
5. Use navigation (without typing URL) to reach Lift management.
6. Confirm the Lift management page loads and behaves as before this feature.
7. In a new tab, open `/settings/lifts` directly.
8. Confirm direct deep link still loads Lift management.
9. Use browser Back/Forward between home and lifts.
10. Confirm transitions remain predictable and do not trap the user.

## Mobile-Focused Checks

1. Set viewport to a narrow phone size.
2. Repeat steps 1 through 6.
3. Confirm Settings navigation remains visible/discoverable.
4. Confirm tap targets in primary navigation are usable without repeated mis-taps.

## Negative Checks

1. Load root URL after clearing browser state/cache.
2. Confirm app does not auto-redirect to Lift management as initial destination.
3. Refresh while on `settings/lifts`.
4. Confirm route remains valid and does not bounce to home unexpectedly.

## Automated Coverage Targets

- Frontend route test for root => home behavior.
- Frontend route test for direct `/settings/lifts` behavior.
- Frontend navigation test confirming Settings path reaches Lift management.
- Optional e2e smoke in mobile viewport validating discoverability and route continuity.
