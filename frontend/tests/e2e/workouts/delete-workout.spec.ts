import { expect, test } from '@playwright/test';

test('confirmed delete removes active workout and shows success guidance', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  let deleteRequests = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'DELETE') {
      deleteRequests += 1;
      await new Promise((resolve) => setTimeout(resolve, 75));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ workoutId }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Delete Workout Happy Path',
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

  await page.goto(`/workouts/${workoutId}`);

  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  const confirmButton = page.getByTestId('active-workout-delete-confirm');
  await confirmButton.dblclick();

  await expect(page).toHaveURL(/\/$/);
  await expect.poll(() => deleteRequests).toBe(1);
});
