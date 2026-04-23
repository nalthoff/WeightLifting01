import { expect, test } from '@playwright/test';

test('failed delete keeps row, shows error, and supports retry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  const setId = `set-${Date.now()}`;
  let deleteAttempt = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Delete Failure Day',
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
            displayName: 'Deadlift',
            addedAtUtc: '2026-04-23T17:21:00Z',
            position: 1,
            sets: [
              {
                id: setId,
                workoutLiftEntryId: entryId,
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-23T17:22:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets/${setId}`, async (route) => {
    deleteAttempt += 1;
    if (deleteAttempt === 1) {
      await route.fulfill({
        status: 500,
        contentType: 'application/problem+json',
        body: JSON.stringify({ title: 'Internal Server Error', status: 500 }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryId,
        setId,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByRole('button', { name: /Delete set 1 for Deadlift entry #1/i }).click();
  await entryCard.getByRole('button', { name: /Confirm delete for set 1 of Deadlift entry #1/i }).click();

  await expect(entryCard.getByText('Set was not removed. Check your connection and retry.')).toBeVisible();
  await expect(entryCard.getByText('Set 1')).toBeVisible();

  await entryCard.getByRole('button', { name: /Confirm delete for set 1 of Deadlift entry #1/i }).click();
  await expect(entryCard.getByText('Set 1')).toHaveCount(0);
  await expect.poll(() => deleteAttempt).toBe(2);
});
