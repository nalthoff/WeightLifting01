import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LiftsApiService } from '../../../src/app/core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../../src/app/core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../../src/app/core/state/workouts-store.service';
import { ActiveWorkoutPageComponent } from '../../../src/app/features/workouts/active-workout-page.component';

describe('ActiveWorkoutPageComponent delete workout', () => {
  let fixture: ComponentFixture<ActiveWorkoutPageComponent>;
  let component: ActiveWorkoutPageComponent;

  const workoutId = 'workout-1';
  const inProgressWorkout = {
    id: workoutId,
    status: 'InProgress' as const,
    label: 'Delete Day',
    startedAtUtc: '2026-04-23T17:00:00Z',
  };

  const workoutsStoreService = {
    activeWorkout: signal<typeof inProgressWorkout | null>(inProgressWorkout),
    activeWorkoutLiftEntries: signal([]),
    historicalFlowNavigationContext: signal({ returnToWorkoutId: null as string | null }),
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
    clearActiveWorkoutIfMatches: jasmine.createSpy('clearActiveWorkoutIfMatches').and.callFake((id: string) => {
      if (workoutsStoreService.activeWorkout()?.id === id) {
        workoutsStoreService.activeWorkout.set(null);
      }
    }),
  };

  const workoutsApiService = {
    getWorkout: jasmine.createSpy('getWorkout').and.returnValue(of({ workout: inProgressWorkout })),
    getActiveWorkoutSummary: jasmine
      .createSpy('getActiveWorkoutSummary')
      .and.returnValue(of({ workout: inProgressWorkout })),
    deleteWorkout: jasmine.createSpy('deleteWorkout').and.returnValue(of({ workoutId })),
  };

  const workoutLiftsApiService = {
    listWorkoutLifts: jasmine.createSpy('listWorkoutLifts').and.returnValue(of({ items: [] })),
  };

  const route: Partial<ActivatedRoute> = {
    snapshot: { paramMap: convertToParamMap({ workoutId }) } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
  };
  const dialog = {
    open: jasmine.createSpy('open').and.returnValue({
      afterClosed: () => of(true),
    }),
  };
  const router = {
    events: of({}),
    createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue({}),
    serializeUrl: jasmine.createSpy('serializeUrl').and.returnValue('/'),
    navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)),
  };

  beforeEach(async () => {
    workoutsApiService.getWorkout.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.and.returnValue(of({ workout: inProgressWorkout }));
    workoutsApiService.deleteWorkout.calls.reset();
    workoutsApiService.deleteWorkout.and.returnValue(of({ workoutId }));
    workoutsStoreService.activeWorkout.set(inProgressWorkout);
    workoutsStoreService.clearActiveWorkoutIfMatches.calls.reset();
    dialog.open.calls.reset();
    dialog.open.and.returnValue({
      afterClosed: () => of(true),
    });
    router.navigate.calls.reset();

    await TestBed.configureTestingModule({
      imports: [ActiveWorkoutPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: Router, useValue: router },
        { provide: MatDialog, useValue: dialog },
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

  it('cancels delete confirmation without mutating workout state', () => {
    dialog.open.and.returnValue({
      afterClosed: () => of(false),
    });
    component.beginDeleteWorkout();

    expect(component.deleteWorkoutSession()).toBeNull();
    expect(workoutsApiService.deleteWorkout).not.toHaveBeenCalled();
    expect(workoutsStoreService.clearActiveWorkoutIfMatches).not.toHaveBeenCalled();
  });

  it('confirms delete, clears active state, and navigates home', () => {
    component.beginDeleteWorkout();
    fixture.detectChanges();

    expect(workoutsApiService.deleteWorkout).toHaveBeenCalledWith(workoutId);
    expect(workoutsStoreService.clearActiveWorkoutIfMatches).toHaveBeenCalledWith(workoutId);
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('surfaces conflict/not-found feedback and refreshes authoritative state', () => {
    workoutsApiService.deleteWorkout.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Workout already completed elsewhere.' },
          }),
      ),
    );

    component.beginDeleteWorkout();
    component.confirmDeleteWorkout();
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('Workout already completed elsewhere.');
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalled();
  });

  it('supports retry after general delete failure', () => {
    let attempt = 0;
    workoutsApiService.deleteWorkout.and.callFake(() => {
      attempt += 1;
      if (attempt === 1) {
        return throwError(() => new HttpErrorResponse({ status: 500 }));
      }

      return of({ workoutId });
    });

    dialog.open.and.returnValue({
      afterClosed: () => of(false),
    });
    component.beginDeleteWorkout();
    component.confirmDeleteWorkout();
    fixture.detectChanges();

    expect(component.deleteErrorMessage()).toBe(
      'Unable to delete workout. Check your connection and try again.',
    );

    component.confirmDeleteWorkout();
    fixture.detectChanges();

    expect(workoutsApiService.deleteWorkout).toHaveBeenCalledTimes(2);
    expect(component.deleteWorkoutSession()).toBeNull();
  });

  it('prevents duplicate delete submits while deletion is already in-flight', () => {
    dialog.open.and.returnValue({
      afterClosed: () => of(false),
    });
    component.beginDeleteWorkout();
    component.deleteWorkoutSession.set({
      workoutId,
      isConfirmingDelete: false,
      isDeleting: true,
      errorMessage: null,
    });

    component.confirmDeleteWorkout();

    expect(workoutsApiService.deleteWorkout).not.toHaveBeenCalled();
  });
});
