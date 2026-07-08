// src/app/features/admin/reset-password-dialog/reset-password-dialog.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
// ✅ Remove MatSelectModule - not used
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { WorkerService } from '../services/worker.service';
import { Worker } from '../models/worker.model';

interface DialogData {
  worker: Worker;
}

@Component({
  selector: 'app-reset-password-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    // ❌ Remove MatSelectModule from here
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="p-4 sm:p-6">
      <h2 class="text-xl font-bold text-gray-800 mb-2">Reset Password</h2>
      <p class="text-sm text-gray-500 mb-4">
        Reset password for <span class="font-medium text-gray-700">{{ data.worker.name }}</span>
      </p>

      <form [formGroup]="resetForm" (ngSubmit)="onSubmit()">
        <div class="space-y-4">
          <!-- New Password -->
          <mat-form-field class="w-full" appearance="outline">
            <mat-label>New Password *</mat-label>
            <input 
              matInput 
              formControlName="newPassword" 
              placeholder="Enter new password"
              [type]="hidePassword ? 'password' : 'text'"
            >
            <button mat-icon-button matSuffix (click)="hidePassword = !hidePassword" type="button">
              <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            <mat-error *ngIf="hasError('newPassword', 'required')">Password is required</mat-error>
            <mat-error *ngIf="hasError('newPassword', 'minlength')">Password must be at least 6 characters</mat-error>
          </mat-form-field>

          <!-- Confirm Password -->
          <mat-form-field class="w-full" appearance="outline">
            <mat-label>Confirm Password *</mat-label>
            <input 
              matInput 
              formControlName="confirmPassword" 
              placeholder="Confirm new password"
              [type]="hideConfirmPassword ? 'password' : 'text'"
            >
            <button mat-icon-button matSuffix (click)="hideConfirmPassword = !hideConfirmPassword" type="button">
              <mat-icon>{{ hideConfirmPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            <mat-error *ngIf="hasError('confirmPassword', 'required')">Please confirm your password</mat-error>
            <mat-error *ngIf="hasError('confirmPassword', 'passwordMismatch')">Passwords do not match</mat-error>
          </mat-form-field>
        </div>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row justify-end gap-2 mt-6 pt-4 border-t border-gray-200">
          <button mat-button type="button" (click)="onCancel()" [disabled]="isLoading" class="w-full sm:w-auto order-2 sm:order-1">
            Cancel
          </button>
          <button 
            mat-raised-button 
            color="primary" 
            type="submit" 
            [disabled]="resetForm.invalid || isLoading"
            class="!bg-primary-600 w-full sm:w-auto order-1 sm:order-2"
          >
            @if (isLoading) {
              <span class="flex items-center justify-center">
                <mat-spinner [diameter]="20" class="mr-2"></mat-spinner>
                Resetting...
              </span>
            } @else {
              <span>Reset Password</span>
            }
          </button>
        </div>
      </form>
    </div>
  `
})
export class ResetPasswordDialogComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private workerService = inject(WorkerService);
  private dialogRef = inject(MatDialogRef<ResetPasswordDialogComponent>);
  private snackBar = inject(MatSnackBar);
  
  data = inject<DialogData>(MAT_DIALOG_DATA);

  resetForm: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor() {
    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      group.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    if (group.get('confirmPassword')?.errors?.['passwordMismatch']) {
      const errors = { ...group.get('confirmPassword')?.errors };
      delete errors['passwordMismatch'];
      group.get('confirmPassword')?.setErrors(Object.keys(errors).length ? errors : null);
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const { newPassword, confirmPassword } = this.resetForm.value;

    this.workerService.resetWorkerPassword(farmId, this.data.worker.id, newPassword, confirmPassword)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
            this.snackBar.open('Password reset successfully', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
            });
          } else {
            this.snackBar.open(response.message || 'Failed to reset password', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error resetting password:', error);
          this.snackBar.open('Failed to reset password', 'Close', {
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
    const control = this.resetForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}