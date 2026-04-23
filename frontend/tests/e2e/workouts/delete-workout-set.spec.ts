import { expect, test } from '@playwright/test';

test('delete set happy path confirms and removes only targeted row inline', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  const setOneId = `set-1-${Date.now()}`;
  const setTwoId = `set-2-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Delete Day',
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
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryId,
        setId: setOneId,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByRole('button', { name: /Delete set 1 for Bench Press entry #1/i }).click();
  await expect(entryCard.getByText('Delete this set row?')).toBeVisible();
  await entryCard.getByRole('button', { name: /Confirm delete for set 1 of Bench Press entry #1/i }).click();

  await expect(entryCard.getByText('Set 1')).toHaveCount(0);
  await expect(entryCard.getByText('Set 2')).toBeVisible();
  await expect(entryCard.getByText('10 reps')).toBeVisible();
});
