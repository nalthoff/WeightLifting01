import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';

import { routes } from '../../../src/app/app.routes';
import { HomePageComponent } from '../../../src/app/features/home/home-page.component';
import { LiftsPageComponent } from '../../../src/app/features/settings/lifts/lifts-page.component';

describe('app routes', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideNoopAnimations()],
    }).compileComponents();
  });

  it('maps the root path to the dedicated home component', async () => {
    const harness = await RouterTestingHarness.create();
    const rendered = await harness.navigateByUrl('/', HomePageComponent);

    expect(rendered).toBeInstanceOf(HomePageComponent);
  });

  it('preserves direct deep links to settings/lifts without redirecting to home', async () => {
    const harness = await RouterTestingHarness.create();
    const rendered = await harness.navigateByUrl('/settings/lifts', LiftsPageComponent);

    expect(rendered).toBeInstanceOf(LiftsPageComponent);
  });
});
