import { expect, test } from '@playwright/test';

test('conflict failure shows message and reconciles to authoritative order', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryOneId = `entry-1-${Date.now()}`;
  const entryTwoId = `entry-2-${Date.now()}`;
  const listResponses = [
    [
      {
        id: entryOneId,
        workoutId,
        liftId: `lift-1-${Date.now()}`,
        displayName: 'Bench Press',
        addedAtUtc: '2026-04-22T14:20:00Z',
        position: 1,
      },
      {
        id: entryTwoId,
        workoutId,
        liftId: `lift-2-${Date.now()}`,
        displayName: 'Squat',
        addedAtUtc: '2026-04-22T14:21:00Z',
        position: 2,
      },
    ],
    [
      {
        id: entryTwoId,
        workoutId,
        liftId: `lift-2-${Date.now()}`,
        displayName: 'Squat',
        addedAtUtc: '2026-04-22T14:21:00Z',
        position: 1,
      },
      {
        id: entryOneId,
        workoutId,
        liftId: `lift-1-${Date.now()}`,
        displayName: 'Bench Press',
        addedAtUtc: '2026-04-22T14:20:00Z',
        position: 2,
      },
    ],
  ];
  let listIndex = 0;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: 'Reorder Failure Day',
          startedAtUtc: '2026-04-22T14:19:00Z',
        },
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts`, async (route) => {
    const body = listResponses[Math.min(listIndex, listResponses.length - 1)];
    listIndex += 1;
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: body }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/reorder`, async (route) => {
    await route.fulfill({
      status: 409,
      contentType: 'application/problem+json',
      body: JSON.stringify({
        title: 'Workout order changed on another device.',
        status: 409,
      }),
    });
  });

  await page.goto(`/workouts/${workoutId}`);

  await page
    .locator('.active-workout__lift-list-item')
    .first()
    .getByRole('button', { name: /Move .* entry #\d+ down/ })
    .click();

  await expect(page.getByText('Workout order changed on another device.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-name').nth(0)).toHaveText('Squat');
  await expect(page.locator('.active-workout__lift-name').nth(1)).toHaveText('Bench Press');
});

test('server failure shows error and keeps list stable without ghost order', async ({ page }) => {
  const workoutId = `workout-${Date.now()}`;
  const entryOneId = `entry-1-${Date.now()}`;
  const entryTwoId = `entry-2-${Date.now()}`;

  await page.route(`**/api/workouts/${workoutId}`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        workout: {
          id: workoutId,
          status: 'InProgress',
          label: null,
          startedAtUtc: '2026-04-22T14:30:00Z',
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
            liftId: `lift-1-${Date.now()}`,
            displayName: 'Row',
            addedAtUtc: '2026-04-22T14:31:00Z',
            position: 1,
          },
          {
            id: entryTwoId,
            workoutId,
            liftId: `lift-2-${Date.now()}`,
            displayName: 'Dip',
            addedAtUtc: '2026-04-22T14:32:00Z',
            position: 2,
          },
        ],
      }),
    });
  });

  await page.route(`**/api/workouts/${workoutId}/lifts/reorder`, async (route) => {
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

  await page.getByRole('button', { name: 'Move Row entry #1 down' }).click();

  await expect(page.getByText('Order was not saved. Check your connection and try again.')).toBeVisible();
  await expect(page.locator('.active-workout__lift-name').nth(0)).toHaveText('Row');
  await expect(page.locator('.active-workout__lift-name').nth(1)).toHaveText('Dip');
});
