import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LiftsApiService } from '../../../src/app/core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../../src/app/core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../../src/app/core/state/workouts-store.service';
import { ActiveWorkoutPageComponent } from '../../../src/app/features/workouts/active-workout-page.component';

describe('ActiveWorkoutPageComponent completion', () => {
  let fixture: ComponentFixture<ActiveWorkoutPageComponent>;
  let component: ActiveWorkoutPageComponent;

  const workoutId = 'workout-1';
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
      .and.callFake((workout: { status: 'InProgress' | 'Completed' } | null) => {
        if (!workout || workout.status !== 'InProgress') {
          workoutsStoreService.activeWorkout.set(null);
          return;
        }

        workoutsStoreService.activeWorkout.set(workout as typeof inProgressWorkout);
      }),
    clearActiveWorkout: jasmine.createSpy('clearActiveWorkout').and.callFake(() => {
      workoutsStoreService.activeWorkout.set(null);
    }),
  };

  const workoutsApiService = {
    getWorkout: jasmine.createSpy('getWorkout').and.returnValue(of({ workout: inProgressWorkout })),
    getActiveWorkoutSummary: jasmine
      .createSpy('getActiveWorkoutSummary')
      .and.returnValue(of({ workout: inProgressWorkout })),
    completeWorkout: jasmine
      .createSpy('completeWorkout')
      .and.returnValue(of({ workout: { ...inProgressWorkout, status: 'Completed' as const } })),
  };

  const workoutLiftsApiService = {
    listWorkoutLifts: jasmine.createSpy('listWorkoutLifts').and.returnValue(of({ items: [] })),
  };

  const route: Partial<ActivatedRoute> = {
    snapshot: { paramMap: convertToParamMap({ workoutId }) } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
  };

  beforeEach(async () => {
    workoutsApiService.getWorkout.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.and.returnValue(of({ workout: inProgressWorkout }));
    workoutsApiService.completeWorkout.calls.reset();
    workoutsApiService.completeWorkout.and.returnValue(
      of({ workout: { ...inProgressWorkout, status: 'Completed' as const } }),
    );
    workoutsStoreService.reconcileActiveWorkout.calls.reset();
    workoutsStoreService.clearActiveWorkout.calls.reset();
    workoutsStoreService.activeWorkout.set(inProgressWorkout);

    await TestBed.configureTestingModule({
      imports: [ActiveWorkoutPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: LiftsApiService, useValue: { listLifts: () => of({ items: [] }) } },
        { provide: WorkoutLiftsApiService, useValue: workoutLiftsApiService },
        { provide: WorkoutsApiService, useValue: workoutsApiService },
        { provide: WorkoutsStoreService, useValue: workoutsStoreService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ActiveWorkoutPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('completes active workout from detail and refreshes authoritative state', () => {
    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="active-workout-complete"]',
    ) as HTMLButtonElement;
    completeButton.click();
    fixture.detectChanges();

    expect(workoutsApiService.completeWorkout).toHaveBeenCalledWith(workoutId);
    expect(workoutsStoreService.reconcileActiveWorkout).toHaveBeenCalled();
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalled();
    expect((fixture.nativeElement.textContent as string)).toContain('Workout completed. Great work.');
  });

  it('uses stale-state title for recoverable conflict failures and refreshes state', () => {
    workoutsApiService.completeWorkout.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Workout already completed elsewhere.' },
          }),
      ),
    );

    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="active-workout-complete"]',
    ) as HTMLButtonElement;
    completeButton.click();
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('Workout already completed elsewhere.');
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalled();
  });

  it('prevents duplicate complete requests while completion is already in-flight', () => {
    component.isCompletingWorkout.set(true);
    fixture.detectChanges();

    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="active-workout-complete"]',
    ) as HTMLButtonElement;
    expect(completeButton.disabled).toBeTrue();

    component.completeWorkout();
    expect(workoutsApiService.completeWorkout).not.toHaveBeenCalled();
  });
});
