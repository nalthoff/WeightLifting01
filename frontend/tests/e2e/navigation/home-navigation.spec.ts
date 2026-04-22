import { expect, test } from '@playwright/test';

test.describe('Home and navigation smoke', () => {
  test('root loads the home shell', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText('WeightLifting01')).toBeVisible();
    await expect(page).toHaveURL(/\/$/);

    // Home should not immediately render lifts management content.
    await expect(page.getByText('Add a lift')).toHaveCount(0);
  });

  test('navigation can reach settings/lifts', async ({ page }) => {
    await page.goto('/');

    await page.getByRole('link', { name: /settings/i }).click();

    await expect(page).toHaveURL(/\/settings\/lifts$/);
    await expect(page.getByText('Add a lift')).toBeVisible();
  });

  test('direct settings/lifts deep link works', async ({ page }) => {
    await page.goto('/settings/lifts');

    await expect(page).toHaveURL(/\/settings\/lifts$/);
    await expect(page.getByText('WeightLifting01')).toBeVisible();
    await expect(page.getByText('Add a lift')).toBeVisible();
  });
});
