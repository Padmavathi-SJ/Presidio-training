// src/app/features/admin/worker-fields/worker-fields.component.ts
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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { finalize, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { WorkerService } from '../services/worker.service';
import { FieldService } from '../services/field.service';
import { ReportGeneratorService } from '../../../core/services/report-generator.service';
import { WorkerFieldAssignment, WorkerFieldFilterDto, ASSIGNMENT_STATUS_COLORS } from '../models/worker-field.model';
import { AssignFieldDialogComponent } from '../assign-field-dialog/assign-field-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-worker-fields',
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
    MatNativeDateModule
  ],
  templateUrl: './worker-fields.component.html'
})
export class WorkerFieldsComponent implements OnInit {
  private authService = inject(AuthService);
  private workerFieldService = inject(WorkerFieldService);
  private workerService = inject(WorkerService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);

  // State Signals
  isLoading = signal(false);
  assignments = signal<WorkerFieldAssignment[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('AssignedDate');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedAssignments = signal<number[]>([]);
  workers = signal<any[]>([]);
  fields = signal<any[]>([]);

  // Computed Signals
  hasAssignments = computed(() => this.assignments().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.assignments().length === 0);
  selectedCount = computed(() => this.selectedAssignments().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasAssignments() && this.selectedCount() === this.assignments().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.assignments().length);
  activeAssignments = computed(() => this.assignments().filter(a => a.isActive).length);
  inactiveAssignments = computed(() => this.assignments().filter(a => !a.isActive).length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'workerName',
    'fieldName',
    'fieldLocation',
    'assignedDate',
    'endDate',
    'status',
    'actions'
  ];

  // Status colors
  statusColors = ASSIGNMENT_STATUS_COLORS;

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    // ✅ Updated filter form with End Date filters
    this.filterForm = this.fb.group({
      workerId: [null],
      fieldId: [null],
      isActive: [null],
      assignedDateFrom: [''],
      assignedDateTo: [''],
      endDateFrom: [''],      // ✅ Added
      endDateTo: ['']         // ✅ Added
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadAssignments();
      }
    });
  }

  ngOnInit(): void {
    this.loadAssignments();
    this.loadDropdownData();
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

  loadDropdownData(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    
    this.workerService.getWorkers(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.workers.set(response.data.items);
        }
      },
      error: (error) => console.error('Error loading workers:', error)
    });

    this.fieldService.getFields(farmId, { page: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields.set(response.data.items);
        }
      },
      error: (error) => console.error('Error loading fields:', error)
    });
  }

  loadAssignments(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoading.set(false);
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);

    const filterValues = this.filterForm.value;
    
    // ✅ Format all dates to UTC ISO strings
    const assignedDateFrom = filterValues.assignedDateFrom 
      ? new Date(filterValues.assignedDateFrom).toISOString() 
      : null;
    
    const assignedDateTo = filterValues.assignedDateTo 
      ? new Date(filterValues.assignedDateTo).toISOString() 
      : null;
    
    // ✅ End Date filters
    const endDateFrom = filterValues.endDateFrom 
      ? new Date(filterValues.endDateFrom).toISOString() 
      : null;
    
    const endDateTo = filterValues.endDateTo 
      ? new Date(filterValues.endDateTo).toISOString() 
      : null;
    
    const filter: WorkerFieldFilterDto = {
      workerId: filterValues.workerId || null,
      fieldId: filterValues.fieldId || null,
      isActive: filterValues.isActive !== '' ? filterValues.isActive : null,
      assignedDateFrom: assignedDateFrom,
      assignedDateTo: assignedDateTo,
      endDateFrom: endDateFrom,    // ✅ Added
      endDateTo: endDateTo,        // ✅ Added
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    console.log('📤 Sending filter:', filter);

    this.workerFieldService.getAssignments(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.assignments.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.showError(response.message || 'Failed to load assignments');
          }
        },
        error: (error) => {
          console.error('Error loading assignments:', error);
          this.showError('Failed to load assignments');
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
  toggleSelection(assignmentId: number): void {
    this.selectedAssignments.update(current => {
      if (current.includes(assignmentId)) {
        return current.filter(id => id !== assignmentId);
      } else {
        return [...current, assignmentId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentAssignments = this.assignments();
    if (this.allSelected()) {
      this.selectedAssignments.set([]);
    } else {
      this.selectedAssignments.set(currentAssignments.map(a => a.id));
    }
  }

  isSelected(assignmentId: number): boolean {
    return this.selectedAssignments().includes(assignmentId);
  }

  // CRUD Operations
  openAssignDialog(): void {
    const dialogRef = this.dialog.open(AssignFieldDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.showSuccess('Field assigned successfully');
      }
    });
  }

  openEditDialog(assignment: WorkerFieldAssignment): void {
    const dialogRef = this.dialog.open(AssignFieldDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        mode: 'edit', 
        assignment 
      }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.showSuccess('Assignment updated successfully');
      }
    });
  }

  removeAssignment(assignment: WorkerFieldAssignment): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Remove Assignment',
        message: `Are you sure you want to remove "${assignment.fieldName}" from "${assignment.workerName}"?`,
        confirmText: 'Remove',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        this.workerFieldService.removeAssignment(farmId, assignment.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.triggerReload();
                this.showSuccess('Assignment removed successfully');
              } else {
                this.showError(response.message || 'Failed to remove assignment');
              }
            },
            error: (error) => {
              console.error('Error removing assignment:', error);
              this.showError('Failed to remove assignment');
            }
          });
      }
    });
  }

  bulkRemove(): void {
    if (!this.hasSelected()) {
      this.showError('Please select assignments to remove');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Remove Assignments',
        message: `Are you sure you want to remove ${this.selectedCount()} selected assignment(s)?`,
        confirmText: 'Remove All',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.showWarning('Bulk remove is not yet implemented');
      }
    });
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      workerId: null,
      fieldId: null,
      isActive: null,
      assignedDateFrom: '',
      assignedDateTo: '',
      endDateFrom: '',     // ✅ Added
      endDateTo: ''        // ✅ Added
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

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getWorkerName(workerId: number): string {
    const worker = this.workers().find(w => w.id === workerId);
    return worker ? worker.name : 'Unknown';
  }

  getFieldName(fieldId: number): string {
    const field = this.fields().find(f => f.id === fieldId);
    return field ? field.fieldName : 'Unknown';
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

  exportPdf(): void {
    const data = this.assignments();
    if (!data.length) {
      this.showWarning('No data to export');
      return;
    }
    const columns = [
      { header: 'Assigned Date', dataKey: 'assignedDate' },
      { header: 'End Date', dataKey: 'endDate' },
      { header: 'Worker', dataKey: 'workerName' },
      { header: 'Field', dataKey: 'fieldName' },
      { header: 'Location', dataKey: 'fieldLocation' },
      { header: 'Status', dataKey: 'status' }
    ];
    // Map status for display
    const printData = data.map(d => ({
      ...d,
      status: d.isActive ? 'Active' : 'Inactive',
      assignedDate: this.formatDate(d.assignedDate),
      endDate: d.endDate ? this.formatDate(d.endDate) : '-'
    }));
    this.reportService.exportToPdf(printData, columns, 'Worker Field Assignments Report', 'Worker_Assignments');
  }

  exportCsv(): void {
    const data = this.assignments();
    if (!data.length) {
      this.showWarning('No data to export');
      return;
    }
    const cleanData = data.map(d => ({
      Assigned_Date: this.formatDate(d.assignedDate),
      End_Date: d.endDate ? this.formatDate(d.endDate) : 'N/A',
      Worker: d.workerName,
      Field: d.fieldName,
      Location: d.fieldLocation || 'N/A',
      Status: d.isActive ? 'Active' : 'Inactive'
    }));
    this.reportService.exportToCsv(cleanData, 'Worker_Assignments');
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}