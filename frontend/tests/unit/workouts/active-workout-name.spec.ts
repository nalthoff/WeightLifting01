import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { LiftsApiService } from '../../../src/app/core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../../src/app/core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../../src/app/core/state/workouts-store.service';
import { ActiveWorkoutPageComponent } from '../../../src/app/features/workouts/active-workout-page.component';

describe('ActiveWorkoutPageComponent workout naming', () => {
  let fixture: ComponentFixture<ActiveWorkoutPageComponent>;
  let component: ActiveWorkoutPageComponent;

  const workoutId = 'workout-name-1';
  const inProgressWorkout = {
    id: workoutId,
    status: 'InProgress' as const,
    label: 'Leg Day',
    startedAtUtc: '2026-04-23T17:00:00Z',
  };

  const workoutsStoreService = {
    activeWorkout: signal<typeof inProgressWorkout | null>(inProgressWorkout),
    activeWorkoutLiftEntries: signal([]),
    setActiveWorkout: jasmine.createSpy('setActiveWorkout'),
    setActiveWorkoutLiftEntries: jasmine.createSpy('setActiveWorkoutLiftEntries'),
    reconcileActiveWorkout: jasmine
      .createSpy('reconcileActiveWorkout')
      .and.callFake((workout: typeof inProgressWorkout | null) => workoutsStoreService.activeWorkout.set(workout)),
    clearActiveWorkout: jasmine.createSpy('clearActiveWorkout').and.callFake(() => workoutsStoreService.activeWorkout.set(null)),
  };

  const workoutsApiService = {
    getWorkout: jasmine.createSpy('getWorkout').and.returnValue(of({ workout: inProgressWorkout })),
    updateWorkoutLabel: jasmine
      .createSpy('updateWorkoutLabel')
      .and.returnValue(of({ workout: { ...inProgressWorkout, label: 'Upper Day' } })),
  };

  const route: Partial<ActivatedRoute> = {
    snapshot: { paramMap: convertToParamMap({ workoutId }) } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
  };
  const dialog = {
    open: jasmine.createSpy('open').and.returnValue({
      afterClosed: () => of('Upper Day'),
    }),
  };

  beforeEach(async () => {
    workoutsApiService.updateWorkoutLabel.calls.reset();
    workoutsApiService.updateWorkoutLabel.and.returnValue(of({ workout: { ...inProgressWorkout, label: 'Upper Day' } }));
    workoutsStoreService.reconcileActiveWorkout.calls.reset();
    workoutsStoreService.activeWorkout.set(inProgressWorkout);

    await TestBed.configureTestingModule({
      imports: [ActiveWorkoutPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: MatDialog, useValue: dialog },
        { provide: LiftsApiService, useValue: { listLifts: () => of({ items: [] }) } },
        { provide: WorkoutLiftsApiService, useValue: { listWorkoutLifts: () => of({ items: [] }) } },
        { provide: WorkoutsApiService, useValue: workoutsApiService },
        { provide: WorkoutsStoreService, useValue: workoutsStoreService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ActiveWorkoutPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renames from menu modal and reconciles active workout state', () => {
    component.beginRenameWorkout();
    fixture.detectChanges();

    expect(workoutsApiService.updateWorkoutLabel).toHaveBeenCalledWith(workoutId, { label: 'Upper Day' });
    expect(workoutsStoreService.reconcileActiveWorkout).toHaveBeenCalled();
    expect(component.workoutNameSuccess()).toBe('Workout name saved.');
  });

  it('surfaces backend validation errors when name is too long', () => {
    workoutsApiService.updateWorkoutLabel.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 422,
            error: { errors: { label: ['Workout label must be 200 characters or fewer.'] } },
          }),
      ),
    );

    dialog.open.and.returnValue({
      afterClosed: () => of('x'.repeat(201)),
    });
    component.beginRenameWorkout();

    expect(component.workoutNameError()).toBe('Workout label must be 200 characters or fewer.');
  });
});
