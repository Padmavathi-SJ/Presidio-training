// src/app/features/admin/workers/workers.component.ts
import { Component, inject, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
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
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { WorkerService } from '../services/worker.service';
import { Worker, WorkerFilterDto, WORKER_ROLES, STATUS_COLORS, ROLE_COLORS } from '../models/worker.model';
import { WorkerFormComponent } from '../worker-form/worker-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ResetPasswordDialogComponent } from '../reset-password-dialog/reset-password-dialog.component';

@Component({
  selector: 'app-workers',
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
    MatBadgeModule
  ],
  templateUrl: './workers.component.html'
})
export class WorkersComponent implements OnInit {
  private authService = inject(AuthService);
  private workerService = inject(WorkerService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State Signals
  isLoading = signal(false);
  workers = signal<Worker[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('CreatedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedWorkers = signal<number[]>([]);
  statistics = signal<any>(null);

  // Computed Signals
  hasWorkers = computed(() => this.workers().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.workers().length === 0);
  selectedCount = computed(() => this.selectedWorkers().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasWorkers() && this.selectedCount() === this.workers().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.workers().length);
  activeWorkers = computed(() => this.workers().filter(w => w.isActive).length);
  inactiveWorkers = computed(() => this.workers().filter(w => !w.isActive).length);
  newThisMonth = computed(() => {
    const thirtyDaysAgo = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000);
    return this.workers().filter(w => new Date(w.createdAt) > thirtyDaysAgo).length;
  });

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'name',
    'email',
    'phone',
    'role',
    'hireDate',
    'status',
    'lastLogin',
    'actions'
  ];

  // Options
  workerRoles = WORKER_ROLES;
  statusColors = STATUS_COLORS;
  roleColors = ROLE_COLORS;

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    this.filterForm = this.fb.group({
      name: [''],
      email: [''],
      role: [''],
      isActive: [null],
      hireDateFrom: [''],
      hireDateTo: ['']
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadWorkers();
      }
    });
  }

  ngOnInit(): void {
    this.loadWorkers();
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

  private triggerReload(): void {
    this.reloadTrigger.update(value => value + 1);
  }

  loadWorkers(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoading.set(false);
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);

    const filterValues = this.filterForm.value;
    
    const filter: WorkerFilterDto = {
      name: filterValues.name || null,
      email: filterValues.email || null,
      role: filterValues.role || null,
      isActive: filterValues.isActive !== '' ? filterValues.isActive : null,
      hireDateFrom: filterValues.hireDateFrom || null,
      hireDateTo: filterValues.hireDateTo || null,
      includeDeleted: false,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.workerService.getWorkers(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.workers.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.showError(response.message || 'Failed to load workers');
          }
        },
        error: (error) => {
          console.error('Error loading workers:', error);
          this.showError('Failed to load workers');
        }
      });
  }

  // Pagination
  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    this.triggerReload();
  }

  // Sorting
  onSortChange(sort: Sort): void {
    this.sortField.set(sort.active);
    this.sortDirection.set(sort.direction || 'desc');
    this.pageIndex.set(0);
    this.triggerReload();
  }

  // Selection
  toggleSelection(workerId: number): void {
    this.selectedWorkers.update(current => {
      if (current.includes(workerId)) {
        return current.filter(id => id !== workerId);
      } else {
        return [...current, workerId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentWorkers = this.workers();
    if (this.allSelected()) {
      this.selectedWorkers.set([]);
    } else {
      this.selectedWorkers.set(currentWorkers.map(w => w.id));
    }
  }

  isSelected(workerId: number): boolean {
    return this.selectedWorkers().includes(workerId);
  }

  // CRUD Operations
  openCreateDialog(): void {
    const dialogRef = this.dialog.open(WorkerFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.showSuccess('Worker created successfully');
      }
    });
  }

  openEditDialog(worker: Worker): void {
    const dialogRef = this.dialog.open(WorkerFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'edit', worker }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.showSuccess('Worker updated successfully');
      }
    });
  }

  deleteWorker(worker: Worker): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Worker',
        message: `Are you sure you want to delete "${worker.name}"? This action can be undone.`,
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
        this.workerService.deleteWorker(farmId, worker.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.triggerReload();
                this.showSuccess('Worker deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete worker');
              }
            },
            error: (error) => {
              console.error('Error deleting worker:', error);
              this.showError('Failed to delete worker');
            }
          });
      }
    });
  }

  toggleWorkerStatus(worker: Worker): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: worker.isActive ? 'Deactivate Worker' : 'Activate Worker',
        message: worker.isActive 
          ? `Are you sure you want to deactivate "${worker.name}"?` 
          : `Are you sure you want to activate "${worker.name}"?`,
        confirmText: worker.isActive ? 'Deactivate' : 'Activate',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.isLoading.set(true);
        const request = worker.isActive 
          ? this.workerService.deactivateWorker(farmId, worker.id)
          : this.workerService.activateWorker(farmId, worker.id);

        request
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.triggerReload();
                this.showSuccess(worker.isActive ? 'Worker deactivated successfully' : 'Worker activated successfully');
              } else {
                this.showError(response.message || 'Failed to update worker status');
              }
            },
            error: (error) => {
              console.error('Error toggling worker status:', error);
              this.showError('Failed to update worker status');
            }
          });
      }
    });
  }

resetPassword(worker: Worker): void {
  const dialogRef = this.dialog.open(ResetPasswordDialogComponent, {
    width: '450px',
    maxWidth: '95vw',
    data: { worker }
  });

  dialogRef.afterClosed().subscribe((result) => {
    if (result) {
      this.triggerReload();
    }
  });
}


  private generateRandomPassword(): string {
    const length = 10;
    const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
    let password = '';
    for (let i = 0; i < length; i++) {
      const randomIndex = Math.floor(Math.random() * charset.length);
      password += charset[randomIndex];
    }
    return password;
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      name: '',
      email: '',
      role: '',
      isActive: null,
      hireDateFrom: '',
      hireDateTo: ''
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  getStatusColor(isActive: boolean): string {
    return this.statusColors[String(isActive)] || 'bg-gray-100 text-gray-700';
  }

  getRoleColor(role: string): string {
    return this.roleColors[role] || 'bg-gray-100 text-gray-700';
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getLastLoginDays(lastLoginDays: number | null): string {
    if (lastLoginDays === null) return 'Never';
    if (lastLoginDays === 0) return 'Today';
    return `${lastLoginDays} day${lastLoginDays > 1 ? 's' : ''} ago`;
  }

  bulkDelete(): void {
    if (!this.hasSelected()) {
      this.showError('Please select workers to delete');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Delete Workers',
        message: `Are you sure you want to delete ${this.selectedCount()} selected worker(s)?`,
        confirmText: 'Delete All',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        // Implement bulk delete when backend supports it
        this.showWarning('Bulk delete is not yet implemented');
      }
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
      duration: 3000,
      panelClass: ['bg-yellow-600', 'text-white']
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}