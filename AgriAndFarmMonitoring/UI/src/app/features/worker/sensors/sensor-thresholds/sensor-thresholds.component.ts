import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';

import { WorkerSensorService } from '../../services/worker-sensor.service';
import { 
  AlertThreshold, 
  SENSOR_TYPES, 
  SENSOR_TYPE_LABELS,
  SENSOR_TYPE_ICONS,
  SENSOR_TYPE_UNITS
} from '../../../admin/models/sensor.model';

@Component({
  selector: 'app-sensor-thresholds',
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
    MatSlideToggleModule,
    MatDividerModule
  ],
  templateUrl: './sensor-thresholds.component.html',
  styleUrls: ['./sensor-thresholds.component.scss']
})
export class SensorThresholdsComponent implements OnInit, OnDestroy {
  private workerSensorService = inject(WorkerSensorService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  isLoading = signal(false);
  thresholds = signal<AlertThreshold[]>([]);
  
  hasThresholds = computed(() => this.thresholds().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.thresholds().length === 0);

  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  displayedColumns = [
    'cropType',
    'growthStage',
    'sensorType',
    'range',
    'severity',
    'isActive'
  ];

  sensorTypes = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  sensorTypeIcons = SENSOR_TYPE_ICONS;
  sensorTypeUnits = SENSOR_TYPE_UNITS;

  filteredThresholds = computed(() => {
    const filters = this.filterForm.value;
    return this.thresholds().filter(t => {
      let match = true;
      if (filters.sensorType && t.sensorType !== filters.sensorType) match = false;
      if (filters.status !== null) {
        const isActive = filters.status === 'active';
        if (t.isActive !== isActive) match = false;
      }
      if (filters.search) {
        const search = filters.search.toLowerCase();
        if (!t.cropType.toLowerCase().includes(search) && 
            !t.growthStage.toLowerCase().includes(search)) {
          match = false;
        }
      }
      return match;
    });
  });

  constructor() {
    this.filterForm = this.fb.group({
      sensorType: [null],
      status: [null],
      search: ['']
    });
  }

  ngOnInit(): void {
    this.loadThresholds();
  }

  loadThresholds(): void {
    this.isLoading.set(true);

    this.workerSensorService.getThresholds()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.thresholds.set(response.data);
          } else {
            this.showError(response.message || 'Failed to load thresholds');
          }
        },
        error: (error: any) => {
          console.error('Error loading thresholds:', error);
          this.showError('Failed to load thresholds');
        }
      });
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      sensorType: null,
      status: null,
      search: ''
    });
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

  getSeverityColor(severity: string): string {
    const colors: Record<string, string> = {
      'LOW': 'bg-blue-100 text-blue-800 border border-blue-200',
      'MEDIUM': 'bg-yellow-100 text-yellow-800 border border-yellow-200',
      'HIGH': 'bg-orange-100 text-orange-800 border border-orange-200',
      'CRITICAL': 'bg-red-100 text-red-800 border border-red-200'
    };
    return colors[severity] || 'bg-gray-100 text-gray-800';
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
