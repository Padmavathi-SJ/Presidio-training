// src/app/features/admin/weather/manual-weather-entry/manual-weather-entry.component.ts
import { Component, inject, Inject, OnInit } from '@angular/core';
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
import { finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';

import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { FieldService } from '../../services/field.service';
import { ManualWeatherEntry, WeatherData, WEATHER_CONDITIONS } from '../../models/weather.model';
import { ApiResponse } from '../../services/task.service';

export interface ManualWeatherEntryData {
  fieldId?: number;
  weatherData?: WeatherData;
  mode?: 'create' | 'edit';
}

@Component({
  selector: 'app-manual-weather-entry',
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
    MatNativeDateModule
  ],
  templateUrl: './manual-weather-entry.component.html',
  styleUrls: ['./manual-weather-entry.component.scss']
})
export class ManualWeatherEntryComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<ManualWeatherEntryComponent>);
  private snackBar = inject(MatSnackBar);

  data = inject<ManualWeatherEntryData>(MAT_DIALOG_DATA);

  entryForm: FormGroup;
  isLoading = false;
  fields: any[] = [];
  weatherConditions = WEATHER_CONDITIONS;
  isEditMode = false;
  entryId: number | null = null;

  constructor() {
    this.isEditMode = this.data.mode === 'edit';
    this.entryId = this.data.weatherData?.id || null;

    this.entryForm = this.fb.group({
      fieldId: [{ value: this.data?.fieldId || null, disabled: this.isEditMode }, [Validators.required]],
      temperature: [null, [Validators.min(-50), Validators.max(60)]],
      humidity: [null, [Validators.min(0), Validators.max(100)]],
      rainfallMm: [null, [Validators.min(0), Validators.max(500)]],
      windSpeed: [null, [Validators.min(0), Validators.max(200)]],
      condition: [null],
      recordedAt: [new Date(), [Validators.required]],
      notes: [null, [Validators.maxLength(500)]]
    });

    if (this.isEditMode && this.data.weatherData) {
      this.populateForm(this.data.weatherData);
    }
  }

  ngOnInit(): void {
    this.loadFields();
  }

  private loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.fields = response.data.items;
        }
      },
      error: (error: any) => console.error('Error loading fields:', error)
    });
  }

  private populateForm(weatherData: WeatherData): void {
    this.entryForm.patchValue({
      fieldId: weatherData.fieldId,
      temperature: weatherData.temperature,
      humidity: weatherData.humidity,
      rainfallMm: weatherData.rainfallMm,
      windSpeed: weatherData.windSpeed,
      condition: weatherData.condition,
      recordedAt: new Date(weatherData.recordedAt),
      notes: (weatherData as any).notes || null
    });
  }

  onSubmit(): void {
    if (this.entryForm.invalid) {
      this.entryForm.markAllAsTouched();
      return;
    }

    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      return;
    }

    this.isLoading = true;
    const formValue = this.entryForm.getRawValue();

    const data: ManualWeatherEntry = {
      fieldId: formValue.fieldId,
      temperature: formValue.temperature || null,
      humidity: formValue.humidity || null,
      rainfallMm: formValue.rainfallMm || null,
      windSpeed: formValue.windSpeed || null,
      condition: formValue.condition || null,
      recordedAt: new Date(formValue.recordedAt).toISOString(),
      notes: formValue.notes || null
    };

    let request: Observable<ApiResponse<any>>;
    if (this.isEditMode && this.entryId) {
      request = this.weatherService.updateWeatherData(farmId, this.entryId, data);
    } else {
      request = this.weatherService.addManualWeatherEntry(farmId, data);
    }

    request
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            this.dialogRef.close({ success: true, data: response.data });
            this.snackBar.open(
              this.isEditMode ? 'Weather data updated successfully' : 'Weather data added successfully',
              'Close',
              { duration: 3000, panelClass: ['success-snackbar'] }
            );
          } else {
            this.snackBar.open(response.message || 'Failed to save weather data', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error: any) => {
          console.error('Error saving weather data:', error);
          this.snackBar.open('Failed to save weather data', 'Close', {
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
    const control = this.entryForm.get(controlName);
    return !!(control && control.hasError(errorName) && control.touched);
  }
}