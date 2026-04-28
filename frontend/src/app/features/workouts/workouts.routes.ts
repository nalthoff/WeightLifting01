import { Routes } from '@angular/router';

export const WORKOUTS_ROUTES: Routes = [
  {
    path: 'history-log',
    loadComponent: () =>
      import('./historical-workout-form.component').then((module) => module.HistoricalWorkoutFormComponent),
  },
  {
    path: ':workoutId',
    loadComponent: () =>
      import('./active-workout-page.component').then((module) => module.ActiveWorkoutPageComponent),
  },
];
