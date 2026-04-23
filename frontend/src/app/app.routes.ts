import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home-page.component').then((module) => module.HomePageComponent),
  },
  {
    path: 'settings/lifts',
    loadChildren: () =>
      import('./features/settings/lifts/lifts.routes').then((module) => module.LIFTS_ROUTES),
  },
  {
    path: 'workouts',
    loadChildren: () =>
      import('./features/workouts/workouts.routes').then((module) => module.WORKOUTS_ROUTES),
  },
  {
    path: 'history',
    loadChildren: () =>
      import('./features/history/history.routes').then((module) => module.HISTORY_ROUTES),
  },
];
