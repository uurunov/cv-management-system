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
import { TextareaModule } from '@openng/optimus-ui/textarea';
import { MessageService } from '@openng/optimus-ui/api';

export interface SalesforceData {
  firstName: string;
  lastName: string;
  company: string;
  phone: string;
  jobTitle: string;
  description: string;
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
    TextareaModule,
  ],
  selector: 'app-profile',
  styleUrl: './profile.css',
  templateUrl: './profile.html',
})
export class Profile {
  protected authService = inject(Auth);
  private messageService = inject(MessageService);
  visible = signal(false);
  private readonly INITIAL_MODEL = {
    firstName: '',
    lastName: '',
    company: '',
    phone: '',
    jobTitle: '',
    description: '',
  };

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
          console.log(field().value());
          await this.authService.sendToSalesforce(field().value()).subscribe({
            next: async () => {
              field().reset({ ...this.INITIAL_MODEL });
              this.closeDialog();
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

  showDialog() {
    this.visible.set(true);
  }

  closeDialog() {
    this.visible.set(false);
  }
}
