import { Component, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ObservationDto } from '../../models/worker-observation.model';

@Component({
  selector: 'app-observation-details',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatButtonModule
  ],
  templateUrl: './observation-details.html',
  styleUrls: ['./observation-details.scss']
})
export class ObservationDetailsComponent {
  private dialogRef = inject(MatDialogRef<ObservationDetailsComponent>);
  private data = inject(MAT_DIALOG_DATA);
  
  observation: ObservationDto = this.data?.observation;

  // Helper methods for template
  getValidationBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'VALIDATED':
      case 'APPROVED': return 'bg-emerald-100 text-emerald-800';
      case 'INVALID':
      case 'REJECTED': return 'bg-red-100 text-red-800';
      case 'QUESTIONED':
      case 'REQUEST_CHANGES': return 'bg-amber-100 text-amber-800';
      default: return 'bg-amber-100 text-amber-800';
    }
  }

  getValidationIcon(status: string): string {
    switch (status?.toUpperCase()) {
      case 'VALIDATED':
      case 'APPROVED': return 'check_circle';
      case 'INVALID':
      case 'REJECTED': return 'cancel';
      case 'QUESTIONED':
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'hourglass_empty';
    }
  }

  formatStatus(status: string | undefined): string {
    if (!status) return 'Pending';
    return status.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  formatCropHealth(health: string | undefined): string {
    if (!health) return '—';
    return health.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  canEdit(): boolean {
    return !!this.observation && (this.observation.validationStatus === 'PENDING' || this.observation.validationStatus === 'QUESTIONED');
  }

  canRespond(): boolean {
    return !!this.observation && this.observation.validationStatus === 'QUESTIONED';
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  editObservation(): void {
    this.dialogRef.close({ action: 'edit', observation: this.observation });
  }

  respondToAdmin(): void {
    this.dialogRef.close({ action: 'respond', observation: this.observation });
  }
}
