// src/app/features/admin/task-form/task-form.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
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
import { CropCycleService } from '../services/crop-cycle.service';
import { Task, CreateTaskDto, UpdateTaskDto, TASK_TYPES, TASK_PRIORITIES } from '../models/task.model';

interface DialogData {
  mode: 'create' | 'edit';
  task?: Task;
}

@Component({
  selector: 'app-task-form',
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
  templateUrl: './task-form.component.html'
})
export class TaskFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private taskService = inject(TaskService);
  private workerService = inject(WorkerService);
  private fieldService = inject(FieldService);
  private cropCycleService = inject(CropCycleService);
  private dialogRef = inject(MatDialogRef<TaskFormComponent>);
  private snackBar = inject(MatSnackBar);

  data = inject<DialogData>(MAT_DIALOG_DATA);

  taskForm: FormGroup;
  isLoading = false;
  isLoadingDropdowns = false;
  mode: 'create' | 'edit' = 'create';
  workers: any[] = [];
  fields: any[] = [];
  cropCycles: any[] = [];
  taskTypes = TASK_TYPES;
  priorities = TASK_PRIORITIES;

  constructor() {
    this.mode = this.data.mode || 'create';

    this.taskForm = this.fb.group({
      workerId: ['', Validators.required],
      fieldId: [null],
      cropCycleId: [null],
      taskName: ['', Validators.required],
      dueDate: [''],
      priority: ['MEDIUM'],
      notes: ['', [Validators.maxLength(500)]]
    });

    if (this.mode === 'edit' && this.data.task) {
      const task = this.data.task;
      this.taskForm.patchValue({
        workerId: task.workerId,
        fieldId: task.fieldId,
        cropCycleId: task.cropCycleId,
        taskName: task.taskName,
        dueDate: task.dueDate ? new Date(task.dueDate) : '',
        priority: task.priority,
        notes: task.notes || ''
      });
    }
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

    this.taskForm.get('fieldId')?.valueChanges.subscribe((fieldId) => {
      if (fieldId) {
        this.loadCropCycles(fieldId);
      } else {
        this.cropCycles = [];
        this.taskForm.get('cropCycleId')?.setValue(null);
      }
    });
  }

  loadCropCycles(fieldId: number): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.cropCycleService.getCropCycles(farmId, { fieldId, page: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        if (response.success) {
          this.cropCycles = response.data.items;
        }
      },
      error: () => this.cropCycles = []
    });
  }

  private dropdownsLoaded = { workers: false, fields: false };

  private checkDropdownsLoaded(): void {
    setTimeout(() => {
      this.isLoadingDropdowns = false;
    }, 500);
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const formValue = this.taskForm.value;
    
    const data: any = {
      workerId: formValue.workerId,
      taskName: formValue.taskName,
      priority: formValue.priority || 'MEDIUM',
      notes: formValue.notes || null
    };

    if (formValue.fieldId) data.fieldId = formValue.fieldId;
    if (formValue.cropCycleId) data.cropCycleId = formValue.cropCycleId;
    if (formValue.dueDate) data.dueDate = new Date(formValue.dueDate).toISOString();

    let request;
    if (this.mode === 'create') {
      request = this.taskService.createTask(farmId, data);
    } else {
      request = this.taskService.updateTask(farmId, this.data.task!.id, data);
    }

    request
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
            this.snackBar.open(
              this.mode === 'create' ? 'Task created successfully' : 'Task updated successfully',
              'Close',
              { duration: 3000, panelClass: ['success-snackbar'] }
            );
          } else {
            this.snackBar.open(response.message || 'Operation failed', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error saving task:', error);
          this.snackBar.open('Failed to save task', 'Close', {
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
    const control = this.taskForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}