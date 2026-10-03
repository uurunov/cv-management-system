import { Routes } from '@angular/router';
import { userGuard } from './guards/user-guard';
import { guestGuard } from './guards/guest-guard';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  {
    path: 'home',
    loadComponent: () => import('../app/pages/home/home').then((page) => page.Home),
    title: 'Home',
  },
  {
    path: 'profile',
    loadComponent: () => import('../app/pages/profile/profile').then((page) => page.Profile),
    canActivate: [userGuard],
    title: 'Profile',
  },
  {
    path: 'login',
    loadComponent: () => import('../app/pages/sign-in/sign-in').then((page) => page.SignIn),
    canActivate: [guestGuard],
    title: 'Login',
  },
  {
    path: 'signup',
    loadComponent: () => import('../app/pages/sign-up/sign-up').then((page) => page.SignUp),
    canActivate: [guestGuard],
    title: 'Register',
  },
  {
    path: '**',
    loadComponent: () => import('../app/pages/not-found/not-found').then((page) => page.NotFound),
    title: 'Page Not Found',
  },
];
