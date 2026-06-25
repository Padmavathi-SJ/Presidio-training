// src/app/features/admin/crop-cycles/crop-cycles.component.ts
import { Component, inject, Input, OnInit, OnChanges, SimpleChanges, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { CropCycleService } from '../services/crop-cycle.service';
import { CropCycleFormComponent } from './crop-cycle-form/crop-cycle-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { 
  CropCycle, 
  CROP_TYPES, 
  GROWTH_STAGES, 
  CROP_STATUSES,
  GROWTH_STAGE_COLORS,
  STATUS_COLORS
} from '../models/crop-cycle.model';

@Component({
  selector: 'app-crop-cycles',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './crop-cycles.component.html'
})
export class CropCyclesComponent implements OnInit, OnChanges {  // ✅ Add OnChanges
  private authService = inject(AuthService);
  private cropCycleService = inject(CropCycleService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  @Input() fieldId!: number;
  @Input() fieldName: string = '';

  isLoading = signal(false);
  cropCycles = signal<CropCycle[]>([]);
  totalCount = signal(0);
  pageSize = signal(6);
  pageIndex = signal(0);

  cropTypes = CROP_TYPES;
  growthStages = GROWTH_STAGES;
  statuses = CROP_STATUSES;

  ngOnInit(): void {
    this.loadCropCycles();
  }

  // ✅ Add ngOnChanges to detect input changes
  ngOnChanges(changes: SimpleChanges): void {
    // If fieldId changes, reload crop cycles
    if (changes['fieldId'] && !changes['fieldId'].firstChange) {
      this.pageIndex.set(0); // Reset to first page
      this.loadCropCycles();
    }
  }

  loadCropCycles(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId || !this.fieldId) {
      this.cropCycles.set([]);
      this.totalCount.set(0);
      return;
    }

    this.isLoading.set(true);

    const filter = {
      fieldId: this.fieldId,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: 'CreatedAt',
      isDescending: false
    };

    console.log(`📤 Loading crop cycles for field ${this.fieldId} (${this.fieldName})`);

    this.cropCycleService.getCropCycles(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            console.log(`✅ Loaded ${response.data.items.length} crop cycles for field ${this.fieldId}`);
            this.cropCycles.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.cropCycles.set([]);
            this.totalCount.set(0);
          }
        },
        error: (error) => {
          console.error('Error loading crop cycles:', error);
          this.cropCycles.set([]);
          this.totalCount.set(0);
          this.snackBar.open('Failed to load crop cycles', 'Close', { duration: 3000 });
        }
      });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CropCycleFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        mode: 'create', 
        fieldId: this.fieldId,
        fieldName: this.fieldName
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCropCycles();
        this.showSuccess('Crop cycle created successfully');
      }
    });
  }

  openEditDialog(cycle: CropCycle): void {
    const dialogRef = this.dialog.open(CropCycleFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        mode: 'edit', 
        cropCycle: cycle,
        fieldId: this.fieldId,
        fieldName: this.fieldName
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCropCycles();
        this.showSuccess('Crop cycle updated successfully');
      }
    });
  }

  deleteCropCycle(cycle: CropCycle): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Crop Cycle',
        message: `Are you sure you want to delete the "${cycle.cropType}" crop cycle?`,
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
        this.cropCycleService.deleteCropCycle(farmId, cycle.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.loadCropCycles();
                this.showSuccess('Crop cycle deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete');
              }
            },
            error: () => {
              this.showError('Failed to delete crop cycle');
            }
          });
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    this.loadCropCycles();
  }

  getStatusColor(status: string): string {
    return STATUS_COLORS[status] || 'bg-gray-100 text-gray-700';
  }

  getGrowthStageColor(stage: string): string {
    return GROWTH_STAGE_COLORS[stage] || 'bg-gray-100 text-gray-700';
  }

  getGrowthProgress(stage: string): number {
    const stages = ['GERMINATION', 'SEEDLING', 'VEGETATIVE', 'FLOWERING', 'FRUITING', 'MATURITY', 'HARVESTED'];
    const index = stages.indexOf(stage);
    if (index === -1) return 0;
    return Math.round(((index + 1) / stages.length) * 100);
  }

  getDaysSincePlanting(cycle: CropCycle): string {
    const days = Math.floor((new Date().getTime() - new Date(cycle.plantingDate).getTime()) / (1000 * 3600 * 24));
    if (days < 0) return 'Not planted yet';
    if (days === 0) return 'Planted today';
    return `${days} day${days > 1 ? 's' : ''} since planting`;
  }

  isOverdue(cycle: CropCycle): boolean {
    if (!cycle.expectedHarvestDate) return false;
    const harvestDate = new Date(cycle.expectedHarvestDate);
    return harvestDate < new Date() && cycle.status === 'ACTIVE';
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['bg-green-600', 'text-white']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['bg-red-600', 'text-white']
    });
  }
}