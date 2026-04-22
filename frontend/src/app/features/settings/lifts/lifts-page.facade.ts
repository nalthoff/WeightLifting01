import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';

import type { LiftListItem } from '../../../core/api/lifts-api.service';
import { LiftsApiService } from '../../../core/api/lifts-api.service';
import { LiftsStoreService } from '../../../core/state/lifts-store.service';

@Injectable({
  providedIn: 'root',
})
export class LiftsPageFacade {
  private readonly liftsApiService = inject(LiftsApiService);
  private readonly liftsStoreService = inject(LiftsStoreService);

  readonly liftName = signal('');
  readonly editingLiftId = signal<string | null>(null);
  readonly editingLiftName = signal('');
  readonly selectedFilter = signal<'active' | 'all'>('active');
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isLoading = signal(false);
  readonly lifts = computed(() => this.liftsStoreService.items());
  readonly activeOnly = computed(() => this.selectedFilter() === 'active');
  private readonly lastLoadedActiveOnly = signal<boolean | null>(null);
  readonly editingLift = computed(() =>
    this.lifts().find((lift) => lift.id === this.editingLiftId()) ?? null,
  );

  load(force = false): void {
    const activeOnly = this.activeOnly();

    if (
      !force &&
      this.liftsStoreService.isLoaded() &&
      this.lastLoadedActiveOnly() === activeOnly
    ) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.liftsApiService
      .listLifts(activeOnly)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.liftsStoreService.setResponse(response);
          this.lastLoadedActiveOnly.set(activeOnly);
        },
        error: () => this.errorMessage.set('Unable to load lifts right now.'),
      });
  }

  setSelectedFilter(filter: 'active' | 'all'): void {
    if (this.selectedFilter() === filter) {
      return;
    }

    this.selectedFilter.set(filter);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.load(true);
  }

  updateLiftName(name: string): void {
    this.liftName.set(name);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  startRename(lift: LiftListItem): void {
    this.editingLiftId.set(lift.id);
    this.editingLiftName.set(lift.name);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  updateEditingLiftName(name: string): void {
    this.editingLiftName.set(name);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  cancelRename(): void {
    this.editingLiftId.set(null);
    this.editingLiftName.set('');
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  prepareForDeactivateDialog(): void {
    this.editingLiftId.set(null);
    this.editingLiftName.set('');
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  notifyDeactivateCancelled(): void {
    this.errorMessage.set(null);
    this.successMessage.set('Deactivation cancelled.');
  }

  submitDeactivate(liftId: string): void {
    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.liftsApiService
      .deactivateLift(liftId)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          this.liftsStoreService.replace({
            id: response.lift.id,
            name: response.lift.name,
            isActive: response.lift.isActive,
          });

          this.successMessage.set('Lift deactivated.');
          this.refreshLiftsForSelectedFilter();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.errorMessage.set('Lift no longer exists. Refresh and try again.');
            this.refreshLiftsForSelectedFilter();
            return;
          }

          this.errorMessage.set('Lift was not deactivated. Try again.');
        },
      });
  }

  submit(): void {
    const normalizedName = this.liftName().trim();

    if (!normalizedName) {
      this.errorMessage.set('Enter a lift name.');
      this.successMessage.set(null);
      return;
    }

    const normalizedLower = normalizedName.toLowerCase();
    const duplicate = this.lifts().some(
      (lift) => lift.name.trim().toLowerCase() === normalizedLower,
    );
    if (duplicate) {
      this.errorMessage.set('Lift name already exists.');
      this.successMessage.set(null);
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.liftsApiService
      .createLift({
        name: normalizedName,
      })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          this.liftsStoreService.upsert({
            id: response.lift.id,
            name: response.lift.name,
            isActive: response.lift.isActive,
          });

          this.successMessage.set('Lift created.');
          this.liftName.set('');

          this.refreshLiftsForSelectedFilter();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.errorMessage.set('Lift name already exists.');
            return;
          }

          this.errorMessage.set('Lift was not created. Try again.');
        },
      });
  }

  submitRename(): void {
    const editingLift = this.editingLift();

    if (!editingLift) {
      return;
    }

    const normalizedName = this.editingLiftName().trim();

    if (!normalizedName) {
      this.errorMessage.set('Enter a lift name.');
      this.successMessage.set(null);
      return;
    }

    if (normalizedName === editingLift.name) {
      this.errorMessage.set(null);
      this.successMessage.set('No changes to save.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.liftsApiService
      .renameLift(editingLift.id, {
        name: normalizedName,
      })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          this.liftsStoreService.replace({
            id: response.lift.id,
            name: response.lift.name,
            isActive: response.lift.isActive,
          });

          this.successMessage.set('Lift renamed.');
          this.editingLiftId.set(null);
          this.editingLiftName.set('');

          this.refreshLiftsForSelectedFilter();
        },
        error: (error: HttpErrorResponse) => {
          this.editingLiftName.set(editingLift.name);
          this.editingLiftId.set(null);

          if (error.status === 409) {
            this.errorMessage.set('Lift name already exists.');
            return;
          }

          if (error.status === 404) {
            this.errorMessage.set('Lift no longer exists. Refresh and try again.');
            return;
          }

          this.errorMessage.set('Lift was not renamed. Try again.');
        },
      });
  }

  private refreshLiftsForSelectedFilter(): void {
    const activeOnly = this.activeOnly();

    this.liftsApiService.listLifts(activeOnly).subscribe({
      next: (refreshResponse) => {
        this.liftsStoreService.setResponse(refreshResponse);
        this.lastLoadedActiveOnly.set(activeOnly);
      },
      error: () => {
        this.successMessage.set(null);
        this.errorMessage.set('Unable to refresh lifts right now.');
      },
    });
  }
}
