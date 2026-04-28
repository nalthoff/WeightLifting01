import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';
import { HistoricalWorkoutFormComponent } from './historical-workout-form.component';

describe('HistoricalWorkoutFormComponent', () => {
  let fixture: ComponentFixture<HistoricalWorkoutFormComponent>;
  let component: HistoricalWorkoutFormComponent;
  let workoutsApiServiceSpy: jasmine.SpyObj<WorkoutsApiService>;
  let workoutsStoreServiceSpy: jasmine.SpyObj<WorkoutsStoreService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    workoutsApiServiceSpy = jasmine.createSpyObj<WorkoutsApiService>('WorkoutsApiService', ['createHistoricalWorkout']);
    workoutsStoreServiceSpy = jasmine.createSpyObj<WorkoutsStoreService>(
      'WorkoutsStoreService',
      [
        'setActiveWorkout',
        'captureHistoricalFlowNavigationContextFromActiveWorkout',
        'clearHistoricalFlowMessage',
        'setHistoricalFlowMessage',
      ],
      {
        hasActiveWorkout: signal(false),
        historicalFlowNavigationContext: signal({ returnToWorkoutId: null }),
      },
    );
    routerSpy = jasmine.createSpyObj<Router>('Router', ['navigate']);
    routerSpy.navigate.and.resolveTo(true);

    await TestBed.configureTestingModule({
      imports: [HistoricalWorkoutFormComponent],
      providers: [
        provideNoopAnimations(),
        { provide: WorkoutsApiService, useValue: workoutsApiServiceSpy },
        { provide: WorkoutsStoreService, useValue: workoutsStoreServiceSpy },
        { provide: Router, useValue: routerSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HistoricalWorkoutFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('requires historical training day', () => {
    component.form.patchValue({
      trainingDayLocalDate: '',
      startTimeLocal: '07:30',
      sessionLengthMinutes: 45,
    });

    expect(component.form.controls.trainingDayLocalDate.hasError('required')).toBeTrue();
    expect(component.canSave()).toBeFalse();
  });

  it('requires historical start time', () => {
    component.form.patchValue({
      trainingDayLocalDate: '2026-04-25',
      startTimeLocal: '',
      sessionLengthMinutes: 45,
    });

    expect(component.form.controls.startTimeLocal.hasError('required')).toBeTrue();
    expect(component.canSave()).toBeFalse();
  });

  it('requires historical duration', () => {
    component.form.patchValue({
      trainingDayLocalDate: '2026-04-25',
      startTimeLocal: '07:30',
      sessionLengthMinutes: null,
    });

    expect(component.form.controls.sessionLengthMinutes.hasError('required')).toBeTrue();
    expect(component.canSave()).toBeFalse();
  });

  it('keeps save button disabled until all required timing fields are valid', () => {
    const saveButton = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;

    component.form.patchValue({
      trainingDayLocalDate: '2026-04-25',
      startTimeLocal: '',
      sessionLengthMinutes: 30,
    });
    fixture.detectChanges();
    expect(saveButton.disabled).toBeTrue();

    component.form.patchValue({
      startTimeLocal: '07:30',
    });
    fixture.detectChanges();
    expect(saveButton.disabled).toBeFalse();
  });

  it('gates save when duration is below minimum', () => {
    component.form.patchValue({
      trainingDayLocalDate: '2026-04-25',
      startTimeLocal: '07:30',
      sessionLengthMinutes: 0,
    });

    expect(component.form.controls.sessionLengthMinutes.hasError('min')).toBeTrue();
    expect(component.canSave()).toBeFalse();
  });

  it('shows required timing messages after submit while preserving start time help text before validation', () => {
    component.form.patchValue({
      trainingDayLocalDate: '',
      startTimeLocal: '',
      sessionLengthMinutes: null,
    });
    fixture.detectChanges();

    const startTimeFieldBeforeSubmit = fixture.nativeElement.querySelector(
      '[formControlName="startTimeLocal"]',
    )?.closest('mat-form-field') as HTMLElement;
    expect(startTimeFieldBeforeSubmit?.textContent).toContain('24-hour HH:mm');

    const formElement = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    formElement.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(component.form.controls.trainingDayLocalDate.touched).toBeTrue();
    expect(component.form.controls.startTimeLocal.touched).toBeTrue();
    expect(component.form.controls.sessionLengthMinutes.touched).toBeTrue();

    const pageText = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(pageText).toContain('Date is required.');
    expect(pageText).toContain('Start time is required.');
    expect(pageText).toContain('Duration is required.');

    const startTimeFieldAfterSubmit = fixture.nativeElement.querySelector(
      '[formControlName="startTimeLocal"]',
    )?.closest('mat-form-field') as HTMLElement;
    expect(startTimeFieldAfterSubmit?.textContent).not.toContain('24-hour HH:mm');
  });

  it('submits to createHistoricalWorkout and navigates to workout entry historical mode on success', () => {
    workoutsApiServiceSpy.createHistoricalWorkout.and.returnValue(
      of({
        workout: {
          id: 'workout-123',
          status: 'Completed',
          startedAtUtc: '2026-04-20T10:00:00Z',
          completedAtUtc: '2026-04-20T10:45:00Z',
          label: 'Catch-up',
        },
      }),
    );
    component.form.patchValue({
      trainingDayLocalDate: '2026-04-20',
      startTimeLocal: '10:00',
      sessionLengthMinutes: 45,
      label: 'Catch-up',
    });

    component.onSave();

    expect(workoutsApiServiceSpy.createHistoricalWorkout).toHaveBeenCalledWith({
      trainingDayLocalDate: '2026-04-20',
      startTimeLocal: '10:00',
      sessionLengthMinutes: 45,
      label: 'Catch-up',
    });
    expect(workoutsStoreServiceSpy.setHistoricalFlowMessage).toHaveBeenCalledWith(
      'success',
      'Historical workout started. Add lifts and sets, then save it to history.',
    );
    expect(workoutsStoreServiceSpy.setActiveWorkout).toHaveBeenCalledWith({
      id: 'workout-123',
      status: 'Completed',
      startedAtUtc: '2026-04-20T10:00:00Z',
      completedAtUtc: '2026-04-20T10:45:00Z',
      label: 'Catch-up',
    });
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/workouts', 'workout-123'], {
      queryParams: { mode: 'historical' },
    });
  });

  it('shows save error and does not navigate when historical save fails', () => {
    workoutsApiServiceSpy.createHistoricalWorkout.and.returnValue(
      throwError(() => ({
        status: 500,
        error: {},
      })),
    );
    component.form.patchValue({
      trainingDayLocalDate: '2026-04-20',
      startTimeLocal: '10:00',
      sessionLengthMinutes: 45,
      label: '',
    });

    component.onSave();
    fixture.detectChanges();

    expect(component.saveErrorMessage()).toBe('Unable to save historical workout. Check your connection and try again.');
    expect(workoutsStoreServiceSpy.setHistoricalFlowMessage).toHaveBeenCalledWith(
      'error',
      'Unable to save historical workout. Check your connection and try again.',
    );
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });
});
