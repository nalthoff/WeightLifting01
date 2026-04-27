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

@Component({
  template: `<h1>History</h1>`,
})
class HistoryTestComponent {}

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: '', component: HomeTestComponent },
          { path: 'history', component: HistoryTestComponent },
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
    const brandHomeLink = compiled.querySelector('[data-testid="brand-home-link"]');

    expect(compiled.textContent).toContain('RackNote');
    expect(compiled.textContent).toContain('Home');
    expect(compiled.textContent).not.toContain('Workout');
    expect(compiled.textContent).not.toContain('Log');
    expect(brandHomeLink?.getAttribute('href')).toContain('/');
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

  it('should show History navigation and route to /history', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const historyLink = compiled.querySelector('[data-testid="history-nav-link"]');

    expect(historyLink).withContext('History nav link should be visible').not.toBeNull();
    expect(historyLink?.textContent).toContain('History');
    expect(historyLink?.getAttribute('href')).toContain('/history');
  });
});
