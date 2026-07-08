import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SensorService } from '../../services/sensor.service';
import { AuthService } from '../../../../core/services/auth.service';
import { AlertThreshold } from '../../models/sensor.model';
import { AlertThresholdDialogComponent } from '../alert-threshold-dialog/alert-threshold-dialog.component';

@Component({
  selector: 'app-alert-thresholds',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatSlideToggleModule
  ],
  templateUrl: './alert-thresholds.component.html',
  styleUrl: './alert-thresholds.component.scss'
})
export class AlertThresholdsComponent implements OnInit {
  private sensorService = inject(SensorService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  farmId = computed(() => this.authService.getFarmId() || 0);

  thresholds = signal<AlertThreshold[]>([]);
  isLoading = signal(false);

  displayedColumns = [
    'cropType', 
    'growthStage', 
    'sensorType', 
    'range', 
    'severity', 
    'status', 
    'actions'
  ];

  ngOnInit() {
    this.loadThresholds();
  }

  loadThresholds() {
    if (!this.farmId()) return;
    
    this.isLoading.set(true);
    this.sensorService.getAlertThresholds(this.farmId()).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.thresholds.set(res.data);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load thresholds', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  openThresholdDialog(threshold?: AlertThreshold) {
    const dialogRef = this.dialog.open(AlertThresholdDialogComponent, {
      width: '600px',
      data: { threshold }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadThresholds();
      }
    });
  }

  toggleActive(threshold: AlertThreshold) {
    const updatedStatus = !threshold.isActive;
    this.sensorService.updateAlertThreshold(this.farmId(), threshold.id, { isActive: updatedStatus }).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open(`Threshold ${updatedStatus ? 'activated' : 'deactivated'}`, 'Close', { duration: 3000 });
          this.thresholds.update(list => list.map(t => t.id === threshold.id ? { ...t, isActive: updatedStatus } : t));
        }
      },
      error: () => {
        this.snackBar.open('Failed to update status', 'Close', { duration: 3000 });
        // Revert toggle visually by reloading
        this.loadThresholds();
      }
    });
  }

  deleteThreshold(threshold: AlertThreshold) {
    if (confirm(`Are you sure you want to delete this threshold for ${threshold.cropType}?`)) {
      this.sensorService.deleteAlertThreshold(this.farmId(), threshold.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Threshold deleted', 'Close', { duration: 3000 });
            this.loadThresholds();
          }
        },
        error: () => {
          this.snackBar.open('Failed to delete threshold', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getSeverityColor(severity: string): string {
    switch (severity.toUpperCase()) {
      case 'CRITICAL': return 'warn';
      case 'HIGH': return 'accent';
      case 'MEDIUM': return 'primary';
      default: return 'default';
    }
  }
}
