import { Component, Inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { WorkerQualityCheckService } from '../services/worker-quality-check.service';
import { QualityCheckDto } from '../models/worker-quality-check.model';

@Component({
  selector: 'app-quality-check-respond',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './quality-check-respond.component.html',
  styleUrl: './quality-check-respond.component.scss'
})
export class QualityCheckRespondComponent {
  respondForm: FormGroup;
  isSubmitting = signal(false);

  constructor(
    private fb: FormBuilder,
    private service: WorkerQualityCheckService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<QualityCheckRespondComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { check: QualityCheckDto }
  ) {
    this.respondForm = this.fb.group({
      responseNotes: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  closeDialog() { this.dialogRef.close(); }

  save() {
    if (this.respondForm.invalid) return;
    this.isSubmitting.set(true);

    this.service.respondToAdmin(this.data.check.id, this.respondForm.getRawValue()).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Response submitted successfully', 'Close', { duration: 3000 });
          this.dialogRef.close({ saved: true });
        }
        this.isSubmitting.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Operation failed', 'Close', { duration: 5000 });
        this.isSubmitting.set(false);
      }
    });
  }
}
