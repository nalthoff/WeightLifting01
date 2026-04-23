import { expect, test } from '@playwright/test';

test('non in-progress workout does not render set edit controls', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryId = `entry-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'Completed',
          label: 'Locked Workout',
          startedAtUtc: '2026-04-23T18:00:00Z',
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
            addedAtUtc: '2026-04-23T18:01:00Z',
            position: 1,
            sets: [
              {
                id: `set-${Date.now()}`,
                workoutLiftEntryId: entryId,
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-23T18:02:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);
  await expect(page.getByRole('button', { name: /Edit set 1 for Deadlift entry #1/i })).toHaveCount(0);
});

test('editing duplicate lift entries updates only targeted workout-lift entry', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryOneId = `entry-1-${Date.now()}`;
  const entryTwoId = `entry-2-${Date.now()}`;
  const setOneId = `set-1-${Date.now()}`;
  const setTwoId = `set-2-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Duplicate Entry Edit Day',
          startedAtUtc: '2026-04-23T18:20:00Z',
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
            id: entryOneId,
            workoutId,
            liftId: `lift-shared-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-23T18:21:00Z',
            position: 1,
            sets: [
              {
                id: setOneId,
                workoutLiftEntryId: entryOneId,
                setNumber: 1,
                reps: 6,
                weight: null,
                createdAtUtc: '2026-04-23T18:22:00Z',
              },
            ],
          },
          {
            id: entryTwoId,
            workoutId,
            liftId: `lift-shared-${Date.now()}`,
            displayName: 'Pull Up',
            addedAtUtc: '2026-04-23T18:23:00Z',
            position: 2,
            sets: [
              {
                id: setTwoId,
                workoutLiftEntryId: entryTwoId,
                setNumber: 1,
                reps: 9,
                weight: null,
                createdAtUtc: '2026-04-23T18:24:00Z',
              },
            ],
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/${entryOneId}/sets/${setOneId}`, async (route) => {
    const request = route.request().postDataJSON() as { reps: number; weight: number | null };
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workoutId,
        workoutLiftEntryId: entryOneId,
        set: {
          id: setOneId,
          workoutLiftEntryId: entryOneId,
          setNumber: 1,
          reps: request.reps,
          weight: request.weight,
          updatedAtUtc: '2026-04-23T18:25:00Z',
        },
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  const firstEntry = page.locator('.active-workout__lift-list-item').nth(0);
  const secondEntry = page.locator('.active-workout__lift-list-item').nth(1);

  await firstEntry.getByRole('button', { name: /Edit set 1 for Pull Up entry #1/i }).click();
  await firstEntry.getByLabel(/Edit reps for set 1 of Pull Up entry #1/i).fill('8');
  await firstEntry.getByRole('button', { name: /Save set 1 for Pull Up entry #1/i }).click();

  await expect(firstEntry.getByText('8 reps')).toBeVisible();
  await expect(secondEntry.getByText('9 reps')).toBeVisible();
});
