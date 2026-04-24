import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface WorkoutRenameDialogData {
  initialLabel: string;
}

@Component({
  selector: 'app-workout-rename-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <h2 mat-dialog-title>Rename workout</h2>
    <mat-dialog-content>
      <p>Workout name is optional. Leave blank to clear it.</p>
      <mat-form-field appearance="outline" subscriptSizing="dynamic" class="workout-rename-dialog__field">
        <mat-label>Workout name</mat-label>
        <input
          matInput
          data-testid="active-workout-rename-input"
          [formControl]="labelControl"
          autocomplete="off"
        />
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button type="button" mat-button data-testid="active-workout-rename-cancel" (click)="dialogRef.close()">
        Cancel
      </button>
      <button type="button" mat-flat-button color="primary" data-testid="active-workout-rename-confirm" (click)="confirm()">
        Save Name
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .workout-rename-dialog__field {
        width: 100%;
      }
    `,
  ],
})
export class WorkoutRenameDialogComponent {
  private readonly data = inject<WorkoutRenameDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<WorkoutRenameDialogComponent, string>);
  readonly labelControl = new FormControl(this.data.initialLabel, { nonNullable: true });

  confirm(): void {
    this.dialogRef.close(this.labelControl.value);
  }
}
