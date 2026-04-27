import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';

import { LiftsApiService } from '../../../src/app/core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../../src/app/core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../../src/app/core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../../src/app/core/state/workouts-store.service';
import type { WorkoutLiftEntryState, WorkoutSetEntry } from '../../../src/app/core/state/workouts-store.models';
import { ActiveWorkoutPageComponent } from '../../../src/app/features/workouts/active-workout-page.component';

describe('ActiveWorkoutPageComponent delete confirmation', () => {
  let fixture: ComponentFixture<ActiveWorkoutPageComponent>;
  let component: ActiveWorkoutPageComponent;

  const workoutId = 'workout-1';
  const entryId = 'entry-1';
  const setId = 'set-1';
  const setEntry: WorkoutSetEntry = {
    id: setId,
    workoutLiftEntryId: entryId,
    setNumber: 1,
    reps: 8,
    weight: 135,
    createdAtUtc: '2026-04-23T17:05:00Z',
  };
  const liftEntry: WorkoutLiftEntryState = {
    id: entryId,
    workoutId,
    liftId: 'lift-1',
    displayName: 'Bench Press',
    addedAtUtc: '2026-04-23T17:01:00Z',
    position: 1,
    sets: [setEntry],
  };

  const workoutsStoreService = {
    activeWorkout: signal<{
      id: string;
      status: string;
      label: string;
      startedAtUtc: string;
    }>({
      id: workoutId,
      status: 'InProgress',
      label: 'Delete Day',
      startedAtUtc: '2026-04-23T17:00:00Z',
    }),
    activeWorkoutLiftEntries: signal([liftEntry]),
    setActiveWorkout: jasmine.createSpy('setActiveWorkout'),
    setActiveWorkoutLiftEntries: jasmine.createSpy('setActiveWorkoutLiftEntries'),
    appendActiveWorkoutLiftEntry: jasmine.createSpy('appendActiveWorkoutLiftEntry'),
    removeActiveWorkoutLiftEntryById: jasmine.createSpy('removeActiveWorkoutLiftEntryById'),
    replaceActiveWorkoutLiftEntries: jasmine.createSpy('replaceActiveWorkoutLiftEntries'),
    appendWorkoutSet: jasmine.createSpy('appendWorkoutSet'),
    applyWorkoutSetUpdate: jasmine.createSpy('applyWorkoutSetUpdate'),
    removeWorkoutSet: jasmine.createSpy('removeWorkoutSet'),
  };

  const workoutsApiService = {
    getWorkout: jasmine
      .createSpy('getWorkout')
      .and.returnValue(of({ workout: { id: workoutId, status: 'InProgress', label: 'Delete Day', startedAtUtc: '2026-04-23T17:00:00Z' } })),
  };

  const workoutLiftsApiService = {
    listWorkoutLifts: jasmine.createSpy('listWorkoutLifts').and.returnValue(of({ items: [] })),
    deleteWorkoutSet: jasmine.createSpy('deleteWorkoutSet').and.returnValue(of({ workoutId, workoutLiftEntryId: entryId, setId })),
  };

  const route: Partial<ActivatedRoute> = {
    snapshot: { paramMap: convertToParamMap({ workoutId }) } as ActivatedRoute['snapshot'],
    paramMap: of(convertToParamMap({ workoutId })),
  };
  const dialog = {
    open: jasmine.createSpy('open').and.returnValue({
      afterClosed: () => of(false),
    }),
  };
  const router = {
    events: of({}),
    createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue({}),
    serializeUrl: jasmine.createSpy('serializeUrl').and.returnValue('/'),
    navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)),
  };

  beforeEach(async () => {
    workoutLiftsApiService.deleteWorkoutSet.calls.reset();
    workoutsStoreService.activeWorkout.set({
      id: workoutId,
      status: 'InProgress',
      label: 'Delete Day',
      startedAtUtc: '2026-04-23T17:00:00Z',
    });
    workoutsStoreService.activeWorkoutLiftEntries.set([liftEntry]);
    workoutsStoreService.removeWorkoutSet.calls.reset();

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
  });

  it('cancels pending set delete without mutating data', () => {
    component.beginSetDelete(entryId, setEntry);

    expect(component.isConfirmingSetDelete(entryId, setId)).toBeTrue();

    component.cancelSetDelete(entryId, setId);

    expect(component.isConfirmingSetDelete(entryId, setId)).toBeFalse();
    expect(workoutLiftsApiService.deleteWorkoutSet).not.toHaveBeenCalled();
    expect(workoutsStoreService.removeWorkoutSet).not.toHaveBeenCalled();
  });

  it('renders in-progress status badge text for active workouts', () => {
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('[data-testid="active-workout-status-badge"]') as HTMLElement | null;
    expect(badge).not.toBeNull();
    expect(badge?.textContent?.trim()).toBe('In Progress');
  });

  it('falls back to unknown tone mapping for unmapped statuses', () => {
    workoutsStoreService.activeWorkout.set({
      id: workoutId,
      status: 'Paused',
      label: 'Delete Day',
      startedAtUtc: '2026-04-23T17:00:00Z',
    });

    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('[data-testid="active-workout-status-badge"]') as HTMLElement | null;
    expect(badge).not.toBeNull();
    expect(badge?.textContent?.trim()).toBe('Paused');
    expect(badge?.classList.contains('active-workout__status-badge--unknown')).toBeTrue();
  });
});
