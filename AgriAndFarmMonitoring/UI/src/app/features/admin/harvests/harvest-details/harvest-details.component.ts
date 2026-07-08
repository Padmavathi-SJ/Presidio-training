import { Component, inject } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { HarvestDto } from '../models/admin-harvest.model';

@Component({
  selector: 'app-harvest-details',
  standalone: true,
  imports: [
    CommonModule, DatePipe, CurrencyPipe, MatIconModule, MatChipsModule,
    MatTooltipModule, MatButtonModule
  ],
  templateUrl: './harvest-details.component.html'
})
export class HarvestDetailsComponent {
  private dialogRef = inject(MatDialogRef<HarvestDetailsComponent>);
  private data = inject(MAT_DIALOG_DATA);
  
  harvest: HarvestDto = this.data?.harvest;

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
    const m: any = { 'A_PLUS': 'A+', 'A': 'A', 'B': 'B', 'C': 'C', 'D': 'D', 'REJECTED': 'Rejected' };
    return m[grade] || grade.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  formatHarvestMethod(method: string | undefined): string {
    if (!method) return '—';
    return method.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
