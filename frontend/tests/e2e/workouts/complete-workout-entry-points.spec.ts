import { expect, test } from '@playwright/test';

test('complete from active workout detail clears active state with success feedback', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const workoutLabel = `Detail Complete ${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: workoutLabel,
          startedAtUtc: '2026-04-23T14:00:00Z',
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

  await page.route('**/api/workouts/active', async (route) => {
    await route.fulfill({
      status: 204,
      contentType: 'application/json',
      body: '',
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
          label: workoutLabel,
          startedAtUtc: '2026-04-23T14:00:00Z',
          completedAtUtc: '2026-04-23T14:45:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByRole('button', { name: /complete workout/i }).click();

  await expect(page.getByText('Workout completed. Great work.')).toBeVisible();
  await expect(page.getByText('This workout is no longer active.')).toBeVisible();
  await expect(page.getByRole('button', { name: /complete workout/i })).toHaveCount(0);
});
