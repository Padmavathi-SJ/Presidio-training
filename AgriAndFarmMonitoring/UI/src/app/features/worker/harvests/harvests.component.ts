// src/app/features/worker/harvests/harvests.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { HarvestListComponent } from './harvest-list/harvest-list.component';
import { HarvestFormComponent } from './harvest-form/harvest-form.component';
import { HarvestDetailsComponent } from './harvest-details/harvest-details.component';
import { WorkerHarvestService } from '../services/worker-harvest.service';
import { HarvestDto } from '../models/worker-harvest.model';

@Component({
  selector: 'app-harvests',
  standalone: true,
  imports: [
    CommonModule,
    HarvestListComponent,
    HarvestFormComponent,
    HarvestDetailsComponent
  ],
  template: `
    <app-harvest-list
      (createHarvest)="openCreateForm()"
      (editHarvest)="openEditForm($event)"
      (viewHarvest)="viewDetails($event)"
      (respondHarvest)="openRespondDialog($event)"
      (deleteHarvest)="deleteHarvest($event)">
    </app-harvest-list>

    <!-- ✅ HarvestFormComponent is now only used as a dialog, not rendered here -->
  `
})
export class HarvestsComponent {
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private harvestService = inject(WorkerHarvestService);

  private formDialogRef: any = null;

  openCreateForm(): void {
    // ✅ Open the form as a dialog
    this.formDialogRef = this.dialog.open(HarvestFormComponent, {
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
        this.onFormSaved();
      }
    });
  }

  openEditForm(harvest: HarvestDto): void {
    console.log('📝 Opening edit form with data:', harvest);
    
    // ✅ Open the form as a dialog with edit data
    this.formDialogRef = this.dialog.open(HarvestFormComponent, {
      width: '620px',
      maxHeight: '90vh',
      disableClose: true,
      data: {
        editingId: harvest.id,
        editData: harvest
      }
    });

    this.formDialogRef.afterClosed().subscribe((result: any) => {
      if (result?.saved) {
        this.onFormSaved();
      }
    });
  }

  viewDetails(harvest: HarvestDto): void {
    const dialogRef = this.dialog.open(HarvestDetailsComponent, {
      width: '560px',
      maxHeight: '90vh',
      data: { harvest: harvest },
      panelClass: 'harvest-details-dialog'
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result?.action === 'edit' && result.harvest) {
        this.openEditForm(result.harvest);
      } else if (result?.action === 'respond' && result.harvest) {
        this.openRespondDialog(result.harvest);
      }
    });
  }

  openRespondDialog(harvest: HarvestDto): void {
    this.snackBar.open('Respond to Admin feature coming soon', 'Close', { duration: 3000 });
  }

  deleteHarvest(id: number): void {
    if (confirm('Are you sure you want to delete this harvest?')) {
      this.harvestService.deleteHarvest(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Harvest deleted successfully', 'Close', { duration: 3000 });
          }
        },
        error: (err) => this.snackBar.open(err.error?.message || 'Failed to delete harvest', 'Close', { duration: 5000 })
      });
    }
  }

  onFormSaved(): void {
    console.log('✅ Form saved, refreshing list...');
    // The list will refresh when the dialog closes
  }
}