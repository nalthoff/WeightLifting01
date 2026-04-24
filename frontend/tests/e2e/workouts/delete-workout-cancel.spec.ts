import { expect, test } from '@playwright/test';

test('cancel delete keeps active workout unchanged and does not call API', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  let deleteRequests = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Cancel Delete Workout',
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

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'DELETE') {
      deleteRequests += 1;
      await route.fulfill({ status: 500, contentType: 'application/json', body: '{}' });
      return;
    }

    await route.fallback();
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  await expect(page.getByRole('heading', { name: 'Delete workout?' })).toBeVisible();
  await page.getByTestId('active-workout-delete-cancel').click();

  await expect(page.getByTestId('active-workout-menu')).toBeVisible();
  await expect(page.getByText('Cancel Delete Workout')).toBeVisible();
  await expect.poll(() => deleteRequests).toBe(0);
});
