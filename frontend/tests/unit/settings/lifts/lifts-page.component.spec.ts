import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

import { DeactivateLiftDialogComponent } from '../../../../src/app/features/settings/lifts/deactivate-lift-dialog/deactivate-lift-dialog.component';
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
    prepareForDeactivateDialog: jasmine.createSpy('prepareForDeactivateDialog'),
    submitDeactivate: jasmine.createSpy('submitDeactivate'),
    notifyDeactivateCancelled: jasmine.createSpy('notifyDeactivateCancelled'),
    setSelectedFilter: jasmine.createSpy('setSelectedFilter'),
    liftName: signal(''),
    editingLiftId: signal<string | null>(null),
    editingLiftName: signal(''),
    selectedFilter: signal<'active' | 'all'>('active'),
    errorMessage: signal<string | null>(null),
    successMessage: signal<string | null>(null),
    isSaving: signal(false),
    isLoading: signal(false),
    lifts: signal([{ id: '1', name: 'Squat', isActive: true }]),
  };

  const afterClosedSpy = jasmine.createSpy('afterClosed');
  const openSpy = jasmine.createSpy('open').and.returnValue({ afterClosed: afterClosedSpy });

  beforeEach(async () => {
    facade.load.calls.reset();
    facade.submit.calls.reset();
    facade.updateLiftName.calls.reset();
    facade.startRename.calls.reset();
    facade.submitRename.calls.reset();
    facade.cancelRename.calls.reset();
    facade.updateEditingLiftName.calls.reset();
    facade.prepareForDeactivateDialog.calls.reset();
    facade.submitDeactivate.calls.reset();
    facade.notifyDeactivateCancelled.calls.reset();
    facade.setSelectedFilter.calls.reset();
    openSpy.calls.reset();
    afterClosedSpy.calls.reset();
    afterClosedSpy.and.returnValue(of(undefined));
    facade.liftName.set('');
    facade.editingLiftId.set(null);
    facade.editingLiftName.set('');
    facade.selectedFilter.set('active');
    facade.errorMessage.set(null);
    facade.successMessage.set(null);
    facade.isSaving.set(false);
    facade.isLoading.set(false);
    facade.lifts.set([{ id: '1', name: 'Squat', isActive: true }]);

    await TestBed.configureTestingModule({
      imports: [LiftsPageComponent],
      providers: [
        { provide: LiftsPageFacade, useValue: facade },
        { provide: MatDialog, useValue: { open: openSpy } },
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

  it('opens deactivate confirmation dialog', () => {
    fixture.detectChanges();

    const deactivateButton = fixture.nativeElement.querySelector(
      '[data-testid="deactivate-lift-1"]',
    ) as HTMLButtonElement;
    deactivateButton.click();

    expect(facade.prepareForDeactivateDialog).toHaveBeenCalled();
    expect(openSpy).toHaveBeenCalledWith(DeactivateLiftDialogComponent, {
      width: 'min(100vw - 2rem, 22rem)',
      maxWidth: '95vw',
      autoFocus: 'first-tabbable',
      data: { liftId: '1', liftName: 'Squat' },
    });
  });

  it('submits deactivate when dialog confirms', () => {
    afterClosedSpy.and.returnValue(of(true));
    fixture.detectChanges();

    const deactivateButton = fixture.nativeElement.querySelector(
      '[data-testid="deactivate-lift-1"]',
    ) as HTMLButtonElement;
    deactivateButton.click();

    expect(facade.submitDeactivate).toHaveBeenCalledWith('1');
    expect(facade.notifyDeactivateCancelled).not.toHaveBeenCalled();
  });

  it('notifies cancel when dialog declines', () => {
    afterClosedSpy.and.returnValue(of(false));
    fixture.detectChanges();

    const deactivateButton = fixture.nativeElement.querySelector(
      '[data-testid="deactivate-lift-1"]',
    ) as HTMLButtonElement;
    deactivateButton.click();

    expect(facade.notifyDeactivateCancelled).toHaveBeenCalled();
    expect(facade.submitDeactivate).not.toHaveBeenCalled();
  });

  it('does nothing when dialog is dismissed without choosing', () => {
    afterClosedSpy.and.returnValue(of(undefined));
    fixture.detectChanges();

    const deactivateButton = fixture.nativeElement.querySelector(
      '[data-testid="deactivate-lift-1"]',
    ) as HTMLButtonElement;
    deactivateButton.click();

    expect(facade.notifyDeactivateCancelled).not.toHaveBeenCalled();
    expect(facade.submitDeactivate).not.toHaveBeenCalled();
  });

  it('applies inactive styling marker when inactive items are visible', () => {
    facade.selectedFilter.set('all');
    facade.lifts.set([{ id: '1', name: 'Squat', isActive: false }]);
    fixture.detectChanges();

    const inactiveBadge = fixture.nativeElement.querySelector('.page__lift-status');

    expect(inactiveBadge?.textContent).toContain('Inactive');
    expect(fixture.nativeElement.querySelector('.page__list-row--inactive')).not.toBeNull();
  });

  it('switches to include inactive filter through facade', () => {
    fixture.detectChanges();

    const includeInactiveButton = fixture.nativeElement.querySelector(
      '[data-testid="lifts-filter-all"] button',
    ) as HTMLButtonElement;
    includeInactiveButton.click();

    expect(facade.setSelectedFilter).toHaveBeenCalledWith('all');
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
