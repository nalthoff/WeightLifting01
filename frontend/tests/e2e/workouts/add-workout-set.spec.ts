import { expect, test } from '@playwright/test';

test('add set happy path appends sequential set rows for one entry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const workoutLiftEntryId = `entry-${Date.now()}`;
  const setRequests: Array<{ reps: number; weight: number | null }> = [];
  const persistedSets: Array<{
    id: string;
    workoutLiftEntryId: string;
    setNumber: number;
    reps: number;
    weight: number | null;
    createdAtUtc: string;
  }> = [];
  let setNumber = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Set Day',
          startedAtUtc: '2026-04-23T12:00:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: workoutLiftEntryId,
            workoutId,
            liftId: `lift-${Date.now()}`,
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-23T12:02:00Z',
            position: 1,
            sets: persistedSets,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}/sets`, async (route) => {
    setNumber += 1;
    const request = route.request().postDataJSON() as { reps: number; weight: number | null };
    setRequests.push(request);
    const savedSet = {
      id: `set-${setNumber}-${Date.now()}`,
      workoutLiftEntryId,
      setNumber,
      reps: request.reps,
      weight: request.weight,
      createdAtUtc: `2026-04-23T12:0${setNumber}:00Z`,
    };
    persistedSets.push(savedSet);

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId,
        set: savedSet,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByLabel(/Reps for Bench Press entry #1/).fill('8');
  await entryCard.getByLabel(/Weight for Bench Press entry #1/).fill('135');
  await entryCard.getByRole('button', { name: /Add set for Bench Press entry #1/i }).click();

  await entryCard.getByLabel(/Reps for Bench Press entry #1/).fill('10');
  await entryCard.getByLabel(/Weight for Bench Press entry #1/).fill('');
  await entryCard.getByRole('button', { name: /Add set for Bench Press entry #1/i }).click();

  await expect(entryCard.locator('.active-workout__set-list-item')).toHaveCount(2);
  await expect(entryCard.getByText('8 reps')).toBeVisible();
  await expect(entryCard.getByText('10 reps')).toBeVisible();
  await expect(entryCard.getByText('-')).toBeVisible();

  await page.reload();

  const entryCardAfterRefresh = page.locator('.active-workout__lift-list-item').first();
  await expect(entryCardAfterRefresh.locator('.active-workout__set-list-item')).toHaveCount(2);
  await expect(entryCardAfterRefresh.getByText('8 reps')).toBeVisible();
  await expect(entryCardAfterRefresh.getByText('10 reps')).toBeVisible();

  expect(setRequests).toEqual([
    { reps: 8, weight: 135 },
    { reps: 10, weight: null },
  ]);
});
