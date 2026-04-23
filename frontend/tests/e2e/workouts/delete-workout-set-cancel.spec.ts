import { expect, test } from '@playwright/test';

test('cancel delete keeps set rows unchanged', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  const setId = `set-${Date.now()}`;
  let deleteRequests = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Cancel Delete Day',
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
            displayName: 'Row',
            addedAtUtc: '2026-04-23T17:01:00Z',
            position: 1,
            sets: [
              {
                id: setId,
                workoutLiftEntryId: entryId,
                setNumber: 1,
                reps: 12,
                weight: 95,
                createdAtUtc: '2026-04-23T17:05:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets/${setId}`, async (route) => {
    deleteRequests += 1;
    await route.fulfill({ status: 500, body: '{}' });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByRole('button', { name: /Delete set 1 for Row entry #1/i }).click();
  await expect(entryCard.getByText('Delete this set row?')).toBeVisible();
  await entryCard.getByRole('button', { name: /Cancel delete for set 1 of Row entry #1/i }).click();

  await expect(entryCard.getByText('Set 1')).toBeVisible();
  await expect(entryCard.getByText('12 reps')).toBeVisible();
  await expect.poll(() => deleteRequests).toBe(0);
});
