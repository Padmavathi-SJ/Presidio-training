// src/app/features/admin/weather/weather-alerts/weather-alerts.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect, untracked } from '@angular/core';
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
import { MatBadgeModule } from '@angular/material/badge';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WorkerWeatherService } from '../../services/worker-weather.service';
import { WeatherSignalRService } from '../../../admin/services/weather-signalr.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import { 
  WeatherAlert, 
  WeatherAlertFilter,
  ALERT_SEVERITY_COLORS,
  WEATHER_ALERT_TYPES,
  WEATHER_ALERT_SEVERITIES
} from '../../../admin/models/weather.model';
import { WorkerWeatherAlertDialogComponent } from '../weather-alert-dialog/weather-alert-dialog.component';

@Component({
  selector: 'app-worker-weather-alerts',
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
    MatBadgeModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSlideToggleModule
  ],
  templateUrl: './weather-alerts.component.html',
  styleUrls: ['./weather-alerts.component.scss']
})
export class WorkerWeatherAlertsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WorkerWeatherService);
  private weatherSignalR = inject(WeatherSignalRService);
  private fieldService = inject(WorkerFieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);

  // State Signals
  isLoading = signal(false);
  alerts = signal<WeatherAlert[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('AlertTime');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedAlerts = signal<number[]>([]);
  fields: any[] = [];

  // Computed Signals
  hasAlerts = computed(() => (this.alerts() || []).length > 0);
  isEmpty = computed(() => !this.isLoading() && (this.alerts() || []).length === 0);
  selectedCount = computed(() => (this.selectedAlerts() || []).length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasAlerts() && this.selectedCount() === (this.alerts() || []).length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < (this.alerts() || []).length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'severity',
    'alertType',
    'title',
    'fieldName',
    'alertTime',
    'isAcknowledged'];

  // Options
  alertTypes = WEATHER_ALERT_TYPES;
  severities = WEATHER_ALERT_SEVERITIES;
  severityColors = ALERT_SEVERITY_COLORS;

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm = this.fb.group({
      fieldId: [null],
      severity: [null],
      isAcknowledged: [null],
      isActive: [true],
      fromDate: [fromDate],
      toDate: [toDate],
      search: ['']
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      // Use untracked for other signals to prevent accidental infinite loops and duplicate calls
      if (trigger > 0 || trigger === 0) {
        untracked(() => this.loadAlerts());
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

 public triggerReload(): void {
  this.reloadTrigger.update(value => value + 1);
}

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Listen for new alerts
    this.weatherSignalR.alertUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert) {
          // Check if alert already exists
          const exists = this.alerts().some(a => a.id === alert.id);
          if (!exists) {
            this.alerts.update(alerts => [alert, ...alerts]);
            this.totalCount.update(count => count + 1);
            this.showAlertNotification(alert);
          } else {
            // Update existing alert
            this.alerts.update(alerts => 
              alerts.map(a => a.id === alert.id ? { ...a, ...alert } : a)
            );
          }
        }
      });
  }

  private loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    this.fieldService.getMyAssignedFields().subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data;
        }
      },
      error: (error) => console.error('Error loading fields:', error)
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

    // Format dates
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : null;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : null;

    const filter: WeatherAlertFilter = {
      fieldId: filterValues.fieldId || null,
      severity: filterValues.severity || null,
      isAcknowledged: filterValues.isAcknowledged !== '' ? filterValues.isAcknowledged : null,
      isActive: filterValues.isActive !== '' ? filterValues.isActive : null,
      fromDate: fromDate,
      toDate: toDate,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.weatherService.getWeatherAlerts(filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.alerts.set(response.data.items || []);
            this.totalCount.set(response.data.totalCount || 0);
            this.selectedAlerts.set([]);
          } else {
            this.showError(response.message || 'Failed to load alerts');
          }
        },
        error: (error) => {
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
  // CRUD OPERATIONS

  viewAlert(alert: WeatherAlert): void {
    const dialogRef = this.dialog.open(WorkerWeatherAlertDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { alert, mode: 'view' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.triggerReload();
      }
    });
  }

  // FILTERS
  // =============================================

  resetFilters(): void {
    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(toDate.getDate() - 7);

    this.filterForm.patchValue({
      fieldId: null,
      severity: null,
      isAcknowledged: null,
      isActive: true,
      fromDate: fromDate,
      toDate: toDate,
      search: ''
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  getSeverityColor(severity: string): string {
    return this.severityColors[severity] || 'bg-gray-100 text-gray-700';
  }

  getSeverityIcon(severity: string): string {
    const iconMap: Record<string, string> = {
      'ADVISORY': 'info',
      'WATCH': 'warning',
      'WARNING': 'warning',
      'EMERGENCY': 'error'
    };
    return iconMap[severity] || 'info';
  }

  getSeverityClass(severity: string): string {
    const classMap: Record<string, string> = {
      'ADVISORY': 'border-l-4 border-yellow-400',
      'WATCH': 'border-l-4 border-orange-400',
      'WARNING': 'border-l-4 border-red-400',
      'EMERGENCY': 'border-l-4 border-red-600'
    };
    return classMap[severity] || '';
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

  private showAlertNotification(alert: WeatherAlert): void {
    const severityEmoji = {
      'ADVISORY': 'ℹ️',
      'WATCH': '👀',
      'WARNING': '⚠️',
      'EMERGENCY': '🚨'
    };

    const message = `${severityEmoji[alert.severity] || '🔔'} New Alert: ${alert.title}`;
    this.snackBar.open(message, 'View', {
      duration: 10000,
      panelClass: ['alert-snackbar', `alert-${alert.severity.toLowerCase()}`],
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

  private showWarning(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['bg-yellow-600', 'text-white']
    });
  }

  private getExportFilter(): WeatherAlertFilter {
    const filterValues = this.filterForm.value;
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : null;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : null;

    return {
      fieldId: filterValues.fieldId || null,
      severity: filterValues.severity || null,
      isAcknowledged: filterValues.isAcknowledged !== '' ? filterValues.isAcknowledged : null,
      isActive: filterValues.isActive !== '' ? filterValues.isActive : null,
      fromDate: fromDate,
      toDate: toDate,
      page: 1,
      pageSize: this.totalCount() || 100000,
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };
  }

  exportPdf(): void {
    if (!this.hasAlerts()) {
      this.showWarning('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.weatherService.getWeatherAlerts(this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const columns = [
              { header: 'Severity', dataKey: 'severity' },
              { header: 'Alert Type', dataKey: 'alertType' },
              { header: 'Title', dataKey: 'title' },
              { header: 'Field', dataKey: 'fieldName' },
              { header: 'Status', dataKey: 'status' },
              { header: 'Alert Time', dataKey: 'alertTimeFormatted' }
            ];
            const data = items.map((a: any) => ({
              ...a,
              fieldName: this.getFieldName(a.fieldId),
              status: a.isAcknowledged ? 'Resolved' : 'Active',
              alertTimeFormatted: this.formatDate(a.alertTime)
            }));
            this.reportService.exportToPdf(data, columns, 'Weather Alerts Report', 'weather_alerts');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  exportCsv(): void {
    if (!this.hasAlerts()) {
      this.showWarning('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.weatherService.getWeatherAlerts(this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const data = items.map((a: any) => ({
              Severity: a.severity,
              'Alert Type': a.alertType,
              Title: a.title,
              Field: this.getFieldName(a.fieldId),
              Message: a.message,
              Status: a.isAcknowledged ? 'Resolved' : 'Active',
              'Alert Time': this.formatDate(a.alertTime)
            }));
            this.reportService.exportToCsv(data, 'weather_alerts');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}