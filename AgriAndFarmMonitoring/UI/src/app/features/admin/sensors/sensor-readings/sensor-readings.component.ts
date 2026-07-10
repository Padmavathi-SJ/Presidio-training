// src/app/features/admin/sensors/sensor-readings/sensor-readings.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
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
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { SensorService } from '../../services/sensor.service';
import { SensorSignalRService } from '../../services/sensor-signalr.service';
import { FieldService } from '../../services/field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import {
  SensorReading,
  SensorReadingFilter,
  SENSOR_TYPES,
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  SENSOR_TYPE_UNITS
} from '../../models/sensor.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

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
    MatSlideToggleModule
  ],
  templateUrl: './sensor-readings.component.html',
  styleUrls: ['./sensor-readings.component.scss']
})
export class SensorReadingsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private reportService = inject(ReportGeneratorService);

  // State Signals
  isLoading = signal(false);
  readings = signal<SensorReading[]>([]);
  totalCount = signal(0);
  pageSize = signal(20);
  pageIndex = signal(0);
  sortField = signal('RecordedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedReadings = signal<number[]>([]);
  fields: any[] = [];

  // Computed Signals
  hasReadings = computed(() => this.readings().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.readings().length === 0);
  selectedCount = computed(() => this.selectedReadings().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasReadings() && this.selectedCount() === this.readings().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.readings().length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'fieldName',
    'sensorType',
    'value',
    'unit',
    'recordedAt',
    'status',
    'actions'
  ];

  // Options
  sensorTypes = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  sensorTypeIcons = SENSOR_TYPE_ICONS;
  sensorTypeUnits = SENSOR_TYPE_UNITS;

  // Trigger for reload
  private reloadTrigger = signal(0);

  // Export in progress
  isExporting = signal(false);

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null],
      sensorType: [null],
      fromDate: [''],
      toDate: [''],
      latestOnly: [false],
      search: ['']
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
    .subscribe((values) => {
      console.log('🔍 Filter values changed:', values);
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

    // Listen for new sensor readings
    this.sensorSignalR.sensorReading$
      .pipe(takeUntil(this.destroy$))
      .subscribe((reading) => {
        if (reading) {
          // Check if reading already exists in the list
          const exists = this.readings().some(r => r.id === reading.id);
          if (!exists) {
            // Add to beginning of list
            this.readings.update(readings => [reading, ...readings]);
            this.totalCount.update(count => count + 1);
          }
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

  
   loadReadings(): void {
  const farmId = this.authService.getFarmId();
  if (!farmId) {
    this.isLoading.set(false);
    this.showError('No farm found. Please login again.');
    return;
  }

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

  this.sensorService.getSensorReadings(farmId, filter)
    .pipe(finalize(() => this.isLoading.set(false)))
    .subscribe({
      next: (response: any) => {
        if (response.success) {
          this.readings.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
          this.selectedReadings.set([]);
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

  // =============================================
  // SELECTION
  // =============================================

  toggleSelection(readingId: number): void {
    this.selectedReadings.update(current => {
      if (current.includes(readingId)) {
        return current.filter(id => id !== readingId);
      } else {
        return [...current, readingId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentReadings = this.readings();
    if (this.allSelected()) {
      this.selectedReadings.set([]);
    } else {
      this.selectedReadings.set(currentReadings.map(r => r.id));
    }
  }

  isSelected(readingId: number): boolean {
    return this.selectedReadings().includes(readingId);
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
  // EXPORT
  // =============================================

  private getExportFilter(): SensorReadingFilter {
    const filterValues = this.filterForm.value;
    const fromDate = filterValues.fromDate ? new Date(filterValues.fromDate).toISOString() : undefined;
    const toDate = filterValues.toDate ? new Date(filterValues.toDate).toISOString() : undefined;
    return {
      fieldId: filterValues.fieldId || null,
      sensorType: filterValues.sensorType || null,
      fromDate: fromDate,
      toDate: toDate,
      latestOnly: filterValues.latestOnly || null,
      page: 1,
      pageSize: this.totalCount() || 100000,
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };
  }

  exportPdf(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId || !this.readings().length) {
      this.showWarning('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.sensorService.getSensorReadings(farmId, this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const columns = [
              { header: 'Recorded', dataKey: 'recordedAt' },
              { header: 'Field', dataKey: 'fieldName' },
              { header: 'Sensor', dataKey: 'sensorType' },
              { header: 'Value', dataKey: 'value' },
              { header: 'Unit', dataKey: 'unit' },
              { header: 'Status', dataKey: 'status' }
            ];
            const printData = items.map((d: any) => ({
              ...d,
              sensorType: this.getSensorTypeLabel(d.sensorType),
              status: this.getStatusLabel(d.value, d.sensorType),
              recordedAt: this.formatDate(d.recordedAt)
            }));
            this.reportService.exportToPdf(printData, columns, 'Sensor Readings Report', 'Sensor_Readings');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  exportCsv(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId || !this.readings().length) {
      this.showWarning('No data to export');
      return;
    }
    this.isLoading.set(true);
    this.sensorService.getSensorReadings(farmId, this.getExportFilter())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const items = response.data.items || [];
            const cleanData = items.map((d: any) => ({
              Recorded: this.formatDate(d.recordedAt),
              Field: this.getFieldName(d.fieldId),
              Sensor: this.getSensorTypeLabel(d.sensorType),
              Value: d.value,
              Unit: d.unit,
              Status: this.getStatusLabel(d.value, d.sensorType)
            }));
            this.reportService.exportToCsv(cleanData, 'Sensor_Readings');
          } else {
            this.showError('Failed to fetch data for export');
          }
        },
        error: () => this.showError('Failed to fetch data for export')
      });
  }

  // =============================================
  // NAVIGATION
  // =============================================

  viewFieldDetails(fieldId: number): void {
    this.router.navigate(['/admin/sensors/field', fieldId]);
  }

  // =============================================
  // FILTERS
  // =============================================

 resetFilters(): void {
  this.filterForm.patchValue({
    fieldId: null,
    sensorType: null,
    fromDate: '',
    toDate: '',
    latestOnly: false,
    search: ''
  });
  this.pageIndex.set(0);
  this.triggerReload();
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
    const field = this.fields.find(f => f.id === fieldId);
    return field ? field.fieldName : 'Unknown Field';
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

  // =============================================
  // BULK OPERATIONS
  // =============================================

  bulkDelete(): void {
    const readingIds = this.selectedReadings();
    if (readingIds.length === 0) {
      this.showError('Please select readings to delete');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Delete Readings',
        message: `Are you sure you want to delete ${readingIds.length} selected reading(s)? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'danger'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        // Note: Bulk delete is not implemented in backend yet
        this.showWarning('Bulk delete is not implemented in the backend yet');
      }
    });
  }

  clearSelection(): void {
    this.selectedReadings.set([]);
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