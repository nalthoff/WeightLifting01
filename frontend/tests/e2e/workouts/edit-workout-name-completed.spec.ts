import { expect, test } from '@playwright/test';

test('completed workout cannot be renamed and keeps fallback heading', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
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

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await expect(page.locator('mat-card-title', { hasText: 'Workout' })).toBeVisible();
  await expect(page.getByTestId('active-workout-menu')).toBeDisabled();
});
