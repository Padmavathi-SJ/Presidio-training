// src/app/features/admin/weather/weather-alerts/weather-alerts.component.ts
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
import { MatBadgeModule } from '@angular/material/badge';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { WeatherSignalRService } from '../../services/weather-signalr.service';
import { FieldService } from '../../services/field.service';
import { 
  WeatherAlert, 
  WeatherAlertFilter,
  ALERT_SEVERITY_COLORS,
  WEATHER_ALERT_TYPES,
  WEATHER_ALERT_SEVERITIES
} from '../../models/weather.model';
import { WeatherAlertDialogComponent } from '../weather-alert-dialog/weather-alert-dialog.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-weather-alerts',
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
export class WeatherAlertsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private weatherSignalR = inject(WeatherSignalRService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

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
  hasAlerts = computed(() => this.alerts().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.alerts().length === 0);
  selectedCount = computed(() => this.selectedAlerts().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasAlerts() && this.selectedCount() === this.alerts().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.alerts().length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'severity',
    'alertType',
    'title',
    'fieldName',
    'alertTime',
    'isAcknowledged',
    'actions'
  ];

  // Options
  alertTypes = WEATHER_ALERT_TYPES;
  severities = WEATHER_ALERT_SEVERITIES;
  severityColors = ALERT_SEVERITY_COLORS;

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null],
      severity: [null],
      isAcknowledged: [null],
      isActive: [true],
      fromDate: [''],
      toDate: [''],
      search: ['']
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadAlerts();
      }
    });
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
          }
        }
      });
  }

  private loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
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

    this.weatherService.getWeatherAlerts(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.alerts.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
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
  // =============================================

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(WeatherAlertDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.triggerReload();
        this.showSuccess('Weather alert created successfully');
      }
    });
  }

  viewAlert(alert: WeatherAlert): void {
    this.dialog.open(WeatherAlertDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { alert, mode: 'view' }
    });
  }

  editAlert(alert: WeatherAlert): void {
    const dialogRef = this.dialog.open(WeatherAlertDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { alert, mode: 'edit' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.triggerReload();
        this.showSuccess('Weather alert updated successfully');
      }
    });
  }

  deleteAlert(alert: WeatherAlert): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Weather Alert',
        message: `Are you sure you want to delete the alert "${alert.title}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        this.weatherService.deleteWeatherAlert(farmId, alert.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.triggerReload();
                this.showSuccess('Weather alert deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete alert');
              }
            },
            error: (error) => {
              console.error('Error deleting alert:', error);
              this.showError('Failed to delete alert');
            }
          });
      }
    });
  }

  acknowledgeAlert(alert: WeatherAlert): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.isLoading.set(true);
    this.weatherService.acknowledgeWeatherAlert(farmId, alert.id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.triggerReload();
            this.showSuccess('Alert acknowledged successfully');
          } else {
            this.showError(response.message || 'Failed to acknowledge alert');
          }
        },
        error: (error) => {
          console.error('Error acknowledging alert:', error);
          this.showError('Failed to acknowledge alert');
        }
      });
  }

  // =============================================
  // BULK OPERATIONS
  // =============================================

  bulkAcknowledge(): void {
    const alertIds = this.selectedAlerts();
    if (alertIds.length === 0) {
      this.showError('Please select alerts to acknowledge');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Acknowledge Alerts',
        message: `Are you sure you want to acknowledge ${alertIds.length} selected alert(s)?`,
        confirmText: 'Acknowledge',
        cancelText: 'Cancel',
        type: 'info'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        // Process each alert
        const requests = alertIds.map(id => 
          this.weatherService.acknowledgeWeatherAlert(farmId, id).toPromise()
        );

        Promise.all(requests)
          .then(() => {
            this.selectedAlerts.set([]);
            this.triggerReload();
            this.showSuccess(`Acknowledged ${alertIds.length} alert(s)`);
          })
          .catch((error) => {
            console.error('Error bulk acknowledging alerts:', error);
            this.showError('Failed to acknowledge some alerts');
          })
          .finally(() => this.isLoading.set(false));
      }
    });
  }

  bulkDelete(): void {
    const alertIds = this.selectedAlerts();
    if (alertIds.length === 0) {
      this.showError('Please select alerts to delete');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Delete Alerts',
        message: `Are you sure you want to delete ${alertIds.length} selected alert(s)? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'danger'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        const requests = alertIds.map(id => 
          this.weatherService.deleteWeatherAlert(farmId, id).toPromise()
        );

        Promise.all(requests)
          .then(() => {
            this.selectedAlerts.set([]);
            this.triggerReload();
            this.showSuccess(`Deleted ${alertIds.length} alert(s)`);
          })
          .catch((error) => {
            console.error('Error bulk deleting alerts:', error);
            this.showError('Failed to delete some alerts');
          })
          .finally(() => this.isLoading.set(false));
      }
    });
  }

  clearSelection(): void {
    this.selectedAlerts.set([]);
  }

  // =============================================
  // FILTERS
  // =============================================

  resetFilters(): void {
    this.filterForm.patchValue({
      fieldId: null,
      severity: null,
      isAcknowledged: null,
      isActive: true,
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
    const field = this.fields.find(f => f.id === fieldId);
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}