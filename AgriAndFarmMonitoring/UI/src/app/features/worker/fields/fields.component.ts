// src/app/features/worker/fields/fields.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { finalize, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

// Angular Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatRippleModule } from '@angular/material/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatExpansionModule } from '@angular/material/expansion';

// App Imports
import { AuthService } from '../../../core/services/auth.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { 
  WorkerFieldList,
  WorkerCropCycle,
  FIELD_STATUS_COLORS,
  GROWTH_STAGE_COLORS,
  GROWTH_STAGE_PROGRESS,
  GROWTH_STAGE_LABELS,
  CROP_TYPE_ICONS
} from '../models/worker-field.model';

@Component({
  selector: 'app-worker-fields',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressBarModule,
    MatRippleModule,
    MatPaginatorModule,
    MatExpansionModule
  ],
  templateUrl: './fields.component.html',
  styleUrls: ['./fields.component.scss']
})
export class FieldsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerFieldService = inject(WorkerFieldService);
  private snackBar = inject(MatSnackBar);

  // State Signals
  isLoading = signal(false);
  isRefreshing = signal(false);
  fields = signal<WorkerFieldList[]>([]);
  expandedFieldId = signal<number | null>(null);
  expandedFieldCropCycles = signal<WorkerCropCycle[]>([]);
  isLoadingCropCycles = signal<number | null>(null);
  totalCount = signal(0);
  pageSize = signal(6);
  pageIndex = signal(0);

  // Computed Signals
  hasFields = computed(() => this.fields().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.fields().length === 0);
  isExpanded = computed(() => this.expandedFieldId() !== null);

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadFields();
  }

  loadFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found. Please contact administrator.');
      return;
    }

    this.isLoading.set(true);
    this.workerFieldService.getMyAssignedFields()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.fields.set(response.data || []);
            this.totalCount.set(response.data?.length || 0);
          } else {
            this.showError(response.message || 'Failed to load fields');
          }
        },
        error: (error) => {
          console.error('Error loading fields:', error);
          this.showError('Failed to load fields');
        }
      });
  }

  refresh(): void {
    this.isRefreshing.set(true);
    this.loadFields();
    setTimeout(() => this.isRefreshing.set(false), 500);
  }

  // Toggle field expansion to show crop cycles
  toggleField(field: WorkerFieldList): void {
    if (this.expandedFieldId() === field.fieldId) {
      // Collapse if already expanded
      this.expandedFieldId.set(null);
      this.expandedFieldCropCycles.set([]);
    } else {
      // Expand and load crop cycles
      this.expandedFieldId.set(field.fieldId);
      this.loadCropCycles(field.fieldId);
    }
  }

  loadCropCycles(fieldId: number): void {
    this.isLoadingCropCycles.set(fieldId);
    this.workerFieldService.getAssignedFieldDetail(fieldId)
      .pipe(finalize(() => this.isLoadingCropCycles.set(null)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.expandedFieldCropCycles.set(response.data.cropCycles || []);
          } else {
            this.expandedFieldCropCycles.set([]);
            this.showError('Failed to load crop cycles');
          }
        },
        error: (error) => {
          console.error('Error loading crop cycles:', error);
          this.expandedFieldCropCycles.set([]);
          this.showError('Failed to load crop cycles');
        }
      });
  }

  getStatusColor(status: string | null): string {
    if (!status) return 'bg-gray-100 text-gray-700';
    return FIELD_STATUS_COLORS[status] || 'bg-gray-100 text-gray-700';
  }

  getStatusIcon(status: string | null): string {
    if (!status) return 'help';
    const icons: Record<string, string> = {
      'ACTIVE': 'check_circle',
      'FALLOW': 'pause_circle',
      'PREPARING': 'build',
      'MAINTENANCE': 'construction',
      'RETIRED': 'cancel'
    };
    return icons[status] || 'help';
  }

  getSoilTypeLabel(soilType: string | null): string {
    if (!soilType) return 'Unknown';
    const labels: Record<string, string> = {
      'CLAY': 'Clay',
      'SANDY': 'Sandy',
      'SILTY': 'Silty',
      'LOAMY': 'Loamy',
      'PEATY': 'Peaty',
      'CHALKY': 'Chalky'
    };
    return labels[soilType] || soilType;
  }

  getGrowthStageColor(stage: string | null): string {
    if (!stage) return 'bg-gray-100 text-gray-700';
    return GROWTH_STAGE_COLORS[stage] || 'bg-gray-100 text-gray-700';
  }

  getGrowthStageLabel(stage: string | null): string {
    if (!stage) return 'Unknown';
    return GROWTH_STAGE_LABELS[stage] || stage;
  }

  getGrowthProgress(stage: string | null): number {
    if (!stage) return 0;
    return GROWTH_STAGE_PROGRESS[stage] || 0;
  }

  getCropTypeIcon(cropType: string | null): string {
    if (!cropType) return 'grass';
    return CROP_TYPE_ICONS[cropType] || 'grass';
  }

  getDaysSincePlanting(cycle: WorkerCropCycle): string {
    const days = cycle.daysSincePlanting || 0;
    if (days === 0) return 'Planted today';
    if (days < 0) return 'Not planted yet';
    return `${days} day${days > 1 ? 's' : ''} since planting`;
  }

  getDaysToHarvest(cycle: WorkerCropCycle): string {
    const days = cycle.daysToHarvest || 0;
    if (days === 0) return 'Ready to harvest!';
    if (days < 0) return 'Harvest overdue!';
    return `${days} day${days > 1 ? 's' : ''} remaining`;
  }

  formatArea(area: number | null): string {
    if (!area) return 'N/A';
    return `${area.toFixed(1)} ha`;
  }

  formatDate(date: Date | string | null): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getCropCountLabel(count: number): string {
    if (count === 0) return 'No active crops';
    if (count === 1) return '1 active crop';
    return `${count} active crops`;
  }

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
  }

  getPaginatedFields(): WorkerFieldList[] {
    const start = this.pageIndex() * this.pageSize();
    const end = start + this.pageSize();
    return this.fields().slice(start, end);
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