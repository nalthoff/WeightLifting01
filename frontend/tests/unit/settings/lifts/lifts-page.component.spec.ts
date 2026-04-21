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
    startRename: jasmine.createSpy('startRename'),
    submitRename: jasmine.createSpy('submitRename'),
    cancelRename: jasmine.createSpy('cancelRename'),
    updateEditingLiftName: jasmine.createSpy('updateEditingLiftName'),
    liftName: signal(''),
    editingLiftId: signal<string | null>(null),
    editingLiftName: signal(''),
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
    facade.startRename.calls.reset();
    facade.submitRename.calls.reset();
    facade.cancelRename.calls.reset();
    facade.updateEditingLiftName.calls.reset();
    facade.liftName.set('');
    facade.editingLiftId.set(null);
    facade.editingLiftName.set('');
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

  it('renders duplicate-name conflict message from the facade', () => {
    facade.errorMessage.set('Lift name already exists.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Lift name already exists.');
  });

  it('starts rename through the facade', () => {
    fixture.detectChanges();

    const editButton = fixture.nativeElement.querySelector('[data-testid="edit-lift-1"]') as HTMLButtonElement;
    editButton.click();

    expect(facade.startRename).toHaveBeenCalledWith({ id: '1', name: 'Squat', isActive: true });
  });

  it('renders and submits the rename editor from the facade state', () => {
    facade.editingLiftId.set('1');
    facade.editingLiftName.set('Paused Squat');
    fixture.detectChanges();

    const renameForm = fixture.nativeElement.querySelector('[data-testid="rename-form-1"]') as HTMLFormElement;
    const renameInput = fixture.nativeElement.querySelector('#renameLiftName-1') as HTMLInputElement;
    renameForm.dispatchEvent(new Event('submit'));

    expect(renameInput.getAttribute('aria-label')).toBe('Lift name');
    expect(facade.submitRename).toHaveBeenCalled();
  });

  it('renders Material-styled sections for the form and list', () => {
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Add a lift');
    expect(compiled.textContent).toContain('Current lifts');
    expect(compiled.querySelectorAll('mat-card').length).toBe(3);
  });
});
