import { expect, test } from '@playwright/test';

test('adding the same lift twice keeps duplicate entries', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const liftId = `lift-${Date.now()}`;
  const liftName = 'Pull Up';
  let addCount = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Duplicate Day',
          startedAtUtc: '2026-04-22T12:10:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: [] }),
      });
      return;
    }

    addCount += 1;
    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutLift: {
          id: `entry-${addCount}-${Date.now()}`,
          workoutId,
          liftId,
          displayName: liftName,
          addedAtUtc: `2026-04-22T12:1${addCount}:00Z`,
          position: addCount,
        },
      }),
    });
  });

  await page.route('**/api/lifts?activeOnly=true', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [{ id: liftId, name: liftName, isActive: true }],
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByRole('button', { name: 'Add Lift' }).click();
  await page.getByRole('button', { name: 'Add selected lift' }).click();

  await page.getByRole('button', { name: 'Add selected lift' }).click();

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(2);
  await expect(page.locator('.active-workout__lift-name', { hasText: liftName })).toHaveCount(2);
});
