import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { finalize } from 'rxjs';

import type { ExistingInProgressWorkoutResponse } from '../../core/api/workouts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';

@Component({
  selector: 'app-home-page',
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    RouterLink,
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent {
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);
  private readonly router = inject(Router);

  readonly workoutLabel = signal('');
  readonly isStartingWorkout = signal(false);
  readonly isLoadingActiveWorkout = signal(false);
  readonly isCompletingWorkout = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly conflictMessage = signal<string | null>(null);
  readonly completeErrorMessage = signal<string | null>(null);
  readonly completeSuccessMessage = signal<string | null>(null);
  readonly conflictWorkout = signal<ExistingInProgressWorkoutResponse['workout'] | null>(null);
  readonly canContinueExisting = computed(() => this.conflictWorkout() !== null);
  readonly activeWorkout = this.workoutsStoreService.activeWorkout.asReadonly();
  readonly hasActiveWorkout = this.workoutsStoreService.hasActiveWorkout;
  readonly activeWorkoutTitle = computed(() => this.activeWorkout()?.label?.trim() || 'Workout');

  constructor() {
    this.refreshActiveWorkoutSummary();
  }

  startWorkout(): void {
    this.isStartingWorkout.set(true);
    this.errorMessage.set(null);
    this.conflictMessage.set(null);
    this.conflictWorkout.set(null);
    this.completeErrorMessage.set(null);
    this.completeSuccessMessage.set(null);

    const normalizedLabel = this.workoutLabel().trim();

    this.workoutsApiService
      .startWorkout(normalizedLabel ? { label: normalizedLabel } : {})
      .pipe(finalize(() => this.isStartingWorkout.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.setActiveWorkout(response.workout);
          this.workoutLabel.set('');
          void this.router.navigate(['/workouts', response.workout.id]);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409 && error.error?.workout) {
            const conflictResponse = error.error as ExistingInProgressWorkoutResponse;
            this.conflictWorkout.set(conflictResponse.workout);
            this.conflictMessage.set('You already have a workout in progress. Continue it?');
            return;
          }

          this.errorMessage.set(
            'Unable to start workout. Check your connection and try again.',
          );
        },
      });
  }

  continueExistingWorkout(): void {
    const workout = this.conflictWorkout() ?? this.activeWorkout();
    if (!workout) {
      this.completeErrorMessage.set(
        'That workout is no longer available. Refreshing your home view.',
      );
      this.refreshActiveWorkoutSummary();
      return;
    }

    this.completeErrorMessage.set(null);
    this.completeSuccessMessage.set(null);
    this.workoutsStoreService.setActiveWorkout(workout);
    this.clearConflictState();
    void this.router.navigate(['/workouts', workout.id]);
  }

  completeActiveWorkout(): void {
    const workout = this.activeWorkout();
    if (!workout || this.isCompletingWorkout()) {
      return;
    }

    this.isCompletingWorkout.set(true);
    this.completeErrorMessage.set(null);
    this.completeSuccessMessage.set(null);
    this.errorMessage.set(null);

    this.workoutsApiService
      .completeWorkout(workout.id)
      .pipe(finalize(() => this.isCompletingWorkout.set(false)))
      .subscribe({
        next: (response) => {
          this.workoutsStoreService.reconcileActiveWorkout(response.workout);
          this.completeSuccessMessage.set('Workout completed. Great work.');
          this.completeErrorMessage.set(null);
          this.refreshActiveWorkoutSummary();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404 || error.status === 409) {
            this.completeErrorMessage.set(
              error.error?.title ?? 'Workout state changed. Home was refreshed.',
            );
            this.refreshActiveWorkoutSummary();
            return;
          }

          this.completeErrorMessage.set(
            'Unable to complete workout. Check your connection and try again.',
          );
          this.refreshActiveWorkoutSummary();
        },
      });
  }

  dismissContinuePrompt(): void {
    this.clearConflictState();
  }

  private refreshActiveWorkoutSummary(): void {
    this.isLoadingActiveWorkout.set(true);
    this.workoutsApiService
      .getActiveWorkoutSummary()
      .pipe(finalize(() => this.isLoadingActiveWorkout.set(false)))
      .subscribe({
        next: (response) => {
          if (!response?.workout) {
            this.workoutsStoreService.clearActiveWorkout();
            return;
          }

          this.workoutsStoreService.reconcileActiveWorkout(response.workout);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404 || error.status === 204) {
            this.workoutsStoreService.clearActiveWorkout();
            return;
          }

          this.completeErrorMessage.set(
            'Could not refresh active workout state. Pull to refresh or try again.',
          );
        },
      });
  }

  private clearConflictState(): void {
    this.conflictMessage.set(null);
    this.conflictWorkout.set(null);
  }
}
