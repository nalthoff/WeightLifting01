export interface WorkoutSetEntry {
  id: string;
  workoutLiftEntryId: string;
  setNumber: number;
  reps: number;
  weight: number | null;
  createdAtUtc: string;
}

export interface WorkoutLiftEntryState {
  id: string;
  workoutId: string;
  liftId: string;
  displayName: string;
  addedAtUtc: string;
  position: number;
  sets: WorkoutSetEntry[];
}
