import { expect, test } from '@playwright/test';

test.describe('Deactivate lift', () => {
  test('successful deactivate hides a lift from active-only and filter shows inactive', async ({
    page,
    request,
  }) => {
    const liftName = `Front Squat ${Date.now()}`;

    const createResponse = await request.post('/api/lifts', {
      data: { name: liftName },
    });
    expect(createResponse.ok()).toBeTruthy();

    await page.goto('/settings/lifts');

    await page.getByRole('button', { name: `Deactivate ${liftName}` }).click();
    await page.getByRole('button', { name: 'Deactivate lift' }).click();

    await expect(page.getByText('Lift deactivated.')).toBeVisible();
    await expect(page.getByText(liftName, { exact: true })).toHaveCount(0);

    await page.getByTestId('lifts-filter-all').click();
    await expect(page.getByText(liftName, { exact: true })).toBeVisible();
    await expect(page.locator('.page__lift-status', { hasText: 'Inactive' })).toBeVisible();
  });

  test('cancelled deactivate keeps lift active and visible', async ({ page, request }) => {
    const liftName = `Overhead Press ${Date.now()}`;

    const createResponse = await request.post('/api/lifts', {
      data: { name: liftName },
    });
    expect(createResponse.ok()).toBeTruthy();

    await page.goto('/settings/lifts');

    await page.getByRole('button', { name: `Deactivate ${liftName}` }).click();
    await page.getByRole('button', { name: 'Keep active' }).click();

    await expect(page.getByText('Deactivation cancelled.')).toBeVisible();
    await expect(page.getByText(liftName, { exact: true })).toBeVisible();
  });

  test('failed deactivate shows error and keeps lift visible', async ({ page, request }) => {
    const liftName = `Strict Press ${Date.now()}`;

    const createResponse = await request.post('/api/lifts', {
      data: { name: liftName },
    });
    expect(createResponse.ok()).toBeTruthy();

    await page.route('**/api/lifts/*/deactivate', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ title: 'Server error' }),
      });
    });

    await page.goto('/settings/lifts');

    await page.getByRole('button', { name: `Deactivate ${liftName}` }).click();
    await page.getByRole('button', { name: 'Deactivate lift' }).click();

    await expect(page.getByText('Lift was not deactivated. Try again.')).toBeVisible();
    await expect(page.getByText(liftName, { exact: true })).toBeVisible();
  });
});
