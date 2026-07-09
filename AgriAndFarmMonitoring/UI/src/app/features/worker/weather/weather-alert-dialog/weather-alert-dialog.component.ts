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
import { WorkerWeatherService } from '../../services/worker-weather.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { 
  WeatherAlert, 
  WeatherAlertCreate,
  WeatherAlertUpdate,
  WEATHER_ALERT_TYPES,
  WEATHER_ALERT_SEVERITIES,
  ALERT_SEVERITY_COLORS
} from '../../../admin/models/weather.model';

export interface WeatherAlertDialogData {
  alert?: WeatherAlert;
  mode: 'view' | 'create' | 'edit';
  fieldId?: number;
}

@Component({
  selector: 'app-worker-weather-alert-dialog',
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
export class WorkerWeatherAlertDialogComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private weatherService = inject(WorkerWeatherService);
  private fieldService = inject(WorkerFieldService);
  private dialogRef = inject(MatDialogRef<WorkerWeatherAlertDialogComponent>);
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
      expiresAt: [{ value: null, disabled: this.isViewMode }],
      resolutionNotes: [{ value: '', disabled: this.data.alert?.isAcknowledged }] // Only enabled if not acknowledged
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
    this.fieldService.getMyAssignedFields().subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data;
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
      expiresAt: alert.expiresAt ? new Date(alert.expiresAt) : null,
      resolutionNotes: (alert as any).resolutionNotes || ''
    });
  }

  getSeverityColor(severity: string): string {
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getFieldName(fieldId: number): string {
    const field = this.fields.find(f => f.fieldId === fieldId);
    return field ? field.fieldName : 'Unknown Field';
  }





  onResolve(): void {
    if (!this.data.alert || this.data.alert.isAcknowledged) return;

    this.isLoading = true;
    const notes = this.alertForm.get('resolutionNotes')?.value || '';

    this.weatherService.resolveAlert(this.data.alert.id, notes)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Alert resolved successfully', 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
            this.dialogRef.close({ success: true });
          } else {
            this.snackBar.open(res.message || 'Failed to resolve alert', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        },
        error: (err) => {
          console.error('Error resolving alert:', err);
          this.snackBar.open('Failed to resolve alert', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
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