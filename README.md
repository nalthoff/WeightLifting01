# WeightLifting01

`WeightLifting01` is a mobile-first web app for quickly logging strength workouts in the gym.
The current implementation includes the first feature slice for managing lifts from
`Settings -> Lifts`, styled with an Angular Material dark theme that uses Iowa State-inspired
cardinal and gold accents.

## Tech Stack

- Frontend: Angular 20
- UI Styling: Angular Material 20 with a custom dark theme
- Backend: ASP.NET Core Web API on .NET 10
- Database: SQL Server / LocalDB via Entity Framework Core
- Tests: xUnit, Angular/Karma, Playwright

## Current Scope

The repository is in early development. Right now the app supports:

- viewing the `Settings -> Lifts` page
- creating a lift with a required name
- rejecting blank or whitespace-only lift names
- showing the new lift in the lift list after a successful save
- using a shared Angular Material dark theme across the app shell and `Settings -> Lifts`

## Repository Layout

- `frontend/`: Angular application
- `backend/`: .NET solution, API project, and backend test projects
- `specs/`: feature specs, plans, tasks, and manual test guides

## Prerequisites

Install the following before running the app locally:

- .NET 10 SDK
- Node.js 22+ and npm
- SQL Server LocalDB
- A Chromium-based browser for Angular and Playwright test runs

## Local Setup

### 1. Install frontend dependencies

```bash
cd frontend
npm install
```

### 2. Restore backend tools and dependencies

```bash
cd ..
dotnet tool restore
dotnet restore "backend/WeightLifting01.slnx"
```

### 3. Create or update the local database

The backend development configuration uses:

- server: `(localdb)\MSSQLLocalDB`
- database: `WeightLifting01`

Apply the existing EF Core migrations:

```bash
dotnet ef database update --project "backend/src/WeightLifting.Api/WeightLifting.Api.csproj"
```

## Running the App

Run the backend and frontend in separate terminals.

### Backend

```bash
cd backend/src/WeightLifting.Api
dotnet watch
```

The API listens on `http://localhost:5264`.

### Frontend

```bash
cd frontend
npm start
```

The Angular dev server runs on `http://localhost:4200`.

During local development, frontend requests to `/api/*` are proxied to the backend at
`http://localhost:5264` via `frontend/proxy.conf.json`.

The default visible theme is dark mode. The frontend theme structure is set up so a light theme
can be added later without replacing the current styling approach.

## Manual Verification

After both apps are running:

1. Open `http://localhost:4200`.
2. Go to `Settings / Lifts`.
3. Try submitting an empty value and confirm the page shows a validation message.
4. Create a lift such as `Front Squat`.
5. Confirm the success message appears and the lift is shown in the list.
6. Confirm the app shell and lifts page share the same dark Material-styled visual system.

There is also a story-specific manual test guide at
`specs/001-create-lift/manual-tests/us1-create-lift.md`.

## Automated Testing

### Backend tests

```bash
dotnet test "backend/WeightLifting01.slnx"
```

This runs:

- unit tests in `backend/tests/WeightLifting.Api.UnitTests/`
- integration tests in `backend/tests/WeightLifting.Api.IntegrationTests/`
- contract tests in `backend/tests/WeightLifting.Api.ContractTests/`

### Frontend unit tests

```bash
cd frontend
npx ng test --watch=false --browsers=ChromeHeadless
```

### Frontend production build

```bash
cd frontend
npm run build
```

### Playwright tests

List the configured end-to-end tests:

```bash
cd frontend
npx playwright test --list
```

Run Playwright tests:

```bash
cd frontend
npx playwright test
```

For end-to-end runs, make sure the frontend and backend are already running locally.

## Notes

- The project follows a mobile-first design approach.
- The frontend uses a custom Angular Material dark theme with Iowa State-inspired brand colors.
- Business logic is intended to live in the backend and be covered by automated tests.
- Production code is organized with one class per file for easier discovery and maintenance.
