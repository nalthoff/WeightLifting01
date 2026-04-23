import { Injectable, computed, signal } from '@angular/core';

import type { WorkoutSessionSummary } from '../api/workouts-api.service';
import type { WorkoutLiftEntry } from '../api/workout-lifts-api.service';
import type { WorkoutLiftEntryState, WorkoutSetEntry } from './workouts-store.models';

@Injectable({
  providedIn: 'root',
})
export class WorkoutsStoreService {
  readonly activeWorkout = signal<WorkoutSessionSummary | null>(null);
  readonly activeWorkoutLiftEntries = signal<WorkoutLiftEntryState[]>([]);
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

    const sortedEntries = entries
      .map((entry) => this.toWorkoutLiftEntryState(entry))
      .sort((left, right) => left.position - right.position);
    this.activeWorkoutLiftEntries.set(sortedEntries);
  }

  appendActiveWorkoutLiftEntry(entry: WorkoutLiftEntry): void {
    const activeWorkout = this.activeWorkout();
    if (!activeWorkout || activeWorkout.id !== entry.workoutId) {
      return;
    }

    const nextEntries = [...this.activeWorkoutLiftEntries(), this.toWorkoutLiftEntryState(entry)].sort(
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

  replaceActiveWorkoutLiftEntries(workoutId: string, entries: WorkoutLiftEntry[]): void {
    const activeWorkout = this.activeWorkout();
    if (!activeWorkout || activeWorkout.id !== workoutId) {
      return;
    }

    const normalizedEntries = entries
      .map((entry) => this.toWorkoutLiftEntryState(entry))
      .sort((left, right) => left.position - right.position);
    this.activeWorkoutLiftEntries.set(normalizedEntries);
  }

  appendWorkoutSet(workoutId: string, workoutLiftEntryId: string, setEntry: WorkoutSetEntry): void {
    const activeWorkout = this.activeWorkout();
    if (!activeWorkout || activeWorkout.id !== workoutId) {
      return;
    }

    const nextEntries = this.activeWorkoutLiftEntries().map((entry) => {
      if (entry.id !== workoutLiftEntryId) {
        return entry;
      }

      const existingSet = entry.sets.find((set) => set.id === setEntry.id);
      if (existingSet) {
        return entry;
      }

      return {
        ...entry,
        sets: [...entry.sets, setEntry].sort((left, right) => left.setNumber - right.setNumber),
      };
    });

    this.activeWorkoutLiftEntries.set(nextEntries);
  }

  private toWorkoutLiftEntryState(entry: WorkoutLiftEntry): WorkoutLiftEntryState {
    return {
      id: entry.id,
      workoutId: entry.workoutId,
      liftId: entry.liftId,
      displayName: entry.displayName,
      addedAtUtc: entry.addedAtUtc,
      position: entry.position,
      sets: [...(entry.sets ?? [])].sort((left, right) => left.setNumber - right.setNumber),
    };
  }
}
