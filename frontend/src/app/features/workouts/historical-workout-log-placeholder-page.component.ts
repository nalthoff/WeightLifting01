import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-historical-workout-log-placeholder-page',
  imports: [CommonModule, MatButtonModule, MatCardModule, RouterLink],
  template: `
    <section class="history-log-placeholder">
      <mat-card appearance="outlined">
        <mat-card-header>
          <mat-card-title>Historical Workout Logging</mat-card-title>
          <mat-card-subtitle>Placeholder route for backfill workflow.</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>
            TODO: Build the full historical workout logging experience in this route.
          </p>
          <p>
            For now, use this page as the navigation target from Home while the full workflow is in
            progress.
          </p>
        </mat-card-content>
        <mat-card-actions>
          <a mat-button color="primary" routerLink="/">Back to Home</a>
        </mat-card-actions>
      </mat-card>
    </section>
  `,
  styles: `
    .history-log-placeholder {
      display: block;
    }
  `,
})
export class HistoricalWorkoutLogPlaceholderPageComponent {}
