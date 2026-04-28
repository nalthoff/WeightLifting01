import { expect, test } from '@playwright/test';

test('historical logging saves then shows lift/set parity surfaces in history detail', async ({ page }) => {
  const workoutId = `historical-parity-${Date.now()}`;
  const label = 'Backfilled Bench Session';
  const historyDuration = '00:45';
  const createdAtUtc = '2026-04-20T10:00:00Z';
  const completedAtUtc = '2026-04-20T10:45:00Z';
  const postedRequests: Array<{
    trainingDayLocalDate: string;
    startTimeLocal: string;
    sessionLengthMinutes: number;
    label: string | null;
  }> = [];

  await page.route('**/api/workouts/active', async (route) => {
    await route.fulfill({
      status: 204,
      contentType: 'application/json',
      body: '',
    });
  });

  await page.route('**/api/workouts/historical', async (route) => {
    postedRequests.push(route.request().postDataJSON());
    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label,
          startedAtUtc: createdAtUtc,
          completedAtUtc,
        },
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
            id: workoutId,
            label,
            completedAtUtc,
            durationDisplay: historyDuration,
            liftCount: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label,
          startedAtUtc: createdAtUtc,
          completedAtUtc,
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: 'entry-1',
            workoutId,
            liftId: 'lift-1',
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-20T10:05:00Z',
            position: 0,
            sets: [
              {
                id: 'set-1',
                workoutLiftEntryId: 'entry-1',
                setNumber: 1,
                reps: 8,
                weight: 185,
                createdAtUtc: '2026-04-20T10:08:00Z',
                updatedAtUtc: '2026-04-20T10:08:00Z',
              },
              {
                id: 'set-2',
                workoutLiftEntryId: 'entry-1',
                setNumber: 2,
                reps: 10,
                weight: null,
                createdAtUtc: '2026-04-20T10:12:00Z',
                updatedAtUtc: '2026-04-20T10:12:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.goto('/');
  await page.getByRole('button', { name: 'Open Historical Logging' }).click();
  await expect(page).toHaveURL(/\/workouts\/history-log$/);
  await expect(page.getByText('Log Past Workout')).toBeVisible();

  await page.getByLabel('Date').fill('2026-04-20');
  await page.getByLabel('Start time').fill('10:00');
  await page.getByLabel('Duration (minutes)').fill('45');
  await page.getByLabel('Label (optional)').fill(label);
  await page.getByRole('button', { name: 'Save Historical Workout' }).click();

  await expect(page).toHaveURL(/\/history$/);
  await expect(page.getByText('Historical workout saved to your history.')).toBeVisible();
  await expect(page.getByText(label, { exact: true })).toBeVisible();
  await expect(page.getByTestId('history-item-duration')).toContainText(`Duration ${historyDuration}`);
  await expect(page.getByTestId('history-item-lift-count')).toContainText('1 lifts');
  expect(postedRequests).toEqual([
    {
      trainingDayLocalDate: '2026-04-20',
      startTimeLocal: '10:00',
      sessionLengthMinutes: 45,
      label,
    },
  ]);

  await page.getByTestId(`history-item-link-${workoutId}`).click();
  await expect(page).toHaveURL(new RegExp(`/history/${workoutId}$`));

  await expect(page.getByText('Bench Press')).toBeVisible();
  const setsTable = page.getByTestId('history-workout-detail-sets');
  await expect(setsTable).toBeVisible();
  const setRows = setsTable.locator('tbody tr');
  await expect(setRows).toHaveCount(2);
  await expect(setRows.nth(0).getByRole('cell').nth(0)).toHaveText('1');
  await expect(setRows.nth(0).getByRole('cell').nth(1)).toHaveText('8');
  await expect(setRows.nth(0).getByRole('cell').nth(2)).toHaveText('185 lb');
  await expect(setRows.nth(1).getByRole('cell').nth(0)).toHaveText('2');
  await expect(setRows.nth(1).getByRole('cell').nth(1)).toHaveText('10');
  await expect(setRows.nth(1).getByRole('cell').nth(2)).toHaveText('-');

  await expect(page.getByRole('button', { name: 'Add Lift' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /Add set/i })).toHaveCount(0);
});

test.skip(
  'TODO: historical logging supports adding lifts and sets before completion',
  async ({ page }) => {
    await page.goto('/workouts/history-log');

    await expect(page.getByText('Log Past Workout')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Save Historical Workout' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Add Lift' })).toHaveCount(0);
    await expect(page.getByRole('button', { name: /Add set/i })).toHaveCount(0);
  },
);
