import { expect, test } from '@playwright/test';

test('remove selected duplicate entry only', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const duplicateEntryOneId = `entry-duplicate-1-${Date.now()}`;
  const duplicateEntryTwoId = `entry-duplicate-2-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Duplicate Remove Day',
          startedAtUtc: '2026-04-22T13:10:00Z',
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
            id: duplicateEntryOneId,
            workoutId,
            liftId: `lift-dup-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T13:11:00Z',
            position: 1,
          },
          {
            id: duplicateEntryTwoId,
            workoutId,
            liftId: `lift-dup-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T13:12:00Z',
            position: 2,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${duplicateEntryTwoId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        removedWorkoutLiftEntryId: duplicateEntryTwoId,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(2);
  await page.getByRole('button', { name: 'Remove Pull Up entry #2' }).click();

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
  await expect(page.locator('.active-workout__lift-name', { hasText: 'Pull Up' })).toHaveCount(1);
  await expect(page.getByText('#1 added', { exact: false })).toBeVisible();
});
