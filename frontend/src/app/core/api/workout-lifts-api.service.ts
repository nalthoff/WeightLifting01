import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import type { WorkoutSetEntry } from '../state/workouts-store.models';

export interface WorkoutLiftEntry {
  id: string;
  workoutId: string;
  liftId: string;
  displayName: string;
  addedAtUtc: string;
  position: number;
  sets?: WorkoutSetEntry[];
}

export interface WorkoutLiftListResponse {
  items: WorkoutLiftEntry[];
}

export interface AddWorkoutLiftRequest {
  liftId: string;
}

export interface AddWorkoutLiftResponse {
  workoutLift: WorkoutLiftEntry;
}

export interface RemoveWorkoutLiftResponse {
  workoutId: string;
  workoutLiftEntryId: string;
  // Backward-compatible alias in case older API shape is returned.
  removedWorkoutLiftEntryId?: string;
}

export interface ReorderWorkoutLiftsRequest {
  orderedWorkoutLiftEntryIds: string[];
}

export interface ReorderWorkoutLiftsResponse {
  workoutId: string;
  items: WorkoutLiftEntry[];
}

export interface CreateWorkoutSetRequest {
  reps: number;
  weight: number | null;
}

export interface CreateWorkoutSetResponse {
  workoutId: string;
  workoutLiftEntryId: string;
  set: WorkoutSetEntry;
}

@Injectable({
  providedIn: 'root',
})
export class WorkoutLiftsApiService {
  private readonly httpClient = inject(HttpClient);

  listWorkoutLifts(workoutId: string): Observable<WorkoutLiftListResponse> {
    return this.httpClient.get<WorkoutLiftListResponse>(`/api/workouts/${workoutId}/lifts`);
  }

  addWorkoutLift(workoutId: string, request: AddWorkoutLiftRequest): Observable<AddWorkoutLiftResponse> {
    return this.httpClient.post<AddWorkoutLiftResponse>(`/api/workouts/${workoutId}/lifts`, request);
  }

  removeWorkoutLift(workoutId: string, workoutLiftEntryId: string): Observable<RemoveWorkoutLiftResponse> {
    return this.httpClient.delete<RemoveWorkoutLiftResponse>(
      `/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}`,
    );
  }

  reorderWorkoutLifts(
    workoutId: string,
    request: ReorderWorkoutLiftsRequest,
  ): Observable<ReorderWorkoutLiftsResponse> {
    return this.httpClient.put<ReorderWorkoutLiftsResponse>(`/api/workouts/${workoutId}/lifts/reorder`, request);
  }

  addWorkoutSet(
    workoutId: string,
    workoutLiftEntryId: string,
    request: CreateWorkoutSetRequest,
  ): Observable<CreateWorkoutSetResponse> {
    return this.httpClient.post<CreateWorkoutSetResponse>(
      `/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}/sets`,
      request,
    );
  }
}
