// src/app/features/admin/sensors/alert-detail-dialog/alert-detail-dialog.component.ts
import { Component, inject, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

import { Alert, ALERT_SEVERITY_COLORS, ALERT_SEVERITY_ICONS, SENSOR_TYPE_LABELS } from '../../models/sensor.model';

export interface AlertDetailDialogData {
  alert: Alert;
  mode: 'view';
}

@Component({
  selector: 'app-alert-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  template: `
    <div class="p-4 sm:p-6">
      <!-- Header -->
      <div class="flex items-start justify-between mb-4">
        <div>
          <h2 class="text-xl font-bold text-gray-800">Alert Details</h2>
          <p class="text-sm text-gray-500 mt-1">View complete information about this alert</p>
        </div>
        <button 
          mat-icon-button 
          (click)="dialogRef.close()"
          class="!w-8 !h-8 !text-gray-400 hover:!text-gray-600"
        >
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <!-- Alert Header -->
      <div class="flex items-center gap-3 mb-4 p-4 rounded-lg {{ getSeverityColor(data.alert.severity) }}">
        <mat-icon class="text-xl">{{ getSeverityIcon(data.alert.severity) }}</mat-icon>
        <div>
          <div class="flex items-center gap-3">
            <span class="text-sm font-semibold text-gray-800">{{ getAlertTypeLabel(data.alert.alertType) }}</span>
            <span class="text-xs px-2 py-0.5 rounded-full font-medium {{ getSeverityColor(data.alert.severity) }}">
              {{ data.alert.severity }}
            </span>
            @if (data.alert.isResolved) {
              <span class="text-xs px-2 py-0.5 rounded-full font-medium bg-green-100 text-green-700">
                <mat-icon class="text-sm">check_circle</mat-icon> Resolved
              </span>
            } @else {
              <span class="text-xs px-2 py-0.5 rounded-full font-medium bg-yellow-100 text-yellow-700">
                <mat-icon class="text-sm">pending</mat-icon> Pending
              </span>
            }
          </div>
          <p class="text-sm text-gray-600 mt-1">{{ data.alert.message || 'No message' }}</p>
        </div>
      </div>

      <!-- Alert Details Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div class="bg-gray-50 rounded-lg p-3">
          <div class="text-xs text-gray-500">Field</div>
          <div class="text-sm font-medium text-gray-800">{{ data.alert.fieldName }}</div>
        </div>
        <div class="bg-gray-50 rounded-lg p-3">
          <div class="text-xs text-gray-500">Crop Type</div>
          <div class="text-sm font-medium text-gray-800">{{ data.alert.cropType || 'N/A' }}</div>
        </div>
        <div class="bg-gray-50 rounded-lg p-3">
          <div class="text-xs text-gray-500">Sensor Value</div>
          <div class="text-sm font-medium text-gray-800">
            @if (data.alert.sensorValue !== null) {
              {{ data.alert.sensorValue }}
              @if (data.alert.thresholdValue !== null) {
                <span class="text-gray-400"> (Threshold: {{ data.alert.thresholdValue }})</span>
              }
            } @else {
              N/A
            }
          </div>
        </div>
        <div class="bg-gray-50 rounded-lg p-3">
          <div class="text-xs text-gray-500">Created At</div>
          <div class="text-sm font-medium text-gray-800">{{ formatDate(data.alert.createdAt) }}</div>
        </div>
        @if (data.alert.resolvedAt) {
          <div class="bg-gray-50 rounded-lg p-3 col-span-2">
            <div class="text-xs text-gray-500">Resolved At</div>
            <div class="text-sm font-medium text-gray-800">{{ formatDate(data.alert.resolvedAt) }}</div>
          </div>
        }
      </div>

      <!-- Actions -->
      <div class="flex justify-end gap-2 mt-6 pt-4 border-t border-gray-200">
        <button 
          mat-button 
          (click)="dialogRef.close()"
        >
          Close
        </button>
      </div>
    </div>
  `,
  styles: [`
    .bg-gray-50 {
      background-color: #f8f9fa;
    }
  `]
})
export class AlertDetailDialogComponent {
  data = inject<AlertDetailDialogData>(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<AlertDetailDialogComponent>);

  getSeverityColor(severity: string | null): string {
    if (!severity) return 'bg-gray-100 text-gray-700';
    return ALERT_SEVERITY_COLORS[severity] || 'bg-gray-100 text-gray-700';
  }

  getSeverityIcon(severity: string | null): string {
    if (!severity) return 'info';
    return ALERT_SEVERITY_ICONS[severity] || 'info';
  }

  getAlertTypeLabel(type: string | null): string {
    if (!type) return 'Unknown';
    return SENSOR_TYPE_LABELS[type] || type;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}