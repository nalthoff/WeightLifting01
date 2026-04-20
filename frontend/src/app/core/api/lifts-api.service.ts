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
}
