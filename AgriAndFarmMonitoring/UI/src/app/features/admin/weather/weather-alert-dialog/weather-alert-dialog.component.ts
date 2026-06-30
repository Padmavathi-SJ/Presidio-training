// src/app/features/admin/weather/weather-alert-dialog/weather-alert-dialog.component.ts
import { Component, inject, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { FieldService } from '../../services/field.service';
import { 
  WeatherAlert, 
  WeatherAlertCreate,
  WeatherAlertUpdate,
  WEATHER_ALERT_TYPES,
  WEATHER_ALERT_SEVERITIES,
  ALERT_SEVERITY_COLORS
} from '../../models/weather.model';

export interface WeatherAlertDialogData {
  alert?: WeatherAlert;
  mode: 'view' | 'create' | 'edit';
  fieldId?: number;
}

@Component({
  selector: 'app-weather-alert-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
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
    MatNativeDateModule,
    MatDividerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './weather-alert-dialog.component.html',
  styleUrls: ['./weather-alert-dialog.component.scss']
})
export class WeatherAlertDialogComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<WeatherAlertDialogComponent>);
  private snackBar = inject(MatSnackBar);

  data = inject<WeatherAlertDialogData>(MAT_DIALOG_DATA);
  
  alertForm: FormGroup;
  isLoading = false;
  fields: any[] = [];
  alertTypes = WEATHER_ALERT_TYPES;
  severities = WEATHER_ALERT_SEVERITIES;
  isEditMode = false;
  isViewMode = false;
  isCreateMode = false;

  constructor() {
    this.isViewMode = this.data.mode === 'view';
    this.isEditMode = this.data.mode === 'edit';
    this.isCreateMode = this.data.mode === 'create';

    this.alertForm = this.fb.group({
      fieldId: [{ value: null, disabled: this.isViewMode }, [Validators.required]],
      alertType: [{ value: '', disabled: this.isViewMode }, [Validators.required]],
      severity: [{ value: '', disabled: this.isViewMode }, [Validators.required]],
      title: [{ value: '', disabled: this.isViewMode }, [Validators.required, Validators.maxLength(200)]],
      message: [{ value: '', disabled: this.isViewMode }, [Validators.required, Validators.maxLength(1000)]],
      temperature: [{ value: null, disabled: this.isViewMode }],
      windSpeed: [{ value: null, disabled: this.isViewMode }],
      rainfallMm: [{ value: null, disabled: this.isViewMode }],
      expiresAt: [{ value: null, disabled: this.isViewMode }]
    });

    if (this.data.alert) {
      this.populateForm(this.data.alert);
    }

    if (this.data.fieldId) {
      this.alertForm.patchValue({ fieldId: this.data.fieldId });
    }

    this.loadFields();
  }

  private loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
        }
      },
      error: (error) => console.error('Error loading fields:', error)
    });
  }

  private populateForm(alert: WeatherAlert): void {
    this.alertForm.patchValue({
      fieldId: alert.fieldId,
      alertType: alert.alertType,
      severity: alert.severity,
      title: alert.title,
      message: alert.message,
      temperature: alert.temperature,
      windSpeed: alert.windSpeed,
      rainfallMm: alert.rainfallMm,
      expiresAt: alert.expiresAt ? new Date(alert.expiresAt) : null
    });
  }

  getSeverityColor(severity: string): string {
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getFieldName(fieldId: number): string {
    const field = this.fields.find(f => f.id === fieldId);
    return field ? field.fieldName : 'Unknown Field';
  }

  onSubmit(): void {
    if (this.alertForm.invalid) {
      this.alertForm.markAllAsTouched();
      return;
    }

    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      return;
    }

    this.isLoading = true;
    const formValue = this.alertForm.getRawValue();
    
    if (this.isCreateMode) {
      const data: WeatherAlertCreate = {
        fieldId: formValue.fieldId,
        alertType: formValue.alertType,
        severity: formValue.severity,
        title: formValue.title,
        message: formValue.message,
        temperature: formValue.temperature || null,
        windSpeed: formValue.windSpeed || null,
        rainfallMm: formValue.rainfallMm || null,
        expiresAt: formValue.expiresAt ? new Date(formValue.expiresAt).toISOString() : null
      };

      this.weatherService.createWeatherAlert(farmId, data)
        .pipe(finalize(() => this.isLoading = false))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.dialogRef.close({ success: true, data: response.data });
              this.snackBar.open('Weather alert created successfully', 'Close', {
                duration: 3000,
                panelClass: ['success-snackbar']
              });
            } else {
              this.snackBar.open(response.message || 'Failed to create alert', 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar']
              });
            }
          },
          error: (error) => {
            console.error('Error creating alert:', error);
            this.snackBar.open('Failed to create alert', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        });
    } else if (this.isEditMode && this.data.alert) {
      const data: WeatherAlertUpdate = {
        severity: formValue.severity,
        title: formValue.title,
        message: formValue.message,
        expiresAt: formValue.expiresAt ? new Date(formValue.expiresAt).toISOString() : null
      };

      this.weatherService.updateWeatherAlert(farmId, this.data.alert.id, data)
        .pipe(finalize(() => this.isLoading = false))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.dialogRef.close({ success: true, data: response.data });
              this.snackBar.open('Weather alert updated successfully', 'Close', {
                duration: 3000,
                panelClass: ['success-snackbar']
              });
            } else {
              this.snackBar.open(response.message || 'Failed to update alert', 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar']
              });
            }
          },
          error: (error) => {
            console.error('Error updating alert:', error);
            this.snackBar.open('Failed to update alert', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        });
    }
  }

  acknowledgeAlert(): void {
    if (!this.data.alert) return;

    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.isLoading = true;
    this.weatherService.acknowledgeWeatherAlert(farmId, this.data.alert.id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close({ success: true, acknowledged: true });
            this.snackBar.open('Alert acknowledged successfully', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
            });
          } else {
            this.snackBar.open(response.message || 'Failed to acknowledge alert', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error acknowledging alert:', error);
          this.snackBar.open('Failed to acknowledge alert', 'Close', {
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
    const control = this.alertForm.get(controlName);
    return !!(control && control.hasError(errorName) && control.touched);
  }
}