// src/app/features/admin/sensors/field-sensor-details/field-sensor-details.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
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
import { MatNativeDateModule } from '@angular/material/core';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatBadgeModule } from '@angular/material/badge';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { SensorService } from '../../services/sensor.service';
import { SensorSignalRService } from '../../services/sensor-signalr.service';
import { FieldService } from '../../services/field.service';
import {
  SensorReading,
  SensorReadingFilter,
  SENSOR_TYPES,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  SENSOR_TYPE_UNITS,
  Alert
} from '../../models/sensor.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

// Register Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'app-field-sensor-details',
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
    MatTableModule,
    MatPaginatorModule,
    MatBadgeModule,
    BaseChartDirective
  ],
  templateUrl: './field-sensor-details.component.html',
  styleUrls: ['./field-sensor-details.component.scss']
})
export class FieldSensorDetailsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private fieldService = inject(FieldService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State
  isLoading = signal(false);
  isRefreshing = signal(false);
  fieldId = signal<number | null>(null);
  field = signal<any | null>(null);
  readings = signal<SensorReading[]>([]);
  alerts = signal<Alert[]>([]);
  selectedSensorType = signal<string | null>(null);
  totalCount = signal(0);
  pageSize = signal(50);
  pageIndex = signal(0);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Computed
  hasReadings = computed(() => this.readings().length > 0);
  hasAlerts = computed(() => this.alerts().length > 0);
  sensorTypes = computed(() => {
    const types = new Set<string>();
    this.readings().forEach(r => types.add(r.sensorType));
    return Array.from(types);
  });
  filteredReadings = computed(() => {
    const type = this.selectedSensorType();
    if (!type) return this.readings();
    return this.readings().filter(r => r.sensorType === type);
  });
  latestReading = computed(() => {
    const sorted = [...this.readings()].sort((a, b) => 
      new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime()
    );
    return sorted[0] || null;
  });

  // Trigger for reload
  private reloadTrigger = signal(0);

  // Charts
  temperatureChartData!: ChartConfiguration['data'];
  temperatureChartOptions!: ChartConfiguration['options'];
  temperatureChartType: ChartType = 'line';

  humidityChartData!: ChartConfiguration['data'];
  humidityChartOptions!: ChartConfiguration['options'];
  humidityChartType: ChartType = 'line';

  soilMoistureChartData!: ChartConfiguration['data'];
  soilMoistureChartOptions!: ChartConfiguration['options'];
  soilMoistureChartType: ChartType = 'line';

  // Table columns
  displayedColumns = [
    'sensorType',
    'value',
    'unit',
    'recordedAt',
    'status'
  ];

  sensorTypesList = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  sensorTypeIcons = SENSOR_TYPE_ICONS;
  sensorTypeUnits = SENSOR_TYPE_UNITS;

  constructor() {
    this.filterForm = this.fb.group({
      fromDate: [''],
      toDate: [''],
      sensorType: [null]
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger >= 0) {
        this.loadData();
      }
    });

    this.initCharts();
  }

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = parseInt(params['fieldId']);
        if (id) {
          this.fieldId.set(id);
          this.loadFieldDetails();
          this.triggerReload();
          this.setupSignalR();
        } else {
          this.router.navigate(['/admin/sensors']);
        }
      });

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
        this.selectedSensorType.set(this.filterForm.get('sensorType')?.value);
        this.pageIndex.set(0);
        this.triggerReload();
      });
  }

  triggerReload(): void {
    this.reloadTrigger.update(value => value + 1);
  }

  private loadFieldDetails(): void {
    const farmId = this.authService.getFarmId();
    const fieldId = this.fieldId();
    if (!farmId || !fieldId) return;

    this.fieldService.getFieldById(farmId, fieldId).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.field.set(response.data);
        }
      },
      error: (error: any) => {
        console.error('Error loading field details:', error);
        this.showError('Failed to load field details');
      }
    });
  }

  private loadData(): void {
    const farmId = this.authService.getFarmId();
    const fieldId = this.fieldId();
    if (!farmId || !fieldId) return;

    this.isLoading.set(true);
    this.loadReadings(farmId, fieldId);
    this.loadAlerts(farmId, fieldId);
  }

  // ✅ FIXED: Convert null to undefined
  private loadReadings(farmId: number, fieldId: number): void {
    const filterValues = this.filterForm.value;
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : undefined;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : undefined;

    this.sensorService.getFieldHistory(farmId, fieldId, fromDate, toDate)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.readings.set(response.data || []);
            this.totalCount.set(response.data?.length || 0);
            this.updateCharts(response.data || []);
            
            const types = new Set<string>();
            (response.data || []).forEach((r: any) => types.add(r.sensorType));
            if (types.size > 0 && !this.selectedSensorType()) {
              this.selectedSensorType.set(Array.from(types)[0]);
              this.filterForm.patchValue({ sensorType: Array.from(types)[0] });
            }
          } else {
            this.showError(response.message || 'Failed to load sensor readings');
          }
        },
        error: (error: any) => {
          console.error('Error loading sensor readings:', error);
          this.showError('Failed to load sensor readings');
        }
      });
  }

  private loadAlerts(farmId: number, fieldId: number): void {
    const filter: any = {
      fieldId: fieldId,
      page: 1,
      pageSize: 100,
      isDescending: true,
      sortBy: 'CreatedAt'
    };

    this.sensorService.getAlerts(farmId, filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.alerts.set(response.data.items || []);
          }
        },
        error: (error: any) => console.error('Error loading alerts:', error)
      });
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    const fieldId = this.fieldId();
    if (!farmId || !fieldId) return;

    this.sensorSignalR.joinFieldGroup(fieldId);

    this.sensorSignalR.sensorReading$
      .pipe(takeUntil(this.destroy$))
      .subscribe((reading) => {
        if (reading && reading.fieldId === fieldId) {
          this.readings.update(readings => [reading, ...readings]);
          this.totalCount.update(count => count + 1);
          this.updateCharts(this.readings());
        }
      });

    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert && alert.fieldId === fieldId) {
          this.alerts.update(alerts => [alert, ...alerts]);
        }
      });
  }

  // =============================================
  // CHARTS
  // =============================================

  private initCharts(): void {
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
          pointRadius: 2
        },
        {
          data: [],
          label: 'Air Temp (°C)',
          borderColor: '#ba1a1a',
          backgroundColor: 'rgba(186, 26, 26, 0.1)',
          fill: true,
          tension: 0.4,
          pointRadius: 2
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

    this.humidityChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Humidity (%)',
        borderColor: '#40916c',
        backgroundColor: 'rgba(64, 145, 108, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 2
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

    this.soilMoistureChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Soil Moisture (%)',
        borderColor: '#2d6a4f',
        backgroundColor: 'rgba(45, 106, 79, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 2
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
  }

  private updateCharts(readings: SensorReading[]): void {
    if (!readings || readings.length === 0) {
      this.clearCharts();
      return;
    }

    const sorted = [...readings].sort((a, b) => 
      new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
    );

    const chartData = sorted.slice(-50);

    const labels = chartData.map(r => 
      new Date(r.recordedAt).toLocaleTimeString('en-US', { 
        hour: '2-digit', 
        minute: '2-digit',
        month: 'short',
        day: 'numeric'
      })
    );

    const soilTemp = chartData.filter(r => r.sensorType === 'SOIL_TEMP');
    const airTemp = chartData.filter(r => r.sensorType === 'AIR_TEMP');
    const humidity = chartData.filter(r => r.sensorType === 'AIR_HUMIDITY');
    const soilMoisture = chartData.filter(r => r.sensorType === 'SOIL_MOISTURE');

    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: labels,
      datasets: [
        {
          ...this.temperatureChartData.datasets[0],
          data: soilTemp.map(r => r.value ?? 0)
        },
        {
          ...this.temperatureChartData.datasets[1],
          data: airTemp.map(r => r.value ?? 0)
        }
      ]
    };

    this.humidityChartData = {
      ...this.humidityChartData,
      labels: labels,
      datasets: [{
        ...this.humidityChartData.datasets[0],
        data: humidity.map(r => r.value ?? 0)
      }]
    };

    this.soilMoistureChartData = {
      ...this.soilMoistureChartData,
      labels: labels,
      datasets: [{
        ...this.soilMoistureChartData.datasets[0],
        data: soilMoisture.map(r => r.value ?? 0)
      }]
    };
  }

  private clearCharts(): void {
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

    this.soilMoistureChartData = {
      ...this.soilMoistureChartData,
      labels: [],
      datasets: [{ ...this.soilMoistureChartData.datasets[0], data: [] }]
    };
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  getSensorTypeLabel(type: string): string {
    return this.sensorTypeLabels[type] || type;
  }

  getSensorTypeIcon(type: string): string {
    return this.sensorTypeIcons[type] || 'sensors';
  }

  getSensorTypeUnit(type: string): string {
    return this.sensorTypeUnits[type] || '';
  }

  getStatusColor(value: number | null, sensorType: string): string {
    if (value === null) return 'bg-gray-100 text-gray-500';
    
    const thresholds: Record<string, { normal: [number, number], warning: [number, number] }> = {
      'SOIL_MOISTURE': { normal: [20, 50], warning: [15, 60] },
      'AIR_TEMP': { normal: [18, 35], warning: [10, 40] },
      'SOIL_TEMP': { normal: [15, 30], warning: [10, 35] },
      'AIR_HUMIDITY': { normal: [40, 80], warning: [30, 90] },
      'SOIL_PH': { normal: [6, 7.5], warning: [5.5, 8] },
    };

    const range = thresholds[sensorType];
    if (!range) return 'bg-gray-100 text-gray-700';

    if (value >= range.normal[0] && value <= range.normal[1]) {
      return 'bg-green-100 text-green-700';
    } else if (value >= range.warning[0] && value <= range.warning[1]) {
      return 'bg-yellow-100 text-yellow-700';
    } else {
      return 'bg-red-100 text-red-700';
    }
  }

  getStatusLabel(value: number | null, sensorType: string): string {
    if (value === null) return 'N/A';
    
    const thresholds: Record<string, { normal: [number, number], warning: [number, number] }> = {
      'SOIL_MOISTURE': { normal: [20, 50], warning: [15, 60] },
      'AIR_TEMP': { normal: [18, 35], warning: [10, 40] },
      'SOIL_TEMP': { normal: [15, 30], warning: [10, 35] },
      'AIR_HUMIDITY': { normal: [40, 80], warning: [30, 90] },
      'SOIL_PH': { normal: [6, 7.5], warning: [5.5, 8] },
    };

    const range = thresholds[sensorType];
    if (!range) return 'Unknown';

    if (value >= range.normal[0] && value <= range.normal[1]) {
      return 'Normal';
    } else if (value >= range.warning[0] && value <= range.warning[1]) {
      return 'Warning';
    } else {
      return 'Critical';
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

  getSeverityColor(severity: string | null): string {
    const colors: Record<string, string> = {
      'LOW': 'bg-green-100 text-green-700',
      'MEDIUM': 'bg-yellow-100 text-yellow-700',
      'HIGH': 'bg-orange-100 text-orange-700',
      'CRITICAL': 'bg-red-100 text-red-700'
    };
    return severity ? colors[severity] || 'bg-gray-100 text-gray-700' : 'bg-gray-100 text-gray-700';
  }

  // =============================================
  // ACTIONS
  // =============================================

  refreshData(): void {
    this.isRefreshing.set(true);
    this.loadData();
    setTimeout(() => this.isRefreshing.set(false), 1000);
  }

  goBack(): void {
    this.router.navigate(['/admin/sensors/dashboard']);
  }

  viewAllSensors(): void {
    this.router.navigate(['/admin/sensors/readings']);
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.fieldId()) {
      this.sensorSignalR.leaveFieldGroup(this.fieldId()!);
    }
  }
}