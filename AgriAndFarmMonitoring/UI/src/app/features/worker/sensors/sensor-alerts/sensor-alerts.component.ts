import { Component, inject, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

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
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { AuthService } from '../../../../core/services/auth.service';
import { WorkerSensorService } from '../../services/worker-sensor.service';
import { SensorSignalRService } from '../../../admin/services/sensor-signalr.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import {
  Alert,
  AlertFilter,
  ALERT_SEVERITY_COLORS,
  ALERT_SEVERITY_ICONS,
  SENSOR_TYPE_LABELS
} from '../../../admin/models/sensor.model';
import { SensorAlertResolveDialogComponent } from './sensor-alert-resolve-dialog/sensor-alert-resolve-dialog.component';

const ALERT_TYPES = [
  'DROUGHT_STRESS',
  'WATERLOGGED',
  'HEAT_STRESS',
  'COLD_STRESS',
  'NUTRIENT_DEFICIENCY',
  'PEST_INFESTATION',
  'DISEASE_OUTBREAK',
  'WEED_PRESSURE',
  'SOIL_PH_ALERT',
  'HARVEST_READY',
  'IRRIGATION_NEEDED',
  'FERTILIZER_NEEDED'
];

export const ALERT_TYPE_LABELS: Record<string, string> = {
  'DROUGHT_STRESS': 'Drought Stress',
  'WATERLOGGED': 'Waterlogged',
  'HEAT_STRESS': 'Heat Stress',
  'COLD_STRESS': 'Cold Stress',
  'NUTRIENT_DEFICIENCY': 'Nutrient Deficiency',
  'PEST_INFESTATION': 'Pest Infestation',
  'DISEASE_OUTBREAK': 'Disease Outbreak',
  'WEED_PRESSURE': 'Weed Pressure',
  'SOIL_PH_ALERT': 'Soil pH Alert',
  'HARVEST_READY': 'Harvest Ready',
  'IRRIGATION_NEEDED': 'Irrigation Needed',
  'FERTILIZER_NEEDED': 'Fertilizer Needed'
};

