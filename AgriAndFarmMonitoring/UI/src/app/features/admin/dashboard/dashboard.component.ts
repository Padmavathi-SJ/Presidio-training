// src/app/features/admin/dashboard/dashboard.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatDividerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  
  isLoading = true;
  currentDate = new Date();
  userName = '';
  farmName = '';

  // ✅ Empty stats - Ready for backend integration
  stats = {
    totalFields: 0,
    newFieldsThisMonth: 0,
    activeCropCycles: 0,
    cropsInHarvest: 0,
    criticalAlerts: 0,
    unresolvedAlerts: 0,
    todayTasks: 0,
    pendingTasks: 0,
    pendingApprovals: 0
  };

  // ✅ Empty activities - Ready for backend integration
  recentActivities: any[] = [];

  ngOnInit(): void {
    // ✅ Check authentication
    if (!this.authService.isLoggedIn()) {
      console.log('🔴 Not authenticated in dashboard');
      return;
    }

    // ✅ Get user data
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = user.name || 'Admin';
      this.farmName = user.farmName || 'Your Farm';
    }

    // ✅ Load dashboard data (replace with actual API call)
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    
    // ✅ TODO: Replace with actual API call to get dashboard stats
    // this.dashboardService.getStats().subscribe({
    //   next: (data) => {
    //     this.stats = data;
    //     this.isLoading = false;
    //   },
    //   error: (error) => {
    //     console.error('Failed to load dashboard data:', error);
    //     this.isLoading = false;
    //   }
    // });

    // ✅ Temporary: Simulate API call
    setTimeout(() => {
      this.isLoading = false;
      console.log('✅ Dashboard ready for backend integration');
    }, 500);
  }
}