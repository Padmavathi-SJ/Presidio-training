// src/app/features/admin/crop-cycles/crop-cycles.component.ts
import { Component, inject, Input, OnInit, OnChanges, SimpleChanges, signal, computed, effect, Output, EventEmitter } from '@angular/core';
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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { CropCycleService } from '../services/crop-cycle.service';
import { CropCycleFormComponent } from './crop-cycle-form/crop-cycle-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { 
  CropCycle, 
  CROP_TYPES,           // ✅ Now available
  GROWTH_STAGES, 
  CROP_STATUSES,
  GROWTH_STAGE_COLORS,
  STATUS_COLORS,
  GROWTH_STAGE_ICONS,
  GROWTH_STAGE_DESCRIPTIONS,
  getGrowthProgress     // ✅ Use helper function
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
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './crop-cycles.component.html'
})
export class CropCyclesComponent implements OnInit, OnChanges {
  private authService = inject(AuthService);
  private cropCycleService = inject(CropCycleService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  private _fieldId = signal<number>(0);
  private _fieldName = signal<string>('');

  @Input() set fieldId(value: number) {
    this._fieldId.set(value);
  }
  get fieldId(): number {
    return this._fieldId();
  }

  @Input() set fieldName(value: string) {
    this._fieldName.set(value);
  }
  get fieldName(): string {
    return this._fieldName();
  }

  @Output() cropCyclesChanged = new EventEmitter<void>();

  isLoading = signal(false);
  cropCycles = signal<CropCycle[]>([]);
  totalCount = signal(0);
  pageSize = signal(6);
  pageIndex = signal(0);

  updatingStage = signal<number | null>(null);

  hasCropCycles = computed(() => this.cropCycles().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.cropCycles().length === 0);
  showPagination = computed(() => this.totalCount() > this.pageSize());

  // ✅ Use imported constants
  cropTypes = CROP_TYPES;
  growthStages = GROWTH_STAGES;
  statuses = CROP_STATUSES;
  stageIcons = GROWTH_STAGE_ICONS;
  stageDescriptions = GROWTH_STAGE_DESCRIPTIONS;

  private loadEffect = effect(() => {
    const id = this._fieldId();
    if (id > 0) {
      this.pageIndex.set(0);
      this.loadCropCycles();
    }
  });

  ngOnInit(): void {
    if (this._fieldId() > 0) {
      this.loadCropCycles();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {}

  loadCropCycles(): void {
    const farmId = this.authService.getFarmId();
    const currentFieldId = this._fieldId();
    
    if (!farmId || !currentFieldId) {
      this.cropCycles.set([]);
      this.totalCount.set(0);
      return;
    }

    this.isLoading.set(true);

    const filter = {
      fieldId: currentFieldId,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: 'CreatedAt',
      isDescending: false
    };

    this.cropCycleService.getCropCycles(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
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
          this.showError('Failed to load crop cycles');
        }
      });
  }

  refreshGrowthStage(cycle: CropCycle, event: Event): void {
    event.stopPropagation();
    
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.updatingStage.set(cycle.id);
    
    this.cropCycleService.updateGrowthStage(farmId, cycle.id)
      .pipe(finalize(() => this.updatingStage.set(null)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.loadCropCycles();
            this.showSuccess('Growth stage updated successfully');
          } else {
            this.showError(response.message || 'Failed to update growth stage');
          }
        },
        error: () => {
          this.showError('Failed to update growth stage');
        }
      });
  }

  private notifyParent(): void {
    this.cropCyclesChanged.emit();
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CropCycleFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      autoFocus: false,
      data: { 
        mode: 'create', 
        fieldId: this._fieldId(),
        fieldName: this._fieldName()
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCropCycles();
        this.notifyParent();
        this.showSuccess('Crop cycle created successfully');
      }
    });
  }

  openEditDialog(cycle: CropCycle): void {
    const dialogRef = this.dialog.open(CropCycleFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      autoFocus: false,
      data: { 
        mode: 'edit', 
        cropCycle: cycle,
        fieldId: this._fieldId(),
        fieldName: this._fieldName()
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCropCycles();
        this.notifyParent();
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
                this.notifyParent();
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

  getGrowthStageIcon(stage: string): string {
    return this.stageIcons[stage] || 'grass';
  }

  getGrowthStageDescription(stage: string): string {
    return this.stageDescriptions[stage] || 'Unknown stage';
  }

  // ✅ Use the helper function
  getGrowthProgress(stage: string): number {
    return getGrowthProgress(stage);
  }

  getDaysSincePlanting(cycle: CropCycle): string {
    const days = Math.floor((new Date().getTime() - new Date(cycle.plantingDate).getTime()) / (1000 * 3600 * 24));
    if (days < 0) return 'Not planted yet';
    if (days === 0) return 'Planted today';
    return `${days} day${days > 1 ? 's' : ''} since planting`;
  }

  getDaysUntilHarvest(cycle: CropCycle): string {
    if (!cycle.expectedHarvestDate) return 'N/A';
    if (cycle.actualHarvestDate) return 'Harvested';
    
    const days = Math.floor((new Date(cycle.expectedHarvestDate).getTime() - new Date().getTime()) / (1000 * 3600 * 24));
    if (days < 0) return 'Overdue';
    if (days === 0) return 'Today';
    return `${days} day${days > 1 ? 's' : ''}`;
  }

  isOverdue(cycle: CropCycle): boolean {
    return cycle.isOverdue ?? false;
  }

  isReadyForHarvest(cycle: CropCycle): boolean {
    return cycle.isReadyForHarvest ?? false;
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['error-snackbar']
    });
  }
}