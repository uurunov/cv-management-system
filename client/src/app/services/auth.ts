import { HttpClient } from '@angular/common/http';
import { inject, Service, signal } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { of } from 'rxjs/internal/observable/of';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { tap } from 'rxjs/internal/operators/tap';
import { RegisterData } from '../pages/sign-up/sign-up';
import { LoginData } from '../pages/sign-in/sign-in';

export interface CurrentUser {
  email: string;
  roles: string[];
}

export interface AuthResponse {
  message: string;
}

export type UserRole = 'Candidate' | 'Recruiter';

@Service()
export class Auth {
  private http = inject(HttpClient);

  currentUser = signal<CurrentUser | null>(null);
  initialAuthChecked = signal(false);

  register(credentials: RegisterData): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', {
      email: credentials.email,
      password: credentials.password,
      role: credentials.role,
    });
  }

  login(credentials: LoginData): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', {
      email: credentials.email,
      password: credentials.password,
    });
  }

  logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/logout', {}).pipe(
      tap(() => {
        this.currentUser.set(null);
      }),
    );
  }

  loginWithProvider(provider: 'Google' | 'Microsoft', role: UserRole = 'Candidate'): void {
    window.location.href = `/api/auth/external/${provider}?role=${role}`;
  }

  fetchCurrentUser(): Observable<CurrentUser | null> {
    return this.http.get<CurrentUser | null>('/api/auth/current-user').pipe(
      tap((user) => {
        this.currentUser.set(user);
      }),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      }),
      finalize(() => {
        this.initialAuthChecked.set(true);
      }),
    );
  }
}
