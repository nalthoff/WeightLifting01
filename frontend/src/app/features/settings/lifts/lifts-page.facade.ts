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
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isLoading = signal(false);
  readonly lifts = computed(() => this.liftsStoreService.items());
  readonly editingLift = computed(() =>
    this.lifts().find((lift) => lift.id === this.editingLiftId()) ?? null,
  );

  load(): void {
    if (this.liftsStoreService.isLoaded()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.liftsApiService
      .listLifts()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => this.liftsStoreService.setResponse(response),
        error: () => this.errorMessage.set('Unable to load lifts right now.'),
      });
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

  submit(): void {
    const normalizedName = this.liftName().trim();

    if (!normalizedName) {
      this.errorMessage.set('Enter a lift name.');
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

          this.liftsApiService.listLifts().subscribe({
            next: (refreshResponse) => this.liftsStoreService.setResponse(refreshResponse),
          });
        },
        error: () => {
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

          this.liftsApiService.listLifts().subscribe({
            next: (refreshResponse) => this.liftsStoreService.setResponse(refreshResponse),
          });
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
}
