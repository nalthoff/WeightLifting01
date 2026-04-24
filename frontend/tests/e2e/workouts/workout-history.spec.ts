import { expect, test } from '@playwright/test';

type WorkoutSummary = {
  id: string;
  status: 'InProgress' | 'Completed';
  label: string | null;
  startedAtUtc: string;
  completedAtUtc?: string | null;
};

test('completing from home then opening history shows the completed workout', async ({ page }) => {
  const workoutId = `history-workout-${Date.now()}`;
  const label = `History Session ${Date.now()}`;
  const startedAtUtc = '2026-04-23T14:00:00Z';
  const completedAtUtc = '2026-04-23T14:45:00Z';

  const activeWorkout: WorkoutSummary = {
    id: workoutId,
    status: 'InProgress',
    label,
    startedAtUtc,
  };
  const completedWorkout: WorkoutSummary = {
    ...activeWorkout,
    status: 'Completed',
    completedAtUtc,
  };

  let activeFetchCount = 0;
  await page.route('**/api/workouts/active', async (route) => {
    activeFetchCount += 1;
    if (activeFetchCount > 1) {
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
      body: JSON.stringify({ workout: activeWorkout }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout: completedWorkout }),
    });
  });

  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: workoutId,
            label,
            completedAtUtc,
            durationDisplay: '00:45',
            liftCount: 3,
          },
        ],
      }),
    });
  });

  await page.goto('/');
  await page.getByRole('button', { name: /complete/i }).click();
  await expect(page.getByText(/Workout completed\. Great work\./i)).toBeVisible();

  const desktopHistoryLink = page.getByTestId('history-nav-link');
  if (await desktopHistoryLink.isVisible()) {
    await desktopHistoryLink.click();
  } else {
    await page.getByRole('button', { name: 'Open navigation menu' }).click();
    await page.getByTestId('history-nav-link-mobile').click();
  }
  await expect(page).toHaveURL(/\/history$/);
  await expect(page.getByText(label, { exact: true })).toBeVisible();
  await expect(page.getByTestId('history-item-date')).toContainText('Completed');
  await expect(page.getByTestId('history-item-duration')).toContainText('Duration 00:45');
  await expect(page.getByTestId('history-item-lift-count')).toContainText('3 lifts');
});

test('history page shows empty state when no completed workouts exist', async ({ page }) => {
  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [] }),
    });
  });

  await page.goto('/history');
  await expect(page).toHaveURL(/\/history$/);
  await expect(page.getByText('No completed workouts yet.')).toBeVisible();
});

test('history page shows error feedback when loading fails', async ({ page }) => {
  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: JSON.stringify({ title: 'Server error' }),
    });
  });

  await page.goto('/history');
  await expect(page).toHaveURL(/\/history$/);
  await expect(
    page.getByText('Unable to load workout history right now. Check your connection and try again.'),
  ).toBeVisible();
});
