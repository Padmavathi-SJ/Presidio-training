import { Component, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { QualityCheckDto } from '../models/admin-quality-check.model';

@Component({
  selector: 'app-quality-check-details',
  standalone: true,
  imports: [
    CommonModule, DatePipe, MatIconModule, MatChipsModule,
    MatButtonModule, MatDialogModule
  ],
  templateUrl: './quality-check-details.component.html'
})
export class QualityCheckDetailsComponent {
  private dialogRef = inject(MatDialogRef<QualityCheckDetailsComponent>);
  public data = inject(MAT_DIALOG_DATA);
  
  check: QualityCheckDto = this.data?.check;

  getApprovalIcon(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED': return 'check_circle';
      case 'REJECTED': return 'cancel';
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'hourglass_empty';
    }
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
