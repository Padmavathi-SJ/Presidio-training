// src/app/features/admin/crop-cycles/crop-cycle-form/crop-cycle-form.component.ts
import { Component, inject, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';
import { CropCycleService } from '../../services/crop-cycle.service';
import { CropCycle, CROP_TYPES, GROWTH_STAGES, CROP_STATUSES } from '../../models/crop-cycle.model';
import { DateAdapter, MAT_DATE_FORMATS, MAT_NATIVE_DATE_FORMATS, NativeDateAdapter } from '@angular/material/core';

interface DialogData {
  mode: 'create' | 'edit';
  fieldId: number;
  fieldName: string;
  cropCycle?: CropCycle;
}

@Component({
  selector: 'app-crop-cycle-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS }
  ],
  templateUrl: './crop-cycle-form.component.html'
})
export class CropCycleFormComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private cropCycleService = inject(CropCycleService);
  private dialogRef = inject(MatDialogRef<CropCycleFormComponent>);
  private snackBar = inject(MatSnackBar);
  
  dialogData = inject<DialogData>(MAT_DIALOG_DATA);

  // ✅ Signal for form
  cropCycleForm: FormGroup;
  
  // ✅ State signals
  isLoading = signal(false);
  isMobile = signal(false);

  // ✅ Constants
  cropTypes = CROP_TYPES;
  growthStages = GROWTH_STAGES;
  statuses = CROP_STATUSES;

  constructor() {
    this.isMobile.set(window.innerWidth < 640);

    this.cropCycleForm = this.fb.group({
      cropType: ['', Validators.required],
      plantingDate: ['', Validators.required],
      expectedHarvestDate: [''],
      growthStage: ['GERMINATION'],
      status: ['ACTIVE']
    });

    if (this.dialogData.mode === 'edit' && this.dialogData.cropCycle) {
      const cycle = this.dialogData.cropCycle;
      this.cropCycleForm.patchValue({
        cropType: cycle.cropType,
        plantingDate: new Date(cycle.plantingDate),
        expectedHarvestDate: cycle.expectedHarvestDate ? new Date(cycle.expectedHarvestDate) : '',
        growthStage: cycle.growthStage,
        status: cycle.status
      });
    }

    this.cropCycleForm.get('plantingDate')?.valueChanges.subscribe(() => {
      this.validateHarvestDate();
    });
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    this.isMobile.set(window.innerWidth < 640);
  }

  validateHarvestDate(): void {
    const plantingDate = this.cropCycleForm.get('plantingDate')?.value;
    const harvestDate = this.cropCycleForm.get('expectedHarvestDate')?.value;
    
    if (plantingDate && harvestDate && harvestDate <= plantingDate) {
      this.cropCycleForm.get('expectedHarvestDate')?.setErrors({ matDatepickerMin: true });
    } else if (plantingDate && harvestDate && harvestDate > plantingDate) {
      this.cropCycleForm.get('expectedHarvestDate')?.setErrors(null);
    }
  }

  getMinHarvestDate(): Date {
    const plantingDate = this.cropCycleForm.get('plantingDate')?.value;
    if (plantingDate) {
      const minDate = new Date(plantingDate);
      minDate.setDate(minDate.getDate() + 1);
      return minDate;
    }
    return new Date();
  }

  onSubmit(): void {
    if (this.cropCycleForm.invalid) {
      this.cropCycleForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading.set(false);
      return;
    }

    const formValue = this.cropCycleForm.value;
    
    const data: any = {
      fieldId: this.dialogData.fieldId,
      cropType: formValue.cropType,
      plantingDate: new Date(formValue.plantingDate).toISOString(),
      expectedHarvestDate: formValue.expectedHarvestDate ? new Date(formValue.expectedHarvestDate).toISOString() : null,
      growthStage: formValue.growthStage,
      status: formValue.status
    };

    let request;
    if (this.dialogData.mode === 'create') {
      request = this.cropCycleService.createCropCycle(farmId, data);
    } else {
      request = this.cropCycleService.updateCropCycle(farmId, this.dialogData.cropCycle!.id, data);
    }

    request
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.dialogRef.close(true);
          } else {
            this.snackBar.open(response.message || 'Operation failed', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Error saving crop cycle:', error);
          this.snackBar.open('Failed to save crop cycle', 'Close', {
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
    const control = this.cropCycleForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}