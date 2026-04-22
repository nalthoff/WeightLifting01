import { expect, test, type Page } from '@playwright/test';

type WorkoutSummary = {
  id: string;
  status: 'InProgress' | 'Completed';
  label: string | null;
  startedAtUtc: string;
  completedAtUtc?: string | null;
};

const activeWorkoutPath = '**/api/workouts/active';

async function stubActiveWorkout(page: Page, workout: WorkoutSummary): Promise<void> {
  await page.route(activeWorkoutPath, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout }),
    });
  });
}

async function stubNoActiveWorkout(page: Page): Promise<void> {
  await page.route(activeWorkoutPath, async (route) => {
    await route.fulfill({
      status: 204,
      contentType: 'application/json',
      body: '',
    });
  });
}

async function stubWorkoutDetailApis(page: Page, workout: WorkoutSummary): Promise<void> {
  await page.route(`**/api/workouts/${workout.id}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout }),
    });
  });

  await page.route(`**/api/workouts/${workout.id}/lifts`, async (route) => {
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
      body: JSON.stringify({ items: [] }),
    });
  });
}

test('home active-workout card is absent when no workout is in progress', async ({ page }) => {
  await stubNoActiveWorkout(page);

  await page.goto('/');

  await expect(page.getByRole('button', { name: /continue/i })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /complete/i })).toHaveCount(0);
});

test('continue from home active-workout card navigates to workout detail', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const label = `Session ${Date.now()}`;
  const workout: WorkoutSummary = {
    id: workoutId,
    status: 'InProgress',
    label,
    startedAtUtc: '2026-04-22T13:00:00Z',
  };

  await stubActiveWorkout(page, workout);
  await stubWorkoutDetailApis(page, workout);

  await page.goto('/');

  await expect(page.getByText(label, { exact: true })).toBeVisible();
  await page.getByRole('button', { name: /continue/i }).click();

  await expect(page).toHaveURL(new RegExp(`/workouts/${workoutId}$`));
  await expect(page.locator('mat-card-title')).toHaveText(label);
  await expect(page.getByText('Status: InProgress')).toBeVisible();
});

test('complete from home removes card, keeps user on home, and shows success feedback', async ({
  page,
}) => {
  const workoutId = `workout-${Date.now()}`;
  const label = `Finishable ${Date.now()}`;
  const startedAtUtc = '2026-04-22T13:05:00Z';
  const activeWorkout: WorkoutSummary = {
    id: workoutId,
    status: 'InProgress',
    label,
    startedAtUtc,
  };
  const completedWorkout: WorkoutSummary = {
    ...activeWorkout,
    status: 'Completed',
    completedAtUtc: '2026-04-22T13:35:00Z',
  };
  let activeFetchCount = 0;

  await page.route(activeWorkoutPath, async (route) => {
    activeFetchCount += 1;
    const responseWorkout = activeFetchCount > 1 ? null : activeWorkout;

    if (!responseWorkout) {
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
      body: JSON.stringify({ workout: responseWorkout }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ workout: completedWorkout }),
    });
  });

  await page.goto('/');

  await page.getByRole('button', { name: /complete/i }).click();

  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByRole('button', { name: /continue/i })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /complete/i })).toHaveCount(0);
  await expect(page.getByText(/completed|success/i)).toBeVisible();
});

test('failed home completion shows explicit error and keeps active workout available', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const label = `Failure ${Date.now()}`;
  const workout: WorkoutSummary = {
    id: workoutId,
    status: 'InProgress',
    label,
    startedAtUtc: '2026-04-22T13:10:00Z',
  };

  await stubActiveWorkout(page, workout);

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    await route.fulfill({
      status: 500,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Server error',
        status: 500,
      }),
    });
  });

  await page.goto('/');

  await page.getByRole('button', { name: /complete/i }).click();

  await expect(page.getByText(label, { exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: /continue/i })).toBeVisible();
  await expect(page.getByText('Unable to complete workout.')).toBeVisible();
});

test('rapid complete taps do not create an ambiguous final state', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const workout: WorkoutSummary = {
    id: workoutId,
    status: 'InProgress',
    label: `Rapid Tap ${Date.now()}`,
    startedAtUtc: '2026-04-22T13:15:00Z',
  };
  let completeRequestCount = 0;
  let activeFetchCount = 0;

  await page.route(activeWorkoutPath, async (route) => {
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
      body: JSON.stringify({ workout }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/complete`, async (route) => {
    completeRequestCount += 1;
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          ...workout,
          status: 'Completed',
          completedAtUtc: '2026-04-22T13:40:00Z',
        },
      }),
    });
  });

  await page.goto('/');

  const completeButton = page.getByRole('button', { name: /complete/i });
  await Promise.allSettled([
    completeButton.click(),
    completeButton.click({ timeout: 1_000 }),
    completeButton.click({ timeout: 1_000 }),
  ]);

  await expect(page.getByRole('button', { name: /continue/i })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /complete/i })).toHaveCount(0);
  await expect(page.getByText(/completed|success/i)).toBeVisible();
  expect(completeRequestCount).toBe(1);
});
