import { Component, inject, signal } from '@angular/core';
import { CardModule } from '@openng/optimus-ui/card';
import { Auth } from '../../services/auth';
import { ButtonModule } from '@openng/optimus-ui/button';
import { AvatarModule } from '@openng/optimus-ui/avatar';
import { ChipModule } from '@openng/optimus-ui/chip';
import { Dialog, DialogModule } from '@openng/optimus-ui/dialog';
import { form, required, FormField, FormRoot } from '@angular/forms/signals';
import { InputTextModule } from '@openng/optimus-ui/inputtext';
import { KeyFilterModule } from '@openng/optimus-ui/keyfilter';
import { InputGroupModule } from '@openng/optimus-ui/inputgroup';
import { InputGroupAddonModule } from '@openng/optimus-ui/inputgroupaddon';
import { MessageService } from '@openng/optimus-ui/api';
import { SelectButtonModule } from '@openng/optimus-ui/selectbutton';

export interface SalesforceData {
  firstName: string;
  lastName: string;
  company: string;
  phone: string;
  jobTitle: string;
  description: string;
}

export interface SupportTicketData {
  summary: string;
  priority: 'Low' | 'Average' | 'High';
}

@Component({
  imports: [
    CardModule,
    ButtonModule,
    AvatarModule,
    ChipModule,
    DialogModule,
    Dialog,
    FormRoot,
    FormField,
    InputTextModule,
    KeyFilterModule,
    InputGroupModule,
    InputGroupAddonModule,
    SelectButtonModule,
  ],
  selector: 'app-profile',
  styleUrl: './profile.css',
  templateUrl: './profile.html',
})
export class Profile {
  protected authService = inject(Auth);
  private messageService = inject(MessageService);
  private readonly INITIAL_MODEL = {
    firstName: '',
    lastName: '',
    company: '',
    phone: '',
    jobTitle: '',
    description: '',
  };
  private readonly INITIAL_TICKET_MODEL = {
    summary: '',
    priority: 'Average' as 'Low' | 'Average' | 'High',
  };

  sfDialog = signal(false);
  sforceFormModel = signal<SalesforceData>({ ...this.INITIAL_MODEL });
  sforceForm = form(
    this.sforceFormModel,
    (schemaPath) => {
      required(schemaPath.firstName, { message: 'First name is required' });
      required(schemaPath.lastName, { message: 'Last name is required' });
      required(schemaPath.company, { message: 'Company is required' });
    },
    {
      submission: {
        action: async (field) => {
          await this.authService.sendToSalesforce(field().value()).subscribe({
            next: async () => {
              field().reset({ ...this.INITIAL_MODEL });
              this.sfDialog.set(false);
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Successfully been added to Salesforce CRM.',
              });
            },
            error: (err) => {
              console.log(err);
              let finalErrorMessage = '';
              if (err.status === 400) {
                const errorMsg: string[] = err.error;
                finalErrorMessage = errorMsg[0];
              } else {
                finalErrorMessage = err.error?.message ?? 'Failed to add to Salesforce CRM.';
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

  priorityOptions = signal(['Low', 'Average', 'High']);
  spTicketDialog = signal(false);
  spTicketFormModel = signal<SupportTicketData>({ ...this.INITIAL_TICKET_MODEL });
  spTicketForm = form(
    this.spTicketFormModel,
    (schemaPath) => {
      required(schemaPath.summary, { message: 'Summary is required' });
    },
    {
      submission: {
        action: async (field) => {
          await this.authService.submitSupportTicket(field().value()).subscribe({
            next: async () => {
              field().reset({ ...this.INITIAL_TICKET_MODEL });
              this.spTicketDialog.set(false);
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Support ticket submitted successfully.',
              });
            },
            error: (err) => {
              console.log(err);
              let finalErrorMessage = '';
              if (err.status === 400) {
                const errorMsg: string[] = err.error;
                finalErrorMessage = errorMsg[0];
              } else {
                finalErrorMessage = err.error?.message ?? 'Failed to submit support ticket.';
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
