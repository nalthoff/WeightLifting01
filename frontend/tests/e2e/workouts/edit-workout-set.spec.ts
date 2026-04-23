import { expect, test } from '@playwright/test';

test('edit set happy path updates only targeted row inline', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  const setOneId = `set-1-${Date.now()}`;
  const setTwoId = `set-2-${Date.now()}`;
  const updateRequests: Array<{ reps: number; weight: number | null }> = [];

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Edit Day',
          startedAtUtc: '2026-04-23T17:00:00Z',
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
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-23T17:01:00Z',
            position: 1,
            sets: [
              {
                id: setOneId,
                workoutLiftEntryId: entryId,
                setNumber: 1,
                reps: 8,
                weight: 135,
                createdAtUtc: '2026-04-23T17:05:00Z',
              },
              {
                id: setTwoId,
                workoutLiftEntryId: entryId,
                setNumber: 2,
                reps: 10,
                weight: null,
                createdAtUtc: '2026-04-23T17:06:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets/${setOneId}`, async (route) => {
    const request = route.request().postDataJSON() as { reps: number; weight: number | null };
    updateRequests.push(request);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryId,
        set: {
          id: setOneId,
          workoutLiftEntryId: entryId,
          setNumber: 1,
          reps: request.reps,
          weight: request.weight,
          updatedAtUtc: '2026-04-23T17:08:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByRole('button', { name: /Edit set 1 for Bench Press entry #1/i }).click();
  await entryCard.getByLabel(/Edit reps for set 1 of Bench Press entry #1/i).fill('9');
  await entryCard.getByLabel(/Edit weight for set 1 of Bench Press entry #1/i).fill('140');
  await entryCard.getByRole('button', { name: /Save set 1 for Bench Press entry #1/i }).click();

  await expect(entryCard.getByText('Set 1')).toBeVisible();
  await expect(entryCard.getByText('9 reps')).toBeVisible();
  await expect(entryCard.getByText('140 lb')).toBeVisible();
  await expect(entryCard.getByText('Set 2')).toBeVisible();
  await expect(entryCard.getByText('10 reps')).toBeVisible();
  await expect(entryCard.getByText('-')).toBeVisible();
  expect(updateRequests).toEqual([{ reps: 9, weight: 140 }]);
});
