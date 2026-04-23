import { expect, test } from '@playwright/test';

test('failed completion from detail can be retried successfully', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const workoutLabel = `Retry ${Date.now()}`;
  let completeRequestCount = 0;
  let activeFetchCount = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: workoutLabel,
          startedAtUtc: '2026-04-23T15:00:00Z',
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
    activeFetchCount += 1;
    const stillActive = activeFetchCount === 1;
    if (!stillActive) {
      await route.fulfill({
        status: 204,
        contentType: 'application/json',
        body: '',
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
          label: workoutLabel,
          startedAtUtc: '2026-04-23T15:00:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    completeRequestCount += 1;
    if (completeRequestCount === 1) {
      await route.fulfill({
        status: 500,
        contentType: 'application/problem+json',
        body: JSON.stringify({
          title: 'Server unavailable',
          status: 500,
        }),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label: workoutLabel,
          startedAtUtc: '2026-04-23T15:00:00Z',
          completedAtUtc: '2026-04-23T15:30:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const completeButton = page.getByRole('button', { name: /complete workout/i });
  await completeButton.click();

  await expect(page.getByText('Unable to complete workout. Check your connection and try again.')).toBeVisible();
  await expect(completeButton).toBeVisible();

  await completeButton.click();

  await expect(page.getByText('Workout completed. Great work.')).toBeVisible();
  await expect(page.getByText('This workout is no longer active.')).toBeVisible();
  await expect.poll(() => completeRequestCount).toBe(2);
});
