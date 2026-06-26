// src/app/features/admin/assign-field-dialog/assign-field-dialog.component.ts
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
import { WorkerService } from '../services/worker.service';
import { FieldService } from '../services/field.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { Worker } from '../models/worker.model';
import { Field } from '../models/field.model';
import { WorkerFieldAssignment } from '../models/worker-field.model';

interface DialogData {
  mode: 'create' | 'edit';
  assignment?: WorkerFieldAssignment;
}

@Component({
  selector: 'app-assign-field-dialog',
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
  templateUrl: './assign-field-dialog.component.html'
})
export class AssignFieldDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private workerService = inject(WorkerService);
  private fieldService = inject(FieldService);
  private workerFieldService = inject(WorkerFieldService);
  private dialogRef = inject(MatDialogRef<AssignFieldDialogComponent>);
  private snackBar = inject(MatSnackBar);
  
  private dialogData = inject<DialogData>(MAT_DIALOG_DATA, { optional: true });

  assignForm: FormGroup;
  isLoading = false;
  isLoadingDropdowns = false;
  workers: Worker[] = [];
  fields: Field[] = [];
  mode: 'create' | 'edit' = 'create';
  assignmentId: number | null = null;

  constructor() {
    this.mode = this.dialogData?.mode || 'create';
    this.assignmentId = this.dialogData?.assignment?.id || null;

    this.assignForm = this.fb.group({
      workerId: ['', Validators.required],
      fieldId: ['', Validators.required],
      assignedDate: [''],
      endDate: [''],
      notes: ['', [Validators.maxLength(500)]]
    });

    this.assignForm.get('assignedDate')?.valueChanges.subscribe(() => {
      this.validateEndDate();
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
    
    if (this.mode === 'edit' && this.dialogData?.assignment) {
      const assignment = this.dialogData.assignment;
      this.assignForm.patchValue({
        workerId: assignment.workerId,
        fieldId: assignment.fieldId,
        assignedDate: assignment.assignedDate ? new Date(assignment.assignedDate) : '',
        endDate: assignment.endDate ? new Date(assignment.endDate) : '',
        notes: assignment.notes || ''
      });
    }
  }

  loadDropdownData(): void {
    this.isLoadingDropdowns = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
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
      error: () => {
        this.checkDropdownsLoaded();
      }
    });

    this.fieldService.getFields(farmId, { page: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        if (response.success) {
          this.fields = response.data.items;
        }
        this.checkDropdownsLoaded();
      },
      error: () => {
        this.checkDropdownsLoaded();
      }
    });
  }

  private dropdownsLoaded = { workers: false, fields: false };

  private checkDropdownsLoaded(): void {
    setTimeout(() => {
      this.isLoadingDropdowns = false;
    }, 1000);
  }

  validateEndDate(): void {
    const assignedDate = this.assignForm.get('assignedDate')?.value;
    const endDate = this.assignForm.get('endDate')?.value;
    
    if (assignedDate && endDate && endDate <= assignedDate) {
      this.assignForm.get('endDate')?.setErrors({ matDatepickerMin: true });
    }
  }

  onSubmit(): void {
  if (this.assignForm.invalid) {
    this.assignForm.markAllAsTouched();
    return;
  }

  this.isLoading = true;
  const farmId = this.authService.getFarmId();
  if (!farmId) {
    this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
    this.isLoading = false;
    return;
  }

    const formValue = this.assignForm.value;
  
  const data: any = {
    workerId: formValue.workerId,
    fieldId: formValue.fieldId,
    notes: formValue.notes || null
  };

   // ✅ Send dates as UTC ISO strings
  if (formValue.assignedDate) {
    data.assignedDate = new Date(formValue.assignedDate).toISOString();
  }
  if (formValue.endDate) {
    data.endDate = new Date(formValue.endDate).toISOString();
  }

   let request;
  if (this.mode === 'create') {
    request = this.workerFieldService.assignFieldToWorker(farmId, data);
  } else {
    request = this.workerFieldService.updateAssignment(farmId, this.assignmentId!, data);
  }

request
    .pipe(finalize(() => this.isLoading = false))
    .subscribe({
      next: (response) => {
        if (response.success) {
          this.dialogRef.close(true);
          this.snackBar.open(
            this.mode === 'create' ? 'Field assigned successfully' : 'Assignment updated successfully',
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
        console.error('Error:', error);
        this.snackBar.open(
          this.mode === 'create' ? 'Failed to assign field' : 'Failed to update assignment',
          'Close',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
      }
    });
}

  onCancel(): void {
    this.dialogRef.close();
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.assignForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}