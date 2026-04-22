import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';

@Component({
  selector: 'app-active-workout-page',
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule],
  templateUrl: './active-workout-page.component.html',
  styleUrl: './active-workout-page.component.scss',
})
export class ActiveWorkoutPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);

  readonly isLoadingWorkout = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly workoutId = computed(() => this.route.snapshot.paramMap.get('workoutId'));
  readonly workout = computed(() => {
    const activeWorkout = this.workoutsStoreService.activeWorkout();
    const workoutId = this.workoutId();

    if (!activeWorkout || !workoutId || activeWorkout.id !== workoutId) {
      return null;
    }

    return activeWorkout;
  });

  constructor() {
    this.ensureWorkoutLoaded();
  }

  private ensureWorkoutLoaded(): void {
    const workoutId = this.workoutId();
    if (!workoutId) {
      this.loadError.set('Workout ID is missing.');
      return;
    }

    const currentWorkout = this.workoutsStoreService.activeWorkout();
    if (currentWorkout?.id === workoutId) {
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
}
