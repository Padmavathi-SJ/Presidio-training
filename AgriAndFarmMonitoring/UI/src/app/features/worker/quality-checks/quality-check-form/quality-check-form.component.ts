import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';

import { WorkerQualityCheckService } from '../services/worker-quality-check.service';
import { QualityCheckDto } from '../models/worker-quality-check.model';
import { WorkerHarvestService } from '../../services/worker-harvest.service';

@Component({
  selector: 'app-quality-check-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule
  ],
  templateUrl: './quality-check-form.component.html',
  styleUrl: './quality-check-form.component.scss'
})
export class QualityCheckFormComponent implements OnInit {
  form!: FormGroup;
  isSubmitting = signal(false);
  isEditMode = false;
  harvests = signal<any[]>([]);
  grades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];

  constructor(
    private fb: FormBuilder,
    private service: WorkerQualityCheckService,
    private harvestService: WorkerHarvestService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<QualityCheckFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { editingId: number | null, editData: QualityCheckDto | null }
  ) {}

  ngOnInit() {
    this.isEditMode = !!this.data.editingId;
    this.initForm();
    this.loadHarvests();
    
    if (this.isEditMode && this.data.editData) {
      this.form.patchValue({
        harvestId: this.data.editData.harvestId,
        checkDate: this.data.editData.checkDate ? new Date(this.data.editData.checkDate).toISOString().split('T')[0] : '',
        moisturePct: this.data.editData.moisturePct,
        defectPct: this.data.editData.defectPct,
        finalGrade: this.data.editData.finalGrade,
        notes: this.data.editData.notes
      });
      this.form.get('harvestId')?.disable();
    }
  }

  initForm() {
    this.form = this.fb.group({
      harvestId: [null, Validators.required],
      checkDate: [new Date().toISOString().split('T')[0], Validators.required],
      moisturePct: [null, [Validators.min(0), Validators.max(100)]],
      defectPct: [null, [Validators.min(0), Validators.max(100)]],
      finalGrade: [null],
      notes: [null]
    });
  }

  loadHarvests() {
    this.harvestService.getMyHarvests({ page: 1, pageSize: 50, isDescending: true }).subscribe({
      next: (res: any) => { if (res.success) this.harvests.set(res.data.items); }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.isSubmitting.set(true);

    const formData = this.form.getRawValue();
    const request$ = this.isEditMode
      ? this.service.update(this.data.editingId!, formData)
      : this.service.create(formData);

    request$.subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open(`Quality check ${this.isEditMode ? 'updated' : 'created'} successfully`, 'Close', { duration: 3000 });
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
