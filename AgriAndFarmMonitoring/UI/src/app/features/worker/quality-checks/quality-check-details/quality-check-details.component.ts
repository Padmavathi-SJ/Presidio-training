import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { QualityCheckDto } from '../models/worker-quality-check.model';

@Component({
  selector: 'app-quality-check-details',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './quality-check-details.component.html',
  styleUrl: './quality-check-details.component.scss'
})
export class QualityCheckDetailsComponent {
  check: QualityCheckDto;

  constructor(
    public dialogRef: MatDialogRef<QualityCheckDetailsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { check: QualityCheckDto }
  ) {
    this.check = data.check;
  }

  closeDialog() { this.dialogRef.close(); }
  editCheck() { this.dialogRef.close({ action: 'edit', check: this.check }); }
  respondToAdmin() { this.dialogRef.close({ action: 'respond', check: this.check }); }

  canEdit(): boolean {
    return this.check.approvalStatus === 'PENDING' || this.check.approvalStatus === 'REQUEST_CHANGES';
  }
  canRespond(): boolean {
    return this.check.approvalStatus === 'REQUEST_CHANGES' || this.check.approvalStatus === 'REJECTED';
  }
  getApprovalIcon(status: string): string {
    switch (status) {
      case 'PENDING': return 'hourglass_empty';
      case 'APPROVED': return 'check_circle';
      case 'REJECTED': return 'cancel';
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'help';
    }
  }
}
