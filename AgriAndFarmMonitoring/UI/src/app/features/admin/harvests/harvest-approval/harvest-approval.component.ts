import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminHarvestService } from '../services/admin-harvest.service';
import { HarvestApprovalDto } from '../models/admin-harvest.model';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-harvest-approval',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule, MatInputModule,
    MatIconModule, MatButtonModule
  ],
  templateUrl: './harvest-approval.component.html'
})
export class HarvestApprovalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<HarvestApprovalComponent>);
  private data = inject(MAT_DIALOG_DATA);
  private harvestService = inject(AdminHarvestService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  approvalForm!: FormGroup;
  isSaving = false;
  harvestId: number = this.data.harvestId;
  farmId = this.authService.getFarmId();

  ngOnInit() {
    this.approvalForm = this.fb.group({
      approvalStatus: ['', Validators.required],
      adminNotes: [''],
      rejectionReason: ['']
    });

    this.approvalForm.get('approvalStatus')?.valueChanges.subscribe(status => {
      const rejectionReason = this.approvalForm.get('rejectionReason');
      if (status === 'REJECTED') {
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
    const dto: HarvestApprovalDto = this.approvalForm.value;

    this.harvestService.approveHarvest(this.farmId, this.harvestId, dto).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.snackBar.open('Harvest reviewed successfully', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        } else {
          this.snackBar.open(res.message || 'Failed to review harvest', 'Close', { duration: 3000 });
        }
        this.isSaving = false;
      },
      error: (err: any) => {
        this.snackBar.open('Failed to review harvest', 'Close', { duration: 3000 });
        this.isSaving = false;
      }
    });
  }
}
