export interface WorkoutSessionSummary {
  id: string;
  status: 'InProgress' | 'Completed';
  label?: string | null;
  startedAtUtc: string;
  completedAtUtc?: string | null;
}

export interface StartWorkoutRequest {
  label?: string;
}

export interface HistoricalWorkoutTimingFields {
  trainingDayLocalDate: string;
  startTimeLocal: string;
  sessionLengthMinutes: number;
}

export interface CreateHistoricalWorkoutRequest extends HistoricalWorkoutTimingFields {
  label?: string | null;
}

export interface WorkoutSessionResponse {
  workout: WorkoutSessionSummary;
}

export type StartWorkoutCreatedResponse = WorkoutSessionResponse;
export type GetWorkoutResponse = WorkoutSessionResponse;
export type GetActiveWorkoutSummaryResponse = WorkoutSessionResponse;
export type CompleteWorkoutResponse = WorkoutSessionResponse;
export type UpdateWorkoutLabelResponse = WorkoutSessionResponse;

export interface DeleteWorkoutResponse {
  workoutId: string;
}

export interface UpdateWorkoutLabelRequest {
  label?: string | null;
}

export interface WorkoutHistorySummary {
  workoutId?: string;
  // Backward-compatible alias in case older payloads still return `id`.
  id?: string;
  label?: string | null;
  completedAtUtc: string;
  durationDisplay: string;
  liftCount: number;
}

export interface GetWorkoutHistoryResponse {
  items: WorkoutHistorySummary[];
}

export interface ExistingInProgressWorkoutResponse {
  title: string;
  status: 409;
  workout: WorkoutSessionSummary;
}
