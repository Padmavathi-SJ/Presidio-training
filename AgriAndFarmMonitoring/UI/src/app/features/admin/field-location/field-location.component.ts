// src/app/features/admin/field-location/field-location.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { FieldService } from '../services/field.service';
import { Field } from '../models/field.model';

@Component({
  selector: 'app-field-location',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="p-6">
      <h2 class="text-xl font-bold text-gray-800 mb-4">Update Field Location</h2>
      <p class="text-sm text-gray-500 mb-4">Update the coordinates for {{ data.field.fieldName }}</p>

      <form [formGroup]="locationForm" (ngSubmit)="onSubmit()">
        <div class="grid grid-cols-2 gap-4">
          <mat-form-field appearance="outline">
            <mat-label>Latitude</mat-label>
            <input matInput type="number" formControlName="latitude" placeholder="-90 to 90" step="0.000001">
            <span matSuffix>°</span>
            <mat-error *ngIf="hasError('latitude', 'required')">Latitude is required</mat-error>
            <mat-error *ngIf="hasError('latitude', 'min')">Must be between -90 and 90</mat-error>
            <mat-error *ngIf="hasError('latitude', 'max')">Must be between -90 and 90</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Longitude</mat-label>
            <input matInput type="number" formControlName="longitude" placeholder="-180 to 180" step="0.000001">
            <span matSuffix>°</span>
            <mat-error *ngIf="hasError('longitude', 'required')">Longitude is required</mat-error>
            <mat-error *ngIf="hasError('longitude', 'min')">Must be between -180 and 180</mat-error>
            <mat-error *ngIf="hasError('longitude', 'max')">Must be between -180 and 180</mat-error>
          </mat-form-field>
        </div>

        <div class="flex justify-end gap-2 mt-6 pt-4 border-t border-gray-200">
          <button mat-button type="button" (click)="onCancel()" [disabled]="isLoading">
            Cancel
          </button>
          <button 
            mat-raised-button 
            color="primary" 
            type="submit" 
            [disabled]="locationForm.invalid || isLoading"
            class="!bg-primary-600"
          >
            @if (isLoading) {
              <span class="flex items-center">
                <mat-spinner [diameter]="20" class="mr-2"></mat-spinner>
                Updating...
              </span>
            } @else {
              <span>Update Location</span>
            }
          </button>
        </div>
      </form>
    </div>
  `
})
export class FieldLocationComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<FieldLocationComponent>);
  private snackBar = inject(MatSnackBar);

  // ✅ Make it public (remove private)
  data = inject<{ field: Field }>(MAT_DIALOG_DATA);

  locationForm: FormGroup;
  isLoading = false;

  constructor() {
    this.locationForm = this.fb.group({
      latitude: [this.data.field.latitude || 0, [Validators.required, Validators.min(-90), Validators.max(90)]],
      longitude: [this.data.field.longitude || 0, [Validators.required, Validators.min(-180), Validators.max(180)]]
    });
  }

  onSubmit(): void {
    if (this.locationForm.invalid) {
      this.locationForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const locationData = {
      latitude: parseFloat(this.locationForm.get('latitude')?.value),
      longitude: parseFloat(this.locationForm.get('longitude')?.value)
    };

    this.fieldService.updateFieldLocation(farmId, this.data.field.id, locationData)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
            this.snackBar.open('Field location updated successfully', 'Close', {
              duration: 3000,
              panelClass: ['bg-green-600', 'text-white']
            });
          } else {
            this.snackBar.open(response.message || 'Failed to update location', 'Close', {
              duration: 5000,
              panelClass: ['bg-red-600', 'text-white']
            });
          }
        },
        error: (error) => {
          console.error('Error updating location:', error);
          this.snackBar.open('Failed to update location', 'Close', {
            duration: 5000,
            panelClass: ['bg-red-600', 'text-white']
          });
        }
      });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.locationForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}