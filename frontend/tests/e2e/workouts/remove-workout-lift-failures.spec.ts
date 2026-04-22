import { expect, test } from '@playwright/test';

test('404 remove failure shows stale-entry feedback and keeps item', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T13:20:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: entryId,
            workoutId,
            liftId: `lift-${Date.now()}`,
            displayName: 'Deadlift',
            addedAtUtc: '2026-04-22T13:21:00Z',
            position: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}`, async (route) => {
    await route.fulfill({
      status: 404,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Not Found',
        status: 404,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByRole('button', { name: 'Remove Deadlift entry #1' }).click();

  await expect(page.getByText('This lift entry was already removed or no longer exists. Refresh and try again.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
});

test('409 remove failure shows conflict feedback and keeps item', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T13:30:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: entryId,
            workoutId,
            liftId: `lift-${Date.now()}`,
            displayName: 'Incline Press',
            addedAtUtc: '2026-04-22T13:31:00Z',
            position: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}`, async (route) => {
    await route.fulfill({
      status: 409,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Workout is already completed.',
        status: 409,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByRole('button', { name: 'Remove Incline Press entry #1' }).click();

  await expect(page.getByText('Workout is already completed.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
});

test('500 remove failure shows default error and keeps item', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T13:40:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: entryId,
            workoutId,
            liftId: `lift-${Date.now()}`,
            displayName: 'Front Squat',
            addedAtUtc: '2026-04-22T13:41:00Z',
            position: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}`, async (route) => {
    await route.fulfill({
      status: 500,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Internal Server Error',
        status: 500,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByRole('button', { name: 'Remove Front Squat entry #1' }).click();

  await expect(page.getByText('Lift was not removed. Check your connection and try again.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-list-item')).toHaveCount(1);
});
