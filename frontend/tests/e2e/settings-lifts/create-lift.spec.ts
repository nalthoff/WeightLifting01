import { expect, test } from '@playwright/test';

test('create lift appears in settings lift list', async ({ page }) => {
  const liftName = `Front Squat ${Date.now()}`;

  await page.goto('/settings/lifts');

  await expect(page.getByText('WeightLifting01')).toBeVisible();
  await expect(page.getByText('Add a lift')).toBeVisible();

  await page.getByLabel('Lift name').fill(liftName);
  await page.getByRole('button', { name: 'Add lift' }).click();

  await expect(page.getByText('Lift created.')).toBeVisible();
  await expect(page.getByText(liftName, { exact: true })).toBeVisible();
});
