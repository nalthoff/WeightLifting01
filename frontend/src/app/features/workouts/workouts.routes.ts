import { Routes } from '@angular/router';

export const WORKOUTS_ROUTES: Routes = [
  {
    path: ':workoutId',
    loadComponent: () =>
      import('./active-workout-page.component').then((module) => module.ActiveWorkoutPageComponent),
  },
];
