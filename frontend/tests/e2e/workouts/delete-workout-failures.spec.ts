import { expect, test, type Page } from '@playwright/test';

async function arrangeWorkoutPage(page: Page, workoutId: string): Promise<void> {
  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          workout: {
            id: workoutId,
            status: 'InProgress',
            label: 'Delete Failure Workout',
            startedAtUtc: '2026-04-23T17:00:00Z',
          },
        }),
      });
      return;
    }

    await route.fallback();
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });
}

test('delete workout 404 shows clear feedback and reconciles state', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  await arrangeWorkoutPage(page, workoutId);

  await page.route('**/api/workouts/active', async (route) => {
    await route.fulfill({ status: 204, contentType: 'application/json', body: '' });
  });

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'DELETE') {
      await route.fulfill({
        status: 404,
        contentType: 'application/problem+json',
        body: JSON.stringify({
          title: 'Workout not found.',
          status: 404,
        }),
      });
      return;
    }

    await route.fallback();
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  await page.getByTestId('active-workout-delete-confirm').click();

  await expect(page.getByText('Workout not found.')).toBeVisible();
  await expect(page.getByText('This workout is no longer active. Return home to start another session.')).toBeVisible();
});

test('delete workout 409 shows conflict feedback and keeps workout', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  await arrangeWorkoutPage(page, workoutId);

  await page.route('**/api/workouts/active', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label: 'Delete Failure Workout',
          startedAtUtc: '2026-04-23T17:00:00Z',
          completedAtUtc: '2026-04-23T17:25:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'DELETE') {
      await route.fulfill({
        status: 409,
        contentType: 'application/problem+json',
        body: JSON.stringify({
          title: 'Only in-progress workouts can be deleted.',
          status: 409,
        }),
      });
      return;
    }

    await route.fallback();
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  await page.getByTestId('active-workout-delete-confirm').click();

  await expect(page.getByText('Only in-progress workouts can be deleted.')).toBeVisible();
  await expect(page.getByText('This workout is no longer active. Return home to start another session.')).toBeVisible();
});

test('delete workout general failure allows retry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  await arrangeWorkoutPage(page, workoutId);

  let deleteAttempts = 0;
  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    if (route.request().method() === 'DELETE') {
      deleteAttempts += 1;
      if (deleteAttempts === 1) {
        await route.fulfill({
          status: 500,
          contentType: 'application/problem+json',
          body: JSON.stringify({ title: 'Server unavailable', status: 500 }),
        });
        return;
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ workoutId }),
      });
      return;
    }

    await route.fallback();
  });

  await page.route('**/api/workouts/active', async (route) => {
    await route.fulfill({ status: 204, contentType: 'application/json', body: '' });
  });

  await page.goto(`/workouts/${workoutId}`);
  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  await page.getByTestId('active-workout-delete-confirm').click();

  await expect(page.getByText('Unable to delete workout. Check your connection and try again.')).toBeVisible();
  await page.getByTestId('active-workout-menu').click();
  await page.getByTestId('active-workout-delete').click();
  await page.getByTestId('active-workout-delete-confirm').click();

  await expect(page).toHaveURL(/\/$/);
  await expect.poll(() => deleteAttempts).toBe(2);
});
