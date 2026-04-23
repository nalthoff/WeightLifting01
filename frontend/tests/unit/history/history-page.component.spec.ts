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
            id: 'history-1',
            label: 'Leg Day',
            completedAtUtc: '2026-04-22T15:30:00Z',
          },
          {
            id: 'history-2',
            label: '   ',
            completedAtUtc: '2026-04-21T11:00:00Z',
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
            id: 'history-1',
            label: 'Leg Day',
            completedAtUtc: '2026-04-22T15:30:00Z',
          },
          {
            id: 'history-2',
            label: '   ',
            completedAtUtc: '2026-04-21T11:00:00Z',
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

  it('renders completed workout labels and fallback label', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(workoutsApiService.getWorkoutHistory).toHaveBeenCalled();
    expect(text).toContain('Leg Day');
    expect(text).toContain('Workout');
    expect(fixture.nativeElement.querySelectorAll('[data-testid="history-item-label"]').length).toBe(2);
  });

  it('renders empty feedback when history is empty', () => {
    workoutsApiService.getWorkoutHistory.and.returnValue(of({ items: [] }));
    fixture = TestBed.createComponent(HistoryPageComponent);
    fixture.detectChanges();

    expect((fixture.nativeElement.textContent as string)).toContain('No completed workouts yet.');
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
