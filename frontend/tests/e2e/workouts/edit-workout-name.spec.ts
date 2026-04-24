import { expect, test } from '@playwright/test';

test('user can save and edit workout name while in progress', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const updatedLabels: string[] = [];

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Session A',
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
    const body = route.request().postDataJSON() as { label?: string };
    updatedLabels.push(body.label ?? '');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: body.label?.trim() || null,
          startedAtUtc: '2026-04-23T17:00:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-rename').click();
  await page.getByTestId('active-workout-rename-input').fill('Upper A');
  await page.getByTestId('active-workout-rename-confirm').click();
  await expect(page.getByText('Workout name saved.')).toBeVisible();
  await expect(page.locator('mat-card-title', { hasText: 'Upper A' })).toBeVisible();

  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-rename').click();
  await page.getByTestId('active-workout-rename-input').fill('Upper B');
  await page.getByTestId('active-workout-rename-confirm').click();
  await expect(page.locator('mat-card-title', { hasText: 'Upper B' })).toBeVisible();
  expect(updatedLabels).toEqual(['Upper A', 'Upper B']);
});
