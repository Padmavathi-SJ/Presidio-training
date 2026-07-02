// src/app/features/admin/sensors/sensor-dashboard/sensor-dashboard.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
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
import { MatGridListModule } from '@angular/material/grid-list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatRippleModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { SensorService } from '../../services/sensor.service';
import { SensorSignalRService } from '../../services/sensor-signalr.service';
import { FieldService } from '../../services/field.service';
import {
  SensorReading,
  SensorStatistics,
  Alert,
  AlertDashboard,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  ALERT_SEVERITY_COLORS,
  SENSOR_TYPE_UNITS
} from '../../models/sensor.model';

// Register Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'app-sensor-dashboard',
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
    MatGridListModule,
    MatBadgeModule,
    MatRippleModule,
    MatSlideToggleModule,
    BaseChartDirective
  ],
  templateUrl: './sensor-dashboard.component.html',
  styleUrls: ['./sensor-dashboard.component.scss']
})
export class SensorDashboardComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private fieldService = inject(FieldService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State Signals
  isLoading = signal(false);
  isRefreshing = signal(false);
  latestReadings = signal<SensorReading[]>([]);
  alertDashboard = signal<AlertDashboard | null>(null);
  fields: any[] = [];
  selectedFieldId = signal<number | null>(null);

  // Computed
  hasReadings = computed(() => this.latestReadings().length > 0);
  hasAlerts = computed(() => (this.alertDashboard()?.unresolvedAlerts || 0) > 0);
  criticalAlerts = computed(() => this.alertDashboard()?.criticalAlerts || 0);

  // Form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Charts
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

    this.initCharts();
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadData();
    this.setupSignalR();
    this.setupFilterSubscription();
  }

  private setupFilterSubscription(): void {
    this.filterForm.get('fieldId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        this.selectedFieldId.set(value);
        this.loadData();
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

  private loadData(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.isLoading.set(true);
    this.loadLatestReadings(farmId);
    this.loadAlertDashboard(farmId);
    this.loadStatistics(farmId);
  }

  private loadLatestReadings(farmId: number): void {
    this.sensorService.getLatestReadings(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.latestReadings.set(response.data || []);
            this.updateCharts(response.data || []);
          }
          this.isLoading.set(false);
        },
        error: (error: any) => {
          console.error('Error loading latest readings:', error);
          this.isLoading.set(false);
        }
      });
  }

  private loadAlertDashboard(farmId: number): void {
    this.sensorService.getAlertDashboard(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.alertDashboard.set(response.data);
          }
        },
        error: (error: any) => console.error('Error loading alert dashboard:', error)
      });
  }

  private loadStatistics(farmId: number): void {
    // Statistics will be loaded in the statistics component
  }

  private initCharts(): void {
    // Temperature Chart
    this.temperatureChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Temperature (°C)',
        borderColor: '#40916c',
        backgroundColor: 'rgba(64, 145, 108, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#40916c'
      }]
    };

    this.temperatureChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
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
        borderColor: '#d4a373',
        backgroundColor: 'rgba(212, 163, 115, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#d4a373'
      }]
    };

    this.humidityChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, max: 100, grid: { color: 'rgba(0,0,0,0.05)' } },
        x: { grid: { display: false } }
      }
    };
  }

  private updateCharts(readings: SensorReading[]): void {
    // Filter temperature readings
    const tempReadings = readings.filter(r => r.sensorType === 'AIR_TEMP' || r.sensorType === 'SOIL_TEMP');
    const humidityReadings = readings.filter(r => r.sensorType === 'AIR_HUMIDITY');

    const labels = readings.map(r => r.fieldName);

    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: labels,
      datasets: [{
        ...this.temperatureChartData.datasets[0],
        data: tempReadings.map(r => r.value ?? 0)
      }]
    };

    this.humidityChartData = {
      ...this.humidityChartData,
      labels: labels,
      datasets: [{
        ...this.humidityChartData.datasets[0],
        data: humidityReadings.map(r => r.value ?? 0)
      }]
    };
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Listen for new sensor readings
    this.sensorSignalR.sensorReading$
      .pipe(takeUntil(this.destroy$))
      .subscribe((reading) => {
        if (reading) {
          this.latestReadings.update(readings => {
            // Update or add the reading
            const index = readings.findIndex(r => 
              r.fieldId === reading.fieldId && r.sensorType === reading.sensorType
            );
            if (index >= 0) {
              readings[index] = reading;
            } else {
              readings.push(reading);
            }
            return [...readings];
          });
          this.updateCharts(this.latestReadings());
        }
      });

    // Listen for new alerts
    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert) {
          // Refresh alert dashboard
          const farmId = this.authService.getFarmId();
          if (farmId) {
            this.loadAlertDashboard(farmId);
          }
        }
      });
  }

  // =============================================
  // ACTIONS
  // =============================================

  refreshData(): void {
    this.isRefreshing.set(true);
    this.loadData();
    setTimeout(() => this.isRefreshing.set(false), 1000);
  }

  viewFieldDetails(fieldId: number): void {
    this.router.navigate(['/admin/sensors/field', fieldId]);
  }

  viewAllReadings(): void {
    this.router.navigate(['/admin/sensors/readings']);
  }

  viewAllAlerts(): void {
    this.router.navigate(['/admin/sensors/alerts']);
  }

  viewStatistics(): void {
    this.router.navigate(['/admin/sensors/statistics']);
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  getSensorTypeLabel(type: string): string {
    return SENSOR_TYPE_LABELS[type] || type;
  }

  getSensorTypeIcon(type: string): string {
    return SENSOR_TYPE_ICONS[type] || 'sensors';
  }

  getSensorTypeUnit(type: string): string {
    return SENSOR_TYPE_UNITS[type] || '';
  }

  getSeverityColor(severity: string): string {
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getStatusColor(value: number | null, sensorType: string): string {
    if (value === null) return 'text-gray-400';
    
    // Different thresholds for different sensor types
    const thresholds: Record<string, { normal: [number, number], warning: [number, number] }> = {
      'SOIL_MOISTURE': { normal: [20, 50], warning: [15, 60] },
      'AIR_TEMP': { normal: [18, 35], warning: [10, 40] },
      'SOIL_TEMP': { normal: [15, 30], warning: [10, 35] },
      'AIR_HUMIDITY': { normal: [40, 80], warning: [30, 90] },
      'SOIL_PH': { normal: [6, 7.5], warning: [5.5, 8] },
    };

    const range = thresholds[sensorType];
    if (!range) return 'text-gray-700';

    if (value >= range.normal[0] && value <= range.normal[1]) {
      return 'text-green-600';
    } else if (value >= range.warning[0] && value <= range.warning[1]) {
      return 'text-yellow-600';
    } else {
      return 'text-red-600';
    }
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}