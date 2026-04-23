import { expect, test } from '@playwright/test';

test('adding sets to duplicate lift entries keeps lists isolated by workout-lift entry id', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryOneId = `entry-1-${Date.now()}`;
  const entryTwoId = `entry-2-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Duplicate Set Day',
          startedAtUtc: '2026-04-23T13:00:00Z',
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
            id: entryOneId,
            workoutId,
            liftId: `lift-dup-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-23T13:02:00Z',
            position: 1,
            sets: [],
          },
          {
            id: entryTwoId,
            workoutId,
            liftId: `lift-dup-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-23T13:03:00Z',
            position: 2,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/*/sets`, async (route) => {
    const url = new URL(route.request().url());
    const pathParts = url.pathname.split('/');
    const targetEntryId = pathParts[pathParts.length - 2];
    const request = route.request().postDataJSON() as { reps: number; weight: number | null };

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: targetEntryId,
        set: {
          id: `set-${targetEntryId}-${Date.now()}`,
          workoutLiftEntryId: targetEntryId,
          setNumber: 1,
          reps: request.reps,
          weight: request.weight,
          createdAtUtc: '2026-04-23T13:05:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const firstEntry = page.locator('.active-workout__lift-list-item').nth(0);
  const secondEntry = page.locator('.active-workout__lift-list-item').nth(1);

  await firstEntry.getByLabel(/Reps for Pull Up entry #1/).fill('7');
  await firstEntry.getByRole('button', { name: /Add set for Pull Up entry #1/i }).click();

  await expect(firstEntry.locator('.active-workout__set-list-item')).toHaveCount(1);
  await expect(secondEntry.locator('.active-workout__set-list-item')).toHaveCount(0);
  await expect(secondEntry.getByText('No sets yet.')).toBeVisible();

  await secondEntry.getByLabel(/Reps for Pull Up entry #2/).fill('9');
  await secondEntry.getByRole('button', { name: /Add set for Pull Up entry #2/i }).click();

  await expect(firstEntry.locator('.active-workout__set-list-item')).toHaveCount(1);
  await expect(secondEntry.locator('.active-workout__set-list-item')).toHaveCount(1);
  await expect(firstEntry.getByText('7 reps')).toBeVisible();
  await expect(secondEntry.getByText('9 reps')).toBeVisible();
});
