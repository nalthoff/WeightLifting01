import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface LiftListItem {
  id: string;
  name: string;
  isActive: boolean;
}

export interface LiftListResponse {
  items: LiftListItem[];
  lastSyncedAtUtc?: string;
}

export interface CreateLiftRequest {
  name: string;
  clientRequestId?: string;
}

export interface CreateLiftResponse {
  lift: {
    id: string;
    name: string;
    isActive: boolean;
    createdAtUtc: string;
  };
}

export interface RenameLiftRequest {
  name: string;
}

export interface RenameLiftResponse {
  lift: {
    id: string;
    name: string;
    isActive: boolean;
    createdAtUtc: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class LiftsApiService {
  private readonly httpClient = inject(HttpClient);

  listLifts(activeOnly = true): Observable<LiftListResponse> {
    return this.httpClient.get<LiftListResponse>('/api/lifts', {
      params: {
        activeOnly: String(activeOnly),
      },
    });
  }

  createLift(request: CreateLiftRequest): Observable<CreateLiftResponse> {
    return this.httpClient.post<CreateLiftResponse>('/api/lifts', request);
  }

  renameLift(liftId: string, request: RenameLiftRequest): Observable<RenameLiftResponse> {
    return this.httpClient.put<RenameLiftResponse>(`/api/lifts/${liftId}`, request);
  }
}
