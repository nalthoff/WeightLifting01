import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { finalize } from 'rxjs';

import type { WorkoutHistorySummary } from '../../core/api/workouts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';

@Component({
  selector: 'app-history-page',
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule],
  templateUrl: './history-page.component.html',
  styleUrl: './history-page.component.scss',
})
export class HistoryPageComponent {
  private readonly workoutsApiService = inject(WorkoutsApiService);

  readonly isLoading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly completedWorkouts = signal<WorkoutHistorySummary[]>([]);

  constructor() {
    this.loadHistory();
  }

  trackWorkout(_index: number, workout: WorkoutHistorySummary): string {
    return workout.id;
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
