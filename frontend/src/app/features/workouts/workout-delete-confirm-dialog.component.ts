import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-workout-delete-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Delete workout?</h2>
    <mat-dialog-content>
      Deleting removes this in-progress workout and its entries permanently.
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button type="button" mat-button data-testid="active-workout-delete-cancel" [mat-dialog-close]="false">
        Keep Workout
      </button>
      <button type="button" mat-flat-button color="warn" data-testid="active-workout-delete-confirm" [mat-dialog-close]="true">
        Delete Workout
      </button>
    </mat-dialog-actions>
  `,
})
export class WorkoutDeleteConfirmDialogComponent {}
