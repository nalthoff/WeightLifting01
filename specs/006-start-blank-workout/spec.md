# Feature Specification: Start blank workout session

**Feature Branch**: `006-start-blank-workout`  
**Created**: 2026-04-22  
**Status**: Draft  
**Input**: User description: "As a lifter, I want to start a blank workout so I can log a training session from scratch without relying on prefilled programming."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start a workout from home in the gym (Priority: P1)

As a lifter on a phone, I open the app to a true home/landing screen and can start a new workout with a prominent action. The workout is officially created as in progress and start time is assigned by the server so the session is trustworthy.

**Why this priority**: This is the core in-gym journey and first end-to-end workout-session slice.

**Independent Test**: From home on a phone-sized viewport, tap the primary start action and confirm a new in-progress workout exists with a non-null server-assigned start timestamp.

**Acceptance Scenarios**:

1. **Given** I am on the home/landing screen and have no active workout, **When** I tap Start Workout, **Then** the system creates a new workout with status In Progress and a server-assigned start timestamp.
2. **Given** workout creation succeeds, **When** I am redirected into the active-session experience, **Then** I can clearly see that my workout is in progress and when it started.
3. **Given** I am on a phone-sized viewport, **When** I start a workout, **Then** I can complete the happy path in no more than 3 taps.

---

### User Story 2 - Name the workout at start (Priority: P2)

As a lifter, I can optionally add a short session label while starting a workout so I can distinguish sessions without requiring structured programming or templates.

**Why this priority**: Useful organization value without blocking quick-start behavior.

**Independent Test**: Start a workout with and without a label; verify both succeed and that a whitespace-only label is stored as no name.

**Acceptance Scenarios**:

1. **Given** I choose to enter a workout name, **When** I submit a non-empty label, **Then** the workout is created with that label.
2. **Given** I leave the label blank or enter only whitespace, **When** I submit start, **Then** the workout is still created and stored as having no label.

---

### User Story 3 - Handle active workout conflicts safely (Priority: P2)

As a lifter who may already have an in-progress workout, I get a clear choice to continue the existing session instead of silently creating duplicates.

**Why this priority**: Prevents conflicting active sessions and protects session integrity.

**Independent Test**: With an existing in-progress workout, attempt Start Workout and verify user is prompted to continue existing session; no second in-progress workout is created.

**Acceptance Scenarios**:

1. **Given** I already have one in-progress workout, **When** I tap Start Workout, **Then** the system does not create a second in-progress workout and prompts me to continue the existing one.
2. **Given** I choose to continue existing workout, **When** I confirm, **Then** I am routed into that active session experience.

---

### Edge Cases

- Start action fails due to offline network, timeout, or server failure: user sees a clear failure state and the app does not show a fake successful session.
- Duplicate taps on Start Workout in a short interval: the system prevents duplicate in-progress workouts for the same user.
- User refreshes after successful start: active session still appears as in progress with original server-assigned start timestamp.
- Existing in-progress workout exists but cannot be loaded due to transient failure: user sees a recoverable error and can retry.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a home/landing screen as the primary entry and expose a prominent Start Workout action that is discoverable on phone-sized viewports.
- **FR-002**: The system MUST create a workout record when Start Workout succeeds.
- **FR-003**: Every newly started workout MUST be persisted with status set to In Progress.
- **FR-004**: Every newly started workout MUST have a non-null start timestamp assigned by the server in UTC.
- **FR-005**: The system MUST provide a minimum active-session experience immediately after successful start that at least confirms workout status as In Progress and displays the recorded start time.
- **FR-006**: The start flow MUST support one optional free-text workout label.
- **FR-007**: If the submitted label is empty or whitespace-only, the system MUST treat it as no label rather than rejecting workout start.
- **FR-008**: The system MUST enforce at most one In Progress workout per user at a time.
- **FR-009**: If a user already has an In Progress workout, the start flow MUST prompt the user to continue that existing workout and MUST NOT silently create another in-progress workout.
- **FR-010**: On start failure (including offline, timeout, or server error), the system MUST show a clear failure outcome and MUST NOT present an unsaved client-only workout as persisted.
- **FR-011**: Validation and workout lifecycle rules MUST be enforced in backend business logic, not in presentation-layer components.
- **FR-012**: Data model and schema changes required for workout persistence MUST be delivered through tracked SQL migration artifacts.
- **FR-013**: New backend business rules introduced by this feature MUST include automated unit tests.
- **FR-014**: New production classes introduced for this feature MUST be organized as one class per file.
- **FR-015**: This feature MUST exclude set logging, lift selection inside a session, templates/programs, analytics dashboards, and social features from this release scope.

### Key Entities *(include if feature involves data)*

- **Workout Session**: A persisted training session owned by one user, including session identifier, status, server-assigned start timestamp, optional label, and lifecycle metadata.
- **Workout Status**: A lifecycle state for a workout session; this feature requires at least In Progress and supports rules that prevent multiple simultaneous In Progress sessions per user.
- **Start Workout Attempt**: A user-initiated action outcome capturing success (created session reference) or failure (clear user-facing reason and no persisted ghost state).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In usability testing on phone-sized screens, users can start an in-progress workout from the primary entry in no more than 3 taps and about 30 seconds.
- **SC-002**: 100% of successful start operations are persisted with status In Progress and a non-null start timestamp verifiable through product data inspection.
- **SC-003**: In conflict scenarios where a user already has an in-progress workout, 100% of start attempts avoid creating a second in-progress workout.
- **SC-004**: In simulated start-failure scenarios (offline, timeout, server failure), users always receive an explicit failure result and zero ghost workouts are observed as apparently saved.

## Assumptions

- Users initiating this flow are already authenticated and scoped to their own workout data.
- The first active-session screen for this slice is intentionally minimal and confirms session start, without set/lift logging interactions.
- If an existing in-progress workout is detected, the primary decision offered is to continue that session; creating a second simultaneous in-progress workout is not allowed.
- Optional workout labels are short free text and are not required to be unique.
- Historical session details beyond session start metadata are deferred to later feature slices unless required by future stories.
