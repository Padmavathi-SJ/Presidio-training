import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SensorService } from '../../services/sensor.service';
import { AuthService } from '../../../../core/services/auth.service';
import { AlertThreshold, SENSOR_TYPES, SENSOR_TYPE_LABELS } from '../../models/sensor.model';
import { CROP_TYPES, GROWTH_STAGES } from '../../models/crop-cycle.model';

@Component({
  selector: 'app-alert-threshold-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSlideToggleModule
  ],
  templateUrl: './alert-threshold-dialog.component.html',
  styleUrl: './alert-threshold-dialog.component.scss'
})
export class AlertThresholdDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private sensorService = inject(SensorService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  
  form: FormGroup;
  isEditMode = false;
  sensorTypes = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  cropTypes = CROP_TYPES;
  growthStages = GROWTH_STAGES;
  
  severities = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

  constructor(
    public dialogRef: MatDialogRef<AlertThresholdDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { threshold?: AlertThreshold }
  ) {
    this.isEditMode = !!data.threshold;
    
    this.form = this.fb.group({
      cropType: [data.threshold?.cropType || '', Validators.required],
      growthStage: [data.threshold?.growthStage || '', Validators.required],
      sensorType: [data.threshold?.sensorType || '', Validators.required],
      minValue: [data.threshold?.minValue || 0, Validators.required],
      maxValue: [data.threshold?.maxValue || 0, Validators.required],
      severity: [data.threshold?.severity || 'MEDIUM', Validators.required],
      notificationEmails: [data.threshold?.notificationEmails || ''],
      isActive: [data.threshold?.isActive ?? true]
    });

    if (this.isEditMode) {
      // Disable keys for editing
      this.form.get('cropType')?.disable();
      this.form.get('growthStage')?.disable();
      this.form.get('sensorType')?.disable();
    }
  }

  ngOnInit(): void {}

  onSubmit() {
    if (this.form.invalid) return;

    const farmId = this.authService.getFarmId() || 0;
    const formData = this.form.getRawValue();

    if (this.isEditMode) {
      const updateData = {
        minValue: formData.minValue,
        maxValue: formData.maxValue,
        severity: formData.severity,
        isActive: formData.isActive,
        notificationEmails: formData.notificationEmails
      };

      this.sensorService.updateAlertThreshold(farmId, this.data.threshold!.id, updateData).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Threshold updated', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Failed to update threshold', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.sensorService.createAlertThreshold(farmId, formData).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Threshold created', 'Close', { duration: 3000 });
            this.dialogRef.close(true);
          }
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Failed to create threshold', 'Close', { duration: 3000 });
        }
      });
    }
  }
}
