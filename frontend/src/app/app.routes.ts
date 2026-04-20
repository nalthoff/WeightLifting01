import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'settings/lifts',
  },
  {
    path: 'settings/lifts',
    loadChildren: () =>
      import('./features/settings/lifts/lifts.routes').then((module) => module.LIFTS_ROUTES),
  },
];
