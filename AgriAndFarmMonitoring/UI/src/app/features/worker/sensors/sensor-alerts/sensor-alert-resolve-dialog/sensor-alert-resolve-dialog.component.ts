import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Alert } from '../../../../admin/models/sensor.model';

@Component({
  selector: 'app-worker-sensor-alert-resolve-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './sensor-alert-resolve-dialog.component.html'
})
export class SensorAlertResolveDialogComponent {
  private fb = inject(FormBuilder);
  dialogRef = inject(MatDialogRef<SensorAlertResolveDialogComponent>);
  data = inject<{ alert: Alert }>(MAT_DIALOG_DATA);

  form: FormGroup = this.fb.group({
    resolutionNotes: ['', [Validators.required, Validators.maxLength(1000)]]
  });

  get alert() {
    return this.data.alert;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    } else {
      this.form.markAllAsTouched();
    }
  }
}
