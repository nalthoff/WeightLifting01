import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { WorkoutLiftsApiService } from '../../../src/app/core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { HistoryWorkoutDetailPageComponent } from '../../../src/app/features/history/history-workout-detail-page.component';

describe('HistoryWorkoutDetailPageComponent', () => {
  let fixture: ComponentFixture<HistoryWorkoutDetailPageComponent>;

  const workoutsApiService = {
    getWorkout: jasmine.createSpy('getWorkout').and.returnValue(
      of({
        workout: {
          id: 'workout-1',
          status: 'Completed' as const,
          label: 'Heavy Day',
          startedAtUtc: '2026-04-24T09:00:00Z',
          completedAtUtc: '2026-04-24T10:05:00Z',
        },
      }),
    ),
  };

  const workoutLiftsApiService = {
    listWorkoutLifts: jasmine.createSpy('listWorkoutLifts').and.returnValue(
      of({
        items: [
          {
            id: 'lift-1',
            workoutId: 'workout-1',
            liftId: 'squat',
            displayName: 'Squat',
            addedAtUtc: '2026-04-24T09:05:00Z',
            position: 0,
            sets: [
              {
                id: 'set-1',
                workoutLiftEntryId: 'lift-1',
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-24T09:06:00Z',
                updatedAtUtc: '2026-04-24T09:06:00Z',
              },
              {
                id: 'set-2',
                workoutLiftEntryId: 'lift-1',
                setNumber: 2,
                reps: 5,
                weight: null,
                createdAtUtc: '2026-04-24T09:11:00Z',
                updatedAtUtc: '2026-04-24T09:11:00Z',
              },
            ],
          },
        ],
      }),
    ),
  };

  beforeEach(async () => {
    workoutsApiService.getWorkout.calls.reset();
    workoutLiftsApiService.listWorkoutLifts.calls.reset();
    workoutsApiService.getWorkout.and.returnValue(
      of({
        workout: {
          id: 'workout-1',
          status: 'Completed' as const,
          label: 'Heavy Day',
          startedAtUtc: '2026-04-24T09:00:00Z',
          completedAtUtc: '2026-04-24T10:05:00Z',
        },
      }),
    );
    workoutLiftsApiService.listWorkoutLifts.and.returnValue(
      of({
        items: [
          {
            id: 'lift-1',
            workoutId: 'workout-1',
            liftId: 'squat',
            displayName: 'Squat',
            addedAtUtc: '2026-04-24T09:05:00Z',
            position: 0,
            sets: [
              {
                id: 'set-1',
                workoutLiftEntryId: 'lift-1',
                setNumber: 1,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-24T09:06:00Z',
                updatedAtUtc: '2026-04-24T09:06:00Z',
              },
              {
                id: 'set-2',
                workoutLiftEntryId: 'lift-1',
                setNumber: 2,
                reps: 5,
                weight: null,
                createdAtUtc: '2026-04-24T09:11:00Z',
                updatedAtUtc: '2026-04-24T09:11:00Z',
              },
            ],
          },
        ],
      }),
    );

    await TestBed.configureTestingModule({
      imports: [HistoryWorkoutDetailPageComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: WorkoutsApiService, useValue: workoutsApiService },
        { provide: WorkoutLiftsApiService, useValue: workoutLiftsApiService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => (key === 'workoutId' ? 'workout-1' : null),
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HistoryWorkoutDetailPageComponent);
  });

  it('loads and renders completed workout detail data', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(workoutsApiService.getWorkout).toHaveBeenCalledWith('workout-1', true);
    expect(workoutLiftsApiService.listWorkoutLifts).toHaveBeenCalledWith('workout-1', true);
    expect(text).toContain('Heavy Day');
    expect(text).toContain('Duration 01:05');
    expect(text).toContain('Squat');
    expect(text).toContain('225 lb');
    expect(text).toContain('-');
  });

  it('renders a not-found style message on 404 failures', () => {
    workoutsApiService.getWorkout.and.returnValue(throwError(() => ({ status: 404 })));
    fixture = TestBed.createComponent(HistoryWorkoutDetailPageComponent);
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('This completed workout is no longer available');
  });

  it('renders retryable load error on non-404 failures', () => {
    workoutLiftsApiService.listWorkoutLifts.and.returnValue(throwError(() => ({ status: 500 })));
    fixture = TestBed.createComponent(HistoryWorkoutDetailPageComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Unable to load workout details right now');
    expect(text).toContain('Try again');
  });
});
