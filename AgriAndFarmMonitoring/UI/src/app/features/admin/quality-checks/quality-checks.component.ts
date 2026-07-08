import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AdminQualityCheckStateService } from './services/admin-quality-check-state.service';
import { QualityCheckListComponent } from './quality-check-list/quality-check-list.component';

@Component({
  selector: 'app-quality-checks',
  standalone: true,
  imports: [CommonModule, MatIconModule, QualityCheckListComponent],
  templateUrl: './quality-checks.component.html',
  styleUrl: './quality-checks.component.scss',
})
export class QualityChecksComponent implements OnInit {
  private qualityState = inject(AdminQualityCheckStateService);
  
  statistics = this.qualityState.statistics;

  ngOnInit() {
    this.qualityState.loadStatistics();
  }
}
