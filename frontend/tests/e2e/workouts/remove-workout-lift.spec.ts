import { expect, test } from '@playwright/test';

test('remove workout lift happy path removes selected entry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const removableEntryId = `entry-removable-${Date.now()}`;
  const keepEntryId = `entry-keep-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Remove Day',
          startedAtUtc: '2026-04-22T13:00:00Z',
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
            id: removableEntryId,
            workoutId,
            liftId: `lift-1-${Date.now()}`,
            displayName: 'Bench Press',
            addedAtUtc: '2026-04-22T13:01:00Z',
            position: 1,
          },
          {
            id: keepEntryId,
            workoutId,
            liftId: `lift-2-${Date.now()}`,
            displayName: 'Squat',
            addedAtUtc: '2026-04-22T13:02:00Z',
            position: 2,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${removableEntryId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        removedWorkoutLiftEntryId: removableEntryId,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(2);
  await page.getByRole('button', { name: 'Remove Bench Press entry #1' }).click();

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
  await expect(page.locator('.active-workout__lift-name', { hasText: 'Bench Press' })).toHaveCount(0);
  await expect(page.locator('.active-workout__lift-name', { hasText: 'Squat' })).toHaveCount(1);
});
