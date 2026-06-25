// src/app/features/worker/dashboard/dashboard.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-worker-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  private authService = inject(AuthService);
  
  currentDate = new Date();
  userName = this.authService.getCurrentUser()?.name || 'Worker';
  farmName = this.authService.getCurrentUser()?.farmName || 'Your Farm';

  stats = {
    assignedTasks: 5,
    pendingApprovals: 2,
    activeAlerts: 1,
    observationsToday: 3
  };
}