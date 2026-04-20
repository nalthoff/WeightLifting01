import { defineConfig, devices } from '@playwright/test';

const e2eDatabasePath = `Data Source=playwright-e2e-${Date.now()}.db`;

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  timeout: 30_000,
  use: {
    baseURL: 'http://localhost:4300',
    trace: 'on-first-retry',
  },
  webServer: [
    {
      command:
        'dotnet run --no-launch-profile --project "../backend/src/WeightLifting.Api/WeightLifting.Api.csproj" --urls http://localhost:5265',
      url: 'http://localhost:5265/api/lifts?activeOnly=true',
      reuseExistingServer: false,
      timeout: 120_000,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Test',
        Persistence__Provider: 'Sqlite',
        ConnectionStrings__DefaultConnection: e2eDatabasePath,
      },
    },
    {
      command: 'npx ng serve --port 4300 --proxy-config proxy.e2e.conf.json',
      url: 'http://localhost:4300',
      reuseExistingServer: false,
      timeout: 120_000,
    },
  ],
  projects: [
    {
      name: 'Mobile Chrome',
      use: {
        ...devices['Pixel 7'],
      },
    },
  ],
});
