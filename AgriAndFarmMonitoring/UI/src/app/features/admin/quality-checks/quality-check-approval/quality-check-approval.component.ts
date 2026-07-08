import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminQualityCheckService } from '../services/admin-quality-check.service';
import { QualityCheckApprovalDto } from '../models/admin-quality-check.model';

@Component({
  selector: 'app-quality-check-approval',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule, MatInputModule,
    MatIconModule, MatButtonModule, MatDialogModule
  ],
  templateUrl: './quality-check-approval.component.html'
})
export class QualityCheckApprovalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<QualityCheckApprovalComponent>);
  public data = inject(MAT_DIALOG_DATA);
  private service = inject(AdminQualityCheckService);
  private snackBar = inject(MatSnackBar);

  approvalForm!: FormGroup;
  isSaving = false;
  checkId: number = this.data.checkId;

  ngOnInit() {
    this.approvalForm = this.fb.group({
      actionType: ['', Validators.required],
      adminNotes: [''],
      rejectionReason: ['']
    });

    this.approvalForm.get('actionType')?.valueChanges.subscribe(action => {
      const rejectionReason = this.approvalForm.get('rejectionReason');
      if (action === 'REJECTED') {
        rejectionReason?.setValidators([Validators.required]);
      } else {
        rejectionReason?.clearValidators();
      }
      rejectionReason?.updateValueAndValidity();
    });
  }

  closeDialog() {
    this.dialogRef.close();
  }

  save() {
    if (this.approvalForm.invalid) return;

    this.isSaving = true;
    const formVal = this.approvalForm.value;
    const dto: QualityCheckApprovalDto = {
      approvalStatus: formVal.actionType,
      adminNotes: formVal.adminNotes,
      rejectionReason: formVal.actionType === 'REJECTED' ? formVal.rejectionReason : null
    };

    this.service.approve(this.checkId, dto).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.snackBar.open('Quality check reviewed successfully', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        } else {
          this.snackBar.open(res.message || 'Failed to review', 'Close', { duration: 3000 });
        }
        this.isSaving = false;
      },
      error: (err: any) => {
        this.snackBar.open('Failed to review quality check', 'Close', { duration: 3000 });
        this.isSaving = false;
      }
    });
  }
}
