import { expect, test } from '@playwright/test';

test('failed save keeps row draft, shows error, and supports retry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  const setId = `set-${Date.now()}`;
  let updateAttempt = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Failure Edit Day',
          startedAtUtc: '2026-04-23T17:20:00Z',
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
            id: entryId,
            workoutId,
            liftId: `lift-${Date.now()}`,
            displayName: 'Row',
            addedAtUtc: '2026-04-23T17:21:00Z',
            position: 1,
            sets: [
              {
                id: setId,
                workoutLiftEntryId: entryId,
                setNumber: 1,
                reps: 8,
                weight: 95,
                createdAtUtc: '2026-04-23T17:22:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets/${setId}`, async (route) => {
    updateAttempt += 1;
    if (updateAttempt === 1) {
      await route.fulfill({
        status: 500,
        contentType: 'application/problem+json',
        body: JSON.stringify({
          title: 'Internal Server Error',
          status: 500,
        }),
      });
      return;
    }

    const request = route.request().postDataJSON() as { reps: number; weight: number | null };
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryId,
        set: {
          id: setId,
          workoutLiftEntryId: entryId,
          setNumber: 1,
          reps: request.reps,
          weight: request.weight,
          updatedAtUtc: '2026-04-23T17:25:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByRole('button', { name: /Edit set 1 for Row entry #1/i }).click();
  await entryCard.getByLabel(/Edit reps for set 1 of Row entry #1/i).fill('11');
  await entryCard.getByLabel(/Edit weight for set 1 of Row entry #1/i).fill('105');
  await entryCard.getByRole('button', { name: /Save set 1 for Row entry #1/i }).click();

  await expect(entryCard.getByText('Set changes were not saved. Check your connection and retry.')).toBeVisible();
  await expect(entryCard.getByLabel(/Edit reps for set 1 of Row entry #1/i)).toHaveValue('11');
  await expect(entryCard.getByLabel(/Edit weight for set 1 of Row entry #1/i)).toHaveValue('105');
  await expect(entryCard.getByRole('button', { name: /Save set 1 for Row entry #1/i })).toHaveText('Retry Save');

  await entryCard.getByRole('button', { name: /Save set 1 for Row entry #1/i }).click();
  await expect(entryCard.getByText('11 reps')).toBeVisible();
  await expect(entryCard.getByText('105 lb')).toBeVisible();
  await expect.poll(() => updateAttempt).toBe(2);
});
