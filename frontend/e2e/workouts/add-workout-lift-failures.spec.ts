import { expect, test } from '@playwright/test';

test('picker empty state shows guidance and add button is hidden', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T12:20:00Z',
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

  await page.route('**/api/lifts?activeOnly=true', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [],
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page.getByRole('button', { name: 'Add Lift' }).click();

  await expect(
    page.getByText('No active lifts available. Add or activate lifts in Settings before adding to this workout.'),
  ).toBeVisible();
  await expect(page.getByRole('button', { name: 'Add selected lift' })).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Go to Settings' })).toBeVisible();
});

test('failed add shows error and does not create ghost entry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const liftId = `lift-${Date.now()}`;
  const liftName = 'Overhead Press';

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T12:25:00Z',
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
      status: 500,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Internal Server Error',
        status: 500,
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

  await expect(page.getByText('Lift was not added. Check your connection and try again.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(0);
  await expect(page.getByText('No lifts added yet. Tap Add Lift to start building this workout.')).toBeVisible();
});
