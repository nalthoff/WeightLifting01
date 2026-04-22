import { Injectable, computed, signal } from '@angular/core';

import type { WorkoutSessionSummary } from '../api/workouts-api.service';

@Injectable({
  providedIn: 'root',
})
export class WorkoutsStoreService {
  readonly activeWorkout = signal<WorkoutSessionSummary | null>(null);
  readonly hasActiveWorkout = computed(() => this.activeWorkout() !== null);

  setActiveWorkout(workout: WorkoutSessionSummary): void {
    this.activeWorkout.set(workout);
  }

  clearActiveWorkout(): void {
    this.activeWorkout.set(null);
  }
}
