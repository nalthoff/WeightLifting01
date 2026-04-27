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

export interface SetRowDeleteSession {
  setId: string;
  isConfirmingDelete: boolean;
  isDeleting: boolean;
  errorMessage: string | null;
}

export interface WorkoutDeleteSession {
  workoutId: string;
  isConfirmingDelete: boolean;
  isDeleting: boolean;
  errorMessage: string | null;
}

export interface LiftHistorySetSummary {
  setNumber: number;
  reps: number;
  weight: number | null;
}

export interface LiftHistorySessionSummary {
  workoutId: string;
  workoutLabel?: string | null;
  completedAtUtc: string;
  sets: LiftHistorySetSummary[];
}

export interface InlineLiftHistoryPanelState {
  isExpanded: boolean;
  isLoading: boolean;
  errorMessage: string | null;
  items: LiftHistorySessionSummary[];
}
