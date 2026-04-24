import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { HistoryPageComponent } from '../../../src/app/features/history/history-page.component';

describe('HistoryPageComponent', () => {
  let fixture: ComponentFixture<HistoryPageComponent>;

  const workoutsApiService = {
    getWorkoutHistory: jasmine.createSpy('getWorkoutHistory').and.returnValue(
      of({
        items: [
          {
            workoutId: 'history-1',
            label: 'Leg Day',
            completedAtUtc: '2026-04-22T15:30:00Z',
            durationDisplay: '00:45',
            liftCount: 4,
          },
          {
            workoutId: 'history-2',
            label: '   ',
            completedAtUtc: '2026-04-21T11:00:00Z',
            durationDisplay: '',
            liftCount: 0,
          },
        ],
      }),
    ),
  };

  beforeEach(async () => {
    workoutsApiService.getWorkoutHistory.calls.reset();
    workoutsApiService.getWorkoutHistory.and.returnValue(
      of({
        items: [
          {
            workoutId: 'history-1',
            label: 'Leg Day',
            completedAtUtc: '2026-04-22T15:30:00Z',
            durationDisplay: '00:45',
            liftCount: 4,
          },
          {
            workoutId: 'history-2',
            label: '   ',
            completedAtUtc: '2026-04-21T11:00:00Z',
            durationDisplay: '',
            liftCount: 0,
          },
        ],
      }),
    );

    await TestBed.configureTestingModule({
      imports: [HistoryPageComponent],
      providers: [
        { provide: WorkoutsApiService, useValue: workoutsApiService },
        provideRouter([]),
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HistoryPageComponent);
  });

  it('renders completed workout labels and row summary metadata', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    const durations = fixture.nativeElement.querySelectorAll('[data-testid="history-item-duration"]');
    const liftCounts = fixture.nativeElement.querySelectorAll('[data-testid="history-item-lift-count"]');

    expect(workoutsApiService.getWorkoutHistory).toHaveBeenCalled();
    expect(text).toContain('Leg Day');
    expect(text).toContain('Workout');
    expect(text).toContain('Duration 00:45');
    expect(text).toContain('Duration --:--');
    expect(text).toContain('4 lifts');
    expect(text).toContain('0 lifts');
    expect(fixture.nativeElement.querySelectorAll('[data-testid="history-item-label"]').length).toBe(2);
    const firstLink = fixture.nativeElement.querySelector('[data-testid="history-item-link-history-1"]') as HTMLAnchorElement;
    expect(firstLink.getAttribute('href')).toContain('/history/history-1');
    expect(durations.length).toBe(2);
    expect(liftCounts.length).toBe(2);
  });

  it('renders empty feedback when history is empty', () => {
    workoutsApiService.getWorkoutHistory.and.returnValue(of({ items: [] }));
    fixture = TestBed.createComponent(HistoryPageComponent);
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('No completed workouts yet.');
  });

  it('treats 404 history response as empty state', () => {
    workoutsApiService.getWorkoutHistory.and.returnValue(
      throwError(() => ({ status: 404 })),
    );
    fixture = TestBed.createComponent(HistoryPageComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('No completed workouts yet.');
    expect(text).not.toContain('Unable to load workout history right now.');
  });

  it('renders load error feedback when request fails', () => {
    workoutsApiService.getWorkoutHistory.and.returnValue(
      throwError(() => ({ status: 500 })),
    );
    fixture = TestBed.createComponent(HistoryPageComponent);
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('Unable to load workout history right now.');
  });
});
