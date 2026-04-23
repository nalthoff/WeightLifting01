import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { finalize, map } from 'rxjs';

import { LiftsApiService } from '../../core/api/lifts-api.service';
import { WorkoutLiftsApiService } from '../../core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';

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
  ],
  templateUrl: './active-workout-page.component.html',
  styleUrl: './active-workout-page.component.scss',
})
export class ActiveWorkoutPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly liftsApiService = inject(LiftsApiService);
  private readonly workoutLiftsApiService = inject(WorkoutLiftsApiService);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);

  readonly isLoadingWorkout = signal(false);
  readonly loadError = signal<string | null>(null);
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

    this.workoutsApiService
      .getWorkout(workoutId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.setActiveWorkout(response.workout);
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

  private loadWorkoutLifts(workoutId: string): void {
    this.isLoadingWorkoutLifts.set(true);
    this.workoutLiftsLoadError.set(null);

    this.workoutLiftsApiService
      .listWorkoutLifts(workoutId)
      .pipe(finalize(() => this.isLoadingWorkoutLifts.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.setActiveWorkoutLiftEntries(workoutId, response.items);
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
}
