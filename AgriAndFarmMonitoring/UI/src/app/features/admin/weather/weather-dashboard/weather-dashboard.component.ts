// src/app/features/admin/weather/weather-dashboard/weather-dashboard.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect, inject as injectDI } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';


// Angular Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatRippleModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// Chart.js Imports
import { BaseChartDirective } from 'ng2-charts';
import {ChartConfiguration, ChartDataset, ChartType, Chart, registerables } from 'chart.js';
import { ChartDataService } from '../../services/chart-data.service';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { WeatherSignalRService } from '../../services/weather-signalr.service';
import { FieldService } from '../../services/field.service';
import { 
  WeatherData, 
  WeatherAlert, 
  WeatherForecast,
  WeatherStatistics,
  ALERT_SEVERITY_COLORS,
  WEATHER_CONDITIONS
} from '../../models/weather.model';
import { WeatherAlertDialogComponent } from '../weather-alert-dialog/weather-alert-dialog.component';
import { ManualWeatherEntryComponent } from '../manual-weather-entry/manual-weather-entry.component';

Chart.register(...registerables);

@Component({
  selector: 'app-weather-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDialogModule,
    MatGridListModule,
    MatBadgeModule,
    MatRippleModule,
    MatSlideToggleModule,
    BaseChartDirective
  ],
  templateUrl: './weather-dashboard.component.html',
  styleUrls: ['./weather-dashboard.component.scss']
})
export class WeatherDashboardComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private weatherSignalR = inject(WeatherSignalRService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private chartDataService = inject(ChartDataService);

  // State
  isLoading = signal(false);
  isRefreshing = signal(false);
  currentWeather = signal<WeatherData[]>([]);
  activeAlerts = signal<WeatherAlert[]>([]);
  statistics = signal<WeatherStatistics | null>(null);
  selectedFieldId = signal<number | null>(null);
  fields: any[] = [];
  autoRefresh = signal(true);
  lastUpdated = signal<string | null>(null);

  // Computed
  hasAlerts = computed(() => this.activeAlerts().length > 0);
  criticalAlerts = computed(() => 
    this.activeAlerts().filter(a => a.severity === 'EMERGENCY' || a.severity === 'WARNING')
  );
  alertCount = computed(() => this.activeAlerts().length);
  hasWeather = computed(() => this.currentWeather().length > 0);
  selectedField = computed(() => {
    const fieldId = this.selectedFieldId();
    if (!fieldId) return null;
    return this.currentWeather().find(w => w.fieldId === fieldId);
  });

  // Form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Chart Data
  temperatureChartData!: ChartConfiguration['data'];
  temperatureChartOptions!: ChartConfiguration['options'];
  temperatureChartType: ChartType = 'line';

  humidityChartData!: ChartConfiguration['data'];
  humidityChartOptions!: ChartConfiguration['options'];
  humidityChartType: ChartType = 'line';

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null]
    });

    // Initialize charts
    this.initTemperatureChart();
    this.initHumidityChart();

    // Listen to filter changes
    this.filterForm.get('fieldId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        this.selectedFieldId.set(value);
        this.loadDashboardData();
      });
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadDashboardData();
    this.setupSignalR();
    this.startAutoRefresh();
  }

  private loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
          if (this.fields.length > 0 && !this.selectedFieldId()) {
            this.selectedFieldId.set(this.fields[0].id);
            this.filterForm.patchValue({ fieldId: this.fields[0].id });
          }
        }
      },
      error: (error) => console.error('Error loading fields:', error)
    });
  }

  private loadDashboardData(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.isLoading.set(true);
    const fieldId = this.selectedFieldId();

    // Load all data in parallel
    const requests = [
      fieldId ? this.loadCurrentWeather(farmId, fieldId) : Promise.resolve(),
      this.loadActiveAlerts(farmId),
      this.loadStatistics(farmId)
    ];

    Promise.all(requests)
      .finally(() => this.isLoading.set(false));
  }

  private loadCurrentWeather(farmId: number, fieldId: number): Promise<void> {
    return new Promise((resolve) => {
      this.weatherService.getCurrentWeather(farmId, fieldId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success && response.data) {
              this.currentWeather.set([response.data]);
              this.lastUpdated.set(new Date().toISOString());
              this.updateCharts([response.data]);
            }
            resolve();
          },
          error: () => resolve()
        });
    });
  }

  private loadActiveAlerts(farmId: number): Promise<void> {
    return new Promise((resolve) => {
      this.weatherService.getActiveWeatherAlerts(farmId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.activeAlerts.set(response.data || []);
            }
            resolve();
          },
          error: () => resolve()
        });
    });
  }

  private loadStatistics(farmId: number): Promise<void> {
    return new Promise((resolve) => {
      this.weatherService.getWeatherStatistics(farmId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.statistics.set(response.data);
            }
            resolve();
          },
          error: () => resolve()
        });
    });
  }

  // =============================================
  // CHARTS INITIALIZATION
  // =============================================

  private initTemperatureChart(): void {
    this.temperatureChartData = {
      labels: ['06:00', '09:00', '12:00', '15:00', '18:00', '21:00'],
      datasets: [
        {
          data: [],
          label: 'Temperature (°C)',
          borderColor: '#40916c',
          backgroundColor: 'rgba(64, 145, 108, 0.1)',
          fill: true,
          tension: 0.4,
          pointRadius: 4,
          pointBackgroundColor: '#40916c'
        }
      ]
    };

    this.temperatureChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: 'rgba(255,255,255,0.95)',
          titleColor: '#191c1d',
          bodyColor: '#191c1d',
          borderColor: '#e1e3e4',
          borderWidth: 1,
          cornerRadius: 8,
          padding: 12
        }
      },
      scales: {
        y: {
          beginAtZero: false,
          grid: {
            color: 'rgba(0,0,0,0.05)'
          }
        },
        x: {
          grid: {
            display: false
          }
        }
      }
    };
  }

  private initHumidityChart(): void {
    this.humidityChartData = {
      labels: ['06:00', '09:00', '12:00', '15:00', '18:00', '21:00'],
      datasets: [
        {
          data: [],
          label: 'Humidity (%)',
          borderColor: '#d4a373',
          backgroundColor: 'rgba(212, 163, 115, 0.1)',
          fill: true,
          tension: 0.4,
          pointRadius: 4,
          pointBackgroundColor: '#d4a373'
        }
      ]
    };

    this.humidityChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: 'rgba(255,255,255,0.95)',
          titleColor: '#191c1d',
          bodyColor: '#191c1d',
          borderColor: '#e1e3e4',
          borderWidth: 1,
          cornerRadius: 8,
          padding: 12
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          max: 100,
          grid: {
            color: 'rgba(0,0,0,0.05)'
          }
        },
        x: {
          grid: {
            display: false
          }
        }
      }
    };
  }

  private updateCharts(weatherData: WeatherData[]): void {
    if (!weatherData || weatherData.length === 0) return;

    // For demo purposes, generate some sample data points
    // In production, this would come from historical data
    const baseTemp = weatherData[0]?.temperature || 25;
    const baseHumidity = weatherData[0]?.humidity || 60;

    const tempData = [
      baseTemp - 2,
      baseTemp + 1,
      baseTemp + 3,
      baseTemp + 4,
      baseTemp + 1,
      baseTemp - 1
    ];

    const humidityData = [
      baseHumidity + 5,
      baseHumidity + 2,
      baseHumidity - 3,
      baseHumidity - 5,
      baseHumidity - 2,
      baseHumidity + 3
    ];

    this.temperatureChartData.datasets[0].data = tempData;
    this.humidityChartData.datasets[0].data = humidityData;

    // Refresh charts
    this.temperatureChartData = { ...this.temperatureChartData };
    this.humidityChartData = { ...this.humidityChartData };
  }

  // =============================================
  // SIGNALR SETUP
  // =============================================

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Join admin group
    this.weatherSignalR.joinAdminGroup(farmId);

    // Listen for weather updates
    this.weatherSignalR.weatherUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe((weather) => {
        if (weather) {
          this.currentWeather.set([weather]);
          this.lastUpdated.set(new Date().toISOString());
          this.updateCharts([weather]);
        }
      });

    // Listen for alert updates
    this.weatherSignalR.alertUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert) {
          this.activeAlerts.update(alerts => [alert, ...alerts]);
          this.showAlertNotification(alert);
        }
      });
  }

  private startAutoRefresh(): void {
    // Auto-refresh every 5 minutes if enabled
    setInterval(() => {
      if (this.autoRefresh()) {
        this.refreshData();
      }
    }, 5 * 60 * 1000);
  }

  // =============================================
  // ACTIONS
  // =============================================

  refreshData(): void {
    this.isRefreshing.set(true);
    const farmId = this.authService.getFarmId();
    const fieldId = this.selectedFieldId();

    if (!farmId || !fieldId) {
      this.isRefreshing.set(false);
      return;
    }

    this.weatherService.refreshWeatherData(farmId, fieldId)
      .pipe(finalize(() => this.isRefreshing.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.loadDashboardData();
            this.snackBar.open('Weather data refreshed successfully', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
            });
          } else {
            this.snackBar.open(response.message || 'Failed to refresh weather', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error refreshing weather:', error);
          this.snackBar.open('Failed to refresh weather', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  refreshAllFields(): void {
    this.isRefreshing.set(true);
    const farmId = this.authService.getFarmId();

    if (!farmId) {
      this.isRefreshing.set(false);
      return;
    }

    this.weatherService.refreshAllWeather(farmId)
      .pipe(finalize(() => this.isRefreshing.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.loadDashboardData();
            this.snackBar.open('All fields weather refreshed successfully', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
            });
          } else {
            this.snackBar.open(response.message || 'Failed to refresh weather', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error refreshing all weather:', error);
          this.snackBar.open('Failed to refresh weather', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  openManualEntry(): void {
    const dialogRef = this.dialog.open(ManualWeatherEntryComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { fieldId: this.selectedFieldId() }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadDashboardData();
        this.snackBar.open('Weather data added successfully', 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
      }
    });
  }

  viewAlertDetails(alert: WeatherAlert): void {
    this.dialog.open(WeatherAlertDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { alert, mode: 'view' }
    });
  }

  acknowledgeAlert(alert: WeatherAlert, event: Event): void {
    event.stopPropagation();
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.weatherService.acknowledgeWeatherAlert(farmId, alert.id)
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.activeAlerts.update(alerts => 
              alerts.map(a => a.id === alert.id ? { ...a, isAcknowledged: true } : a)
            );
            this.snackBar.open('Alert acknowledged successfully', 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar']
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

  toggleAutoRefresh(): void {
    this.autoRefresh.update(value => !value);
  }

  getSeverityColor(severity: string): string {
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getWeatherIcon(condition: string | null): string {
    if (!condition) return 'wb_sunny';
    const iconMap: Record<string, string> = {
      'CLEAR': 'wb_sunny',
      'CLOUDY': 'cloud',
      'RAINY': 'umbrella',
      'STORMY': 'flash_on',
      'SNOWY': 'ac_unit',
      'FOGGY': 'foggy',
      'WINDY': 'air'
    };
    return iconMap[condition] || 'wb_sunny';
  }

  getWeatherClass(condition: string | null): string {
    if (!condition) return 'bg-primary-50 text-primary-700';
    const classMap: Record<string, string> = {
      'CLEAR': 'bg-yellow-50 text-yellow-700',
      'CLOUDY': 'bg-gray-100 text-gray-700',
      'RAINY': 'bg-blue-50 text-blue-700',
      'STORMY': 'bg-purple-50 text-purple-700',
      'SNOWY': 'bg-blue-50 text-blue-700',
      'FOGGY': 'bg-gray-100 text-gray-700',
      'WINDY': 'bg-cyan-50 text-cyan-700'
    };
    return classMap[condition] || 'bg-primary-50 text-primary-700';
  }

  private showAlertNotification(alert: WeatherAlert): void {
    const severityEmoji = {
      'ADVISORY': 'ℹ️',
      'WATCH': '👀',
      'WARNING': '⚠️',
      'EMERGENCY': '🚨'
    };

    const message = `${severityEmoji[alert.severity] || '🔔'} ${alert.title}`;
    this.snackBar.open(message, 'View', {
      duration: 10000,
      panelClass: ['alert-snackbar', `alert-${alert.severity.toLowerCase()}`],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    }).onAction().subscribe(() => {
      this.viewAlertDetails(alert);
    });
  }

  getAlertSeverityIcon(severity: string): string {
    const iconMap: Record<string, string> = {
      'ADVISORY': 'info',
      'WATCH': 'warning',
      'WARNING': 'warning',
      'EMERGENCY': 'error'
    };
    return iconMap[severity] || 'info';
  }

  getAlertSeverityClass(severity: string): string {
    const classMap: Record<string, string> = {
      'ADVISORY': 'border-l-4 border-yellow-400',
      'WATCH': 'border-l-4 border-orange-400',
      'WARNING': 'border-l-4 border-red-400',
      'EMERGENCY': 'border-l-4 border-red-600'
    };
    return classMap[severity] || '';
  }

  formatDate(date: string | null): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatTime(date: string): string {
    return new Date(date).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // =============================================
  // LIFECYCLE
  // =============================================

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.weatherSignalR.stopConnection();
  }
}