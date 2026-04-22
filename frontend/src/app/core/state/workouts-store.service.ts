import { Injectable, computed, signal } from '@angular/core';

import type { WorkoutSessionSummary } from '../api/workouts-api.service';
import type { WorkoutLiftEntry } from '../api/workout-lifts-api.service';

@Injectable({
  providedIn: 'root',
})
export class WorkoutsStoreService {
  readonly activeWorkout = signal<WorkoutSessionSummary | null>(null);
  readonly activeWorkoutLiftEntries = signal<WorkoutLiftEntry[]>([]);
  readonly hasActiveWorkout = computed(() => this.activeWorkout() !== null);

  setActiveWorkout(workout: WorkoutSessionSummary): void {
    this.activeWorkout.set(workout);
    this.activeWorkoutLiftEntries.set([]);
  }

  reconcileActiveWorkout(workout: WorkoutSessionSummary | null): void {
    if (!workout || workout.status !== 'InProgress') {
      this.clearActiveWorkout();
      return;
    }

    if (this.activeWorkout()?.id !== workout.id) {
      this.activeWorkoutLiftEntries.set([]);
    }

    this.activeWorkout.set(workout);
  }

  clearActiveWorkout(): void {
    this.activeWorkout.set(null);
    this.activeWorkoutLiftEntries.set([]);
  }

  setActiveWorkoutLiftEntries(workoutId: string, entries: WorkoutLiftEntry[]): void {
    if (this.activeWorkout()?.id !== workoutId) {
      return;
    }

    const sortedEntries = [...entries].sort((left, right) => left.position - right.position);
    this.activeWorkoutLiftEntries.set(sortedEntries);
  }

  appendActiveWorkoutLiftEntry(entry: WorkoutLiftEntry): void {
    const activeWorkout = this.activeWorkout();
    if (!activeWorkout || activeWorkout.id !== entry.workoutId) {
      return;
    }

    const nextEntries = [...this.activeWorkoutLiftEntries(), entry].sort(
      (left, right) => left.position - right.position,
    );
    this.activeWorkoutLiftEntries.set(nextEntries);
  }

  removeActiveWorkoutLiftEntryById(workoutId: string, workoutLiftEntryId: string): void {
    const activeWorkout = this.activeWorkout();
    if (!activeWorkout || activeWorkout.id !== workoutId) {
      return;
    }

    const remainingEntries = this.activeWorkoutLiftEntries().filter((entry) => entry.id !== workoutLiftEntryId);
    this.activeWorkoutLiftEntries.set(remainingEntries);
  }
}