@Component({
  selector: 'app-sensor-alerts',
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
    MatDividerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule
  ],
  templateUrl: './sensor-alerts.component.html',
  styleUrls: ['./sensor-alerts.component.scss']
})
export class SensorAlertsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerSensorService = inject(WorkerSensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private workerFieldService = inject(WorkerFieldService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private reportService = inject(ReportGeneratorService);

  isLoading = signal(false);
  alerts = signal<Alert[]>([]);
  totalCount = signal(0);
  pageSize = signal(20);
  pageIndex = signal(0);
  sortField = signal('CreatedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  fields: any[] = [];

  hasAlerts = computed(() => this.alerts().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.alerts().length === 0);

  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  displayedColumns = [
    'severity',
    'alertType',
    'fieldName',
    'message',
    'sensorValue',
    'createdAt',
    'isResolved',
    'actions'
  ];

  alertSeverityColors = ALERT_SEVERITY_COLORS;
  alertSeverityIcons = ALERT_SEVERITY_ICONS;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  alertTypeOptions = ALERT_TYPES;
  severityOptions = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

  private reloadTrigger = signal(0);

  getAlertTypeLabel(type: string): string {
    return ALERT_TYPE_LABELS[type] || type;
  }

  constructor() {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm = this.fb.group({
      fieldId: [null],
      alertType: [null],
      severity: [null],
      isResolved: [null],
      fromDate: [fromDate],
      toDate: [toDate],
      search: ['']
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadAlerts();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadAlerts();
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

    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert: any) => {
        if (alert) {
          if (this.fields.some(f => f.fieldId === alert.fieldId)) {
            const exists = this.alerts().some(a => a.id === alert.id);
            if (!exists) {
              this.alerts.update(alerts => [alert, ...alerts]);
              this.totalCount.update(count => count + 1);
            }
          }
        }
      });

    this.sensorSignalR.alertResolved$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data: any) => {
        if (data) {
          this.alerts.update(alerts => {
            return alerts.map(a => {
              if (a.id === data.alertId) {
                return { ...a, isResolved: true, resolvedAt: data.resolvedAt, resolvedBy: data.resolvedBy, resolutionNotes: data.resolutionNotes };
              }
              return a;
            });
          });
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

  loadAlerts(): void {
    this.isLoading.set(true);

    const filterValues = this.filterForm.value;

    const filter: AlertFilter = {
      fieldId: filterValues.fieldId || null,
      alertType: filterValues.alertType || null,
      severity: filterValues.severity || null,
      isResolved: filterValues.isResolved !== null ? filterValues.isResolved : null,
      fromDate: filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : null,
      toDate: filterValues.toDate ? new Date(filterValues.toDate).toISOString() : null,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.workerSensorService.getAlerts(filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.alerts.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.showError(response.message || 'Failed to load alerts');
          }
        },
        error: (error: any) => {
          console.error('Error loading alerts:', error);
          this.showError('Failed to load alerts');
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

  onFilterChange(): void {
    this.pageIndex.set(0);
    this.reloadTrigger.update(v => v + 1);
  }

  openResolveDialog(alert: Alert): void {
    const dialogRef = this.dialog.open(SensorAlertResolveDialogComponent, {
      width: '600px',
      data: { alert }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.resolutionNotes) {
        this.workerSensorService.resolveAlert(alert.id, result).subscribe({
          next: () => {
            this.snackBar.open('Alert resolved successfully', 'Close', { duration: 3000 });
            this.reloadTrigger.update(v => v + 1);
          },
          error: (err) => {
            console.error('Failed to resolve alert', err);
            this.snackBar.open(err.error?.message || 'Failed to resolve alert', 'Close', { duration: 5000 });
          }
        });
      }
    });
  }

  resetFilters(): void {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm.patchValue({
      fieldId: null,
      sensorType: null,
      alertType: null,
      severity: null,
      isResolved: null,
      fromDate: fromDate,
      toDate: toDate
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

  getSensorTypeLabel(type: string): string {
    return this.sensorTypeLabels[type] || type;
  }

  getSeverityColor(severity: string): string {
    return this.alertSeverityColors[severity] || 'bg-gray-100 text-gray-800';
  }

  getSeverityIcon(severity: string): string {
    return this.alertSeverityIcons[severity] || 'info';
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
    if (!this.hasAlerts()) {
      this.showWarning('No data to export');
      return;
    }
    const columns = [
      { header: 'Severity', dataKey: 'severity' },
      { header: 'Alert Type', dataKey: 'alertTypeLabel' },
      { header: 'Field', dataKey: 'fieldName' },
      { header: 'Message', dataKey: 'message' },
      { header: 'Status', dataKey: 'status' },
      { header: 'Created At', dataKey: 'createdAtFormatted' }
    ];
    const data = this.alerts().map(a => ({
      ...a,
      alertTypeLabel: this.getAlertTypeLabel(a.alertType || ''),
      fieldName: this.getFieldName(a.fieldId),
      status: a.isResolved ? 'Resolved' : 'Active',
      createdAtFormatted: this.formatDate(a.createdAt)
    }));
    this.reportService.exportToPdf(data, columns, 'Sensor Alerts Report', 'sensor_alerts');
  }

  exportCsv(): void {
    if (!this.hasAlerts()) {
      this.showWarning('No data to export');
      return;
    }
    const data = this.alerts().map(a => ({
      Severity: a.severity,
      'Alert Type': this.getAlertTypeLabel(a.alertType || ''),
      Field: this.getFieldName(a.fieldId),
      Message: a.message,
      'Sensor Value': a.sensorValue,
      Status: a.isResolved ? 'Resolved' : 'Active',
      'Created At': this.formatDate(a.createdAt)
    }));
    this.reportService.exportToCsv(data, 'sensor_alerts');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
