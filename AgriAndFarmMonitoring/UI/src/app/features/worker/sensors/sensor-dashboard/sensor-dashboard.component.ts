import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject, forkJoin } from 'rxjs';

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

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WorkerSensorService } from '../../services/worker-sensor.service';
import { SensorSignalRService } from '../../../admin/services/sensor-signalr.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import {
  SensorReading,
  Alert,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  ALERT_SEVERITY_COLORS,
  SENSOR_TYPE_UNITS
} from '../../../admin/models/sensor.model';

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
    BaseChartDirective
  ],
  templateUrl: './sensor-dashboard.component.html',
  styleUrls: ['./sensor-dashboard.component.scss']
})
export class SensorDashboardComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerSensorService = inject(WorkerSensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private workerFieldService = inject(WorkerFieldService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  isLoading = signal(false);
  isRefreshing = signal(false);
  latestReadings = signal<SensorReading[]>([]);
  unresolvedAlerts = signal<Alert[]>([]);
  fields: any[] = [];
  selectedFieldId = signal<number | null>(null);

  // Computed
  filteredLatestReadings = computed(() => {
    const fieldId = this.selectedFieldId();
    if (!fieldId) return this.latestReadings();
    return this.latestReadings().filter(r => r.fieldId === fieldId);
  });

  groupedReadings = computed(() => {
    const readings = this.filteredLatestReadings();
    const groups: { [key: string]: SensorReading[] } = {};
    for (const r of readings) {
      if (!groups[r.fieldName]) groups[r.fieldName] = [];
      groups[r.fieldName].push(r);
    }
    return Object.keys(groups).map(k => ({ 
      fieldName: k, 
      fieldId: groups[k][0].fieldId, 
      readings: groups[k] 
    }));
  });

  alertDashboard = computed(() => {
    const alerts = this.unresolvedAlerts();
    return {
      unresolvedAlerts: alerts.length,
      criticalAlerts: alerts.filter(a => a.severity === 'CRITICAL').length,
      highAlerts: alerts.filter(a => a.severity === 'HIGH').length,
      mediumAlerts: alerts.filter(a => a.severity === 'MEDIUM').length,
      lowAlerts: alerts.filter(a => a.severity === 'LOW').length,
      totalAlerts: alerts.length, // Only unresolved shown in worker mostly
      resolvedAlerts: 0,
      recentAlerts: alerts.slice(0, 5)
    };
  });

  hasReadings = computed(() => this.filteredLatestReadings().length > 0);
  hasAlerts = computed(() => this.unresolvedAlerts().length > 0);
  criticalAlerts = computed(() => this.alertDashboard().criticalAlerts);

  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Charts
  temperatureChartData!: ChartConfiguration['data'];
  temperatureChartOptions!: ChartConfiguration['options'];
  temperatureChartType: ChartType = 'bar';

  humidityChartData!: ChartConfiguration['data'];
  humidityChartOptions!: ChartConfiguration['options'];
  humidityChartType: ChartType = 'bar';

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
        this.updateCharts(this.latestReadings());
      });
  }

  private loadFields(): void {
    this.workerFieldService.getMyAssignedFields().subscribe({
      next: (response: any) => {
        if (response.success) {
          this.fields = response.data;
        }
      },
      error: (error: any) => console.error('Error loading fields:', error)
    });
  }

  private loadData(): void {
    this.isLoading.set(true);

    forkJoin({
      readings: this.workerSensorService.getLatestReadings(),
      alerts: this.workerSensorService.getUnresolvedAlerts()
    }).pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (results) => {
        const readingsData = (results.readings as any).data || results.readings;
        this.latestReadings.set(readingsData || []);
        this.unresolvedAlerts.set((results.alerts as any) || []);
        this.updateCharts(this.latestReadings());
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
      }
    });
  }

  private initCharts(): void {
    // Temperature Chart
    this.temperatureChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Temperature (°C)',
        borderColor: '#40916c',
        backgroundColor: 'rgba(64, 145, 108, 0.7)',
        borderWidth: 1,
        borderRadius: 4
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
        borderColor: '#0284c7',
        backgroundColor: 'rgba(2, 132, 199, 0.7)',
        borderWidth: 1,
        borderRadius: 4
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
    const fieldId = this.selectedFieldId();
    let chartReadings = readings;
    if (fieldId) {
      chartReadings = readings.filter(r => r.fieldId === fieldId);
    }

    const tempReadings = chartReadings.filter(r => r.sensorType === 'AIR_TEMP' || r.sensorType === 'SOIL_TEMP');
    const humidityReadings = chartReadings.filter(r => r.sensorType === 'AIR_HUMIDITY');

    const labels = Array.from(new Set(chartReadings.map(r => r.fieldName)));

    const colors = [
      'rgba(64, 145, 108, 0.7)',
      'rgba(2, 132, 199, 0.7)',
      'rgba(212, 163, 115, 0.7)',
      'rgba(225, 29, 72, 0.7)',
      'rgba(217, 119, 6, 0.7)',
      'rgba(147, 51, 234, 0.7)'
    ];

    const tempBgColors = labels.map((_, i) => colors[i % colors.length]);
    const tempBorderColors = tempBgColors.map(c => c.replace('0.7', '1'));
    
    const sortedTemp = labels.map(l => {
      const r = tempReadings.find(tr => tr.fieldName === l);
      return r ? (r.value ?? 0) : 0;
    });

    const sortedHum = labels.map(l => {
      const r = humidityReadings.find(hr => hr.fieldName === l);
      return r ? (r.value ?? 0) : 0;
    });

    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: labels,
      datasets: [{
        ...this.temperatureChartData.datasets[0],
        data: sortedTemp,
        backgroundColor: tempBgColors,
        borderColor: tempBorderColors
      }]
    };

    this.humidityChartData = {
      ...this.humidityChartData,
      labels: labels,
      datasets: [{
        ...this.humidityChartData.datasets[0],
        data: sortedHum,
        backgroundColor: tempBgColors,
        borderColor: tempBorderColors
      }]
    };
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.sensorSignalR.sensorReading$
      .pipe(takeUntil(this.destroy$))
      .subscribe((reading: any) => {
        if (reading) {
          if (this.fields.some(f => f.fieldId === reading.fieldId)) {
            this.latestReadings.update(readings => {
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
        }
      });

    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert: any) => {
        if (alert) {
          if (this.fields.some(f => f.fieldId === alert.fieldId)) {
            this.workerSensorService.getUnresolvedAlerts().subscribe(alerts => {
               this.unresolvedAlerts.set((alerts as any) || []);
            });
          }
        }
      });
  }

  refreshData(): void {
    this.isRefreshing.set(true);
    this.loadData();
    setTimeout(() => this.isRefreshing.set(false), 1000);
  }

  viewFieldDetails(fieldId: number): void {
    // Workers might not have field details page or its different
    // this.router.navigate(['/worker/fields', fieldId]);
  }

  viewAllReadings(): void {
    this.router.navigate(['/worker/sensors/readings']);
  }

  viewAllAlerts(): void {
    this.router.navigate(['/worker/sensors/alerts']);
  }

  getSensorTypeLabel(type: string): string {
    return SENSOR_TYPE_LABELS[type] || type;
  }

  getSensorTypeIcon(type: string): string {
    return SENSOR_TYPE_ICONS[type] || 'sensors';
  }

  getSensorTypeUnit(type: string): string {
    return SENSOR_TYPE_UNITS[type] || '';
  }

  getSeverityColor(severity: string | null | undefined): string {
    if (!severity) return 'bg-gray-100 text-gray-800 border-gray-200';
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getStatusColor(value: number | null, sensorType: string): string {
    if (value === null) return 'text-gray-400';
    
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
