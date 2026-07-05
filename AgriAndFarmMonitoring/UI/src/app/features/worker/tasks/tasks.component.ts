import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { finalize } from 'rxjs';

import { MaterialModule } from '../../../shared/material.module';
import { WorkerTaskService } from '../services/worker-task.service';
import { 
  WorkerTaskDto, 
  WorkerTaskStatisticsDto, 
  WorkerTaskFilterDto,
  UpdateWorkerTaskStatusDto 
} from '../models/worker-task.model';

@Component({
  selector: 'app-worker-tasks',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
    MatTabsModule,
    DatePipe
  ],
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.scss']
})
export class TasksComponent implements OnInit {
  private taskService = inject(WorkerTaskService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  statistics: WorkerTaskStatisticsDto | null = null;
  tasks = new MatTableDataSource<WorkerTaskDto>([]);
  
  displayedColumns: string[] = ['taskName', 'fieldName', 'priority', 'status', 'dueDate', 'actions'];
  
  totalTasks = 0;
  pageSize = 10;
  pageIndex = 0;
  isLoading = false;
  isStatsLoading = false;
  activeTab = 0;

  filterForm: FormGroup;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.filterForm = this.fb.group({
      taskName: [''],
      status: [''],
      priority: [''],
      isOverdue: [null],
      dueDateFrom: [''],
      dueDateTo: ['']
    });
  }

  ngOnInit(): void {
    this.loadStatistics();
    this.loadTasks();

    this.filterForm.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadTasks();
      });
  }

  loadStatistics(): void {
    this.isStatsLoading = true;
    this.taskService.getStatistics()
      .pipe(finalize(() => this.isStatsLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.statistics = res.data;
          }
        },
        error: (err) => this.showError('Failed to load task statistics.')
      });
  }

  onTabChange(event: any): void {
    this.activeTab = event.index;
    this.pageIndex = 0;
    // When switching to history, we might want to clear or change default filters
    if (this.activeTab === 1) {
      this.filterForm.patchValue({ status: 'Completed' }, { emitEvent: false });
    } else {
      this.filterForm.patchValue({ status: '' }, { emitEvent: false });
    }
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    const formVal = this.filterForm.value;
    const filter: WorkerTaskFilterDto = {
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      ...formVal,
      // Format dates if they exist
      dueDateFrom: formVal.dueDateFrom ? new Date(formVal.dueDateFrom).toISOString() : undefined,
      dueDateTo: formVal.dueDateTo ? new Date(formVal.dueDateTo).toISOString() : undefined
    };

    // Remove empty values
    Object.keys(filter).forEach(key => {
      const k = key as keyof WorkerTaskFilterDto;
      if (filter[k] === '' || filter[k] === null || filter[k] === undefined) {
        delete filter[k];
      }
    });

    const request$ = this.activeTab === 0 
      ? this.taskService.getMyTasks(filter)
      : this.taskService.getTaskHistory(filter);

    request$
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.tasks.data = res.data.items || [];
            this.totalTasks = res.data.totalCount;
          }
        },
        error: (err) => this.showError('Failed to load tasks.')
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTasks();
  }

  onSortChange(sortState: Sort): void {
    if (sortState.direction) {
      this.filterForm.patchValue({
        sortBy: sortState.active,
        isDescending: sortState.direction === 'desc'
      }, { emitEvent: false });
    } else {
      this.filterForm.patchValue({
        sortBy: 'DueDate',
        isDescending: false
      }, { emitEvent: false });
    }
    this.loadTasks();
  }

  updateTaskStatus(task: WorkerTaskDto, newStatus: string): void {
    const dto: UpdateWorkerTaskStatusDto = { status: newStatus };
    
    // Optionally prompt for completion notes if changing to Completed
    if (newStatus === 'COMPLETED') {
      const notes = prompt('Enter completion notes (optional):');
      if (notes !== null) {
        dto.completionNotes = notes;
      } else {
        return; // user cancelled
      }
    }

    this.taskService.updateTaskStatus(task.id, dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open(`Task marked as ${newStatus}`, 'Close', { duration: 3000 });
          this.loadTasks();
          this.loadStatistics();
        }
      },
      error: (err) => this.showError(err.error?.message || 'Failed to update task status')
    });
  }

  getPriorityColor(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'high':
      case 'urgent': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'completed': return 'primary';
      case 'in_progress':
      case 'in progress': return 'accent';
      case 'pending': return 'warn';
      default: return 'default';
    }
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { 
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
