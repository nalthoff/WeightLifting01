import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';

import { defineConfig, devices } from '@playwright/test';

const e2eDir = path.join(os.tmpdir(), 'WeightLifting01-e2e');
fs.mkdirSync(e2eDir, { recursive: true });
const e2eDatabaseFile = path.join(e2eDir, `run-${Date.now()}.db`);
process.env.WEIGHTLIFTING_E2E_SQLITE_FILE = e2eDatabaseFile;

/** Absolute path so the DB is never created relative to repo / API cwd. */
const e2eDatabasePath = `Data Source=${e2eDatabaseFile.replace(/\\/g, '/')}`;

/** Set to "1" only if you intentionally reuse already-running servers (see README risk). */
const reuseExistingServer = process.env.PW_E2E_REUSE === '1';

export default defineConfig({
  testDir: '.',
  testMatch: ['tests/e2e/**/*.spec.ts', 'e2e/**/*.spec.ts'],
  fullyParallel: true,
  timeout: 30_000,
  globalTeardown: './playwright.global-teardown.ts',
  use: {
    baseURL: 'http://localhost:4300',
    trace: 'on-first-retry',
  },
  webServer: [
    {
      command:
        'dotnet run --no-launch-profile --project "../backend/src/WeightLifting.Api/WeightLifting.Api.csproj" --urls http://localhost:5265',
      url: 'http://localhost:5265/api/lifts?activeOnly=true',
      reuseExistingServer,
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
      reuseExistingServer,
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
