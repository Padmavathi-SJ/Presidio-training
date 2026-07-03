// src/app/features/worker/fields/fields.component.ts
import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
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
import { MatGridListModule } from '@angular/material/grid-list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatRippleModule } from '@angular/material/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

// App Imports
import { AuthService } from '../../../core/services/auth.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { WorkerFieldList } from '../models/worker-field.model';

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
    MatGridListModule,
    MatBadgeModule,
    MatRippleModule,
    MatPaginatorModule
  ],
  templateUrl: './fields.component.html',
  styleUrls: ['./fields.component.scss']
})
export class FieldsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerFieldService = inject(WorkerFieldService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // State Signals
  isLoading = signal(false);
  isRefreshing = signal(false);
  fields = signal<WorkerFieldList[]>([]);
  totalCount = signal(0);
  pageSize = signal(9);
  pageIndex = signal(0);

  // Computed Signals
  hasFields = computed(() => this.fields().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.fields().length === 0);

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

  viewFieldDetails(fieldId: number): void {
    this.router.navigate(['/worker/fields', fieldId]);
  }

  getStatusColor(status: string | null): string {
    if (!status) return 'bg-gray-100 text-gray-700';
    
    const colors: Record<string, string> = {
      'ACTIVE': 'bg-green-100 text-green-700 border-green-400',
      'FALLOW': 'bg-yellow-100 text-yellow-700 border-yellow-400',
      'PREPARING': 'bg-blue-100 text-blue-700 border-blue-400',
      'MAINTENANCE': 'bg-orange-100 text-orange-700 border-orange-400',
      'RETIRED': 'bg-gray-100 text-gray-700 border-gray-400'
    };
    return colors[status] || 'bg-gray-100 text-gray-700';
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

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    // Since we load all fields at once, just update the displayed items
  }

  // Get paginated fields
  getPaginatedFields(): WorkerFieldList[] {
    const start = this.pageIndex() * this.pageSize();
    const end = start + this.pageSize();
    return this.fields().slice(start, end);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}