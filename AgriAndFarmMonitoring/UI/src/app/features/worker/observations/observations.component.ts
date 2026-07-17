import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ObservationListComponent } from './observation-list/observation-list';
import { ObservationFormComponent } from './observation-form/observation-form';
import { ObservationDetailsComponent } from './observation-details/observation-details';
import { WorkerObservationStateService } from '../services/worker-observation-state.service';
import { ObservationDto } from '../models/worker-observation.model';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-observations',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    ObservationListComponent
  ],
  styleUrls: ['./observations.component.scss'],
  templateUrl: './observations.component.html'
})
export class ObservationsComponent {
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  public state = inject(WorkerObservationStateService);

  private formDialogRef: any = null;

  openCreateForm(): void {
    this.formDialogRef = this.dialog.open(ObservationFormComponent, {
      width: '620px',
      maxHeight: '90vh',
      disableClose: true,
      data: {
        editingId: null,
        editData: null
      }
    });

    this.formDialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        console.log('✅ Observation created, list auto-updated via signals');
      }
    });
  }

  openEditForm(observation: ObservationDto): void {
    this.formDialogRef = this.dialog.open(ObservationFormComponent, {
      width: '620px',
      maxHeight: '90vh',
      disableClose: true,
      data: {
        editingId: observation.id,
        editData: observation
      }
    });

    this.formDialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        console.log('✅ Observation updated, list auto-updated via signals');
      }
    });
  }

  viewDetails(observation: ObservationDto): void {
    const dialogRef = this.dialog.open(ObservationDetailsComponent, {
      width: '560px',
      maxHeight: '90vh',
      data: { observation: observation },
      panelClass: 'observation-details-dialog'
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.action === 'edit' && result.observation) {
        this.openEditForm(result.observation);
      } else if (result?.action === 'respond' && result.observation) {
        this.openRespondDialog(result.observation);
      }
    });
  }

  openRespondDialog(observation: ObservationDto): void {
    this.snackBar.open('Respond to Admin feature coming soon', 'Close', { duration: 3000 });
  }

  deleteObservation(id: number): void {
    if (confirm('Are you sure you want to delete this observation?')) {
      this.state.deleteObservation(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation deleted successfully', 'Close', { duration: 3000 });
          }
        },
        error: (err: any) => this.snackBar.open(err.error?.message || 'Failed to delete observation', 'Close', { duration: 5000 })
      });
    }
  }
}