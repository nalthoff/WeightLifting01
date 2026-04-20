import { expect, test } from '@playwright/test';

test('create lift appears in settings lift list', async ({ page }) => {
  await page.goto('/settings/lifts');

  await page.getByLabel('Lift name').fill('Front Squat');
  await page.getByRole('button', { name: 'Add lift' }).click();

  await expect(page.getByText('Lift created.')).toBeVisible();
  await expect(page.getByText('Front Squat')).toBeVisible();
});
