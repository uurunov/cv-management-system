import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { ButtonModule } from '@openng/optimus-ui/button';
import { Auth } from './services/auth';
import { ToolbarModule } from '@openng/optimus-ui/toolbar';
import { ChipModule } from '@openng/optimus-ui/chip';
import { ConfirmationService, MessageService } from '@openng/optimus-ui/api';
import { ToastModule } from '@openng/optimus-ui/toast';
import { ConfirmDialogModule } from '@openng/optimus-ui/confirmdialog';
import { filter } from 'rxjs/operators';

@Component({
  imports: [
    RouterOutlet,
    ButtonModule,
    ToolbarModule,
    ChipModule,
    ToastModule,
    ConfirmDialogModule,
  ],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
  providers: [MessageService, ConfirmationService],
})
export class App implements OnInit {
  protected authService = inject(Auth);
  protected router = inject(Router);
  currentRoute = signal('');
  private messageService = inject(MessageService);

  ngOnInit() {
    this.authService.fetchCurrentUser().subscribe();
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute.set(event.urlAfterRedirects);
      });
  }

  protected logout() {
    this.authService.logout().subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'You are logged out.',
      });
    });
  }
}
