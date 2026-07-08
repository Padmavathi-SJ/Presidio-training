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
import { AdminYieldReportService, YieldReportDto } from '../services/admin-yield-report.service';
import { GenerateReportComponent } from './generate-report/generate-report.component';
import { provideNativeDateAdapter } from '@angular/material/core';

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
export class YieldReportsComponent implements OnInit {
  private reportService = inject(AdminYieldReportService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  reports = signal<YieldReportDto[]>([]);
  isLoading = signal<boolean>(false);

  displayedColumns: string[] = ['reportName', 'dateRange', 'totalYieldKg', 'totalValue', 'exportedAt', 'actions'];

  ngOnInit(): void {
    this.loadReports();
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
    const dialogRef = this.dialog.open(GenerateReportComponent, {
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
    this.snackBar.open('Downloading...', 'Close', { duration: 2000 });
    this.reportService.exportReport(report.id, 'csv')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          this.snackBar.open('Export triggered.', 'Close', { duration: 3000 });
        },
        error: () => this.snackBar.open('Failed to export', 'Close', { duration: 3000 })
      });
  }
}
