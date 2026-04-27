import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { finalize, map } from 'rxjs';

import { LiftsApiService } from '../../core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import type {
  InlineLiftHistoryPanelState,
  LiftHistorySessionSummary,
  SetRowDeleteSession,
  SetRowEditSession,
  WorkoutDeleteSession,
  WorkoutLiftEntryState,
  WorkoutSetEntry,
} from '../../core/state/workouts-store.models';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';
import { WorkoutDeleteConfirmDialogComponent } from './workout-delete-confirm-dialog.component';
import { WorkoutRenameDialogComponent } from './workout-rename-dialog.component';

@Component({
  selector: 'app-active-workout-page',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './active-workout-page.component.html',
  styleUrl: './active-workout-page.component.scss',
})
export class ActiveWorkoutPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly liftsApiService = inject(LiftsApiService);
  private readonly workoutLiftsApiService = inject(WorkoutLiftsApiService);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);

  readonly isLoadingWorkout = signal(false);
  readonly isRefreshingActiveWorkout = signal(false);
  readonly isCompletingWorkout = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly completeErrorMessage = signal<string | null>(null);
  readonly completeSuccessMessage = signal<string | null>(null);
  readonly workoutNameError = signal<string | null>(null);
  readonly workoutNameSuccess = signal<string | null>(null);
  readonly isSavingWorkoutName = signal(false);
  readonly deleteWorkoutSession = signal<WorkoutDeleteSession | null>(null);
  readonly deleteErrorMessage = signal<string | null>(null);
  readonly deleteSuccessMessage = signal<string | null>(null);
  readonly isLoadingWorkoutLifts = signal(false);
  readonly workoutLiftsLoadError = signal<string | null>(null);
  readonly isPickerOpen = signal(false);
  readonly isLoadingActiveLifts = signal(false);
  readonly activeLiftsLoadError = signal<string | null>(null);
  readonly activeLiftOptions = signal<{ id: string; name: string }[]>([]);
  readonly selectedLiftId = signal<string>('');
  readonly isAddingLift = signal(false);
  readonly addLiftError = signal<string | null>(null);
  readonly isRemovingLift = signal(false);
  readonly removingWorkoutLiftEntryId = signal<string | null>(null);
  readonly removeLiftError = signal<string | null>(null);
  readonly isReorderingLift = signal(false);
  readonly reorderLiftError = signal<string | null>(null);
  readonly addSetErrorByEntryId = signal<Record<string, string | null>>({});
  readonly isAddingSetByEntryId = signal<Record<string, boolean>>({});
  readonly addSetDraftByEntryId = signal<Record<string, { reps: string; weight: string }>>({});
  readonly inlineHistoryStateByEntryId = signal<Record<string, InlineLiftHistoryPanelState>>({});
  readonly setEditSessionByKey = signal<Record<string, SetRowEditSession>>({});
  readonly setDeleteSessionByKey = signal<Record<string, SetRowDeleteSession>>({});
  private readonly routeWorkoutId = signal<string | null>(this.route.snapshot.paramMap.get('workoutId'));
  readonly workoutId = this.routeWorkoutId.asReadonly();
  readonly workout = computed(() => {
    const activeWorkout = this.workoutsStoreService.activeWorkout();
    const workoutId = this.workoutId();

    if (!activeWorkout || !workoutId || activeWorkout.id !== workoutId) {
      return null;
    }

    return activeWorkout;
  });
  readonly workoutLiftEntries = computed(() => {
    const workout = this.workout();
    if (!workout) {
      return [];
    }

    return this.workoutsStoreService.activeWorkoutLiftEntries();
  });
  private readonly statusBadgeByCode: Record<string, { label: string; tone: 'progress' | 'complete' | 'unknown' }> = {
    InProgress: { label: 'In Progress', tone: 'progress' },
    Completed: { label: 'Completed', tone: 'complete' },
  };

  constructor() {
    this.ensureWorkoutLoaded(this.workoutId());

    this.route.paramMap
      .pipe(
        map((params) => params.get('workoutId')),
        takeUntilDestroyed(),
      )
      .subscribe((workoutId) => {
        this.routeWorkoutId.set(workoutId);
        this.ensureWorkoutLoaded(workoutId);
      });
  }

  openAddLiftPicker(): void {
    if (!this.workout()) {
      return;
    }

    this.isPickerOpen.set(true);
    this.addLiftError.set(null);

    if (this.activeLiftOptions().length === 0) {
      this.loadActiveLifts();
    }
  }

  completeWorkout(): void {
    const workout = this.workout();
    if (!workout || workout.status !== 'InProgress' || this.isCompletingWorkout()) {
      return;
    }

    this.isCompletingWorkout.set(true);
    this.completeErrorMessage.set(null);
    this.completeSuccessMessage.set(null);

    this.workoutsApiService
      .completeWorkout(workout.id)
      .pipe(finalize(() => this.isCompletingWorkout.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.reconcileActiveWorkout(response.workout);
          this.completeSuccessMessage.set('Workout completed. Great work.');
          this.completeErrorMessage.set(null);
          this.refreshActiveWorkoutState();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404 || error.status === 409) {
            this.completeErrorMessage.set(error.error?.title ?? 'Workout state changed. View refreshed.');
            this.refreshActiveWorkoutState();
            return;
          }

          this.completeErrorMessage.set('Unable to complete workout. Check your connection and try again.');
          this.refreshActiveWorkoutState();
        },
      });
  }

  canEditWorkoutName(): boolean {
    return this.workout()?.status === 'InProgress';
  }

  beginRenameWorkout(): void {
    const workout = this.workout();
    if (!workout || !this.canEditWorkoutName() || this.isSavingWorkoutName()) {
      return;
    }

    this.workoutNameError.set(null);
    this.workoutNameSuccess.set(null);

    this.dialog
      .open(WorkoutRenameDialogComponent, {
        autoFocus: false,
        data: {
          initialLabel: workout.label ?? '',
        },
      })
      .afterClosed()
      .subscribe((label: string | undefined) => {
        if (typeof label !== 'string') {
          return;
        }

        this.saveWorkoutName(label);
      });
  }

  private saveWorkoutName(label: string): void {
    const workout = this.workout();
    if (!workout || !this.canEditWorkoutName() || this.isSavingWorkoutName()) {
      return;
    }

    this.isSavingWorkoutName.set(true);
    this.workoutNameError.set(null);
    this.workoutNameSuccess.set(null);

    this.workoutsApiService
      .updateWorkoutLabel(workout.id, {
        label,
      })
      .pipe(finalize(() => this.isSavingWorkoutName.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.reconcileActiveWorkout(response.workout);
          this.workoutNameError.set(null);
          this.workoutNameSuccess.set(response.workout.label ? 'Workout name saved.' : 'Workout name cleared.');
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.workoutNameError.set('This workout no longer exists. Refresh and try again.');
            this.refreshActiveWorkoutState();
            return;
          }

          if (error.status === 409) {
            this.workoutNameError.set(error.error?.title ?? 'Only in-progress workouts can edit name.');
            this.refreshActiveWorkoutState();
            return;
          }

          if (error.status === 422) {
            this.workoutNameError.set(error.error?.errors?.label?.[0] ?? 'Workout name is invalid.');
            return;
          }

          this.workoutNameError.set('Workout name was not saved. Check your connection and try again.');
        },
      });
  }

  beginDeleteWorkout(): void {
    const workout = this.workout();
    if (!workout || workout.status !== 'InProgress') {
      return;
    }

    const session = this.deleteWorkoutSession();
    if (session?.isDeleting) {
      return;
    }

    this.deleteErrorMessage.set(null);
    this.deleteSuccessMessage.set(null);

    this.dialog
      .open(WorkoutDeleteConfirmDialogComponent, {
        autoFocus: false,
      })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed !== true) {
          return;
        }

        this.confirmDeleteWorkout();
      });
  }

  cancelDeleteWorkout(): void {
    const session = this.deleteWorkoutSession();
    if (session?.isDeleting) {
      return;
    }

    this.deleteWorkoutSession.set(null);
    this.deleteErrorMessage.set(null);
  }

  confirmDeleteWorkout(): void {
    const workout = this.workout();
    const session = this.deleteWorkoutSession();
    if (!workout || workout.status !== 'InProgress' || session?.isDeleting) {
      return;
    }

    this.deleteWorkoutSession.set({
      workoutId: workout.id,
      isConfirmingDelete: false,
      isDeleting: true,
      errorMessage: null,
    });
    this.deleteErrorMessage.set(null);
    this.deleteSuccessMessage.set(null);

    this.workoutsApiService
      .deleteWorkout(workout.id)
      .pipe(
        finalize(() => {
          const latestSession = this.deleteWorkoutSession();
          if (latestSession?.workoutId === workout.id) {
            this.deleteWorkoutSession.set({
              ...latestSession,
              isDeleting: false,
            });
          }
        }),
      )
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.clearActiveWorkoutIfMatches(response.workoutId);
          this.deleteWorkoutSession.set(null);
          this.deleteErrorMessage.set(null);
          this.deleteSuccessMessage.set(null);
          this.completeErrorMessage.set(null);
          this.completeSuccessMessage.set(null);
          void this.router.navigate(['/']);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            const message = error.error?.title ?? 'This workout no longer exists. View refreshed.';
            this.deleteErrorMessage.set(message);
            this.refreshActiveWorkoutState();
            return;
          }

          if (error.status === 409) {
            const message = error.error?.title ?? 'This workout cannot be deleted in its current state.';
            this.deleteErrorMessage.set(message);
            this.refreshActiveWorkoutState();
            return;
          }

          const message = 'Unable to delete workout. Check your connection and try again.';
          this.deleteErrorMessage.set(message);
        },
      });
  }

  closeAddLiftPicker(): void {
    this.isPickerOpen.set(false);
    this.addLiftError.set(null);
  }

  updateSelectedLiftId(liftId: string): void {
    this.selectedLiftId.set(liftId);
    this.addLiftError.set(null);
  }

  addSelectedLift(): void {
    if (this.isAddingLift()) {
      return;
    }

    const workoutId = this.workoutId();
    const liftId = this.selectedLiftId();

    if (!workoutId || !liftId) {
      this.addLiftError.set('Select a lift to add.');
      return;
    }

    this.isAddingLift.set(true);
    this.addLiftError.set(null);

    this.workoutLiftsApiService
      .addWorkoutLift(workoutId, { liftId })
      .pipe(finalize(() => this.isAddingLift.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.appendActiveWorkoutLiftEntry(response.workoutLift);
          this.isPickerOpen.set(false);
          this.addLiftError.set(null);
          this.removeLiftError.set(null);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.addLiftError.set('This workout or lift no longer exists. Refresh and try again.');
            return;
          }

          if (error.status === 409 || error.status === 422) {
            this.addLiftError.set(
              error.error?.title ?? 'This lift could not be added in the workout\'s current state.',
            );
            return;
          }

          this.addLiftError.set('Lift was not added. Check your connection and try again.');
        },
      });
  }

  removeWorkoutLiftEntry(workoutLiftEntryId: string): void {
    if (this.isRemovingLift()) {
      return;
    }

    const workoutId = this.workoutId();
    if (!workoutId) {
      this.removeLiftError.set('Workout ID is missing.');
      return;
    }

    this.isRemovingLift.set(true);
    this.removingWorkoutLiftEntryId.set(workoutLiftEntryId);
    this.removeLiftError.set(null);
    this.reorderLiftError.set(null);

    this.workoutLiftsApiService
      .removeWorkoutLift(workoutId, workoutLiftEntryId)
      .pipe(
        finalize(() => {
          this.isRemovingLift.set(false);
          this.removingWorkoutLiftEntryId.set(null);
        }),
      )
      .subscribe({
        next: (response) => {
          const removedEntryId = response.workoutLiftEntryId ?? response.removedWorkoutLiftEntryId;
          if (!removedEntryId) {
            this.removeLiftError.set('Lift was removed, but the list could not be refreshed. Reload and try again.');
            return;
          }

          this.workoutsStoreService.removeActiveWorkoutLiftEntryById(
            response.workoutId,
            removedEntryId,
          );
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.removeLiftError.set('This lift entry was already removed or no longer exists. Refresh and try again.');
            return;
          }

          if (error.status === 409) {
            this.removeLiftError.set(
              error.error?.title ?? 'This workout is not in a removable state right now. Try again in a moment.',
            );
            return;
          }

          this.removeLiftError.set('Lift was not removed. Check your connection and try again.');
        },
      });
  }

  canMoveLiftUp(index: number): boolean {
    return index > 0 && !this.isReorderingLift();
  }

  canMoveLiftDown(index: number): boolean {
    return index < this.workoutLiftEntries().length - 1 && !this.isReorderingLift();
  }

  moveLiftUp(workoutLiftEntryId: string, currentIndex: number): void {
    if (!this.canMoveLiftUp(currentIndex)) {
      return;
    }

    this.reorderLift(workoutLiftEntryId, currentIndex - 1);
  }

  moveLiftDown(workoutLiftEntryId: string, currentIndex: number): void {
    if (!this.canMoveLiftDown(currentIndex)) {
      return;
    }

    this.reorderLift(workoutLiftEntryId, currentIndex + 1);
  }

  trackWorkoutLiftEntry(_index: number, entry: { id: string }): string {
    return entry.id;
  }

  trackWorkoutSet(_index: number, setEntry: WorkoutSetEntry): string {
    return setEntry.id;
  }

  getWorkoutStatusBadge(status: string | null | undefined): { label: string; tone: 'progress' | 'complete' | 'unknown' } {
    if (!status) {
      return { label: 'Unknown', tone: 'unknown' };
    }

    return this.statusBadgeByCode[status] ?? { label: status, tone: 'unknown' };
  }

  getAddSetDraft(entryId: string): { reps: string; weight: string } {
    return this.addSetDraftByEntryId()[entryId] ?? { reps: '', weight: '' };
  }

  updateAddSetReps(entryId: string, repsValue: string): void {
    this.updateAddSetDraft(entryId, { reps: repsValue });
    this.setAddSetError(entryId, null);
  }

  updateAddSetWeight(entryId: string, weightValue: string): void {
    this.updateAddSetDraft(entryId, { weight: weightValue });
    this.setAddSetError(entryId, null);
  }

  isAddingSet(entryId: string): boolean {
    return this.isAddingSetByEntryId()[entryId] === true;
  }

  getAddSetError(entryId: string): string | null {
    return this.addSetErrorByEntryId()[entryId] ?? null;
  }

  addSet(entryId: string): void {
    if (this.isAddingSet(entryId)) {
      return;
    }

    const workoutId = this.workoutId();
    if (!workoutId) {
      this.setAddSetError(entryId, 'Workout ID is missing.');
      return;
    }

    const draft = this.getAddSetDraft(entryId);
    const reps = Number(draft.reps);
    if (!Number.isInteger(reps) || reps < 1) {
      this.setAddSetError(entryId, 'Reps are required and must be at least 1.');
      return;
    }

    let weight: number | null = null;
    if (draft.weight.trim().length > 0) {
      const parsedWeight = Number(draft.weight);
      if (!Number.isFinite(parsedWeight) || parsedWeight < 0) {
        this.setAddSetError(entryId, 'Weight must be 0 or greater when provided.');
        return;
      }

      weight = parsedWeight;
    }

    this.setIsAddingSet(entryId, true);
    this.setAddSetError(entryId, null);

    this.workoutLiftsApiService
      .addWorkoutSet(workoutId, entryId, { reps, weight })
      .pipe(finalize(() => this.setIsAddingSet(entryId, false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.appendWorkoutSet(response.workoutId, response.workoutLiftEntryId, response.set);
          this.updateAddSetDraft(entryId, {
            reps: String(response.set.reps),
            weight: response.set.weight === null ? '' : String(response.set.weight),
          });
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.setAddSetError(entryId, 'This workout entry no longer exists. Refresh and try again.');
            return;
          }

          if (error.status === 409 || error.status === 422) {
            this.setAddSetError(
              entryId,
              error.error?.title ?? 'This set could not be added in the workout\'s current state.',
            );
            return;
          }

          this.setAddSetError(entryId, 'Set was not saved. Check your connection and try again.');
        },
      });
  }

  getSetsForEntry(entry: WorkoutLiftEntryState): WorkoutSetEntry[] {
    return entry.sets;
  }

  canEditWorkoutSetRows(): boolean {
    return this.workout()?.status === 'InProgress';
  }

  isEditingSetRow(entryId: string, setId: string): boolean {
    return this.getSetEditSession(entryId, setId) !== null;
  }

  beginSetEdit(entryId: string, setEntry: WorkoutSetEntry): void {
    if (!this.canEditWorkoutSetRows()) {
      return;
    }

    const rowKey = this.buildSetRowKey(entryId, setEntry.id);
    this.setEditSessionByKey.update((current) => ({
      ...current,
      [rowKey]: {
        setId: setEntry.id,
        draftReps: String(setEntry.reps),
        draftWeight: setEntry.weight === null ? '' : String(setEntry.weight),
        isSaving: false,
        errorMessage: null,
        isDirty: false,
      },
    }));
  }

  cancelSetEdit(entryId: string, setId: string): void {
    const rowKey = this.buildSetRowKey(entryId, setId);
    this.setEditSessionByKey.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([key]) => key !== rowKey)),
    );
  }

  getSetEditSession(entryId: string, setId: string): SetRowEditSession | null {
    return this.setEditSessionByKey()[this.buildSetRowKey(entryId, setId)] ?? null;
  }

  updateSetEditReps(entryId: string, setEntry: WorkoutSetEntry, repsValue: string): void {
    const session = this.getSetEditSession(entryId, setEntry.id);
    if (!session || session.isSaving) {
      return;
    }

    this.patchSetEditSession(entryId, setEntry, {
      draftReps: repsValue,
      errorMessage: null,
      isDirty: this.isSetDraftDirty(setEntry, repsValue, session.draftWeight),
    });
  }

  updateSetEditWeight(entryId: string, setEntry: WorkoutSetEntry, weightValue: string): void {
    const session = this.getSetEditSession(entryId, setEntry.id);
    if (!session || session.isSaving) {
      return;
    }

    this.patchSetEditSession(entryId, setEntry, {
      draftWeight: weightValue,
      errorMessage: null,
      isDirty: this.isSetDraftDirty(setEntry, session.draftReps, weightValue),
    });
  }

  saveSetEdit(entryId: string, setEntry: WorkoutSetEntry): void {
    const session = this.getSetEditSession(entryId, setEntry.id);
    if (!session || session.isSaving) {
      return;
    }

    if (!this.canEditWorkoutSetRows()) {
      this.patchSetEditSession(entryId, setEntry, {
        errorMessage: 'Only in-progress workouts can be edited.',
      });
      return;
    }

    const workoutId = this.workoutId();
    if (!workoutId) {
      this.patchSetEditSession(entryId, setEntry, {
        errorMessage: 'Workout ID is missing.',
      });
      return;
    }

    const reps = Number(session.draftReps);
    if (!Number.isInteger(reps) || reps < 1) {
      this.patchSetEditSession(entryId, setEntry, {
        errorMessage: 'Reps are required and must be at least 1.',
      });
      return;
    }

    let weight: number | null = null;
    if (session.draftWeight.trim().length > 0) {
      const parsedWeight = Number(session.draftWeight);
      if (!Number.isFinite(parsedWeight) || parsedWeight < 0) {
        this.patchSetEditSession(entryId, setEntry, {
          errorMessage: 'Weight must be 0 or greater when provided.',
        });
        return;
      }

      weight = parsedWeight;
    }

    this.patchSetEditSession(entryId, setEntry, {
      isSaving: true,
      errorMessage: null,
    });

    this.workoutLiftsApiService
      .updateWorkoutSet(workoutId, entryId, setEntry.id, { reps, weight })
      .pipe(finalize(() => this.patchSetEditSession(entryId, setEntry, { isSaving: false })))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.applyWorkoutSetUpdate(response.workoutId, response.workoutLiftEntryId, response.set);
          this.cancelSetEdit(entryId, setEntry.id);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.patchSetEditSession(entryId, setEntry, {
              errorMessage: 'This set no longer exists in this workout entry. Refresh and try again.',
            });
            return;
          }

          if (error.status === 409 || error.status === 422) {
            this.patchSetEditSession(entryId, setEntry, {
              errorMessage: error.error?.title ?? 'This set could not be updated in the workout\'s current state.',
            });
            return;
          }

          this.patchSetEditSession(entryId, setEntry, {
            errorMessage: 'Set changes were not saved. Check your connection and retry.',
          });
        },
      });
  }

  isConfirmingSetDelete(entryId: string, setId: string): boolean {
    const session = this.getSetDeleteSession(entryId, setId);
    return session?.isConfirmingDelete === true;
  }

  beginSetDelete(entryId: string, setEntry: WorkoutSetEntry): void {
    if (!this.canEditWorkoutSetRows()) {
      return;
    }

    const rowKey = this.buildSetRowKey(entryId, setEntry.id);
    this.setDeleteSessionByKey.update((current) => ({
      ...current,
      [rowKey]: {
        setId: setEntry.id,
        isConfirmingDelete: true,
        isDeleting: false,
        errorMessage: null,
      },
    }));
  }

  cancelSetDelete(entryId: string, setId: string): void {
    const rowKey = this.buildSetRowKey(entryId, setId);
    this.setDeleteSessionByKey.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([key]) => key !== rowKey)),
    );
  }

  getSetDeleteSession(entryId: string, setId: string): SetRowDeleteSession | null {
    return this.setDeleteSessionByKey()[this.buildSetRowKey(entryId, setId)] ?? null;
  }

  confirmSetDelete(entryId: string, setEntry: WorkoutSetEntry): void {
    const session = this.getSetDeleteSession(entryId, setEntry.id);
    if (!session || session.isDeleting) {
      return;
    }

    if (!this.canEditWorkoutSetRows()) {
      this.patchSetDeleteSession(entryId, setEntry.id, {
        errorMessage: 'Only in-progress workouts can remove sets.',
      });
      return;
    }

    const workoutId = this.workoutId();
    if (!workoutId) {
      this.patchSetDeleteSession(entryId, setEntry.id, {
        errorMessage: 'Workout ID is missing.',
      });
      return;
    }

    this.patchSetDeleteSession(entryId, setEntry.id, {
      isDeleting: true,
      errorMessage: null,
    });

    this.workoutLiftsApiService
      .deleteWorkoutSet(workoutId, entryId, setEntry.id)
      .pipe(finalize(() => this.patchSetDeleteSession(entryId, setEntry.id, { isDeleting: false })))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.removeWorkoutSet(response.workoutId, response.workoutLiftEntryId, response.setId);
          this.cancelSetDelete(entryId, setEntry.id);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.patchSetDeleteSession(entryId, setEntry.id, {
              errorMessage: 'This set no longer exists in this workout entry. Refresh and try again.',
            });
            return;
          }

          if (error.status === 409) {
            this.patchSetDeleteSession(entryId, setEntry.id, {
              errorMessage: error.error?.title ?? 'This workout is not in a removable state right now.',
            });
            return;
          }

          this.patchSetDeleteSession(entryId, setEntry.id, {
            errorMessage: 'Set was not removed. Check your connection and retry.',
          });
        },
      });
  }

  formatSetWeight(setEntry: WorkoutSetEntry): string {
    if (setEntry.weight === null) {
      return '-';
    }

    return `${setEntry.weight} lb`;
  }

  toggleLiftHistory(entryId: string): void {
    const state = this.getInlineHistoryState(entryId);
    if (state.isExpanded) {
      this.patchInlineHistoryState(entryId, { isExpanded: false });
      return;
    }

    this.patchInlineHistoryState(entryId, { isExpanded: true });
    this.loadInlineLiftHistory(entryId);
  }

  isLiftHistoryExpanded(entryId: string): boolean {
    return this.getInlineHistoryState(entryId).isExpanded;
  }

  isLiftHistoryLoading(entryId: string): boolean {
    return this.getInlineHistoryState(entryId).isLoading;
  }

  getLiftHistoryError(entryId: string): string | null {
    return this.getInlineHistoryState(entryId).errorMessage;
  }

  getLiftHistoryItems(entryId: string): LiftHistorySessionSummary[] {
    return this.getInlineHistoryState(entryId).items;
  }

  getLiftHistorySummaryLines(item: LiftHistorySessionSummary): string[] {
    if (item.sets.length === 0) {
      return ['0 x 0 @ -'];
    }

    const setsByWeight = new Map<string, { weight: number | null; reps: number; setCount: number }>();
    for (const setItem of item.sets) {
      const key = setItem.weight === null ? 'bodyweight' : String(setItem.weight);
      const existing = setsByWeight.get(key);
      if (existing) {
        existing.setCount += 1;
        continue;
      }

      setsByWeight.set(key, {
        weight: setItem.weight,
        reps: setItem.reps,
        setCount: 1,
      });
    }

    return [...setsByWeight.values()].map((summary) =>
      `${summary.setCount} x ${summary.reps} @ ${this.formatHistoryWeight(summary.weight)}`,
    );
  }

  formatHistoryWeight(weight: number | null): string {
    if (weight === null) {
      return '-';
    }

    return `${weight} lb`;
  }

  private ensureWorkoutLoaded(workoutId: string | null): void {
    if (!workoutId) {
      this.loadError.set('Workout ID is missing.');
      return;
    }

    const currentWorkout = this.workoutsStoreService.activeWorkout();
    if (currentWorkout?.id === workoutId) {
      this.loadWorkoutLifts(workoutId);
      return;
    }

    this.isLoadingWorkout.set(true);
    this.loadError.set(null);
    this.completeErrorMessage.set(null);
    this.deleteWorkoutSession.set(null);
    this.deleteErrorMessage.set(null);

    this.workoutsApiService
      .getWorkout(workoutId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.setActiveWorkout(response.workout);
          this.workoutNameError.set(null);
          this.workoutNameSuccess.set(null);
          this.loadWorkoutLifts(response.workout.id);
          this.isLoadingWorkout.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.isLoadingWorkout.set(false);
          this.loadError.set(
            error.status === 404
              ? 'This workout could not be found. Start or continue a workout from home.'
              : 'Unable to load workout details. Check your connection and try again.',
          );
        },
      });
  }

  private refreshActiveWorkoutState(): void {
    this.isRefreshingActiveWorkout.set(true);
    this.workoutsApiService
      .getActiveWorkoutSummary()
      .pipe(finalize(() => this.isRefreshingActiveWorkout.set(false)))
      .subscribe({
        next: (response) => {
          if (!response?.workout) {
            this.workoutsStoreService.clearActiveWorkout();
            this.loadError.set('This workout is no longer active. Return home to start another session.');
            return;
          }

          this.workoutsStoreService.reconcileActiveWorkout(response.workout);
          if (!this.workoutsStoreService.activeWorkout()) {
            this.loadError.set('This workout is no longer active. Return home to start another session.');
            return;
          }

          if (response.workout.id !== this.workoutId()) {
            this.loadError.set('Another workout is active now. Return home to continue it.');
          } else {
            this.loadError.set(null);
          }
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404 || error.status === 204) {
            this.workoutsStoreService.clearActiveWorkout();
            this.deleteWorkoutSession.set(null);
            this.loadError.set('This workout is no longer active. Return home to start another session.');
            return;
          }

          const refreshMessage = 'Could not refresh active workout state. Pull to refresh or try again.';
          if (this.deleteWorkoutSession()) {
            this.deleteErrorMessage.set(refreshMessage);
          } else {
            this.completeErrorMessage.set(refreshMessage);
          }
        },
      });
  }

  private loadWorkoutLifts(workoutId: string): void {
    this.isLoadingWorkoutLifts.set(true);
    this.workoutLiftsLoadError.set(null);

    this.workoutLiftsApiService
      .listWorkoutLifts(workoutId)
      .pipe(finalize(() => this.isLoadingWorkoutLifts.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.setActiveWorkoutLiftEntries(workoutId, response.items);
          this.pruneEntryState(response.items.map((entry) => entry.id));
        },
        error: () => {
          this.workoutLiftsLoadError.set('Unable to load workout lifts right now.');
        },
      });
  }

  private reorderLift(workoutLiftEntryId: string, targetIndex: number): void {
    if (this.isReorderingLift()) {
      return;
    }

    const workoutId = this.workoutId();
    if (!workoutId) {
      this.reorderLiftError.set('Workout ID is missing.');
      return;
    }

    const currentEntries = this.workoutLiftEntries();
    const sourceIndex = currentEntries.findIndex((entry) => entry.id === workoutLiftEntryId);
    if (sourceIndex === -1 || sourceIndex === targetIndex) {
      return;
    }

    const reorderedEntries = [...currentEntries];
    const [movedEntry] = reorderedEntries.splice(sourceIndex, 1);
    reorderedEntries.splice(targetIndex, 0, movedEntry);

    const orderedWorkoutLiftEntryIds = reorderedEntries.map((entry) => entry.id);
    this.isReorderingLift.set(true);
    this.reorderLiftError.set(null);
    this.removeLiftError.set(null);

    this.workoutLiftsApiService
      .reorderWorkoutLifts(workoutId, { orderedWorkoutLiftEntryIds })
      .pipe(finalize(() => this.isReorderingLift.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.replaceActiveWorkoutLiftEntries(response.workoutId, response.items);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.reorderLiftError.set('Workout state changed. Reloaded the latest saved order.');
            this.loadWorkoutLifts(workoutId);
            return;
          }

          if (error.status === 409) {
            this.reorderLiftError.set(error.error?.title ?? 'This workout cannot be reordered right now.');
            this.loadWorkoutLifts(workoutId);
            return;
          }

          if (error.status === 422) {
            this.reorderLiftError.set(error.error?.title ?? 'Unable to save that order. The latest saved order was restored.');
            this.loadWorkoutLifts(workoutId);
            return;
          }

          this.reorderLiftError.set('Order was not saved. Check your connection and try again.');
          this.loadWorkoutLifts(workoutId);
        },
      });
  }

  private loadActiveLifts(): void {
    this.isLoadingActiveLifts.set(true);
    this.activeLiftsLoadError.set(null);

    this.liftsApiService
      .listLifts(true)
      .pipe(finalize(() => this.isLoadingActiveLifts.set(false)))
      .subscribe({
        next: (response) => {
          const activeLifts = response.items
            .filter((lift) => lift.isActive)
            .sort((left, right) => left.name.localeCompare(right.name))
            .map((lift) => ({ id: lift.id, name: lift.name }));
          this.activeLiftOptions.set(activeLifts);

          if (activeLifts.length === 0) {
            this.selectedLiftId.set('');
            return;
          }

          const hasSelectedLift = activeLifts.some((lift) => lift.id === this.selectedLiftId());
          if (!hasSelectedLift) {
            this.selectedLiftId.set(activeLifts[0].id);
          }
        },
        error: () => {
          this.activeLiftsLoadError.set('Unable to load active lifts.');
        },
      });
  }

  private updateAddSetDraft(entryId: string, patch: Partial<{ reps: string; weight: string }>): void {
    this.addSetDraftByEntryId.update((current) => {
      const existing = current[entryId] ?? { reps: '', weight: '' };
      return {
        ...current,
        [entryId]: {
          ...existing,
          ...patch,
        },
      };
    });
  }

  private setAddSetError(entryId: string, message: string | null): void {
    this.addSetErrorByEntryId.update((current) => ({ ...current, [entryId]: message }));
  }

  private setIsAddingSet(entryId: string, isAdding: boolean): void {
    this.isAddingSetByEntryId.update((current) => ({ ...current, [entryId]: isAdding }));
  }

  private pruneEntryState(entryIds: string[]): void {
    const allowedIds = new Set(entryIds);

    this.addSetDraftByEntryId.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([entryId]) => allowedIds.has(entryId))),
    );
    this.addSetErrorByEntryId.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([entryId]) => allowedIds.has(entryId))),
    );
    this.isAddingSetByEntryId.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([entryId]) => allowedIds.has(entryId))),
    );
    this.inlineHistoryStateByEntryId.update((current) =>
      Object.fromEntries(Object.entries(current).filter(([entryId]) => allowedIds.has(entryId))),
    );
    this.setEditSessionByKey.update((current) =>
      Object.fromEntries(
        Object.entries(current).filter(([key]) => allowedIds.has(this.extractEntryIdFromSetRowKey(key))),
      ),
    );
    this.setDeleteSessionByKey.update((current) =>
      Object.fromEntries(
        Object.entries(current).filter(([key]) => allowedIds.has(this.extractEntryIdFromSetRowKey(key))),
      ),
    );
  }

  private patchSetEditSession(
    entryId: string,
    setEntry: WorkoutSetEntry,
    patch: Partial<SetRowEditSession>,
  ): void {
    const rowKey = this.buildSetRowKey(entryId, setEntry.id);
    this.setEditSessionByKey.update((current) => {
      const existing = current[rowKey];
      if (!existing) {
        return current;
      }

      return {
        ...current,
        [rowKey]: {
          ...existing,
          ...patch,
        },
      };
    });
  }

  private patchSetDeleteSession(entryId: string, setId: string, patch: Partial<SetRowDeleteSession>): void {
    const rowKey = this.buildSetRowKey(entryId, setId);
    this.setDeleteSessionByKey.update((current) => {
      const existing = current[rowKey];
      if (!existing) {
        return current;
      }

      return {
        ...current,
        [rowKey]: {
          ...existing,
          ...patch,
        },
      };
    });
  }

  private isSetDraftDirty(setEntry: WorkoutSetEntry, repsValue: string, weightValue: string): boolean {
    const normalizedWeight = weightValue.trim();
    const persistedWeight = setEntry.weight === null ? '' : String(setEntry.weight);
    return repsValue !== String(setEntry.reps) || normalizedWeight !== persistedWeight;
  }

  private buildSetRowKey(entryId: string, setId: string): string {
    return `${entryId}:${setId}`;
  }

  private extractEntryIdFromSetRowKey(key: string): string {
    const separatorIndex = key.indexOf(':');
    if (separatorIndex === -1) {
      return key;
    }

    return key.slice(0, separatorIndex);
  }

  private loadInlineLiftHistory(entryId: string): void {
    const workoutId = this.workoutId();
    if (!workoutId) {
      this.patchInlineHistoryState(entryId, {
        isLoading: false,
        errorMessage: 'Workout ID is missing.',
      });
      return;
    }

    this.patchInlineHistoryState(entryId, {
      isLoading: true,
      errorMessage: null,
    });

    this.workoutLiftsApiService
      .getInlineLiftHistory(workoutId, entryId)
      .subscribe({
        next: (response) => {
          this.patchInlineHistoryState(entryId, {
            isLoading: false,
            errorMessage: null,
            items: response.items,
          });
        },
        error: (error: HttpErrorResponse) => {
          let message = 'History is unavailable right now. Keep logging and try again.';
          if (error.status === 404) {
            message = 'This lift entry no longer exists. Refresh and try again.';
          } else if (error.status === 409) {
            message = error.error?.title ?? 'History is available only while workout entry is active.';
          }

          this.patchInlineHistoryState(entryId, {
            isLoading: false,
            errorMessage: message,
            items: [],
          });
        },
      });
  }

  private getInlineHistoryState(entryId: string): InlineLiftHistoryPanelState {
    return this.inlineHistoryStateByEntryId()[entryId] ?? {
      isExpanded: false,
      isLoading: false,
      errorMessage: null,
      items: [],
    };
  }

  private patchInlineHistoryState(entryId: string, patch: Partial<InlineLiftHistoryPanelState>): void {
    this.inlineHistoryStateByEntryId.update((current) => {
      const existing = current[entryId] ?? {
        isExpanded: false,
        isLoading: false,
        errorMessage: null,
        items: [],
      };
      return {
        ...current,
        [entryId]: {
          ...existing,
          ...patch,
        },
      };
    });
  }
}
