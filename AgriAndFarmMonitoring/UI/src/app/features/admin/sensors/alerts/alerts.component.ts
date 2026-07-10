// src/app/features/admin/sensors/alerts/alerts.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// Angular Material Imports
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { SensorService } from '../../services/sensor.service';
import { SensorSignalRService } from '../../services/sensor-signalr.service';
import { FieldService } from '../../services/field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import {
  Alert,
  AlertFilter,
  AlertDashboard,
  ALERT_SEVERITY_COLORS,
  ALERT_SEVERITY_ICONS,
  ALERT_SEVERITY_ORDER,
  SENSOR_TYPE_LABELS
} from '../../models/sensor.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertDetailDialogComponent } from '../alert-detail-dialog/alert-detail-dialog.component';

@Component({
  selector: 'app-alerts',
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
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatBadgeModule,
    MatSlideToggleModule
  ],
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.scss']
})
export class AlertsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);

  // State Signals
  isLoading = signal(false);
  isGeneratingAlert = signal(false);
  isResolving = signal<number | null>(null);
  alerts = signal<Alert[]>([]);
  totalCount = signal(0);
  pageSize = signal(20);
  pageIndex = signal(0);
  sortField = signal('CreatedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedAlerts = signal<number[]>([]);
  fields: any[] = [];
  dashboardStats = signal<AlertDashboard | null>(null);

  // Computed Signals
  hasAlerts = computed(() => this.alerts().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.alerts().length === 0);
  selectedCount = computed(() => this.selectedAlerts().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasAlerts() && this.selectedCount() === this.alerts().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.alerts().length);
  unresolvedAlerts = computed(() => this.alerts().filter(a => !a.isResolved).length);
  criticalAlerts = computed(() => this.alerts().filter(a => a.severity === 'CRITICAL' && !a.isResolved).length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'severity',
    'alertType',
    'fieldName',
    'message',
    'sensorValue',
    'createdAt',
    'status',
    'actions'
  ];

  // Options
  severityColors = ALERT_SEVERITY_COLORS;
  severityIcons = ALERT_SEVERITY_ICONS;
  severityOrder = ALERT_SEVERITY_ORDER;
  severityOptions = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];
  alertTypeOptions: string[] = [];

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null],
      severity: [null],
      alertType: [null],
      isResolved: [null],
      fromDate: [''],
      toDate: [''],
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
    this.loadDashboardStats();
    this.loadAlerts();
    this.setupFilterSubscription();
    this.setupSignalR();
    this.loadAlertTypes();
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

    // Listen for new alerts
    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert) {
          // Check if alert already exists
          const exists = this.alerts().some(a => a.id === alert.id);
          if (!exists) {
            this.alerts.update(alerts => [alert, ...alerts]);
            this.totalCount.update(count => count + 1);
            this.loadDashboardStats();
            this.showAlertNotification(alert);
          }
        }
      });

    // Listen for resolved alerts
    this.sensorSignalR.alertResolved$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => {
        if (data) {
          this.alerts.update(alerts =>
            alerts.map(a => a.id === data.alertId ? { ...a, isResolved: true } : a)
          );
          this.loadDashboardStats();
        }
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

  private loadAlertTypes(): void {
    // Extract unique alert types from alerts
    const types = new Set<string>();
    this.alerts().forEach(a => {
      if (a.alertType) types.add(a.alertType);
    });
    this.alertTypeOptions = Array.from(types);
  }

  private loadDashboardStats(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.sensorService.getAlertDashboard(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.dashboardStats.set(response.data);
          }
        },
        error: (error: any) => console.error('Error loading dashboard stats:', error)
      });
  }

  loadAlerts(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoading.set(false);
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);

    const filterValues = this.filterForm.value;

    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : null;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : null;

    const filter: AlertFilter = {
      fieldId: filterValues.fieldId || null,
      severity: filterValues.severity || null,
      alertType: filterValues.alertType || null,
      isResolved: filterValues.isResolved !== '' ? filterValues.isResolved : null,
      fromDate: fromDate,
      toDate: toDate,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.sensorService.getAlerts(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.alerts.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
            this.selectedAlerts.set([]);
            this.loadAlertTypes();
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

  // =============================================
  // SELECTION
  // =============================================

  toggleSelection(alertId: number): void {
    this.selectedAlerts.update(current => {
      if (current.includes(alertId)) {
        return current.filter(id => id !== alertId);
      } else {
        return [...current, alertId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentAlerts = this.alerts();
    if (this.allSelected()) {
      this.selectedAlerts.set([]);
    } else {
      this.selectedAlerts.set(currentAlerts.map(a => a.id));
    }
  }

  isSelected(alertId: number): boolean {
    return this.selectedAlerts().includes(alertId);
  }

  // =============================================
  // PAGINATION & SORTING
  // =============================================

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

  // =============================================
  // ALERT ACTIONS
  // =============================================

  viewAlert(alert: Alert): void {
    this.dialog.open(AlertDetailDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { alert, mode: 'view' }
    });
  }

  resolveAlert(alert: Alert): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Resolve Alert',
        message: `Are you sure you want to resolve this alert?`,
        confirmText: 'Resolve',
        cancelText: 'Cancel',
        type: 'info'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isResolving.set(alert.id);
        this.sensorService.resolveAlert(farmId, alert.id, { alertId: alert.id, resolutionNotes: 'Resolved by admin' })
          .pipe(finalize(() => this.isResolving.set(null)))
          .subscribe({
            next: (response: any) => {
              if (response.success) {
                this.triggerReload();
                this.loadDashboardStats();
                this.showSuccess('Alert resolved successfully');
              } else {
                this.showError(response.message || 'Failed to resolve alert');
              }
            },
            error: (error: any) => {
              console.error('Error resolving alert:', error);
              this.showError('Failed to resolve alert');
            }
          });
      }
    });
  }

  // =============================================
  // BULK OPERATIONS
  // =============================================

  bulkResolve(): void {
    const alertIds = this.selectedAlerts();
    if (alertIds.length === 0) {
      this.showError('Please select alerts to resolve');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Resolve Alerts',
        message: `Are you sure you want to resolve ${alertIds.length} selected alert(s)?`,
        confirmText: 'Resolve All',
        cancelText: 'Cancel',
        type: 'info'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        // Process each alert
        const requests = alertIds.map(id => 
          this.sensorService.resolveAlert(farmId, id, { alertId: id }).toPromise()
        );

        Promise.all(requests)
          .then(() => {
            this.selectedAlerts.set([]);
            this.triggerReload();
            this.loadDashboardStats();
            this.showSuccess(`Resolved ${alertIds.length} alert(s)`);
          })
          .catch((error) => {
            console.error('Error bulk resolving alerts:', error);
            this.showError('Failed to resolve some alerts');
          })
          .finally(() => this.isLoading.set(false));
      }
    });
  }

  clearSelection(): void {
    this.selectedAlerts.set([]);
  }

  generateHourlyAlert(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.isGeneratingAlert.set(true);
    this.sensorService.generateHourlyAlert(farmId)
      .pipe(finalize(() => this.isGeneratingAlert.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.showSuccess('Hourly alert simulation triggered');
            this.triggerReload();
          } else {
            this.showError('Failed to trigger alert simulation');
          }
        },
        error: (err: any) => {
          console.error(err);
          this.showError('Error triggering alert simulation');
        }
      });
  }

  // =============================================
  // EXPORTS
  // =============================================

  private getExportFilter(): AlertFilter {
    const filterValues = this.filterForm.value;
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : null;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : null;
    return {
      fieldId: filterValues.fieldId || null,
      severity: filterValues.severity || null,
      alertType: filterValues.alertType || null,
      isResolved: filterValues.isResolved !== '' ? filterValues.isResolved : null,
      fromDate: fromDate,
      toDate: toDate,
      page: 1,
      pageSize: this.totalCount() || 100000,
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };
  }

  exportPdf(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId || !this.alerts().length) {
      this.showError('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.sensorService.getAlerts(farmId, this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const columns = [
              { header: 'Created At', dataKey: 'createdAt' },
              { header: 'Severity', dataKey: 'severity' },
              { header: 'Type', dataKey: 'alertType' },
              { header: 'Field', dataKey: 'fieldName' },
              { header: 'Message', dataKey: 'message' },
              { header: 'Status', dataKey: 'status' }
            ];
            const printData = items.map((d: any) => ({
              ...d,
              alertType: this.getAlertTypeLabel(d.alertType),
              status: d.isResolved ? 'Resolved' : 'Unresolved',
              createdAt: this.formatDate(d.createdAt)
            }));
            this.reportService.exportToPdf(printData, columns, 'Sensor Alerts Report', 'Sensor_Alerts');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  exportCsv(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId || !this.alerts().length) {
      this.showError('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.sensorService.getAlerts(farmId, this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const cleanData = items.map((d: any) => ({
              Created_At: this.formatDate(d.createdAt),
              Severity: d.severity,
              Type: this.getAlertTypeLabel(d.alertType),
              Field: d.fieldName,
              Message: d.message,
              Status: d.isResolved ? 'Resolved' : 'Unresolved',
              Resolved_At: d.resolvedAt ? this.formatDate(d.resolvedAt) : 'N/A'
            }));
            this.reportService.exportToCsv(cleanData, 'Sensor_Alerts');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  // =============================================
  // FILTERS
  // =============================================

  resetFilters(): void {
    this.filterForm.patchValue({
      fieldId: null,
      severity: null,
      alertType: null,
      isResolved: null,
      fromDate: '',
      toDate: '',
      search: ''
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  getSeverityColor(severity: string | null): string {
    if (!severity) return 'bg-gray-100 text-gray-700';
    return this.severityColors[severity] || 'bg-gray-100 text-gray-700';
  }

  getSeverityIcon(severity: string | null): string {
    if (!severity) return 'info';
    return this.severityIcons[severity] || 'info';
  }

  getSeverityClass(severity: string | null): string {
    const classMap: Record<string, string> = {
      'LOW': 'border-l-4 border-green-400',
      'MEDIUM': 'border-l-4 border-yellow-400',
      'HIGH': 'border-l-4 border-orange-400',
      'CRITICAL': 'border-l-4 border-red-600'
    };
    return severity ? classMap[severity] || '' : '';
  }

  getAlertTypeLabel(type: string | null): string {
    if (!type) return 'Unknown';
    return SENSOR_TYPE_LABELS[type] || type;
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
    const field = this.fields.find(f => f.id === fieldId);
    return field ? field.fieldName : 'Unknown Field';
  }

  private showAlertNotification(alert: Alert): void {
    const severityEmoji: Record<string, string> = {
      'LOW': 'ℹ️',
      'MEDIUM': '⚡',
      'HIGH': '⚠️',
      'CRITICAL': '🚨'
    };

    const emoji = severityEmoji[alert.severity || ''] || '🔔';
    const message = `${emoji} ${alert.alertType}: ${alert.message || 'New alert'}`;
    
    this.snackBar.open(message, 'View', {
      duration: 10000,
      panelClass: ['alert-snackbar', `alert-${alert.severity?.toLowerCase()}`],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    }).onAction().subscribe(() => {
      this.viewAlert(alert);
    });
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}