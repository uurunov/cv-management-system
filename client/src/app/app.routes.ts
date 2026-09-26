import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  {
    path: 'home',
    loadComponent: () => import('../app/pages/home/home').then((page) => page.Home),
    title: 'Home',
  },
  {
    path: 'login',
    loadComponent: () => import('../app/pages/sign-in/sign-in').then((page) => page.SignIn),
    title: 'Login',
  },
  {
    path: 'signup',
    loadComponent: () => import('../app/pages/sign-up/sign-up').then((page) => page.SignUp),
    title: 'Register',
  },
  {
    path: '**',
    loadComponent: () => import('../app/pages/not-found/not-found').then((page) => page.NotFound),
    title: 'Page Not Found',
  },
];
