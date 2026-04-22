import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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
  readonly errorMessage = signal<string | null>(null);
  readonly conflictMessage = signal<string | null>(null);
  readonly conflictWorkout = signal<ExistingInProgressWorkoutResponse['workout'] | null>(null);
  readonly canContinueExisting = computed(() => this.conflictWorkout() !== null);

  startWorkout(): void {
    this.isStartingWorkout.set(true);
    this.errorMessage.set(null);
    this.conflictMessage.set(null);
    this.conflictWorkout.set(null);

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
    const workout = this.conflictWorkout();
    if (!workout) {
      return;
    }

    this.workoutsStoreService.setActiveWorkout(workout);
    this.clearConflictState();
    void this.router.navigate(['/workouts', workout.id]);
  }

  dismissContinuePrompt(): void {
    this.clearConflictState();
  }

  private clearConflictState(): void {
    this.conflictMessage.set(null);
    this.conflictWorkout.set(null);
  }
}
