import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
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
    deleteWorkout: jasmine.createSpy('deleteWorkout').and.returnValue(
      of({
        workoutId: 'workout-1',
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
  const dialog = {
    open: jasmine.createSpy('open').and.returnValue({
      afterClosed: () => of(true),
    }),
  };

  beforeEach(async () => {
    workoutsApiService.getWorkout.calls.reset();
    workoutsApiService.deleteWorkout.calls.reset();
    workoutLiftsApiService.listWorkoutLifts.calls.reset();
    dialog.open.calls.reset();
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
        { provide: MatDialog, useValue: dialog },
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
    const badge = fixture.nativeElement.querySelector('[data-testid="history-workout-detail-status-badge"]') as HTMLElement | null;
    expect(workoutsApiService.getWorkout).toHaveBeenCalledWith('workout-1', true);
    expect(workoutLiftsApiService.listWorkoutLifts).toHaveBeenCalledWith('workout-1', true);
    expect(text).toContain('Heavy Day');
    expect(badge?.textContent?.trim()).toBe('Completed');
    expect(text).toContain('Duration 01:05');
    expect(text).toContain('Squat');
    expect(text).toContain('225 lb');
    expect(text).toContain('-');
  });

  it('renders sets sorted by set number for consistent historical display', () => {
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
                id: 'set-2',
                workoutLiftEntryId: 'lift-1',
                setNumber: 2,
                reps: 5,
                weight: 225,
                createdAtUtc: '2026-04-24T09:11:00Z',
                updatedAtUtc: '2026-04-24T09:11:00Z',
              },
              {
                id: 'set-1',
                workoutLiftEntryId: 'lift-1',
                setNumber: 1,
                reps: 8,
                weight: 185,
                createdAtUtc: '2026-04-24T09:06:00Z',
                updatedAtUtc: '2026-04-24T09:06:00Z',
              },
            ],
          },
        ],
      }),
    );
    fixture = TestBed.createComponent(HistoryWorkoutDetailPageComponent);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('[data-testid="history-workout-detail-sets"] tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].querySelectorAll('td')[0].textContent?.trim()).toBe('1');
    expect(rows[1].querySelectorAll('td')[0].textContent?.trim()).toBe('2');
  });

  it('uses unknown badge tone for unmapped workout statuses', () => {
    workoutsApiService.getWorkout.and.returnValue(
      of({
        workout: {
          id: 'workout-1',
          status: 'Archived',
          label: 'Heavy Day',
          startedAtUtc: '2026-04-24T09:00:00Z',
          completedAtUtc: '2026-04-24T10:05:00Z',
        },
      }),
    );

    fixture = TestBed.createComponent(HistoryWorkoutDetailPageComponent);
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('[data-testid="history-workout-detail-status-badge"]') as HTMLElement | null;
    expect(badge?.textContent?.trim()).toBe('Archived');
    expect(badge?.classList.contains('history-workout-detail-page__status-badge--unknown')).toBeTrue();
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

  it('opens confirmation flow and deletes workout from history detail', async () => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));

    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.beginDeleteWorkout();

    expect(dialog.open).toHaveBeenCalled();
    expect(workoutsApiService.deleteWorkout).toHaveBeenCalledWith('workout-1');
    expect(router.navigate).toHaveBeenCalledWith(['/history']);
  });

  it('surfaces delete error feedback when delete fails', async () => {
    workoutsApiService.deleteWorkout.and.returnValue(
      throwError(() => ({ status: 500 })),
    );

    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.beginDeleteWorkout();
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('Unable to delete workout. Check your connection and try again.');
  });
});
