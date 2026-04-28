import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { finalize } from 'rxjs';

import type { WorkoutHistorySummary } from '../../core/api/workouts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';

@Component({
  selector: 'app-history-page',
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule],
  templateUrl: './history-page.component.html',
  styleUrl: './history-page.component.scss',
})
export class HistoryPageComponent {
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly completedWorkouts = signal<WorkoutHistorySummary[]>([]);
  readonly historicalFlowMessage = this.workoutsStoreService.historicalFlowMessage.asReadonly();
  readonly historicalFlowNavigationContext = this.workoutsStoreService.historicalFlowNavigationContext.asReadonly();

  constructor() {
    this.loadHistory();
  }

  trackWorkout(_index: number, workout: WorkoutHistorySummary): string {
    return workout.workoutId ?? workout.id ?? '';
  }

  getWorkoutId(workout: WorkoutHistorySummary): string {
    return workout.workoutId ?? workout.id ?? '';
  }

  getDisplayLabel(workout: WorkoutHistorySummary): string {
    return workout.label?.trim() || 'Workout';
  }

  getDurationDisplay(workout: WorkoutHistorySummary): string {
    return workout.durationDisplay?.trim() || '--:--';
  }

  getLiftCountDisplay(workout: WorkoutHistorySummary): string {
    return `${workout.liftCount} lifts`;
  }

  dismissHistoricalFlowMessage(): void {
    this.workoutsStoreService.clearHistoricalFlowMessage();
  }

  returnToActiveWorkout(): void {
    const returnToWorkoutId = this.historicalFlowNavigationContext().returnToWorkoutId;
    if (!returnToWorkoutId) {
      return;
    }

    void this.router.navigate(['/workouts', returnToWorkoutId]);
  }

  private loadHistory(): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.workoutsApiService
      .getWorkoutHistory()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.completedWorkouts.set(response.items);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.completedWorkouts.set([]);
            return;
          }

          this.loadError.set('Unable to load workout history right now. Check your connection and try again.');
        },
      });
  }
}
