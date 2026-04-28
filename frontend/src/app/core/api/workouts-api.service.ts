import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type {
  CompleteWorkoutResponse,
  CreateHistoricalWorkoutRequest,
  DeleteWorkoutResponse,
  GetActiveWorkoutSummaryResponse,
  GetWorkoutHistoryResponse,
  GetWorkoutResponse,
  StartWorkoutCreatedResponse,
  StartWorkoutRequest,
  UpdateWorkoutLabelRequest,
  UpdateWorkoutLabelResponse,
} from './workouts.models';

export type {
  CompleteWorkoutResponse,
  CreateHistoricalWorkoutRequest,
  DeleteWorkoutResponse,
  ExistingInProgressWorkoutResponse,
  GetActiveWorkoutSummaryResponse,
  GetWorkoutHistoryResponse,
  GetWorkoutResponse,
  HistoricalWorkoutTimingFields,
  StartWorkoutCreatedResponse,
  StartWorkoutRequest,
  UpdateWorkoutLabelRequest,
  UpdateWorkoutLabelResponse,
  WorkoutHistorySummary,
  WorkoutSessionResponse,
  WorkoutSessionSummary,
} from './workouts.models';

@Injectable({
  providedIn: 'root',
})
export class WorkoutsApiService {
  private readonly httpClient = inject(HttpClient);

  startWorkout(request: StartWorkoutRequest = {}): Observable<StartWorkoutCreatedResponse> {
    return this.httpClient.post<StartWorkoutCreatedResponse>('/api/workouts', request);
  }

  createHistoricalWorkout(request: CreateHistoricalWorkoutRequest): Observable<StartWorkoutCreatedResponse> {
    return this.httpClient.post<StartWorkoutCreatedResponse>('/api/workouts/historical', request);
  }

  getWorkout(workoutId: string, forHistory = false): Observable<GetWorkoutResponse> {
    const suffix = forHistory ? '?forHistory=true' : '';
    return this.httpClient.get<GetWorkoutResponse>(`/api/workouts/${workoutId}${suffix}`);
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

  updateWorkoutLabel(workoutId: string, request: UpdateWorkoutLabelRequest): Observable<UpdateWorkoutLabelResponse> {
    return this.httpClient.put<UpdateWorkoutLabelResponse>(`/api/workouts/${workoutId}/label`, request);
  }

  getWorkoutHistory(): Observable<GetWorkoutHistoryResponse> {
    return this.httpClient.get<GetWorkoutHistoryResponse>('/api/workouts/history');
  }
}
