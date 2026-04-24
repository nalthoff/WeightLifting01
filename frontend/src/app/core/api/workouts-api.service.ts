import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

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

export interface StartWorkoutCreatedResponse {
  workout: WorkoutSessionSummary;
}

export interface GetWorkoutResponse {
  workout: WorkoutSessionSummary;
}

export interface GetActiveWorkoutSummaryResponse {
  workout: WorkoutSessionSummary;
}

export interface CompleteWorkoutResponse {
  workout: WorkoutSessionSummary;
}

export interface DeleteWorkoutResponse {
  workoutId: string;
}

export interface WorkoutHistorySummary {
  id: string;
  label?: string | null;
  completedAtUtc: string;
}

export interface GetWorkoutHistoryResponse {
  items: WorkoutHistorySummary[];
}

export interface ExistingInProgressWorkoutResponse {
  title: string;
  status: 409;
  workout: WorkoutSessionSummary;
}

@Injectable({
  providedIn: 'root',
})
export class WorkoutsApiService {
  private readonly httpClient = inject(HttpClient);

  startWorkout(request: StartWorkoutRequest = {}): Observable<StartWorkoutCreatedResponse> {
    return this.httpClient.post<StartWorkoutCreatedResponse>('/api/workouts', request);
  }

  getWorkout(workoutId: string): Observable<GetWorkoutResponse> {
    return this.httpClient.get<GetWorkoutResponse>(`/api/workouts/${workoutId}`);
  }

  getActiveWorkoutSummary(): Observable<GetActiveWorkoutSummaryResponse> {
    return this.httpClient.get<GetActiveWorkoutSummaryResponse>('/api/workouts/active');
  }

  completeWorkout(workoutId: string): Observable<CompleteWorkoutResponse> {
    return this.httpClient.post<CompleteWorkoutResponse>(`/api/workouts/${workoutId}/complete`, {});
  }

  deleteWorkout(workoutId: string): Observable<DeleteWorkoutResponse> {
    return this.httpClient.delete<DeleteWorkoutResponse>(`/api/workouts/${workoutId}`);
  }

  getWorkoutHistory(): Observable<GetWorkoutHistoryResponse> {
    return this.httpClient.get<GetWorkoutHistoryResponse>('/api/workouts/history');
  }
}
