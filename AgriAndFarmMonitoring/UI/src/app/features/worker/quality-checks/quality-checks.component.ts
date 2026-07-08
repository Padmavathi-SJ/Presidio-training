import { Component, effect, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { QualityCheckListComponent } from './quality-check-list/quality-check-list.component';
import { QualityCheckFormComponent } from './quality-check-form/quality-check-form.component';
import { QualityCheckDetailsComponent } from './quality-check-details/quality-check-details.component';
import { QualityCheckRespondComponent } from './quality-check-respond/quality-check-respond.component';
import { WorkerQualityCheckStateService } from './services/worker-quality-check-state.service';
import { WorkerQualityCheckService } from './services/worker-quality-check.service';
import { QualityCheckDto } from './models/worker-quality-check.model';

@Component({
  selector: 'app-quality-checks',
  standalone: true,
  imports: [
    CommonModule,
    QualityCheckListComponent,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './quality-checks.component.html',
  styleUrl: './quality-checks.component.scss'
})
export class QualityChecksComponent implements OnInit {
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  public state = inject(WorkerQualityCheckStateService);
  private service = inject(WorkerQualityCheckService);

  private formDialogRef: any = null;

  constructor() {
    effect(() => {
    });
  }

  ngOnInit() {
    this.state.loadStatistics();
  }

  openCreateForm(): void {
    this.formDialogRef = this.dialog.open(QualityCheckFormComponent, {
      width: '620px',
      maxHeight: '90vh',
      disableClose: true,
      data: { editingId: null, editData: null }
    });

    this.formDialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        this.state.refresh();
      }
    });
  }

  openEditForm(check: QualityCheckDto): void {
    this.formDialogRef = this.dialog.open(QualityCheckFormComponent, {
      width: '620px',
      maxHeight: '90vh',
      disableClose: true,
      data: { editingId: check.id, editData: check }
    });

    this.formDialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        this.state.refresh();
      }
    });
  }

  viewDetails(check: QualityCheckDto): void {
    const dialogRef = this.dialog.open(QualityCheckDetailsComponent, {
      width: '560px',
      maxHeight: '90vh',
      data: { check }
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.action === 'edit' && result.check) {
        this.openEditForm(result.check);
      } else if (result?.action === 'respond' && result.check) {
        this.openRespondDialog(result.check);
      }
    });
  }

  openRespondDialog(check: QualityCheckDto): void {
    const dialogRef = this.dialog.open(QualityCheckRespondComponent, {
      width: '500px',
      maxHeight: '90vh',
      disableClose: true,
      data: { check }
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        this.state.refresh();
      }
    });
  }

  deleteCheck(id: number): void {
    if (confirm('Are you sure you want to delete this quality check?')) {
      this.service.delete(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Quality check deleted successfully', 'Close', { duration: 3000 });
            this.state.refresh();
          }
        },
        error: (err: any) => this.snackBar.open(err.error?.message || 'Failed to delete check', 'Close', { duration: 5000 })
      });
    }
  }
}
