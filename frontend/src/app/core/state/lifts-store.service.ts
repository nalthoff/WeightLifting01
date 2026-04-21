import { Injectable, signal } from '@angular/core';

import type { LiftListItem, LiftListResponse } from '../api/lifts-api.service';

@Injectable({
  providedIn: 'root',
})
export class LiftsStoreService {
  readonly items = signal<LiftListItem[]>([]);
  readonly lastSyncedAtUtc = signal<string | null>(null);
  readonly isLoaded = signal(false);

  setResponse(response: LiftListResponse): void {
    this.items.set(response.items);
    this.lastSyncedAtUtc.set(response.lastSyncedAtUtc ?? null);
    this.isLoaded.set(true);
  }

  upsert(item: LiftListItem): void {
    const nextItems = [...this.items()];
    const existingIndex = nextItems.findIndex((existingItem) => existingItem.id === item.id);

    if (existingIndex >= 0) {
      nextItems[existingIndex] = item;
    } else {
      nextItems.push(item);
    }

    nextItems.sort((left, right) => left.name.localeCompare(right.name));
    this.items.set(nextItems);
    this.isLoaded.set(true);
  }

  replace(item: LiftListItem): void {
    const nextItems = [...this.items()];
    const existingIndex = nextItems.findIndex((existingItem) => existingItem.id === item.id);

    if (existingIndex < 0) {
      return;
    }

    nextItems[existingIndex] = item;
    nextItems.sort((left, right) => left.name.localeCompare(right.name));
    this.items.set(nextItems);
    this.isLoaded.set(true);
  }
}
