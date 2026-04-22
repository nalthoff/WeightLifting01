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
