import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AdminHarvestStateService } from './services/admin-harvest-state.service';
import { HarvestListComponent } from './harvest-list/harvest-list.component';

@Component({
  selector: 'app-harvests',
  standalone: true,
  imports: [CommonModule, MatIconModule, HarvestListComponent],
  templateUrl: './harvests.component.html',
  styleUrl: './harvests.component.scss',
})
export class Harvests {
  private harvestState = inject(AdminHarvestStateService);
  
  statistics = this.harvestState.statistics;
}
