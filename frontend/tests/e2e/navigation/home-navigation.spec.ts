import { expect, test } from '@playwright/test';

test.describe('Home and navigation smoke', () => {
  test('root loads the home shell', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText('RackNote')).toBeVisible();
    await expect(page).toHaveURL(/\/$/);

    // Home should not immediately render lifts management content.
    await expect(page.getByText('Add a lift')).toHaveCount(0);
  });

  test('navigation can reach settings/lifts', async ({ page }) => {
    await page.goto('/');

    const desktopSettingsLink = page.getByTestId('settings-nav-link');
    if (await desktopSettingsLink.isVisible()) {
      await desktopSettingsLink.click();
    } else {
      await page.getByRole('button', { name: 'Open navigation menu' }).click();
      await page.getByTestId('settings-nav-link-mobile').click();
    }

    await expect(page).toHaveURL(/\/settings\/lifts$/);
    await expect(page.getByText('Add a lift')).toBeVisible();
  });

  test('direct settings/lifts deep link works', async ({ page }) => {
    await page.goto('/settings/lifts');

    await expect(page).toHaveURL(/\/settings\/lifts$/);
    await expect(page.getByText('RackNote')).toBeVisible();
    await expect(page.getByText('Add a lift')).toBeVisible();
  });
});
