# Data Model: Default home landing and Settings navigation

This feature does not introduce persisted entities or backend schema changes. The model below
captures UI and route-state concepts needed to validate behavior.

## RouteSurface

- **Purpose**: Represents the top-level user-visible surfaces in the app shell.
- **Values**:
  - `home`: Default landing surface at root entry.
  - `settings-lifts`: Existing Lift management surface.
- **Rules**:
  - Root entry resolves to `home`.
  - Direct address to `settings-lifts` remains valid.

## NavigationItem

- **Purpose**: Represents an item in primary app navigation.
- **Fields**:
  - `label` (string): User-facing text (must clearly expose "Settings").
  - `target` (route path): Destination path.
  - `isActive` (derived boolean): Whether current route matches target or section context.
- **Rules**:
  - At least one primary item exposes Settings pathing to Lift management.
  - Navigation remains usable on mobile viewport widths.

## HomeSurfaceState

- **Purpose**: Defines allowed home-screen composition for this phase.
- **Fields**:
  - `hasFeatureContent` (boolean): Must remain `false` for this feature.
  - `hasAccessibilityLandmark` (boolean): May be `true` if needed.
  - `hasNavigationContext` (boolean): `true` through shared app shell.
- **Rules**:
  - No workout logging or lift management modules render in home main content.
  - Optional semantic heading/landmark is permitted when accessibility requires it.

## Behavioral Notes

- No changes to lift domain entities (`Lift`, `LiftListItem`, etc.) are required.
- No API requests or payload shape changes are required to deliver this feature.
- Success depends on route resolution and navigation discoverability rather than data mutation.
