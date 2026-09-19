import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'signin',
    loadComponent: () => import('../app/pages/sign-in/sign-in').then((page) => page.SignIn),
    title: 'Sign in',
  },
  { path: '', redirectTo: 'signin' },
  {
    path: '**',
    loadComponent: () => import('../app/pages/not-found/not-found').then((page) => page.NotFound),
    title: 'Page Not Found',
  },
];
