// src/app/features/admin/tasks-bulk-operations/tasks-bulk-operations.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter, MAT_DATE_FORMATS, MAT_NATIVE_DATE_FORMATS, NativeDateAdapter } from '@angular/material/core';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { TaskService } from '../services/task.service';
import { WorkerService } from '../services/worker.service';
import { FieldService } from '../services/field.service';
import { TASK_TYPES, TASK_PRIORITIES, BulkAssignTaskDto } from '../models/task.model';

@Component({
  selector: 'app-tasks-bulk-operations',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS }
  ],
  templateUrl: './tasks-bulk-operations.component.html'
})
export class TasksBulkOperationsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private taskService = inject(TaskService);
  private workerService = inject(WorkerService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<TasksBulkOperationsComponent>);
  private snackBar = inject(MatSnackBar);

  bulkForm: FormGroup;
  isLoading = false;
  isLoadingDropdowns = false;
  workers: any[] = [];
  fields: any[] = [];
  taskTypes = TASK_TYPES;
  priorities = TASK_PRIORITIES;

  constructor() {
    this.bulkForm = this.fb.group({
      workerIds: [[], Validators.required],
      taskName: ['', Validators.required],
      fieldId: [null],
      dueDate: [''],
      priority: ['MEDIUM'],
      notes: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
  }

  loadDropdownData(): void {
    this.isLoadingDropdowns = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoadingDropdowns = false;
      return;
    }

    const filter = { page: 1, pageSize: 100 };
    
    this.workerService.getWorkers(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.workers = response.data.items;
        }
        this.checkDropdownsLoaded();
      },
      error: () => this.checkDropdownsLoaded()
    });

    this.fieldService.getFields(farmId, filter).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
        }
        this.checkDropdownsLoaded();
      },
      error: () => this.checkDropdownsLoaded()
    });
  }

  private dropdownsLoaded = { workers: false, fields: false };

  private checkDropdownsLoaded(): void {
    setTimeout(() => {
      this.isLoadingDropdowns = false;
    }, 500);
  }

  onSubmit(): void {
    if (this.bulkForm.invalid) {
      this.bulkForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const formValue = this.bulkForm.value;
    
    const data: BulkAssignTaskDto = {
      workerIds: formValue.workerIds,
      taskName: formValue.taskName,
      priority: formValue.priority || 'MEDIUM',
      notes: formValue.notes || null
    };

    if (formValue.fieldId) data.fieldId = formValue.fieldId;
    if (formValue.dueDate) data.dueDate = new Date(formValue.dueDate).toISOString();

    this.taskService.bulkAssignTasks(farmId, data)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close({ success: true, data: response.data });
            this.snackBar.open(
              `Assigned ${response.data.successCount} of ${response.data.totalRequests} tasks`,
              'Close',
              { duration: 3000, panelClass: ['success-snackbar'] }
            );
          } else {
            this.snackBar.open(response.message || 'Failed to assign tasks', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error bulk assigning tasks:', error);
          this.snackBar.open('Failed to assign tasks', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.bulkForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}