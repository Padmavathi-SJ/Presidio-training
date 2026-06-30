// src/app/features/admin/tasks/tasks.component.ts
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
import { TaskService } from '../services/task.service';
import { WorkerService } from '../services/worker.service';
import { FieldService } from '../services/field.service';
import { FilterByStatusPipe } from '../pipes/filter-by-status.pipe';
import { 
  Task, 
  TaskFilterDto, 
  TASK_STATUSES, 
  TASK_PRIORITIES, 
  TASK_TYPES,
  STATUS_COLORS, 
  PRIORITY_COLORS,
  UpdateTaskStatusDto,
  ReassignTaskDto,
  BulkStatusUpdateDto,
  BulkReassignDto
} from '../models/task.model';
import { TaskFormComponent } from '../task-form/task-form.component';
import { TaskDetailComponent } from '../task-detail/task-detail.component';
import { TaskStatisticsComponent } from '../task-statistics/task-statistics.component';
import { TasksBulkOperationsComponent } from '../tasks-bulk-operations/tasks-bulk-operations.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-tasks',
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
    TaskStatisticsComponent,
    FilterByStatusPipe
  ],
  templateUrl: './tasks.component.html'
})
export class TasksComponent implements OnInit {
  private authService = inject(AuthService);
  private taskService = inject(TaskService);
  private workerService = inject(WorkerService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  // State Signals
  isLoading = signal(false);
  tasks = signal<Task[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('AssignedDate');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedTasks = signal<number[]>([]);
  statistics = signal<any>(null);
  viewMode = signal<'table' | 'kanban'>('table');

  // Computed Signals
  hasTasks = computed(() => this.tasks().length > 0);
  isEmpty = computed(() => !this.isLoading() && this.tasks().length === 0);
  selectedCount = computed(() => this.selectedTasks().length);
  hasSelected = computed(() => this.selectedCount() > 0);
  allSelected = computed(() => this.hasTasks() && this.selectedCount() === this.tasks().length);
  isIndeterminate = computed(() => this.selectedCount() > 0 && this.selectedCount() < this.tasks().length);

  // Filter form
  filterForm: FormGroup;
  private destroy$ = new Subject<void>();

  // Table columns
  displayedColumns = [
    'select',
    'taskName',
    'workerName',
    'fieldName',
    'priority',
    'status',
    'dueDate',
    'actions'
  ];

  // Options
  statuses = TASK_STATUSES;
  priorities = TASK_PRIORITIES;
  taskTypes = TASK_TYPES;
  statusColors = STATUS_COLORS;
  priorityColors = PRIORITY_COLORS;
  workers: any[] = [];
  fields: any[] = [];

  // Trigger for reload
  private reloadTrigger = signal(0);

  constructor() {
    this.filterForm = this.fb.group({
      workerId: [null],
      fieldId: [null],
      status: [null],
      priority: [null],
      taskName: [''],
      assignedDateFrom: [''],
      assignedDateTo: [''],
      dueDateFrom: [''],
      dueDateTo: [''],
      isOverdue: [null],
      activeOnly: [null]
    });

    effect(() => {
      const trigger = this.reloadTrigger();
      if (trigger > 0 || trigger === 0) {
        this.loadTasks();
      }
    });
  }

  ngOnInit(): void {
    this.loadTasks();
    this.loadStatistics();
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

public triggerReload(): void {
  this.reloadTrigger.update(value => value + 1);
}

  loadDropdownData(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const filter = { page: 1, pageSize: 100 };
    
    this.workerService.getWorkers(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.workers = response.data.items;
        }
      },
      error: (error) => console.error('Error loading workers:', error)
    });

    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
        }
      },
      error: (error) => console.error('Error loading fields:', error)
    });
  }

  loadTasks(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoading.set(false);
      this.showError('No farm found. Please login again.');
      return;
    }

    this.isLoading.set(true);

    const filterValues = this.filterForm.value;
    
    // Format dates to ISO
    const assignedDateFrom = filterValues.assignedDateFrom 
      ? new Date(filterValues.assignedDateFrom).toISOString() : null;
    const assignedDateTo = filterValues.assignedDateTo 
      ? new Date(filterValues.assignedDateTo).toISOString() : null;
    const dueDateFrom = filterValues.dueDateFrom 
      ? new Date(filterValues.dueDateFrom).toISOString() : null;
    const dueDateTo = filterValues.dueDateTo 
      ? new Date(filterValues.dueDateTo).toISOString() : null;

    const filter: TaskFilterDto = {
      workerId: filterValues.workerId || null,
      fieldId: filterValues.fieldId || null,
      status: filterValues.status || null,
      priority: filterValues.priority || null,
      taskName: filterValues.taskName || null,
      assignedDateFrom: assignedDateFrom,
      assignedDateTo: assignedDateTo,
      dueDateFrom: dueDateFrom,
      dueDateTo: dueDateTo,
      isOverdue: filterValues.isOverdue !== '' ? filterValues.isOverdue : null,
      activeOnly: filterValues.activeOnly !== '' ? filterValues.activeOnly : null,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };

    this.taskService.getTasks(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.tasks.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.showError(response.message || 'Failed to load tasks');
          }
        },
        error: (error) => {
          console.error('Error loading tasks:', error);
          this.showError('Failed to load tasks');
        }
      });
  }

  loadStatistics(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.taskService.getTaskStatistics(farmId).subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics.set(response.data);
        }
      },
      error: (error) => console.error('Error loading statistics:', error)
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
  toggleSelection(taskId: number): void {
    this.selectedTasks.update(current => {
      if (current.includes(taskId)) {
        return current.filter(id => id !== taskId);
      } else {
        return [...current, taskId];
      }
    });
  }

  toggleAllSelection(): void {
    const currentTasks = this.tasks();
    if (this.allSelected()) {
      this.selectedTasks.set([]);
    } else {
      this.selectedTasks.set(currentTasks.map(t => t.id));
    }
  }

  isSelected(taskId: number): boolean {
    return this.selectedTasks().includes(taskId);
  }

  // CRUD Operations
  openCreateDialog(): void {
    const dialogRef = this.dialog.open(TaskFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.loadStatistics();
        this.showSuccess('Task created successfully');
      }
    });
  }

  openEditDialog(task: Task): void {
    const dialogRef = this.dialog.open(TaskFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'edit', task }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.triggerReload();
        this.loadStatistics();
        this.showSuccess('Task updated successfully');
      }
    });
  }

  viewTaskDetail(task: Task): void {
    this.dialog.open(TaskDetailComponent, {
      width: '500px',
      maxWidth: '95vw',
      data: { task }
    });
  }

  updateTaskStatus(task: Task, status: string): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const data: UpdateTaskStatusDto = { status };

    this.isLoading.set(true);
    this.taskService.updateTaskStatus(farmId, task.id, data)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.triggerReload();
            this.loadStatistics();
            this.showSuccess(`Task status updated to ${status}`);
          } else {
            this.showError(response.message || 'Failed to update status');
          }
        },
        error: (error) => {
          console.error('Error updating status:', error);
          this.showError('Failed to update status');
        }
      });
  }

  reassignTask(task: Task, newWorkerId: number): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    const data: ReassignTaskDto = { newWorkerId };

    this.isLoading.set(true);
    this.taskService.reassignTask(farmId, task.id, data)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.triggerReload();
            this.showSuccess('Task reassigned successfully');
          } else {
            this.showError(response.message || 'Failed to reassign task');
          }
        },
        error: (error) => {
          console.error('Error reassigning task:', error);
          this.showError('Failed to reassign task');
        }
      });
  }

  deleteTask(task: Task): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete this task?`,
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
        this.taskService.deleteTask(farmId, task.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.triggerReload();
                this.loadStatistics();
                this.showSuccess('Task deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete task');
              }
            },
            error: (error) => {
              console.error('Error deleting task:', error);
              this.showError('Failed to delete task');
            }
          });
      }
    });
  }

  // Bulk Operations
  openBulkAssignDialog(): void {
    const dialogRef = this.dialog.open(TasksBulkOperationsComponent, {
      width: '600px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.triggerReload();
        this.loadStatistics();
      }
    });
  }

  bulkUpdateStatus(status: string): void {
    const taskIds = this.selectedTasks();
    if (taskIds.length === 0) {
      this.showError('Please select tasks to update');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Update Status',
        message: `Are you sure you want to update ${taskIds.length} selected task(s) to ${status}?`,
        confirmText: 'Update',
        cancelText: 'Cancel',
        type: 'info'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        const data: BulkStatusUpdateDto = { taskIds, status };

        this.isLoading.set(true);
        this.taskService.bulkUpdateStatus(farmId, data)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.selectedTasks.set([]);
                this.triggerReload();
                this.loadStatistics();
                this.showSuccess(`Updated ${response.data.successCount} tasks to ${status}`);
              } else {
                this.showError(response.message || 'Failed to update tasks');
              }
            },
            error: (error) => {
              console.error('Error bulk updating status:', error);
              this.showError('Failed to update tasks');
            }
          });
      }
    });
  }

// src/app/features/admin/tasks/tasks.component.ts
// Add/Update these methods for Excel operations

// Excel Operations
onFileSelected(event: Event, type: 'assign' | 'status' | 'reassign'): void {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    const file = input.files[0];
    
    // Validate file type
    const validExtensions = ['.xlsx', '.xls'];
    const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    
    if (!validExtensions.includes(extension)) {
      this.showError('Please upload a valid Excel file (.xlsx or .xls)');
      input.value = '';
      return;
    }
    
    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      this.showError('File size must be less than 10MB');
      input.value = '';
      return;
    }
    
    this.handleExcelImport(file, type);
  }
  input.value = '';
}

handleExcelImport(file: File, type: 'assign' | 'status' | 'reassign'): void {
  const farmId = this.authService.getFarmId();
  if (!farmId) {
    this.showError('No farm found');
    return;
  }

  this.isLoading.set(true);
  let request;
  let successMessage = '';

  switch (type) {
    case 'assign':
      request = this.taskService.bulkAssignFromExcel(farmId, file);
      successMessage = 'Tasks assigned successfully';
      break;
    case 'status':
      request = this.taskService.bulkUpdateStatusFromExcel(farmId, file);
      successMessage = 'Status updated successfully';
      break;
    case 'reassign':
      request = this.taskService.bulkReassignFromExcel(farmId, file);
      successMessage = 'Tasks reassigned successfully';
      break;
  }

  request
    .pipe(finalize(() => this.isLoading.set(false)))
    .subscribe({
      next: (response) => {
        if (response.success) {
          this.triggerReload();
          this.loadStatistics();
          const result = response.data;
          if (result.failedCount === 0) {
            this.showSuccess(`${successMessage}: ${result.successCount} tasks processed`);
          } else {
            this.showWarning(
              `${successMessage}: ${result.successCount} tasks processed, ${result.failedCount} failed. ` +
              `Check errors for details.`
            );
            console.warn('Excel import errors:', result.errors);
          }
        } else {
          this.showError(response.message || 'Failed to process Excel file');
        }
      },
      error: (error) => {
        console.error('Error processing Excel:', error);
        this.showError('Failed to process Excel file');
      }
    });
}

downloadTemplate(type: 'assign' | 'status' | 'reassign'): void {
  const farmId = this.authService.getFarmId();
  if (!farmId) {
    this.showError('No farm found');
    return;
  }

  this.isLoading.set(true);
  let request;
  let fileName = '';

  switch (type) {
    case 'assign':
      request = this.taskService.downloadBulkAssignTemplate(farmId);
      fileName = 'bulk_assign_template.xlsx';
      break;
    case 'status':
      request = this.taskService.downloadStatusUpdateTemplate(farmId);
      fileName = 'bulk_status_update_template.xlsx';
      break;
    case 'reassign':
      request = this.taskService.downloadReassignTemplate(farmId);
      fileName = 'bulk_reassign_template.xlsx';
      break;
  }

  request
    .pipe(finalize(() => this.isLoading.set(false)))
    .subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.showSuccess('Template downloaded successfully');
      },
      error: (error) => {
        console.error('Error downloading template:', error);
        this.showError('Failed to download template');
      }
    });
}

private showWarning(message: string): void {
  this.snackBar.open(message, 'Close', {
    duration: 5000,
    panelClass: ['bg-yellow-600', 'text-white']
  });
}

  resetFilters(): void {
    this.filterForm.patchValue({
      workerId: null,
      fieldId: null,
      status: null,
      priority: null,
      taskName: '',
      assignedDateFrom: '',
      assignedDateTo: '',
      dueDateFrom: '',
      dueDateTo: '',
      isOverdue: null,
      activeOnly: null
    });
    this.pageIndex.set(0);
    this.triggerReload();
  }

  getStatusColor(status: string): string {
    return this.statusColors[status] || 'bg-gray-100 text-gray-700';
  }

  getPriorityColor(priority: string): string {
    return this.priorityColors[priority] || 'bg-gray-100 text-gray-700';
  }

formatDate(date: string | null): string {
  if (!date) return '-';
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
}

  toggleViewMode(): void {
    this.viewMode.set(this.viewMode() === 'table' ? 'kanban' : 'table');
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