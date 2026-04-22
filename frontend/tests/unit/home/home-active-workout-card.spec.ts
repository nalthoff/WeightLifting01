import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../../src/app/core/state/workouts-store.service';
import { HomePageComponent } from '../../../src/app/features/home/home-page.component';

describe('HomePageComponent active workout card', () => {
  let fixture: ComponentFixture<HomePageComponent>;

  const inProgressWorkout = {
    id: 'workout-1',
    status: 'InProgress' as const,
    label: 'Upper Day',
    startedAtUtc: '2026-04-22T15:00:00Z',
  };

  const workoutsApiService = {
    startWorkout: jasmine.createSpy('startWorkout').and.returnValue(of({ workout: inProgressWorkout })),
    getActiveWorkoutSummary: jasmine
      .createSpy('getActiveWorkoutSummary')
      .and.returnValue(of({ workout: inProgressWorkout })),
    completeWorkout: jasmine
      .createSpy('completeWorkout')
      .and.returnValue(of({ workout: { ...inProgressWorkout, status: 'Completed' as const } })),
  };

  const workoutsStoreService = {
    activeWorkout: signal<typeof inProgressWorkout | null>(null),
    activeWorkoutLiftEntries: signal([]),
    hasActiveWorkout: signal(false),
    setActiveWorkout: jasmine.createSpy('setActiveWorkout').and.callFake((workout: typeof inProgressWorkout) => {
      workoutsStoreService.activeWorkout.set(workout);
      workoutsStoreService.hasActiveWorkout.set(true);
    }),
    reconcileActiveWorkout: jasmine
      .createSpy('reconcileActiveWorkout')
      .and.callFake((workout: typeof inProgressWorkout | { status: 'Completed' } | null) => {
        if (!workout || workout.status !== 'InProgress') {
          workoutsStoreService.activeWorkout.set(null);
          workoutsStoreService.hasActiveWorkout.set(false);
          return;
        }

        workoutsStoreService.activeWorkout.set(workout);
        workoutsStoreService.hasActiveWorkout.set(true);
      }),
    clearActiveWorkout: jasmine.createSpy('clearActiveWorkout').and.callFake(() => {
      workoutsStoreService.activeWorkout.set(null);
      workoutsStoreService.hasActiveWorkout.set(false);
    }),
  };

  const router = {
    navigate: jasmine.createSpy('navigate').and.resolveTo(true),
  };

  beforeEach(async () => {
    workoutsApiService.startWorkout.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.calls.reset();
    workoutsApiService.getActiveWorkoutSummary.and.returnValue(of({ workout: inProgressWorkout }));
    workoutsApiService.completeWorkout.calls.reset();
    workoutsApiService.completeWorkout.and.returnValue(
      of({ workout: { ...inProgressWorkout, status: 'Completed' as const } }),
    );
    workoutsStoreService.setActiveWorkout.calls.reset();
    workoutsStoreService.reconcileActiveWorkout.calls.reset();
    workoutsStoreService.clearActiveWorkout.calls.reset();
    workoutsStoreService.activeWorkout.set(null);
    workoutsStoreService.hasActiveWorkout.set(false);
    router.navigate.calls.reset();

    await TestBed.configureTestingModule({
      imports: [HomePageComponent],
      providers: [
        { provide: WorkoutsApiService, useValue: workoutsApiService },
        { provide: WorkoutsStoreService, useValue: workoutsStoreService },
        { provide: Router, useValue: router },
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomePageComponent);
  });

  it('renders active workout card with fallback title and continue action', () => {
    workoutsApiService.getActiveWorkoutSummary.and.returnValue(of({ workout: { ...inProgressWorkout, label: '' } }));
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Workout');
    expect(text).toContain('Continue');
    expect(fixture.nativeElement.querySelector('[data-testid="home-active-workout-continue"]')).not.toBeNull();
  });

  it('continues active workout to detail route', () => {
    fixture.detectChanges();

    const continueButton = fixture.nativeElement.querySelector(
      '[data-testid="home-active-workout-continue"]',
    ) as HTMLButtonElement;
    continueButton.click();

    expect(workoutsStoreService.setActiveWorkout).toHaveBeenCalledWith(inProgressWorkout);
    expect(router.navigate).toHaveBeenCalledWith(['/workouts', inProgressWorkout.id]);
  });

  it('completes active workout and shows success feedback while staying home', () => {
    fixture.detectChanges();

    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="home-active-workout-complete"]',
    ) as HTMLButtonElement;
    completeButton.click();
    fixture.detectChanges();

    expect(workoutsApiService.completeWorkout).toHaveBeenCalledWith(inProgressWorkout.id);
    expect(workoutsStoreService.reconcileActiveWorkout).toHaveBeenCalled();
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalledTimes(2);
    expect((fixture.nativeElement.textContent as string)).toContain('Workout completed. Great work.');
    expect(router.navigate).not.toHaveBeenCalledWith(['/workouts', inProgressWorkout.id]);
  });

  it('shows completion error feedback and refreshes active workout state', () => {
    workoutsApiService.completeWorkout.and.returnValue(
      throwError(() => ({ status: 500, error: { title: 'Server unavailable' } })),
    );
    fixture.detectChanges();

    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="home-active-workout-complete"]',
    ) as HTMLButtonElement;
    completeButton.click();
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain(
      'Unable to complete workout. Check your connection and try again.',
    );
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalledTimes(2);
  });

  it('uses race-state response title for recoverable feedback and refresh', () => {
    workoutsApiService.completeWorkout.and.returnValue(
      throwError(() => ({ status: 409, error: { title: 'Workout already completed elsewhere.' } })),
    );
    fixture.detectChanges();

    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="home-active-workout-complete"]',
    ) as HTMLButtonElement;
    completeButton.click();
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('Workout already completed elsewhere.');
    expect(workoutsApiService.getActiveWorkoutSummary).toHaveBeenCalledTimes(2);
  });

  it('prevents duplicate complete requests while loading', () => {
    workoutsApiService.completeWorkout.and.returnValue(of({ workout: { ...inProgressWorkout, status: 'Completed' as const } }));
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.isCompletingWorkout.set(true);
    const completeButton = fixture.nativeElement.querySelector(
      '[data-testid="home-active-workout-complete"]',
    ) as HTMLButtonElement;

    expect(completeButton.disabled).toBeTrue();
    component.completeActiveWorkout();

    expect(workoutsApiService.completeWorkout).not.toHaveBeenCalled();
  });
});
