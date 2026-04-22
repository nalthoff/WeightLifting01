import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import type { LiftListItem } from '../../../core/api/lifts-api.service';
import {
  DeactivateLiftDialogComponent,
} from './deactivate-lift-dialog/deactivate-lift-dialog.component';
import { LiftsPageFacade } from './lifts-page.facade';

@Component({
  selector: 'app-lifts-page',
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './lifts-page.component.html',
  styleUrl: './lifts-page.component.scss',
})
export class LiftsPageComponent implements OnInit {
  readonly facade = inject(LiftsPageFacade);
  private readonly dialog = inject(MatDialog);

  ngOnInit(): void {
    this.facade.load();
  }

  openDeactivateDialog(lift: LiftListItem): void {
    if (!lift.isActive) {
      return;
    }

    this.facade.prepareForDeactivateDialog();

    this.dialog
      .open(DeactivateLiftDialogComponent, {
        width: 'min(100vw - 2rem, 22rem)',
        maxWidth: '95vw',
        autoFocus: 'first-tabbable',
        data: { liftId: lift.id, liftName: lift.name },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result === true) {
          this.facade.submitDeactivate(lift.id);
          return;
        }

        if (result === false) {
          this.facade.notifyDeactivateCancelled();
        }
      });
  }
}
