import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { WorkerYieldReportService, YieldReportDto } from '../services/worker-yield-report.service';
import { WorkerGenerateReportComponent } from './worker-generate-report/worker-generate-report.component';
import { provideNativeDateAdapter } from '@angular/material/core';
import { WorkerFieldService } from '../services/worker-field.service';
// Note: We might need a dialog component to actually generate the report, but for now we'll just check if they can generate

@Component({
  selector: 'app-yield-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './yield-reports.component.html',
  styleUrls: ['./yield-reports.component.scss']
})
export class YieldReports implements OnInit {
  private reportService = inject(WorkerYieldReportService);
  private fieldService = inject(WorkerFieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  reports = signal<YieldReportDto[]>([]);
  isLoading = signal<boolean>(false);
  hasAssignedFields = signal<boolean>(false);

  displayedColumns: string[] = ['reportName', 'dateRange', 'totalYieldKg', 'totalValue', 'exportedAt', 'actions'];

  ngOnInit(): void {
    this.checkAssignedFields();
    this.loadReports();
  }

  checkAssignedFields(): void {
    this.fieldService.getMyAssignedFields()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response: any) => {
          this.hasAssignedFields.set(response.data && response.data.length > 0);
        },
        error: (err: any) => console.error('Failed to check fields', err)
      });
  }

  loadReports(): void {
    this.isLoading.set(true);
    this.reportService.getReports({ pageNumber: 1, pageSize: 100 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.reports.set(response.data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load reports', err);
          this.isLoading.set(false);
          this.snackBar.open('Failed to load reports', 'Close', { duration: 3000 });
        }
      });
  }

  generateReport(): void {
    if (!this.hasAssignedFields()) {
      this.snackBar.open('You need assigned fields to generate reports.', 'Close', { duration: 3000 });
      return;
    }
    const dialogRef = this.dialog.open(WorkerGenerateReportComponent, {
      width: '600px',
      disableClose: true,
      panelClass: 'professional-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Report generated successfully', 'Close', { duration: 3000 });
        this.loadReports();
      }
    });
  }

  downloadReport(report: YieldReportDto): void {
    // Actually call export API and trigger download
    this.snackBar.open('Downloading...', 'Close', { duration: 2000 });
    this.reportService.exportReport(report.id, 'csv')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          // Normally would handle blob download here
          this.snackBar.open('Export triggered.', 'Close', { duration: 3000 });
        },
        error: () => this.snackBar.open('Failed to export', 'Close', { duration: 3000 })
      });
  }
}
