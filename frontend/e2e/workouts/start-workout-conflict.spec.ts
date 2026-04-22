import { expect, test } from '@playwright/test';

test('conflict prompts continue-existing workout and navigates to existing session', async ({ page }) => {
  const existingLabel = `Existing Session ${Date.now()}`;
  const existingWorkoutId = `workout-existing-${Date.now()}`;
  const startedAtUtc = '2026-04-22T12:10:00Z';

  await page.route('**/api/workouts', async (route) => {
    if (route.request().method() !== 'POST') {
      await route.continue();
      return;
    }

    await route.fulfill({
      status: 409,
      contentType: 'application/json',
      body: JSON.stringify({
        title: 'Workout already in progress',
        status: 409,
        workout: {
          id: existingWorkoutId,
          status: 'InProgress',
          label: existingLabel,
          startedAtUtc,
        },
      }),
    });
  });

  await page.goto('/');

  await page.getByRole('button', { name: 'Start Workout' }).click();

  await expect(page.getByText('You already have a workout in progress. Continue it?')).toBeVisible();
  await page.getByRole('button', { name: 'Continue workout' }).click();

  await expect(page).toHaveURL(new RegExp(`/workouts/${existingWorkoutId}$`));
  await expect(page.getByText(`Label: ${existingLabel}`)).toBeVisible();
  await expect(page.getByText('Status: InProgress')).toBeVisible();
});
