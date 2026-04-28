import { expect, test, type Page } from '@playwright/test';

type WorkoutSummary = {
  id: string;
  status: 'InProgress' | 'Completed';
  label: string | null;
  startedAtUtc: string;
  completedAtUtc?: string | null;
};

const activeWorkoutPath = '**/api/workouts/active';

async function stubWorkoutDetailApis(page: Page, workout: WorkoutSummary): Promise<void> {
  await page.route(`**/api/workouts/${workout.id}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout }),
    });
  });

  await page.route(`**/api/workouts/${workout.id}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });

  await page.route('**/api/lifts?activeOnly=true', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });
}

test('active workout remains available after logging a historical catch-up workout', async ({ page }) => {
  const activeWorkoutId = `active-workout-${Date.now()}`;
  const historicalWorkoutId = `historical-workout-${Date.now()}`;
  const activeWorkoutLabel = `Current session ${Date.now()}`;
  const historicalWorkoutLabel = 'Catch-up session';
  const activeWorkout: WorkoutSummary = {
    id: activeWorkoutId,
    status: 'InProgress',
    label: activeWorkoutLabel,
    startedAtUtc: '2026-04-28T13:00:00Z',
  };

  await page.route(activeWorkoutPath, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout: activeWorkout }),
    });
  });

  const historicalWorkout: WorkoutSummary = {
    id: historicalWorkoutId,
    status: 'InProgress',
    label: historicalWorkoutLabel,
    startedAtUtc: '2026-04-20T10:00:00Z',
  };
  const completedHistoricalWorkout: WorkoutSummary = {
    ...historicalWorkout,
    status: 'Completed',
    completedAtUtc: '2026-04-20T10:45:00Z',
  };

  await page.route('**/api/workouts/historical', async (route) => {
    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: historicalWorkout,
      }),
    });
  });

  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: historicalWorkoutId,
            label: historicalWorkoutLabel,
            completedAtUtc: '2026-04-20T10:45:00Z',
            durationDisplay: '00:45',
            liftCount: 0,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${historicalWorkoutId}/complete`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout: completedHistoricalWorkout }),
    });
  });

  await stubWorkoutDetailApis(page, activeWorkout);
  await stubWorkoutDetailApis(page, historicalWorkout);

  await page.goto('/');
  await expect(page.getByText(activeWorkoutLabel, { exact: true })).toBeVisible();

  await page.getByRole('button', { name: 'Open Historical Logging' }).click();
  await expect(page).toHaveURL(/\/workouts\/history-log$/);

  await page.getByLabel('Date').fill('2026-04-20');
  await page.getByLabel('Start time').fill('10:00');
  await page.getByLabel('Duration (minutes)').fill('45');
  await page.getByLabel('Label (optional)').fill(historicalWorkoutLabel);
  await page.getByRole('button', { name: 'Save Historical Workout' }).click();

  await expect(page).toHaveURL(new RegExp(`/workouts/${historicalWorkoutId}\\?mode=historical$`));
  await page.getByTestId('active-workout-complete').click();
  await expect(page).toHaveURL(new RegExp(`/history/${historicalWorkoutId}$`));

  await page.goto('/');
  await expect(page.getByText(activeWorkoutLabel, { exact: true })).toBeVisible();
  await page.getByRole('button', { name: /continue/i }).click();
  await expect(page).toHaveURL(new RegExp(`/workouts/${activeWorkoutId}$`));
});
