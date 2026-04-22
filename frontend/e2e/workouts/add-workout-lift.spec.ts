import { expect, test } from '@playwright/test';

test('add workout lift happy path appends entry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const liftId = `lift-${Date.now()}`;
  const liftName = 'Bench Press';

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T12:00:00Z',
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

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutLift: {
          id: `entry-${Date.now()}`,
          workoutId,
          liftId,
          displayName: liftName,
          addedAtUtc: '2026-04-22T12:02:00Z',
          position: 1,
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

  await expect(page.getByText('No lifts added yet. Tap Add Lift to start building this workout.')).toBeVisible();
  await page.getByRole('button', { name: 'Add Lift' }).click();
  await page.getByRole('button', { name: 'Add selected lift' }).click();

  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
  await expect(page.locator('.active-workout__lift-name', { hasText: liftName })).toHaveCount(1);
});
