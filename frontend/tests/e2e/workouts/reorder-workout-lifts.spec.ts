import { expect, test } from '@playwright/test';

test('reorder workout lifts happy path updates visible order', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryOneId = `entry-1-${Date.now()}`;
  const entryTwoId = `entry-2-${Date.now()}`;
  const entryThreeId = `entry-3-${Date.now()}`;
  const reorderedIds: string[][] = [];

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Reorder Day',
          startedAtUtc: '2026-04-22T14:00:00Z',
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
            liftId: `lift-1-${Date.now()}`,
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-22T14:01:00Z',
            position: 1,
            sets: [
              {
                id: `set-bench-${Date.now()}`,
                workoutLiftEntryId: entryOneId,
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-22T14:04:00Z',
              },
            ],
          },
          {
            id: entryTwoId,
            workoutId,
            liftId: `lift-2-${Date.now()}`,
            displayName: 'Squat',
            addedAtUtc: '2026-04-22T14:02:00Z',
            position: 2,
            sets: [],
          },
          {
            id: entryThreeId,
            workoutId,
            liftId: `lift-3-${Date.now()}`,
            displayName: 'Deadlift',
            addedAtUtc: '2026-04-22T14:03:00Z',
            position: 3,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/reorder`, async (route) => {
    const request = route.request().postDataJSON() as { orderedWorkoutLiftEntryIds: string[] };
    reorderedIds.push(request.orderedWorkoutLiftEntryIds);

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        items: [
          {
            id: entryTwoId,
            workoutId,
            liftId: `lift-2-${Date.now()}`,
            displayName: 'Squat',
            addedAtUtc: '2026-04-22T14:02:00Z',
            position: 1,
            sets: [],
          },
          {
            id: entryOneId,
            workoutId,
            liftId: `lift-1-${Date.now()}`,
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-22T14:01:00Z',
            position: 2,
            sets: [
              {
                id: `set-bench-${Date.now()}`,
                workoutLiftEntryId: entryOneId,
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-22T14:04:00Z',
              },
            ],
          },
          {
            id: entryThreeId,
            workoutId,
            liftId: `lift-3-${Date.now()}`,
            displayName: 'Deadlift',
            addedAtUtc: '2026-04-22T14:03:00Z',
            position: 3,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByRole('button', { name: 'Move Bench Press entry #1 down' }).click();

  await expect(page.locator('.active-workout__lift-name').nth(0)).toHaveText('Squat');
  await expect(page.locator('.active-workout__lift-name').nth(1)).toHaveText('Bench Press');
  await expect(page.locator('.active-workout__lift-name').nth(2)).toHaveText('Deadlift');
  const reorderedBenchEntry = page.locator('.active-workout__lift-list-item').nth(1);
  await expect(reorderedBenchEntry.getByText('5 reps')).toBeVisible();
  await expect.poll(() => reorderedIds.length).toBe(1);
  expect(reorderedIds[0]).toEqual([entryTwoId, entryOneId, entryThreeId]);
});
