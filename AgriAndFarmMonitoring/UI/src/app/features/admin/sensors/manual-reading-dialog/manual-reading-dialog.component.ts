import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SensorService } from '../../services/sensor.service';
import { FieldService } from '../../services/field.service';
import { CropCycleService } from '../../services/crop-cycle.service';
import { AuthService } from '../../../../core/services/auth.service';
import { SENSOR_TYPES, SENSOR_TYPE_LABELS, SENSOR_TYPE_UNITS } from '../../models/sensor.model';
import { Field } from '../../models/field.model';
import { CropCycle } from '../../models/crop-cycle.model';

@Component({
  selector: 'app-manual-reading-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './manual-reading-dialog.component.html',
  styleUrl: './manual-reading-dialog.component.scss'
})
export class ManualReadingDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private sensorService = inject(SensorService);
  private fieldService = inject(FieldService);
  private cropCycleService = inject(CropCycleService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  
  form: FormGroup;
  sensorTypes = SENSOR_TYPES;
  sensorTypeLabels = SENSOR_TYPE_LABELS;
  sensorTypeUnits = SENSOR_TYPE_UNITS;

  fields = signal<Field[]>([]);
  cropCycles = signal<CropCycle[]>([]);

  constructor(public dialogRef: MatDialogRef<ManualReadingDialogComponent>) {
    this.form = this.fb.group({
      fieldId: [null, Validators.required],
      cropCycleId: [{ value: null, disabled: true }, Validators.required],
      sensorType: ['', Validators.required],
      value: [null, Validators.required],
      unit: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const farmId = this.authService.getFarmId() || 0;
    
    // Load Fields
    this.fieldService.getFields(farmId, {}).subscribe(res => {
      if (res.success && res.data) {
        this.fields.set(res.data.items);
      }
    });

    // Auto-update Unit based on SensorType
    this.form.get('sensorType')?.valueChanges.subscribe(type => {
      if (type) {
        this.form.patchValue({ unit: this.sensorTypeUnits[type] || '' });
      }
    });

    // Load CropCycles when Field changes
    this.form.get('fieldId')?.valueChanges.subscribe(fieldId => {
      if (fieldId) {
        this.form.get('cropCycleId')?.enable();
        this.form.patchValue({ cropCycleId: null });
        this.cropCycleService.getCropCycles(farmId, { fieldId: fieldId }).subscribe(res => {
          if (res.success && res.data) {
            // Only active crop cycles usually? Or all? Let's just set all returned.
            this.cropCycles.set(res.data.items);
          }
        });
      } else {
        this.form.get('cropCycleId')?.disable();
        this.cropCycles.set([]);
      }
    });
  }

  onSubmit() {
    if (this.form.invalid) return;

    const farmId = this.authService.getFarmId() || 0;
    const formData = this.form.getRawValue();

    this.sensorService.addManualReading(farmId, formData).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Manual reading inserted successfully', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to insert reading', 'Close', { duration: 3000 });
      }
    });
  }
}
