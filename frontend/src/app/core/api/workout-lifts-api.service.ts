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

export interface UpdateWorkoutSetRequest {
  reps: number;
  weight: number | null;
}

export interface UpdateWorkoutSetResponse {
  workoutId: string;
  workoutLiftEntryId: string;
  set: WorkoutSetEntry;
}

export interface DeleteWorkoutSetResponse {
  workoutId: string;
  workoutLiftEntryId: string;
  setId: string;
}

export interface InlineLiftHistorySet {
  setNumber: number;
  reps: number;
  weight: number | null;
}

export interface InlineLiftHistorySession {
  workoutId: string;
  workoutLabel?: string | null;
  completedAtUtc: string;
  sets: InlineLiftHistorySet[];
}

export interface InlineLiftHistoryResponse {
  workoutId: string;
  workoutLiftEntryId: string;
  items: InlineLiftHistorySession[];
}

@Injectable({
  providedIn: 'root',
})
export class WorkoutLiftsApiService {
  private readonly httpClient = inject(HttpClient);

  listWorkoutLifts(workoutId: string, forHistory = false): Observable<WorkoutLiftListResponse> {
    const suffix = forHistory ? '?forHistory=true' : '';
    return this.httpClient.get<WorkoutLiftListResponse>(`/api/workouts/${workoutId}/lifts${suffix}`);
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

  updateWorkoutSet(
    workoutId: string,
    workoutLiftEntryId: string,
    setId: string,
    request: UpdateWorkoutSetRequest,
  ): Observable<UpdateWorkoutSetResponse> {
    return this.httpClient.put<UpdateWorkoutSetResponse>(
      `/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}/sets/${setId}`,
      request,
    );
  }

  deleteWorkoutSet(
    workoutId: string,
    workoutLiftEntryId: string,
    setId: string,
  ): Observable<DeleteWorkoutSetResponse> {
    return this.httpClient.delete<DeleteWorkoutSetResponse>(
      `/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}/sets/${setId}`,
    );
  }

  getInlineLiftHistory(workoutId: string, workoutLiftEntryId: string): Observable<InlineLiftHistoryResponse> {
    return this.httpClient.get<InlineLiftHistoryResponse>(
      `/api/workouts/${workoutId}/lifts/${workoutLiftEntryId}/history`,
    );
  }
}
