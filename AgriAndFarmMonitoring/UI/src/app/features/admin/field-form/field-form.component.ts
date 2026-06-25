// src/app/features/admin/field-form/field-form.component.ts
import { Component, Inject, inject, OnInit } from '@angular/core';
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
import { MatDividerModule } from '@angular/material/divider'; // ✅ ADD THIS
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { FieldService } from '../services/field.service';
import { Field, CreateFieldDto, UpdateFieldDto, FIELD_STATUS_OPTIONS, SOIL_TYPE_OPTIONS } from '../models/field.model';

interface DialogData {
  mode: 'create' | 'edit';
  field?: Field;
}

@Component({
  selector: 'app-field-form',
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
    MatDividerModule // ✅ ADD THIS
  ],
  templateUrl: './field-form.component.html'
})
export class FieldFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<FieldFormComponent>);
  private snackBar = inject(MatSnackBar);
  
  data = inject<DialogData>(MAT_DIALOG_DATA);


  fieldForm: FormGroup;
  isLoading = false;
  mode: 'create' | 'edit' = 'create';
  statusOptions = FIELD_STATUS_OPTIONS;
  soilTypeOptions = SOIL_TYPE_OPTIONS;

  constructor() {
    this.mode = this.data.mode || 'create';

    this.fieldForm = this.fb.group({
      fieldName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[a-zA-Z0-9\s\-_]+$/)]],
      location: ['', [Validators.maxLength(200)]],
      areaHectares: ['', [Validators.min(0.01), Validators.max(10000)]],
      soilType: [''],
      status: [''],
      latitude: ['', [Validators.min(-90), Validators.max(90)]],
      longitude: ['', [Validators.min(-180), Validators.max(180)]]
    });

    // If both latitude and longitude are provided, they must be provided together
    this.fieldForm.get('latitude')?.valueChanges.subscribe(() => {
      this.validateCoordinates();
    });
    this.fieldForm.get('longitude')?.valueChanges.subscribe(() => {
      this.validateCoordinates();
    });
  }

  ngOnInit(): void {
    if (this.mode === 'edit' && this.data.field) {
      const field = this.data.field;
      this.fieldForm.patchValue({
        fieldName: field.fieldName,
        location: field.location,
        areaHectares: field.areaHectares,
        soilType: field.soilType,
        status: field.status,
        latitude: field.latitude,
        longitude: field.longitude
      });
    }
  }

  validateCoordinates(): void {
    const lat = this.fieldForm.get('latitude')?.value;
    const lng = this.fieldForm.get('longitude')?.value;

    if ((lat && !lng) || (!lat && lng)) {
      this.fieldForm.get('longitude')?.setErrors({ coordinateMismatch: true });
      this.fieldForm.get('latitude')?.setErrors({ coordinateMismatch: true });
    } else {
      const latErrors = this.fieldForm.get('latitude')?.errors;
      const lngErrors = this.fieldForm.get('longitude')?.errors;
      
      if (latErrors) {
        delete latErrors['coordinateMismatch'];
        if (Object.keys(latErrors).length === 0) {
          this.fieldForm.get('latitude')?.setErrors(null);
        }
      }
      
      if (lngErrors) {
        delete lngErrors['coordinateMismatch'];
        if (Object.keys(lngErrors).length === 0) {
          this.fieldForm.get('longitude')?.setErrors(null);
        }
      }
    }
  }

  onSubmit(): void {
    if (this.fieldForm.invalid) {
      this.fieldForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const formValue = this.fieldForm.value;
    
    // Clean empty strings and nulls
    const cleanedData: any = {};
    Object.keys(formValue).forEach(key => {
      const value = formValue[key];
      if (value !== '' && value !== null && value !== undefined) {
        cleanedData[key] = value;
      }
    });

    // Parse numeric values
    if (cleanedData.areaHectares) cleanedData.areaHectares = parseFloat(cleanedData.areaHectares);
    if (cleanedData.latitude) cleanedData.latitude = parseFloat(cleanedData.latitude);
    if (cleanedData.longitude) cleanedData.longitude = parseFloat(cleanedData.longitude);

    let request;
    if (this.mode === 'create') {
      request = this.fieldService.createField(farmId, cleanedData as CreateFieldDto);
    } else {
      request = this.fieldService.updateField(farmId, this.data.field!.id, cleanedData as UpdateFieldDto);
    }

    request
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
            this.snackBar.open(
              this.mode === 'create' ? 'Field created successfully' : 'Field updated successfully',
              'Close',
              { duration: 3000, panelClass: ['bg-green-600', 'text-white'] }
            );
          } else {
            this.snackBar.open(response.message || 'Operation failed', 'Close', {
              duration: 5000,
              panelClass: ['bg-red-600', 'text-white']
            });
          }
        },
        error: (error) => {
          console.error('Error saving field:', error);
          this.snackBar.open('Failed to save field', 'Close', {
            duration: 5000,
            panelClass: ['bg-red-600', 'text-white']
          });
        }
      });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // Helper to check if field has error
  hasError(controlName: string, errorName: string): boolean {
    const control = this.fieldForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}