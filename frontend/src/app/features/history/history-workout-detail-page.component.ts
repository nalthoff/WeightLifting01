import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { finalize, forkJoin } from 'rxjs';

import { WorkoutLiftsApiService } from '../../core/api/workout-lifts-api.service';
import { WorkoutsApiService } from '../../core/api/workouts-api.service';

type WorkoutLiftEntry = {
  id: string;
  displayName: string;
  position: number;
  sets?: {
    id: string;
    setNumber: number;
    reps: number;
    weight: number | null;
  }[];
};

@Component({
  selector: 'app-history-workout-detail-page',
  imports: [CommonModule, RouterLink, MatButtonModule, MatCardModule],
  templateUrl: './history-workout-detail-page.component.html',
  styleUrl: './history-workout-detail-page.component.scss',
})
export class HistoryWorkoutDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutLiftsApiService = inject(WorkoutLiftsApiService);

  readonly workoutId = signal<string | null>(this.route.snapshot.paramMap.get('workoutId'));
  readonly isLoading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly workout = signal<{
    id: string;
    status?: string | null;
    label?: string | null;
    startedAtUtc: string;
    completedAtUtc?: string | null;
  } | null>(null);
  readonly lifts = signal<WorkoutLiftEntry[]>([]);
  readonly hasLifts = computed(() => this.lifts().length > 0);
  private readonly statusBadgeByCode: Record<string, { label: string; tone: 'progress' | 'complete' | 'unknown' }> = {
    InProgress: { label: 'In Progress', tone: 'progress' },
    Completed: { label: 'Completed', tone: 'complete' },
  };

  constructor() {
    this.loadWorkoutDetail();
  }

  getWorkoutLabel(): string {
    return this.workout()?.label?.trim() || 'Workout';
  }

  getDurationDisplay(): string {
    const workout = this.workout();
    if (!workout?.startedAtUtc || !workout.completedAtUtc) {
      return '--:--';
    }

    const started = Date.parse(workout.startedAtUtc);
    const completed = Date.parse(workout.completedAtUtc);
    if (!Number.isFinite(started) || !Number.isFinite(completed) || completed < started) {
      return '--:--';
    }

    const totalMinutes = Math.floor((completed - started) / 60000);
    const hours = Math.floor(totalMinutes / 60)
      .toString()
      .padStart(2, '0');
    const minutes = (totalMinutes % 60).toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  getSetWeightDisplay(weight: number | null): string {
    if (weight === null || weight === undefined) {
      return '-';
    }

    return `${weight} lb`;
  }

  getWorkoutStatusBadge(status: string | null | undefined): { label: string; tone: 'progress' | 'complete' | 'unknown' } {
    if (!status) {
      return { label: 'Unknown', tone: 'unknown' };
    }

    return this.statusBadgeByCode[status] ?? { label: status, tone: 'unknown' };
  }

  retryLoad(): void {
    this.loadWorkoutDetail();
  }

  trackLift(_index: number, entry: WorkoutLiftEntry): string {
    return entry.id;
  }

  trackSet(_index: number, set: { id: string }): string {
    return set.id;
  }

  private loadWorkoutDetail(): void {
    const workoutId = this.workoutId();
    if (!workoutId) {
      this.loadError.set('Workout could not be loaded. Return to history and try again.');
      return;
    }

    this.isLoading.set(true);
    this.loadError.set(null);

    forkJoin({
      workoutResponse: this.workoutsApiService.getWorkout(workoutId, true),
      liftsResponse: this.workoutLiftsApiService.listWorkoutLifts(workoutId, true),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ workoutResponse, liftsResponse }) => {
          this.workout.set(workoutResponse.workout);
          this.lifts.set([...liftsResponse.items].sort((left, right) => left.position - right.position));
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.loadError.set('This completed workout is no longer available. Return to history to choose another workout.');
            return;
          }

          this.loadError.set('Unable to load workout details right now. Check your connection and try again.');
        },
      });
  }
}
