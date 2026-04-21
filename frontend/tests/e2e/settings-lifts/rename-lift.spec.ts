import { expect, test } from '@playwright/test';

test('rename existing lift updates the settings list', async ({ page, request }) => {
  const originalName = `Front Squat ${Date.now()}`;
  const renamedName = `Paused Front Squat ${Date.now()}`;

  const createResponse = await request.post('/api/lifts', {
    data: {
      name: originalName,
    },
  });

  expect(createResponse.ok()).toBeTruthy();

  await page.goto('/settings/lifts');

  await page.getByRole('button', { name: `Edit ${originalName}` }).click();
  await page.locator('input[name="renameLiftName"]').fill(renamedName);
  await page.getByRole('button', { name: 'Save' }).click();

  await expect(page.getByText('Lift renamed.')).toBeVisible();
  await expect(page.getByText(renamedName, { exact: true })).toBeVisible();
});

test('rename failure keeps the saved name visible', async ({ page, request }) => {
  const originalName = `Strict Press ${Date.now()}`;

  const createResponse = await request.post('/api/lifts', {
    data: {
      name: originalName,
    },
  });

  expect(createResponse.ok()).toBeTruthy();

  await page.route('**/api/lifts/*', async (route) => {
    if (route.request().method() === 'PUT') {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ title: 'Server error' }),
      });
      return;
    }

    await route.continue();
  });

  await page.goto('/settings/lifts');

  await page.getByRole('button', { name: `Edit ${originalName}` }).click();
  await page.locator('input[name="renameLiftName"]').fill(`Paused ${originalName}`);
  await page.getByRole('button', { name: 'Save' }).click();

  await expect(page.getByText('Lift was not renamed. Try again.')).toBeVisible();
  await expect(page.getByText(originalName, { exact: true })).toBeVisible();
});
