import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { LiftsPageComponent } from '../../../../src/app/features/settings/lifts/lifts-page.component';
import { LiftsPageFacade } from '../../../../src/app/features/settings/lifts/lifts-page.facade';

describe('LiftsPageComponent', () => {
  let fixture: ComponentFixture<LiftsPageComponent>;

  const facade = {
    load: jasmine.createSpy('load'),
    submit: jasmine.createSpy('submit'),
    updateLiftName: jasmine.createSpy('updateLiftName'),
    liftName: signal(''),
    errorMessage: signal<string | null>(null),
    successMessage: signal<string | null>(null),
    isSaving: signal(false),
    isLoading: signal(false),
    lifts: signal([{ id: '1', name: 'Squat', isActive: true }]),
  };

  beforeEach(async () => {
    facade.load.calls.reset();
    facade.submit.calls.reset();
    facade.updateLiftName.calls.reset();
    facade.liftName.set('');
    facade.errorMessage.set(null);
    facade.successMessage.set(null);
    facade.isSaving.set(false);
    facade.isLoading.set(false);
    facade.lifts.set([{ id: '1', name: 'Squat', isActive: true }]);

    await TestBed.configureTestingModule({
      imports: [LiftsPageComponent],
      providers: [
        { provide: LiftsPageFacade, useValue: facade },
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LiftsPageComponent);
  });

  it('loads lifts on init', () => {
    fixture.detectChanges();

    expect(facade.load).toHaveBeenCalled();
  });

  it('submits through the facade', () => {
    fixture.detectChanges();

    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));

    expect(facade.submit).toHaveBeenCalled();
  });

  it('renders messages from the facade', () => {
    facade.errorMessage.set('Enter a lift name.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Enter a lift name.');
  });

  it('renders Material-styled sections for the form and list', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Add a lift');
    expect(compiled.textContent).toContain('Current lifts');
    expect(compiled.querySelectorAll('mat-card').length).toBe(3);
  });
});
