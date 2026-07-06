// src/app/features/worker/harvests/components/harvest-details/harvest-details.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { HarvestDto } from '../../models/worker-harvest.model';

@Component({
  selector: 'app-harvest-details',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    CurrencyPipe,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatButtonModule
  ],
  templateUrl: './harvest-details.component.html',
  styleUrls: ['./harvest-details.component.scss']
})
export class HarvestDetailsComponent {
  private dialogRef = inject(MatDialogRef<HarvestDetailsComponent>);
  private data = inject(MAT_DIALOG_DATA);
  
  harvest: HarvestDto = this.data?.harvest;

  // Helper methods for template
  getApprovalBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED': return 'bg-emerald-100 text-emerald-800';
      case 'REJECTED': return 'bg-red-100 text-red-800';
      case 'REQUEST_CHANGES': return 'bg-amber-100 text-amber-800';
      default: return 'bg-amber-100 text-amber-800';
    }
  }

  getApprovalIcon(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED': return 'check_circle';
      case 'REJECTED': return 'cancel';
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'hourglass_empty';
    }
  }

  getQualityClass(grade: string | undefined): string {
    switch (grade?.toUpperCase()) {
      case 'A_PLUS': return 'bg-amber-400 text-white';
      case 'A': return 'bg-emerald-600 text-white';
      case 'B': return 'bg-emerald-400 text-white';
      case 'C': return 'bg-amber-300 text-on-surface';
      case 'D': return 'bg-orange-400 text-white';
      case 'REJECTED': return 'bg-red-500 text-white';
      default: return 'bg-gray-200 text-on-surface';
    }
  }

  formatQualityGrade(grade: string | undefined): string {
    if (!grade) return '—';
    const displayMap: { [key: string]: string } = {
      'A_PLUS': 'A+',
      'A': 'A',
      'B': 'B',
      'C': 'C',
      'D': 'D',
      'REJECTED': 'Rejected'
    };
    return displayMap[grade] || grade.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  formatHarvestMethod(method: string | undefined): string {
    if (!method) return '—';
    return method.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  canEdit(): boolean {
    return !!this.harvest && (this.harvest.approvalStatus === 'PENDING' || this.harvest.approvalStatus === 'REQUEST_CHANGES');
  }

  canRespond(): boolean {
    return !!this.harvest && this.harvest.approvalStatus === 'REQUEST_CHANGES';
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  editHarvest(): void {
    this.dialogRef.close({ action: 'edit', harvest: this.harvest });
  }

  respondToAdmin(): void {
    this.dialogRef.close({ action: 'respond', harvest: this.harvest });
  }
}