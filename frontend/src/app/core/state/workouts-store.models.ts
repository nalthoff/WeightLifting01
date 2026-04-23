export interface WorkoutSetEntry {
  id: string;
  workoutLiftEntryId: string;
  setNumber: number;
  reps: number;
  weight: number | null;
  createdAtUtc: string;
  updatedAtUtc?: string;
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

export interface SetRowEditSession {
  setId: string;
  draftReps: string;
  draftWeight: string;
  isSaving: boolean;
  errorMessage: string | null;
  isDirty: boolean;
}
