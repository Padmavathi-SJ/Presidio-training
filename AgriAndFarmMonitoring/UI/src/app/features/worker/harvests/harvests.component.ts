// src/app/features/worker/harvests/harvests.component.ts
import { Component, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { HarvestListComponent } from './harvest-list/harvest-list.component';
import { HarvestFormComponent } from './harvest-form/harvest-form.component';
import { HarvestDetailsComponent } from './harvest-details/harvest-details.component';
import { HarvestStateService } from '../services/harvest-state.service';
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
  `
})
export class HarvestsComponent {
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private harvestState = inject(HarvestStateService);

  private formDialogRef: any = null;

  constructor() {
    // ✅ Auto-refresh when state changes
    effect(() => {
      // The list will auto-update via signals
    });
  }

  openCreateForm(): void {
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
        console.log('✅ Harvest created, list auto-updated via signals');
      }
    });
  }

  openEditForm(harvest: HarvestDto): void {
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
        console.log('✅ Harvest updated, list auto-updated via signals');
      }
    });
  }

  viewDetails(harvest: HarvestDto): void {
    this.harvestState.selectHarvest(harvest);
    
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
      this.harvestState.clearSelection();
    });
  }

  openRespondDialog(harvest: HarvestDto): void {
    this.snackBar.open('Respond to Admin feature coming soon', 'Close', { duration: 3000 });
  }

  deleteHarvest(id: number): void {
    if (confirm('Are you sure you want to delete this harvest?')) {
      this.harvestState.deleteHarvest(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Harvest deleted successfully', 'Close', { duration: 3000 });
          }
        },
        error: (err: any) => this.snackBar.open(err.error?.message || 'Failed to delete harvest', 'Close', { duration: 5000 })
      });
    }
  }
}