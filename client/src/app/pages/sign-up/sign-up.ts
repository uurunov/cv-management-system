import { Component, inject, signal } from '@angular/core';
import { Auth, UserRole } from '../../services/auth';
import { ButtonModule } from '@openng/optimus-ui/button';
import {
  email,
  form,
  FormField,
  required,
  FormRoot,
  minLength,
  maxLength,
  pattern,
} from '@angular/forms/signals';
import { IconFieldModule } from '@openng/optimus-ui/iconfield';
import { InputIconModule } from '@openng/optimus-ui/inputicon';
import { InputTextModule } from '@openng/optimus-ui/inputtext';
import { MessageModule } from '@openng/optimus-ui/message';
import { Router } from '@angular/router';
import { SelectButtonModule } from '@openng/optimus-ui/selectbutton';
import { MessageService } from '@openng/optimus-ui/api';

export interface RegisterData {
  email: string;
  password: string;
  role: UserRole;
}

@Component({
  imports: [
    ButtonModule,
    FormField,
    FormRoot,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    MessageModule,
    SelectButtonModule,
  ],
  selector: 'app-sign-up',
  styleUrl: './sign-up.css',
  templateUrl: './sign-up.html',
})
export class SignUp {
  protected authService = inject(Auth);
  private messageService = inject(MessageService);
  protected router = inject(Router);
  roleOptions = signal(['Candidate', 'Recruiter']);
  private readonly INITIAL_MODEL = { email: '', password: '', role: 'Candidate' as UserRole };

  isLoading = signal(false);
  registerFormModel = signal<RegisterData>({ ...this.INITIAL_MODEL });

  registerForm = form(
    this.registerFormModel,
    (schemaPath) => {
      required(schemaPath.email, { message: 'Email is required' });
      email(schemaPath.email, { message: 'Please enter a valid email address' });
      required(schemaPath.password, { message: 'Password is required' });
      required(schemaPath.role, { message: 'Role is required' });
      minLength(schemaPath.password, 6, { message: 'Password must be at least 6 characters' });
      maxLength(schemaPath.password, 50, { message: 'Password is too long' });
      pattern(schemaPath.password, /^(?=.*[A-Z])(?=.*[a-z])(?=.*[^A-Za-z0-9]).{6,}$/, {
        message:
          'Password must contain at least one uppercase, lowercase, and non alphanumeric character',
      });
    },
    {
      submission: {
        action: async (field) => {
          await this.authService.register(field().value()).subscribe({
            next: async () => {
              field().reset({ ...this.INITIAL_MODEL });
              await this.authService.fetchCurrentUser().subscribe(() => {
                this.router.navigate(['home']);
              });
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Registration successful.',
              });
            },
            error: (err) => {
              console.log(err);

              let finalErrorMessage = '';
              if (err.status === 400) {
                const errorMsg: string[] = err.error;
                finalErrorMessage = errorMsg[0];
              } else {
                finalErrorMessage =
                  err.error?.message ?? 'Registration failed. Please try again later.';
              }

              this.messageService.add({
                severity: 'error',
                summary: 'Failure',
                detail: finalErrorMessage,
              });
            },
          });
        },
      },
    },
  );
}
