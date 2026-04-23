import { expect, test } from '@playwright/test';

test('server failure shows explicit feedback and no ghost saved row', async ({ page }) => {
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
          startedAtUtc: '2026-04-23T14:00:00Z',
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
            addedAtUtc: '2026-04-23T14:02:00Z',
            position: 1,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets`, async (route) => {
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

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByLabel(/Reps for Deadlift entry #1/).fill('5');
  await entryCard.getByRole('button', { name: /Add set for Deadlift entry #1/i }).click();

  await expect(entryCard.getByText('Set was not saved. Check your connection and try again.')).toBeVisible();
  await expect(entryCard.locator('.active-workout__set-list-item')).toHaveCount(0);
  await expect(entryCard.getByText('No sets yet.')).toBeVisible();
});

test('conflict failure shows API title and keeps visible numbering consistent', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  let callCount = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Failure Day',
          startedAtUtc: '2026-04-23T14:20:00Z',
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
            displayName: 'Row',
            addedAtUtc: '2026-04-23T14:22:00Z',
            position: 1,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets`, async (route) => {
    callCount += 1;
    if (callCount === 1) {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          workoutId,
          workoutLiftEntryId: entryId,
          set: {
            id: `set-1-${Date.now()}`,
            workoutLiftEntryId: entryId,
            setNumber: 1,
            reps: 8,
            weight: null,
            createdAtUtc: '2026-04-23T14:23:00Z',
          },
        }),
      });
      return;
    }

    await route.fulfill({
      status: 409,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Workout is no longer in progress.',
        status: 409,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByLabel(/Reps for Row entry #1/).fill('8');
  await entryCard.getByRole('button', { name: /Add set for Row entry #1/i }).click();
  await expect(entryCard.locator('.active-workout__set-list-item')).toHaveCount(1);

  await entryCard.getByLabel(/Reps for Row entry #1/).fill('6');
  await entryCard.getByRole('button', { name: /Add set for Row entry #1/i }).click();

  await expect(entryCard.getByText('Workout is no longer in progress.')).toBeVisible();
  await expect(entryCard.locator('.active-workout__set-list-item')).toHaveCount(1);
  await expect(entryCard.getByText('Set 1')).toBeVisible();
});

test('in-flight guard prevents duplicate requests from repeated taps', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;
  let requestCount = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-23T14:30:00Z',
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
            displayName: 'Dip',
            addedAtUtc: '2026-04-23T14:31:00Z',
            position: 1,
            sets: [],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryId}/sets`, async (route) => {
    requestCount += 1;
    await page.waitForTimeout(120);

    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryId,
        set: {
          id: `set-${Date.now()}`,
          workoutLiftEntryId: entryId,
          setNumber: 1,
          reps: 12,
          weight: null,
          createdAtUtc: '2026-04-23T14:32:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const entryCard = page.locator('.active-workout__lift-list-item').first();
  await entryCard.getByLabel(/Reps for Dip entry #1/).fill('12');

  const addSetButton = entryCard.getByRole('button', { name: /Add set for Dip entry #1/i });
  await addSetButton.click();
  await addSetButton.click();

  await expect.poll(() => requestCount).toBe(1);
  await expect(entryCard.locator('.active-workout__set-list-item')).toHaveCount(1);
});
