import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

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
  const secondEntryId = 'entry-2';
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
  const secondLiftEntry: WorkoutLiftEntryState = {
    id: secondEntryId,
    workoutId,
    liftId: 'lift-2',
    displayName: 'Overhead Press',
    addedAtUtc: '2026-04-23T17:02:00Z',
    position: 2,
    sets: [],
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
    addWorkoutSet: jasmine.createSpy('addWorkoutSet').and.returnValue(
      of({
        workoutId,
        workoutLiftEntryId: entryId,
        set: {
          id: 'set-2',
          workoutLiftEntryId: entryId,
          setNumber: 2,
          reps: 10,
          weight: 145,
          createdAtUtc: '2026-04-23T17:07:00Z',
        },
      }),
    ),
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
    workoutLiftsApiService.addWorkoutSet.calls.reset();
    workoutLiftsApiService.addWorkoutSet.and.returnValue(
      of({
        workoutId,
        workoutLiftEntryId: entryId,
        set: {
          id: 'set-2',
          workoutLiftEntryId: entryId,
          setNumber: 2,
          reps: 10,
          weight: 145,
          createdAtUtc: '2026-04-23T17:07:00Z',
        },
      }),
    );
    workoutLiftsApiService.deleteWorkoutSet.calls.reset();
    workoutsStoreService.activeWorkout.set({
      id: workoutId,
      status: 'InProgress',
      label: 'Delete Day',
      startedAtUtc: '2026-04-23T17:00:00Z',
    });
    workoutsStoreService.activeWorkoutLiftEntries.set([liftEntry, secondLiftEntry]);
    workoutsStoreService.removeWorkoutSet.calls.reset();
    workoutsStoreService.appendWorkoutSet.calls.reset();

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

  it('prefills same entry draft with saved reps and weight after a successful add-set', () => {
    const savedSet: WorkoutSetEntry = {
      id: 'set-3',
      workoutLiftEntryId: entryId,
      setNumber: 3,
      reps: 12,
      weight: 155,
      createdAtUtc: '2026-04-23T17:10:00Z',
    };
    workoutLiftsApiService.addWorkoutSet.and.returnValue(
      of({ workoutId, workoutLiftEntryId: entryId, set: savedSet }),
    );
    component.updateAddSetReps(entryId, '8');
    component.updateAddSetWeight(entryId, '135');

    component.addSet(entryId);

    expect(workoutLiftsApiService.addWorkoutSet).toHaveBeenCalledWith(workoutId, entryId, { reps: 8, weight: 135 });
    expect(workoutsStoreService.appendWorkoutSet).toHaveBeenCalledWith(workoutId, entryId, savedSet);
    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '12', weight: '155' });
  });

  it('keeps add-set draft prefill isolated to the updated entry', () => {
    const savedSet: WorkoutSetEntry = {
      id: 'set-4',
      workoutLiftEntryId: entryId,
      setNumber: 4,
      reps: 9,
      weight: 140,
      createdAtUtc: '2026-04-23T17:11:00Z',
    };
    workoutLiftsApiService.addWorkoutSet.and.returnValue(
      of({ workoutId, workoutLiftEntryId: entryId, set: savedSet }),
    );
    component.updateAddSetReps(entryId, '6');
    component.updateAddSetWeight(entryId, '125');
    component.updateAddSetReps(secondEntryId, '5');
    component.updateAddSetWeight(secondEntryId, '95');

    component.addSet(entryId);

    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '9', weight: '140' });
    expect(component.getAddSetDraft(secondEntryId)).toEqual({ reps: '5', weight: '95' });
  });

  it('keeps weight blank but prefills reps when successful add-set returns null weight', () => {
    const savedSet: WorkoutSetEntry = {
      id: 'set-5',
      workoutLiftEntryId: entryId,
      setNumber: 5,
      reps: 15,
      weight: null,
      createdAtUtc: '2026-04-23T17:12:00Z',
    };
    workoutLiftsApiService.addWorkoutSet.and.returnValue(
      of({ workoutId, workoutLiftEntryId: entryId, set: savedSet }),
    );
    component.updateAddSetReps(entryId, '10');
    component.updateAddSetWeight(entryId, '');

    component.addSet(entryId);

    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '15', weight: '' });
  });

  it('preserves existing draft values after a failed add-set attempt', () => {
    workoutLiftsApiService.addWorkoutSet.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );
    component.updateAddSetReps(entryId, '11');
    component.updateAddSetWeight(entryId, '150');

    component.addSet(entryId);

    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '11', weight: '150' });
    expect(component.getAddSetError(entryId)).toBe('Set was not saved. Check your connection and try again.');
  });

  it('updates draft from successful response after a failed add-set retry', () => {
    const savedSet: WorkoutSetEntry = {
      id: 'set-6',
      workoutLiftEntryId: entryId,
      setNumber: 6,
      reps: 13,
      weight: 165,
      createdAtUtc: '2026-04-23T17:13:00Z',
    };
    workoutLiftsApiService.addWorkoutSet.and.returnValues(
      throwError(() => new HttpErrorResponse({ status: 500 })),
      of({ workoutId, workoutLiftEntryId: entryId, set: savedSet }),
    );
    component.updateAddSetReps(entryId, '7');
    component.updateAddSetWeight(entryId, '120');

    component.addSet(entryId);
    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '7', weight: '120' });

    component.addSet(entryId);

    expect(workoutLiftsApiService.addWorkoutSet).toHaveBeenCalledTimes(2);
    expect(component.getAddSetDraft(entryId)).toEqual({ reps: '13', weight: '165' });
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
