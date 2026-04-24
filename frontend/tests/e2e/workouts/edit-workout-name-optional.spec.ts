import { expect, test } from '@playwright/test';

test('blank workout name clears label without blocking completion flow', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Named',
          startedAtUtc: '2026-04-23T17:00:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/label`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-23T17:00:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label: null,
          startedAtUtc: '2026-04-23T17:00:00Z',
          completedAtUtc: '2026-04-23T18:00:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-rename').click();
  await page.getByTestId('active-workout-rename-input').fill('   ');
  await page.getByTestId('active-workout-rename-confirm').click();
  await expect(page.getByText('Workout name cleared.')).toBeVisible();
  await expect(page.locator('mat-card-title', { hasText: 'Workout' })).toBeVisible();

  await page.getByTestId('active-workout-complete').click();
  await expect(page.getByText('Workout completed. Great work.')).toBeVisible();
});
