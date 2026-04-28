import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { finalize } from 'rxjs';

import { WorkoutsApiService } from '../../core/api/workouts-api.service';
import { WorkoutsStoreService } from '../../core/state/workouts-store.service';

const timeOfDayPattern = /^([01]\d|2[0-3]):([0-5]\d)$/;

@Component({
  selector: 'app-historical-workout-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
  ],
  templateUrl: './historical-workout-form.component.html',
  styleUrl: './historical-workout-form.component.scss',
})
export class HistoricalWorkoutFormComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly workoutsStoreService = inject(WorkoutsStoreService);
  private readonly router = inject(Router);

  readonly isSaving = signal(false);
  readonly saveErrorMessage = signal<string | null>(null);
  readonly saveSuccessMessage = signal<string | null>(null);
  readonly hasActiveWorkout = this.workoutsStoreService.hasActiveWorkout;
  readonly returnToWorkoutId = computed(
    () => this.workoutsStoreService.historicalFlowNavigationContext().returnToWorkoutId,
  );

  readonly form = this.formBuilder.group({
    trainingDayLocalDate: this.formBuilder.control('', [Validators.required]),
    startTimeLocal: this.formBuilder.control('', [Validators.required, Validators.pattern(timeOfDayPattern)]),
    sessionLengthMinutes: this.formBuilder.control<number | null>(null, [
      Validators.required,
      Validators.min(1),
      Validators.max(1440),
    ]),
    label: this.formBuilder.control(''),
  });

  constructor() {
    this.workoutsStoreService.captureHistoricalFlowNavigationContextFromActiveWorkout();
    this.workoutsStoreService.clearHistoricalFlowMessage();
  }

  canSave(): boolean {
    return this.form.valid && !this.isSaving();
  }

  onSave(): void {
    this.form.markAllAsTouched();

    if (!this.canSave()) {
      return;
    }

    const request = {
      trainingDayLocalDate: this.form.controls.trainingDayLocalDate.value,
      startTimeLocal: this.form.controls.startTimeLocal.value,
      sessionLengthMinutes: this.form.controls.sessionLengthMinutes.value ?? 0,
      label: this.form.controls.label.value.trim() || null,
    };

    this.isSaving.set(true);
    this.saveErrorMessage.set(null);
    this.saveSuccessMessage.set(null);

    this.workoutsApiService
      .createHistoricalWorkout(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          const successMessage = 'Historical workout started. Add lifts and sets, then save it to history.';
          this.saveSuccessMessage.set(successMessage);
          this.workoutsStoreService.setHistoricalFlowMessage('success', successMessage);
          this.workoutsStoreService.setActiveWorkout(response.workout);
          void this.router.navigate(['/workouts', response.workout.id], {
            queryParams: { mode: 'historical' },
          });
        },
        error: (error: HttpErrorResponse) => {
          const message =
            error.status === 422 || error.status === 409
              ? (error.error?.title ?? 'Historical workout details are invalid. Review and try again.')
              : 'Unable to save historical workout. Check your connection and try again.';
          this.saveErrorMessage.set(message);
          this.workoutsStoreService.setHistoricalFlowMessage('error', message);
        },
      });
  }

  openNativePicker(input: HTMLInputElement): void {
    // Chromium-based browsers support showPicker() for date/time controls.
    if (typeof input.showPicker === 'function') {
      input.showPicker();
      return;
    }

    input.focus();
  }
}
