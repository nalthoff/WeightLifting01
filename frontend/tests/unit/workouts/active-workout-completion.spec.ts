import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
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
    historicalFlowNavigationContext: signal({ returnToWorkoutId: null as string | null }),
    setActiveWorkout: jasmine.createSpy('setActiveWorkout'),
    setActiveWorkoutLiftEntries: jasmine.createSpy('setActiveWorkoutLiftEntries'),
    setHistoricalFlowMessage: jasmine.createSpy('setHistoricalFlowMessage'),
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
    snapshot: {
      paramMap: convertToParamMap({ workoutId }),
      queryParamMap: convertToParamMap({}),
    } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
    queryParamMap: of(convertToParamMap({})),
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
    workoutsApiService.completeWorkout.calls.reset();
    workoutsApiService.completeWorkout.and.returnValue(
      of({ workout: { ...inProgressWorkout, status: 'Completed' as const } }),
    );
    workoutsStoreService.reconcileActiveWorkout.calls.reset();
    workoutsStoreService.clearActiveWorkout.calls.reset();
    workoutsStoreService.setHistoricalFlowMessage.calls.reset();
    workoutsStoreService.historicalFlowNavigationContext.set({ returnToWorkoutId: null });
    workoutsStoreService.activeWorkout.set(inProgressWorkout);
    router.navigate.calls.reset();

    await TestBed.configureTestingModule({
      imports: [ActiveWorkoutPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: Router, useValue: router },
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

describe('ActiveWorkoutPageComponent completion historical return', () => {
  let fixture: ComponentFixture<ActiveWorkoutPageComponent>;
  let component: ActiveWorkoutPageComponent;

  const workoutId = 'historical-1';
  const inProgressWorkout = {
    id: workoutId,
    status: 'InProgress' as const,
    label: 'Catch-up session',
    startedAtUtc: '2026-04-23T17:00:00Z',
  };
  const workoutsStoreService = {
    activeWorkout: signal<typeof inProgressWorkout | null>(inProgressWorkout),
    activeWorkoutLiftEntries: signal([]),
    historicalFlowNavigationContext: signal({ returnToWorkoutId: 'active-1' }),
    setActiveWorkout: jasmine.createSpy('setActiveWorkout'),
    setActiveWorkoutLiftEntries: jasmine.createSpy('setActiveWorkoutLiftEntries'),
    setHistoricalFlowMessage: jasmine.createSpy('setHistoricalFlowMessage'),
    reconcileActiveWorkout: jasmine.createSpy('reconcileActiveWorkout').and.callFake(() => {
      workoutsStoreService.activeWorkout.set(null);
    }),
    clearActiveWorkout: jasmine.createSpy('clearActiveWorkout').and.callFake(() => {
      workoutsStoreService.activeWorkout.set(null);
    }),
  };
  const workoutsApiService = {
    getWorkout: jasmine.createSpy('getWorkout').and.returnValue(of({ workout: inProgressWorkout })),
    getActiveWorkoutSummary: jasmine.createSpy('getActiveWorkoutSummary').and.returnValue(of({ workout: null })),
    completeWorkout: jasmine
      .createSpy('completeWorkout')
      .and.returnValue(of({ workout: { ...inProgressWorkout, status: 'Completed' as const } })),
  };
  const workoutLiftsApiService = {
    listWorkoutLifts: jasmine.createSpy('listWorkoutLifts').and.returnValue(of({ items: [] })),
  };
  const route: Partial<ActivatedRoute> = {
    snapshot: {
      paramMap: convertToParamMap({ workoutId }),
      queryParamMap: convertToParamMap({ mode: 'historical' }),
    } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
    queryParamMap: of(convertToParamMap({ mode: 'historical' })),
  };
  const router = {
    events: of({}),
    createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue({}),
    serializeUrl: jasmine.createSpy('serializeUrl').and.returnValue('/'),
    navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)),
  };

  beforeEach(async () => {
    workoutsApiService.completeWorkout.calls.reset();
    workoutsStoreService.setHistoricalFlowMessage.calls.reset();
    router.navigate.calls.reset();

    await TestBed.configureTestingModule({
      imports: [ActiveWorkoutPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: Router, useValue: router },
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

  it('returns to preserved active workout after historical completion', () => {
    component.completeWorkout();
    fixture.detectChanges();

    expect(workoutsStoreService.setHistoricalFlowMessage).toHaveBeenCalledWith(
      'info',
      'Historical workout saved. Returned to your active workout in progress.',
    );
    expect(router.navigate).toHaveBeenCalledWith(['/workouts', 'active-1']);
  });

  it('shows a return affordance while in historical mode', () => {
    const returnButton = fixture.nativeElement.querySelector(
      '[data-testid="historical-return-to-active"]',
    ) as HTMLButtonElement | null;

    expect(returnButton).not.toBeNull();
    returnButton?.click();

    expect(router.navigate).toHaveBeenCalledWith(['/workouts', 'active-1']);
  });
});
