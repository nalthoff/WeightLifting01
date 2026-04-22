import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

export interface DeactivateLiftDialogData {
  liftId: string;
  liftName: string;
}

@Component({
  selector: 'app-deactivate-lift-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './deactivate-lift-dialog.component.html',
  styleUrl: './deactivate-lift-dialog.component.scss',
})
export class DeactivateLiftDialogComponent {
  readonly data = inject<DeactivateLiftDialogData>(MAT_DIALOG_DATA);
}
