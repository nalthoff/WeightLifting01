import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface WorkoutSessionSummary {
  id: string;
  status: 'InProgress';
  label?: string | null;
  startedAtUtc: string;
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
}
