import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';

import { LiftsApiService } from '../../../core/api/lifts-api.service';
import { LiftsStoreService } from '../../../core/state/lifts-store.service';

@Injectable({
  providedIn: 'root',
})
export class LiftsPageFacade {
  private readonly liftsApiService = inject(LiftsApiService);
  private readonly liftsStoreService = inject(LiftsStoreService);

  readonly liftName = signal('');
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isLoading = signal(false);
  readonly lifts = computed(() => this.liftsStoreService.items());

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
}
