import { Injectable, signal, computed, inject } from '@angular/core';
import { WorkerWeatherService } from './worker-weather.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { WeatherData, WeatherForecast, WeatherAlert } from '../../admin/models/weather.model';
import { finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class WorkerWeatherStateService {
  private weatherService = inject(WorkerWeatherService);
  private snackBar = inject(MatSnackBar);

  // State
  private _currentWeather = signal<WeatherData | null>(null);
  private _forecast = signal<WeatherForecast | null>(null);
  private _alerts = signal<WeatherAlert[]>([]);
  private _isLoading = signal<boolean>(false);
  private _selectedFieldId = signal<number | null>(null);

  // Expose signals as readonly
  currentWeather = this._currentWeather.asReadonly();
  forecast = this._forecast.asReadonly();
  alerts = this._alerts.asReadonly();
  isLoading = this._isLoading.asReadonly();
  selectedFieldId = this._selectedFieldId.asReadonly();

  // Computed state
  unreadAlertCount = computed(() => 
    this._alerts().filter(a => !a.isAcknowledged).length
  );
  
  hasWeatherWarning = computed(() => {
    const weather = this._currentWeather();
    if (!weather) return false;
    
    return (weather.temperature ?? 0) > 35 || 
           (weather.temperature ?? 0) < 5 ||
           (weather.windSpeed ?? 0) > 30 ||
           (weather.condition?.toLowerCase().includes('storm') ?? false);
  });

  // Actions
  setSelectedField(fieldId: number | null) {
    this._selectedFieldId.set(fieldId);
    if (fieldId) {
      this.loadWeatherData(fieldId);
    } else {
      this._currentWeather.set(null);
      this._forecast.set(null);
    }
  }

  loadWeatherData(fieldId: number) {
    this._isLoading.set(true);
    
    this.weatherService.getCurrentWeather(fieldId)
      .pipe(finalize(() => this._isLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this._currentWeather.set(res.data);
          } else {
            this._currentWeather.set(null);
            this.showError(res.message);
          }
        },
        error: (err) => {
          this._currentWeather.set(null);
          this.showError('Failed to load current weather');
        }
      });

    this.weatherService.getForecast(fieldId)
      .subscribe({
        next: (res) => {
          if (res.success) {
            this._forecast.set(res.data);
          } else {
            this._forecast.set(null);
          }
        },
        error: () => {
          this._forecast.set(null);
        }
      });
  }

  loadAlerts() {
    // Only load active alerts
    this.weatherService.getWeatherAlerts({ isActive: true, pageSize: 50 })
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this._alerts.set(res.data.items);
          }
        },
        error: (err) => {
          console.error('Failed to load weather alerts', err);
        }
      });
  }

  private showError(message: string) {
    this.snackBar.open(message, 'Close', { duration: 3000 });
  }
}
