// src/app/features/admin/worker-form/worker-form.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter, MAT_DATE_FORMATS, MAT_NATIVE_DATE_FORMATS, NativeDateAdapter } from '@angular/material/core';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { WorkerService } from '../services/worker.service';
import { Worker, CreateWorkerDto, UpdateWorkerDto, WORKER_ROLES } from '../models/worker.model';

interface DialogData {
  mode: 'create' | 'edit';
  worker?: Worker;
}

@Component({
  selector: 'app-worker-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  // ✅ Add providers for DateAdapter
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS }
  ],
  templateUrl: './worker-form.component.html'
})
export class WorkerFormComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private workerService = inject(WorkerService);
  private dialogRef = inject(MatDialogRef<WorkerFormComponent>);
  private snackBar = inject(MatSnackBar);
  
   dialogData = inject<DialogData>(MAT_DIALOG_DATA);

  workerForm: FormGroup;
  isLoading = false;
  mode: 'create' | 'edit' = 'create';
  workerRoles = WORKER_ROLES;
  hidePassword = true;

  constructor() {
    this.mode = this.dialogData.mode || 'create';

    this.workerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.maxLength(20)]],
      role: ['Worker'],
      hireDate: ['', []]
    });

    if (this.mode === 'create') {
      this.workerForm.addControl('password', this.fb.control('', [Validators.required, Validators.minLength(6)]));
    }

    if (this.mode === 'edit' && this.dialogData.worker) {
      const worker = this.dialogData.worker;
      this.workerForm.patchValue({
        name: worker.name,
        email: worker.email,
        phone: worker.phone,
        role: worker.role || 'Worker',
        hireDate: worker.hireDate ? new Date(worker.hireDate) : null
      });
    }
  }

  onSubmit(): void {
    if (this.workerForm.invalid) {
      this.workerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const formValue = this.workerForm.value;
    
    const cleanedData: any = {};
    Object.keys(formValue).forEach(key => {
      const value = formValue[key];
      if (value !== '' && value !== null && value !== undefined) {
        cleanedData[key] = value;
      }
    });

    if (!cleanedData.role) {
      cleanedData.role = 'Worker';
    }

    if (cleanedData.hireDate) {
      cleanedData.hireDate = new Date(cleanedData.hireDate).toISOString();
    }

    let request;
    if (this.mode === 'create') {
      request = this.workerService.createWorker(farmId, cleanedData as CreateWorkerDto);
    } else {
      request = this.workerService.updateWorker(farmId, this.dialogData.worker!.id, cleanedData as UpdateWorkerDto);
    }

    request
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
            this.snackBar.open(
              this.mode === 'create' ? 'Worker created successfully' : 'Worker updated successfully',
              'Close',
              { duration: 3000, panelClass: ['success-snackbar'] }
            );
          } else {
            this.snackBar.open(response.message || 'Operation failed', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error saving worker:', error);
          this.snackBar.open('Failed to save worker', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.workerForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}