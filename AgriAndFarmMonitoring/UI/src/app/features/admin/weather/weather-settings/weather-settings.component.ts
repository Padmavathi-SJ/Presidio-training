// src/app/features/admin/weather/weather-settings/weather-settings.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// Angular Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { WeatherApiSettings } from '../../models/weather.model';

@Component({
  selector: 'app-weather-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSliderModule
  ],
  templateUrl: './weather-settings.component.html',
  styleUrls: ['./weather-settings.component.scss']
})
export class WeatherSettingsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State Signals
  isLoading = signal(false);
  isSaving = signal(false);
  settings = signal<WeatherApiSettings | null>(null);

  // Form
  settingsForm!: FormGroup;
  private destroy$ = new Subject<void>();

  // Options
  apiProviders = [
    { value: 'OpenWeatherMap', label: 'OpenWeatherMap' },
    { value: 'WeatherAPI', label: 'WeatherAPI' },
    { value: 'TomorrowIO', label: 'TomorrowIO' }
  ];

  updateIntervals = [
    { value: 15, label: '15 minutes' },
    { value: 30, label: '30 minutes' },
    { value: 60, label: '1 hour' },
    { value: 120, label: '2 hours' },
    { value: 180, label: '3 hours' },
    { value: 360, label: '6 hours' }
  ];

  constructor() {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  private initForm(): void {
    this.settingsForm = this.fb.group({
      apiProvider: ['OpenWeatherMap', [Validators.required]],
      apiKey: ['', [Validators.required, Validators.minLength(10)]],
      baseUrl: ['', [Validators.required]],
      updateIntervalMinutes: [60, [Validators.required, Validators.min(15), Validators.max(360)]],
      autoUpdateEnabled: [true]
    });
  }

  private loadSettings(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);
    this.weatherService.getWeatherSettings(farmId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.settings.set(response.data);
            this.patchForm(response.data);
          } else {
            this.showError(response.message || 'Failed to load weather settings');
          }
        },
        error: (error) => {
          console.error('Error loading weather settings:', error);
          // If settings don't exist, use defaults
          const defaultSettings: WeatherApiSettings = {
            apiProvider: 'OpenWeatherMap',
            apiKey: '',
            baseUrl: 'https://api.openweathermap.org/data/2.5',
            updateIntervalMinutes: 60,
            autoUpdateEnabled: true
          };
          this.settings.set(defaultSettings);
          this.patchForm(defaultSettings);
          this.showWarning('Using default weather settings. Please configure your API key.');
        }
      });
  }

  private patchForm(settings: WeatherApiSettings): void {
    this.settingsForm.patchValue({
      apiProvider: settings.apiProvider,
      apiKey: settings.apiKey,
      baseUrl: settings.baseUrl,
      updateIntervalMinutes: settings.updateIntervalMinutes,
      autoUpdateEnabled: settings.autoUpdateEnabled
    });
  }

  onSubmit(): void {
    if (this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }

    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isSaving.set(true);
    const formValue = this.settingsForm.value;

    const settings: WeatherApiSettings = {
      apiProvider: formValue.apiProvider,
      apiKey: formValue.apiKey,
      baseUrl: formValue.baseUrl,
      updateIntervalMinutes: formValue.updateIntervalMinutes,
      autoUpdateEnabled: formValue.autoUpdateEnabled
    };

    this.weatherService.updateWeatherSettings(farmId, settings)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.settings.set(settings);
            this.showSuccess('Weather settings updated successfully');
          } else {
            this.showError(response.message || 'Failed to update weather settings');
          }
        },
        error: (error) => {
          console.error('Error updating weather settings:', error);
          this.showError('Failed to update weather settings');
        }
      });
  }

  resetForm(): void {
    if (this.settings()) {
      this.patchForm(this.settings()!);
    } else {
      this.settingsForm.reset({
        apiProvider: 'OpenWeatherMap',
        apiKey: '',
        baseUrl: 'https://api.openweathermap.org/data/2.5',
        updateIntervalMinutes: 60,
        autoUpdateEnabled: true
      });
    }
    this.settingsForm.markAsPristine();
    this.showInfo('Settings reset to last saved values');
  }

  // =============================================
  // TEST CONNECTION
  // =============================================

  testConnection(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found. Please login again.');
      return;
    }

    // Get the first field with coordinates
    this.isLoading.set(true);
    // For now, just show a message - in production, you would test the API connection
    setTimeout(() => {
      this.isLoading.set(false);
      this.showSuccess('API connection test successful! (Mock)');
      // In production, you would call a test endpoint:
      // this.weatherService.testApiConnection(farmId).subscribe(...)
    }, 1500);
  }

  // =============================================
  // HELPER METHODS
  // =============================================

  hasError(controlName: string, errorName: string): boolean {
    const control = this.settingsForm.get(controlName);
    return !!(control && control.hasError(errorName) && control.touched);
  }

  getUpdateIntervalLabel(value: number): string {
    const option = this.updateIntervals.find(i => i.value === value);
    return option ? option.label : `${value} minutes`;
  }

  formatUpdateInterval(value: number): string {
    if (value < 60) {
      return `${value} min`;
    }
    const hours = Math.floor(value / 60);
    const minutes = value % 60;
    if (minutes === 0) {
      return `${hours} hr${hours > 1 ? 's' : ''}`;
    }
    return `${hours} hr ${minutes} min`;
  }

  // =============================================
  // NOTIFICATION METHODS
  // =============================================

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  private showWarning(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['warning-snackbar']
    });
  }

  private showInfo(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['info-snackbar']
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}