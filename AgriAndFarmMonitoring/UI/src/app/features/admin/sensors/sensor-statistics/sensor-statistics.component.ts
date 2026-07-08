// src/app/features/admin/sensors/sensor-statistics/sensor-statistics.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// Chart.js Imports
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';

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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { SensorService } from '../../services/sensor.service';
import { FieldService } from '../../services/field.service';
import {
  SensorStatistics,
  DailySensorStats,
  WeeklySensorStats,
  MonthlySensorStats,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_UNITS
} from '../../models/sensor.model';

// Register Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'app-sensor-statistics',
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatTabsModule,
    MatInputModule,
    MatPaginatorModule,
    BaseChartDirective
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './sensor-statistics.component.html',
  styleUrls: ['./sensor-statistics.component.scss']
})
export class SensorStatisticsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private fieldService = inject(FieldService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State Signals
  isLoading = signal(false);
  statistics = signal<SensorStatistics | null>(null);
  fields: any[] = [];
  selectedTabIndex = signal(0);
  pageIndex = signal(0);
  pageSize = signal(10);

  // Form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Chart Data
  soilMoistureChartData!: ChartConfiguration['data'];
  soilMoistureChartOptions!: ChartConfiguration['options'];
  soilMoistureChartType: ChartType = 'line';

  temperatureChartData!: ChartConfiguration['data'];
  temperatureChartOptions!: ChartConfiguration['options'];
  temperatureChartType: ChartType = 'line';

  humidityChartData!: ChartConfiguration['data'];
  humidityChartOptions!: ChartConfiguration['options'];
  humidityChartType: ChartType = 'line';

  alertChartData!: ChartConfiguration['data'];
  alertChartOptions!: ChartConfiguration['options'];
  alertChartType: ChartType = 'bar';

  // Group options
  groupOptions = [
    { value: 'day', label: 'Daily' },
    { value: 'week', label: 'Weekly' },
    { value: 'month', label: 'Monthly' }
  ];

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null],
      groupBy: ['day'],
      fromDate: [''],
      toDate: ['']
    });

    this.initCharts();
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadStatistics();
    this.setupFilterSubscription();
  }

  private setupFilterSubscription(): void {
    this.filterForm.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.loadStatistics();
      });
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

  loadStatistics(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);

    const filterValues = this.filterForm.value;
    const groupBy = filterValues.groupBy || 'day';
  const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : undefined;
  const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : undefined;
  
    // Note: The API expects groupBy parameter but may need to be implemented on backend
    // If the backend doesn't support groupBy yet, we can filter the data client-side
    this.sensorService.getSensorStatistics(farmId, groupBy, fromDate, toDate)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.statistics.set(response.data);
            this.updateCharts(response.data);
          } else {
            // If statistics endpoint is not fully implemented, show mock data
            this.loadMockStatistics();
            this.showWarning('Using sample statistics data. Full statistics endpoint may not be implemented.');
          }
        },
        error: (error: any) => {
          console.error('Error loading statistics:', error);
          // Fallback to mock data
          this.loadMockStatistics();
          this.showWarning('Using sample statistics data. Please check the backend statistics endpoint.');
        }
      });
  }

  // Fallback mock data when backend statistics endpoint is not fully implemented
  private loadMockStatistics(): void {
    const mockStats: SensorStatistics = {
      period: 'day',
      dailyStats: this.generateMockDailyStats(),
      weeklyStats: {},
      monthlyStats: {}
    };
    this.statistics.set(mockStats);
    this.updateCharts(mockStats);
  }

  private generateMockDailyStats(): Record<string, DailySensorStats> {
    const stats: Record<string, DailySensorStats> = {};
    const now = new Date();
    
    for (let i = 0; i < 30; i++) {
      const date = new Date(now);
      date.setDate(date.getDate() - i);
      const dateStr = date.toISOString().split('T')[0];
      
      stats[dateStr] = {
        date: date.toISOString(),
        avgSoilMoisture: 25 + Math.random() * 20,
        avgSoilTemp: 18 + Math.random() * 10,
        avgAirTemp: 20 + Math.random() * 12,
        avgHumidity: 50 + Math.random() * 30,
        readingsCount: Math.floor(20 + Math.random() * 40),
        alertCount: Math.floor(Math.random() * 5)
      };
    }
    return stats;
  }

  private initCharts(): void {
    // Soil Moisture Chart
    this.soilMoistureChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Soil Moisture (%)',
        borderColor: '#2d6a4f',
        backgroundColor: 'rgba(45, 106, 79, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#2d6a4f'
      }]
    };

    this.soilMoistureChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: true, position: 'top', labels: { usePointStyle: true, boxWidth: 8 } }
      },
      scales: {
        y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' } },
        x: { grid: { display: false } }
      }
    };

    // Temperature Chart
    this.temperatureChartData = {
      labels: [],
      datasets: [
        {
          data: [],
          label: 'Soil Temp (°C)',
          borderColor: '#d4a373',
          backgroundColor: 'rgba(212, 163, 115, 0.1)',
          fill: true,
          tension: 0.4,
          pointRadius: 3,
          pointBackgroundColor: '#d4a373'
        },
        {
          data: [],
          label: 'Air Temp (°C)',
          borderColor: '#ba1a1a',
          backgroundColor: 'rgba(186, 26, 26, 0.1)',
          fill: true,
          tension: 0.4,
          pointRadius: 3,
          pointBackgroundColor: '#ba1a1a'
        }
      ]
    };

    this.temperatureChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: true, position: 'top', labels: { usePointStyle: true, boxWidth: 8 } }
      },
      scales: {
        y: { beginAtZero: false, grid: { color: 'rgba(0,0,0,0.05)' } },
        x: { grid: { display: false } }
      }
    };

    // Humidity Chart
    this.humidityChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Humidity (%)',
        borderColor: '#40916c',
        backgroundColor: 'rgba(64, 145, 108, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#40916c'
      }]
    };

    this.humidityChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: true, position: 'top', labels: { usePointStyle: true, boxWidth: 8 } }
      },
      scales: {
        y: { beginAtZero: true, max: 100, grid: { color: 'rgba(0,0,0,0.05)' } },
        x: { grid: { display: false } }
      }
    };

    // Alert Chart
    this.alertChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Alerts',
        backgroundColor: 'rgba(186, 26, 26, 0.6)',
        borderColor: '#ba1a1a',
        borderWidth: 1,
        borderRadius: 4
      }]
    };

    this.alertChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' } },
        x: { grid: { display: false } }
      }
    };
  }

  private updateCharts(stats: SensorStatistics): void {
    if (!stats || !stats.dailyStats) {
      this.clearCharts();
      return;
    }

    const sortedKeys = Object.keys(stats.dailyStats).sort();
    const labels = sortedKeys.map(key => {
      const date = new Date(key);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });

    // Soil Moisture Data
    const soilMoistureData = sortedKeys.map(key => stats.dailyStats[key].avgSoilMoisture ?? 0);

    // Temperature Data
    const soilTempData = sortedKeys.map(key => stats.dailyStats[key].avgSoilTemp ?? 0);
    const airTempData = sortedKeys.map(key => stats.dailyStats[key].avgAirTemp ?? 0);

    // Humidity Data
    const humidityData = sortedKeys.map(key => stats.dailyStats[key].avgHumidity ?? 0);

    // Alert Data
    const alertData = sortedKeys.map(key => stats.dailyStats[key].alertCount || 0);

    // Update Soil Moisture Chart
    this.soilMoistureChartData = {
      ...this.soilMoistureChartData,
      labels: labels,
      datasets: [{
        ...this.soilMoistureChartData.datasets[0],
        data: soilMoistureData
      }]
    };

    // Update Temperature Chart
    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: labels,
      datasets: [
        {
          ...this.temperatureChartData.datasets[0],
          data: soilTempData
        },
        {
          ...this.temperatureChartData.datasets[1],
          data: airTempData
        }
      ]
    };

    // Update Humidity Chart
    this.humidityChartData = {
      ...this.humidityChartData,
      labels: labels,
      datasets: [{
        ...this.humidityChartData.datasets[0],
        data: humidityData
      }]
    };

    // Update Alert Chart
    this.alertChartData = {
      ...this.alertChartData,
      labels: labels,
      datasets: [{
        ...this.alertChartData.datasets[0],
        data: alertData
      }]
    };
  }

  private clearCharts(): void {
    this.soilMoistureChartData = {
      ...this.soilMoistureChartData,
      labels: [],
      datasets: [{ ...this.soilMoistureChartData.datasets[0], data: [] }]
    };

    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: [],
      datasets: [
        { ...this.temperatureChartData.datasets[0], data: [] },
        { ...this.temperatureChartData.datasets[1], data: [] }
      ]
    };

    this.humidityChartData = {
      ...this.humidityChartData,
      labels: [],
      datasets: [{ ...this.humidityChartData.datasets[0], data: [] }]
    };

    this.alertChartData = {
      ...this.alertChartData,
      labels: [],
      datasets: [{ ...this.alertChartData.datasets[0], data: [] }]
    };
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  getDailyStatsArray(): { key: string; value: DailySensorStats }[] {
    const stats = this.statistics();
    if (!stats || !stats.dailyStats) return [];
    
    return Object.entries(stats.dailyStats)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([key, value]) => ({ key, value }));
  }

  getPaginatedStats(): { key: string; value: DailySensorStats }[] {
    const arr = this.getDailyStatsArray();
    const start = this.pageIndex() * this.pageSize();
    return arr.slice(start, start + this.pageSize());
  }

  handlePageEvent(e: PageEvent) {
    this.pageSize.set(e.pageSize);
    this.pageIndex.set(e.pageIndex);
  }

  getSensorTypeLabel(type: string): string {
    return SENSOR_TYPE_LABELS[type] || type;
  }

  getSensorTypeUnit(type: string): string {
    return SENSOR_TYPE_UNITS[type] || '';
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  refreshData(): void {
    this.loadStatistics();
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      fieldId: null,
      groupBy: 'day',
      fromDate: '',
      toDate: ''
    });
    this.loadStatistics();
  }

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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}