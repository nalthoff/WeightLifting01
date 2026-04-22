import { expect, test } from '@playwright/test';

test('start workout from home navigates to active workout', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const startedAtUtc = '2026-04-22T12:00:00Z';

  await page.route('**/api/workouts', async (route) => {
    if (route.request().method() !== 'POST') {
      await route.continue();
      return;
    }

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc,
        },
      }),
    });
  });

  await page.goto('/');

  await page.getByRole('button', { name: 'Start Workout' }).click();

  await expect(page).toHaveURL(new RegExp(`/workouts/${workoutId}$`));
  await expect(page.locator('mat-card-title')).toHaveText('Workout');
  await expect(page.getByText('Status: InProgress')).toBeVisible();
  await expect(page.getByText('Started:')).toBeVisible();
});
