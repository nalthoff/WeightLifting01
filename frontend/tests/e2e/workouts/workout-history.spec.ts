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

test('opening a history row shows completed workout detail with lifts and sets', async ({ page }) => {
  const workoutId = `history-detail-${Date.now()}`;

  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: workoutId,
            label: 'Detail Session',
            completedAtUtc: '2026-04-24T16:15:00Z',
            durationDisplay: '01:15',
            liftCount: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label: 'Detail Session',
          startedAtUtc: '2026-04-24T15:00:00Z',
          completedAtUtc: '2026-04-24T16:15:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: 'entry-1',
            workoutId,
            liftId: 'lift-1',
            displayName: 'Squat',
            addedAtUtc: '2026-04-24T15:05:00Z',
            position: 0,
            sets: [
              {
                id: 'set-1',
                workoutLiftEntryId: 'entry-1',
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-24T15:07:00Z',
                updatedAtUtc: '2026-04-24T15:07:00Z',
              },
              {
                id: 'set-2',
                workoutLiftEntryId: 'entry-1',
                setNumber: 2,
                reps: 5,
                weight: null,
                createdAtUtc: '2026-04-24T15:11:00Z',
                updatedAtUtc: '2026-04-24T15:11:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.goto('/history');
  await page.getByTestId(`history-item-link-${workoutId}`).click();

  await expect(page).toHaveURL(new RegExp(`/history/${workoutId}$`));
  await expect(page.getByText('Detail Session')).toBeVisible();
  await expect(page.getByTestId('history-workout-detail-duration')).toContainText('Duration 01:15');
  await expect(page.getByText('Squat')).toBeVisible();
  await expect(page.getByText('225 lb')).toBeVisible();
  await expect(page.getByRole('cell', { name: '-' })).toBeVisible();
});

test('history detail shows actionable feedback when selected workout is unavailable', async ({ page }) => {
  const workoutId = `history-missing-${Date.now()}`;

  await page.route('**/api/workouts/history', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: [
          {
            id: workoutId,
            label: 'Missing Session',
            completedAtUtc: '2026-04-24T17:15:00Z',
            durationDisplay: '00:30',
            liftCount: 1,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 404,
      contentType: 'application/json',
      body: JSON.stringify({ title: 'Workout not found', status: 404 }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts?forHistory=true`, async (route) => {
    await route.fulfill({
      status: 404,
      contentType: 'application/json',
      body: JSON.stringify({ title: 'Workout not found', status: 404 }),
    });
  });

  await page.goto('/history');
  await page.getByTestId(`history-item-link-${workoutId}`).click();

  await expect(page.getByText('This completed workout is no longer available')).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to history' })).toBeVisible();
});
