import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'signin',
    loadComponent: () => import('./app').then((page) => page.App),
    title: 'Sign in',
  },
  { path: '', redirectTo: 'signin' },
  {
    path: '**',
    loadComponent: () => import('../app/pages/not-found/not-found').then((page) => page.NotFound),
    title: 'Page Not Found',
  },
];
