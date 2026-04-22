import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { App } from './app';

@Component({
  template: `<h1>Home</h1>`,
})
class HomeTestComponent {}

@Component({
  template: `<h1>Lifts</h1>`,
})
class LiftsTestComponent {}

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: '', component: HomeTestComponent },
          { path: 'settings/lifts', component: LiftsTestComponent },
        ]),
        provideNoopAnimations(),
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render a minimal home state shell by default', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('WeightLifting01');
    expect(compiled.textContent).toContain('Home');
    expect(compiled.textContent).not.toContain('Workout');
    expect(compiled.textContent).not.toContain('Log');
  });

  it('should show Settings navigation and route to /settings/lifts', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const settingsLink = compiled.querySelector('[data-testid="settings-nav-link"]');

    expect(settingsLink).withContext('Settings nav link should be visible').not.toBeNull();
    expect(settingsLink?.textContent).toContain('Settings');
    expect(settingsLink?.getAttribute('href')).toContain('/settings/lifts');
  });
});
