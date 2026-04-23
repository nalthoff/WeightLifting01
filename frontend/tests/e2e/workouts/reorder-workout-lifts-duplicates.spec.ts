import { expect, test } from '@playwright/test';

test('reordering duplicate entries preserves identity and count', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const duplicateOneId = `entry-duplicate-1-${Date.now()}`;
  const duplicateTwoId = `entry-duplicate-2-${Date.now()}`;
  const thirdEntryId = `entry-third-${Date.now()}`;
  const reorderedIds: string[][] = [];

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Duplicate Reorder Day',
          startedAtUtc: '2026-04-22T14:10:00Z',
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
            id: duplicateOneId,
            workoutId,
            liftId: `lift-duplicate-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T14:11:00Z',
            position: 1,
          },
          {
            id: duplicateTwoId,
            workoutId,
            liftId: `lift-duplicate-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T14:12:00Z',
            position: 2,
          },
          {
            id: thirdEntryId,
            workoutId,
            liftId: `lift-other-${Date.now()}`,
            displayName: 'Overhead Press',
            addedAtUtc: '2026-04-22T14:13:00Z',
            position: 3,
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
            id: duplicateTwoId,
            workoutId,
            liftId: `lift-duplicate-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T14:12:00Z',
            position: 1,
          },
          {
            id: duplicateOneId,
            workoutId,
            liftId: `lift-duplicate-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-22T14:11:00Z',
            position: 2,
          },
          {
            id: thirdEntryId,
            workoutId,
            liftId: `lift-other-${Date.now()}`,
            displayName: 'Overhead Press',
            addedAtUtc: '2026-04-22T14:13:00Z',
            position: 3,
          },
        ],
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByRole('button', { name: 'Move Pull Up entry #2 up' }).click();

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(3);
  await expect(page.locator('.active-workout__lift-name', { hasText: 'Pull Up' })).toHaveCount(2);
  await expect(page.locator('.active-workout__lift-name').nth(0)).toHaveText('Pull Up');
  await expect(page.locator('.active-workout__lift-name').nth(1)).toHaveText('Pull Up');
  await expect.poll(() => reorderedIds.length).toBe(1);
  expect(reorderedIds[0]).toEqual([duplicateTwoId, duplicateOneId, thirdEntryId]);
});
