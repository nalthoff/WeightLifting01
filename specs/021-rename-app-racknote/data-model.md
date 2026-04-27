# Data Model: Rename app to RackNote

## Entity: AppIdentitySurface

**Purpose**: Represents a user-visible place where the product name is shown.

### Fields

- `surfaceId` (string, required) - Unique identifier for the identity surface.
- `surfaceType` (enum, required) - `InAppBranding`, `PageMetadata`, `Documentation`.
- `location` (string, required) - Path or context where the name appears.
- `displayValue` (string, required) - Expected visible product name.

### Validation and Rules

- `displayValue` must equal `RackNote` for all in-scope user-facing surfaces.
- Any in-scope `AppIdentitySurface` with `displayValue = WeightLifting01` fails acceptance.
- Surface behavior must remain unchanged beyond displayed identity text.

## Entity: IdentityVerificationScenario

**Purpose**: Defines acceptance checks ensuring identity consistency across key journeys.

### Fields

- `scenarioId` (string, required)
- `viewport` (enum, required) - `Mobile`, `Desktop`
- `entryPoint` (string, required) - Shell/home/docs access point under test
- `expectedName` (string, required) - `RackNote`
- `legacyNameAllowed` (boolean, required) - Always `false`

### Validation and Rules

- `expectedName` is fixed to `RackNote` in this feature scope.
- `legacyNameAllowed` must remain `false` for all scenarios.
- Scenario execution must confirm no functional regression in core workout flows.
