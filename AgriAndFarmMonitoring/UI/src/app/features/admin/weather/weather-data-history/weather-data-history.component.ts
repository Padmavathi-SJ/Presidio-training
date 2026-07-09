// src/app/features/admin/weather/weather-data-history/weather-data-history.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// Angular Material Imports
import { MatTableModule, MatTable } from '@angular/material/table';
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

// Chart.js Imports
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartType, Chart, registerables } from 'chart.js';

// App Imports
import { AuthService } from '../../../../core/services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { FieldService } from '../../services/field.service';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import { WeatherData, WeatherHistoryFilter, WEATHER_CONDITIONS } from '../../models/weather.model';
import { ManualWeatherEntryComponent } from '../manual-weather-entry/manual-weather-entry.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

// ✅ Register Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'app-weather-data-history',
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
    MatSlideToggleModule,
    BaseChartDirective
  ],
  templateUrl: './weather-data-history.component.html',
  styleUrls: ['./weather-data-history.component.scss']
})
export class WeatherDataHistoryComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);

  // State Signals
  isLoading = signal(false);
  weatherData = signal<WeatherData[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('RecordedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedData = signal<number[]>([]);
  fields: any[] = [];
  weatherConditions = WEATHER_CONDITIONS;

  // Computed Signals
  hasData = computed(() => this.weatherData().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.weatherData().length === 0);
  selectedCount = computed(() => this.selectedData().length);
  hasSelected = computed(() => this.selectedCount() > 0);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'fieldName',
    'temperature',
    'humidity',
    'windSpeed',
    'rainfallMm',
    'condition',
    'recordedAt',
    'actions'
  ];

  // Chart Data
  temperatureChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Temperature (°C)', borderColor: '#40916c', backgroundColor: 'rgba(64, 145, 108, 0.1)', fill: true, tension: 0.4 }]
  };
  temperatureChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: false, grid: { color: 'rgba(0,0,0,0.05)' } }, x: { grid: { display: false } } }
  };
  temperatureChartType: ChartType = 'line';

  humidityChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Humidity (%)', borderColor: '#d4a373', backgroundColor: 'rgba(212, 163, 115, 0.1)', fill: true, tension: 0.4 }]
  };
  humidityChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, max: 100, grid: { color: 'rgba(0,0,0,0.05)' } }, x: { grid: { display: false } } }
  };
  humidityChartType: ChartType = 'line';

  rainfallChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [{ data: [], label: 'Rainfall (mm)', backgroundColor: 'rgba(59, 130, 246, 0.6)', borderColor: '#3b82f6', borderWidth: 1 }]
  };
  rainfallChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' } }, x: { grid: { display: false } } }
  };
  rainfallChartType: ChartType = 'bar';

  // ✅ Use a single trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [null],
      fromDate: [''],
      toDate: [''],
      search: ['']
    });

    // ✅ Simplified effect - only call loadWeatherData
    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger >= 0) {
        this.loadWeatherData();
      }
    });

    // ✅ Separate effect for chart updates when weather data changes
    effect(() => {
      const data = this.weatherData();
      if (data && data.length > 0) {
        this.updateCharts(data);
      } else {
        this.clearCharts();
      }
    });
  }

  ngOnInit(): void {
    this.loadFields();
    // Initial load
    this.triggerReload();
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
        this.pageIndex.set(0);
        this.triggerReload();
      });
  }

  triggerReload(): void {
    this.reloadTrigger.update(value => value + 1);
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

  loadWeatherData(): void {
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

    const filter: WeatherHistoryFilter = {
      fieldId: filterValues.fieldId || null,
      fromDate: fromDate,
      toDate: toDate,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize()
    };

    this.weatherService.getWeatherHistory(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.weatherData.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
            this.selectedData.set([]);
          } else {
            this.showError(response.message || 'Failed to load weather data');
          }
        },
        error: (error: any) => {
          console.error('Error loading weather data:', error);
          this.showError('Failed to load weather data');
          this.weatherData.set([]);
          this.totalCount.set(0);
        }
      });
  }

  // =============================================
  // CHARTS - Updated to use signal data
  // =============================================

  private updateCharts(data: WeatherData[]): void {
    if (!data || data.length === 0) {
      this.clearCharts();
      return;
    }

    // Sort by recorded date
    const sortedData = [...data].sort((a, b) => 
      new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
    );

    // Limit to last 30 records for charts
    const chartData = sortedData.slice(-30);

    const labels = chartData.map(d => 
      new Date(d.recordedAt).toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    );

    const temperatures = chartData.map(d => d.temperature ?? 0);
    const humidities = chartData.map(d => d.humidity ?? 0);
    const rainfalls = chartData.map(d => d.rainfallMm ?? 0);

    // Update Temperature Chart
    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: labels,
      datasets: [
        {
          ...this.temperatureChartData.datasets[0],
          data: temperatures
        }
      ]
    };

    // Update Humidity Chart
    this.humidityChartData = {
      ...this.humidityChartData,
      labels: labels,
      datasets: [
        {
          ...this.humidityChartData.datasets[0],
          data: humidities
        }
      ]
    };

    // Update Rainfall Chart
    this.rainfallChartData = {
      ...this.rainfallChartData,
      labels: labels,
      datasets: [
        {
          ...this.rainfallChartData.datasets[0],
          data: rainfalls
        }
      ]
    };
  }

  private clearCharts(): void {
    this.temperatureChartData = {
      ...this.temperatureChartData,
      labels: [],
      datasets: [{ ...this.temperatureChartData.datasets[0], data: [] }]
    };

    this.humidityChartData = {
      ...this.humidityChartData,
      labels: [],
      datasets: [{ ...this.humidityChartData.datasets[0], data: [] }]
    };

    this.rainfallChartData = {
      ...this.rainfallChartData,
      labels: [],
      datasets: [{ ...this.rainfallChartData.datasets[0], data: [] }]
    };
  }

  // =============================================
  // SELECTION
  // =============================================

  toggleSelection(dataId: number): void {
    this.selectedData.update(current => {
      if (current.includes(dataId)) {
        return current.filter(id => id !== dataId);
      } else {
        return [...current, dataId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentData = this.weatherData();
    if (this.selectedCount() === currentData.length) {
      this.selectedData.set([]);
    } else {
      this.selectedData.set(currentData.map(d => d.id));
    }
  }

  isSelected(dataId: number): boolean {
    return this.selectedData().includes(dataId);
  }

  // =============================================
  // PAGINATION & SORTING
  // =============================================

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    this.triggerReload();
  }

  // =============================================
  // CRUD OPERATIONS
  // =============================================

  openManualEntry(): void {
    const dialogRef = this.dialog.open(ManualWeatherEntryComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { fieldId: this.filterForm.get('fieldId')?.value }
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.success) {
        this.triggerReload();
        this.showSuccess('Weather data added successfully');
      }
    });
  }

  editWeatherData(data: WeatherData): void {
    const dialogRef = this.dialog.open(ManualWeatherEntryComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        fieldId: data.fieldId,
        weatherData: data,
        mode: 'edit'
      }
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.success) {
        this.triggerReload();
        this.showSuccess('Weather data updated successfully');
      }
    });
  }

  deleteWeatherData(data: WeatherData): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Weather Data',
        message: `Are you sure you want to delete the weather record for ${data.fieldName} on ${this.formatDate(data.recordedAt)}?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        this.weatherService.deleteWeatherData(farmId, data.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response: any) => {
              if (response.success) {
                this.triggerReload();
                this.showSuccess('Weather data deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete weather data');
              }
            },
            error: (error: any) => {
              console.error('Error deleting weather data:', error);
              this.showError('Failed to delete weather data');
            }
          });
      }
    });
  }

  // =============================================
  // BULK OPERATIONS
  // =============================================

  bulkDelete(): void {
    const dataIds = this.selectedData();
    if (dataIds.length === 0) {
      this.showError('Please select weather data to delete');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Delete Weather Data',
        message: `Are you sure you want to delete ${dataIds.length} selected weather record(s)? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'danger'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        const requests = dataIds.map(id => 
          this.weatherService.deleteWeatherData(farmId, id).toPromise()
        );

        Promise.all(requests)
          .then(() => {
            this.selectedData.set([]);
            this.triggerReload();
            this.showSuccess(`Deleted ${dataIds.length} weather record(s)`);
          })
          .catch((error: any) => {
            console.error('Error bulk deleting weather data:', error);
            this.showError('Failed to delete some records');
          })
          .finally(() => this.isLoading.set(false));
      }
    });
  }

  clearSelection(): void {
    this.selectedData.set([]);
  }

  // =============================================
  // FILTERS
  // =============================================

  resetFilters(): void {
    this.filterForm.patchValue({
      fieldId: null,
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

  getWeatherIcon(condition: string | null): string {
    if (!condition) return 'wb_sunny';
    const iconMap: Record<string, string> = {
      'CLEAR': 'wb_sunny',
      'CLOUDY': 'cloud',
      'RAINY': 'umbrella',
      'STORMY': 'flash_on',
      'SNOWY': 'ac_unit',
      'FOGGY': 'foggy',
      'WINDY': 'air'
    };
    return iconMap[condition] || 'wb_sunny';
  }

  getWeatherClass(condition: string | null): string {
    if (!condition) return 'bg-primary-50 text-primary-700';
    const classMap: Record<string, string> = {
      'CLEAR': 'bg-yellow-50 text-yellow-700',
      'CLOUDY': 'bg-gray-100 text-gray-700',
      'RAINY': 'bg-blue-50 text-blue-700',
      'STORMY': 'bg-purple-50 text-purple-700',
      'SNOWY': 'bg-blue-50 text-blue-700',
      'FOGGY': 'bg-gray-100 text-gray-700',
      'WINDY': 'bg-cyan-50 text-cyan-700'
    };
    return classMap[condition] || 'bg-primary-50 text-primary-700';
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
  // EXPORT
  // =============================================

  exportPdf(): void {
    const data = this.weatherData();
    if (!data || data.length === 0) {
      this.showError('No data to export');
      return;
    }
    const columns = [
      { header: 'Recorded', dataKey: 'recordedAt' },
      { header: 'Field', dataKey: 'fieldName' },
      { header: 'Temperature (°C)', dataKey: 'temperature' },
      { header: 'Humidity (%)', dataKey: 'humidity' },
      { header: 'Wind Speed (m/s)', dataKey: 'windSpeed' },
      { header: 'Rainfall (mm)', dataKey: 'rainfallMm' },
      { header: 'Condition', dataKey: 'condition' }
    ];
    // Map status for display
    const printData = data.map(d => ({
      ...d,
      temperature: d.temperature ?? 'N/A',
      humidity: d.humidity ?? 'N/A',
      windSpeed: d.windSpeed ?? 'N/A',
      rainfallMm: d.rainfallMm ?? 'N/A',
      condition: d.condition || 'N/A',
      recordedAt: this.formatDate(d.recordedAt)
    }));
    this.reportService.exportToPdf(printData, columns, 'Weather Data Report', 'Weather_Data');
  }

  exportCsv(): void {
    const data = this.weatherData();
    if (!data || data.length === 0) {
      this.showError('No data to export');
      return;
    }
    const cleanData = data.map(d => ({
      Recorded: this.formatDate(d.recordedAt),
      Field: d.fieldName,
      'Temperature (°C)': d.temperature ?? 'N/A',
      'Humidity (%)': d.humidity ?? 'N/A',
      'Wind Speed (m/s)': d.windSpeed ?? 'N/A',
      'Rainfall (mm)': d.rainfallMm ?? 'N/A',
      Condition: d.condition || 'N/A'
    }));
    this.reportService.exportToCsv(cleanData, 'Weather_Data');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}