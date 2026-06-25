// src/app/shared/components/confirm-dialog/confirm-dialog.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'info' | 'warning' | 'danger';
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-start gap-3">
        <div 
          class="p-2 rounded-full flex-shrink-0 mt-1"
          [class]="{
            'bg-blue-100 text-blue-600': data.type === 'info',
            'bg-yellow-100 text-yellow-600': data.type === 'warning',
            'bg-red-100 text-red-600': data.type === 'danger'
          }"
        >
          <mat-icon>
            {{ data.type === 'info' ? 'info' : data.type === 'warning' ? 'warning' : 'error' }}
          </mat-icon>
        </div>
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-800">{{ data.title }}</h3>
          <p class="text-gray-600 mt-1">{{ data.message }}</p>
        </div>
      </div>

      <div class="flex justify-end gap-2 mt-6 pt-4 border-t border-gray-200">
        <button mat-button (click)="onCancel()">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button 
          mat-raised-button 
          (click)="onConfirm()"
          [class]="{
            'bg-blue-600 text-white': data.type === 'info',
            'bg-yellow-600 text-white': data.type === 'warning',
            'bg-red-600 text-white': data.type === 'danger'
          }"
        >
          {{ data.confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
  `
})
export class ConfirmDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  
  // ✅ Make it public (remove private)
  data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}