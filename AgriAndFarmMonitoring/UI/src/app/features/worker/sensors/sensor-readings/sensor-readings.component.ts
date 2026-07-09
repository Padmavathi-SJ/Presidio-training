import { Component, inject, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { AuthService } from '../../../../core/services/auth.service';
import { WorkerSensorService } from '../../services/worker-sensor.service';
import { SensorSignalRService } from '../../../admin/services/sensor-signalr.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import {
  SensorReading,
  SensorReadingFilter,
  SENSOR_TYPES,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  SENSOR_TYPE_UNITS
} from '../../../admin/models/sensor.model';

@Component({
  selector: 'app-sensor-readings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './sensor-readings.component.html',
  styleUrls: ['./sensor-readings.component.scss']
})
export class SensorReadingsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerSensorService = inject(WorkerSensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private workerFieldService = inject(WorkerFieldService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);

  isLoading = signal(false);
  readings = signal<SensorReading[]>([]);
  totalCount = signal(0);
  pageSize = signal(20);
  pageIndex = signal(0);
  sortField = signal('RecordedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  fields: any[] = [];

  hasReadings = computed(() => this.readings().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.readings().length === 0);

  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  displayedColumns = [
    'fieldName',
    'sensorType',
    'value',
    'unit',
    'recordedAt',
    'status'
  ];

  sensorTypes = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  sensorTypeIcons = SENSOR_TYPE_ICONS;
  sensorTypeUnits = SENSOR_TYPE_UNITS;

  private reloadTrigger = signal(0);

  constructor() {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm = this.fb.group({
      fieldId: [null],
      sensorType: [null],
      fromDate: [fromDate],
      toDate: [toDate],
      latestOnly: [false]
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadReadings();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadReadings();
    this.setupFilterSubscription();
    this.setupSignalR();
  }

  private setupFilterSubscription(): void {
    this.filterForm.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.pageIndex.set(0);
        this.triggerReload();
      });
  }

  triggerReload(): void {
    this.reloadTrigger.update(value => value + 1);
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.sensorSignalR.sensorReading$
      .pipe(takeUntil(this.destroy$))
      .subscribe((reading: any) => {
        if (reading) {
          // Add to beginning of list if they have access to the field
          if (this.fields.some(f => f.fieldId === reading.fieldId)) {
            const exists = this.readings().some(r => r.id === reading.id);
            if (!exists) {
              this.readings.update(readings => [reading, ...readings]);
              this.totalCount.update(count => count + 1);
            }
          }
        }
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

  loadReadings(): void {
    this.isLoading.set(true);

    const filterValues = this.filterForm.value;
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : undefined;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : undefined;

    const filter: SensorReadingFilter = {
      fieldId: filterValues.fieldId || null,
      sensorType: filterValues.sensorType || null,
      fromDate: fromDate,
      toDate: toDate,
      latestOnly: filterValues.latestOnly || null,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.workerSensorService.getAllReadings(filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.readings.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
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

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    this.triggerReload();
  }

  onSortChange(sort: Sort): void {
    this.sortField.set(sort.active);
    this.sortDirection.set(sort.direction || 'desc');
    this.pageIndex.set(0);
    this.triggerReload();
  }

  resetFilters(): void {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm.patchValue({
      fieldId: null,
      sensorType: null,
      fromDate: fromDate,
      toDate: toDate,
      latestOnly: false
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

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
      'LIGHT_INTENSITY': { normal: [2000, 8000], warning: [1000, 10000] },
      'RAINFALL': { normal: [0, 50], warning: [0, 100] },
      'LEAF_WETNESS': { normal: [0, 50], warning: [0, 80] },
      'BATTERY': { normal: [20, 100], warning: [10, 100] },
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
      'LIGHT_INTENSITY': { normal: [2000, 8000], warning: [1000, 10000] },
      'RAINFALL': { normal: [0, 50], warning: [0, 100] },
      'LEAF_WETNESS': { normal: [0, 50], warning: [0, 80] },
      'BATTERY': { normal: [20, 100], warning: [10, 100] },
    };

    const range = thresholds[sensorType];
    if (!range) return 'N/A';

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
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getFieldName(fieldId: number): string {
    const field = this.fields.find(f => f.fieldId === fieldId);
    return field ? field.fieldName : 'Unknown Field';
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
      panelClass: ['bg-yellow-600', 'text-white']
    });
  }

  exportPdf(): void {
    if (!this.hasReadings()) {
      this.showWarning('No data to export');
      return;
    }
    const columns = [
      { header: 'Field', dataKey: 'fieldName' },
      { header: 'Sensor Type', dataKey: 'sensorType' },
      { header: 'Value', dataKey: 'value' },
      { header: 'Status', dataKey: 'statusLabel' },
      { header: 'Recorded At', dataKey: 'recordedAtFormatted' }
    ];
    const data = this.readings().map(r => ({
      ...r,
      fieldName: this.getFieldName(r.fieldId),
      sensorType: this.getSensorTypeLabel(r.sensorType),
      value: `${r.value} ${this.getSensorTypeUnit(r.sensorType)}`,
      statusLabel: this.getStatusLabel(r.value, r.sensorType),
      recordedAtFormatted: this.formatDate(r.recordedAt)
    }));
    this.reportService.exportToPdf(data, columns, 'Sensor Readings Report', 'sensor_readings');
  }

  exportCsv(): void {
    if (!this.hasReadings()) {
      this.showWarning('No data to export');
      return;
    }
    const data = this.readings().map(r => ({
      Field: this.getFieldName(r.fieldId),
      'Sensor Type': this.getSensorTypeLabel(r.sensorType),
      Value: r.value,
      Unit: this.getSensorTypeUnit(r.sensorType),
      Status: this.getStatusLabel(r.value, r.sensorType),
      'Recorded At': this.formatDate(r.recordedAt)
    }));
    this.reportService.exportToCsv(data, 'sensor_readings');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
