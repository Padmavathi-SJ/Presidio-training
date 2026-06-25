// src/app/features/admin/admin-layout/admin-layout.component.ts
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule
  ],
  templateUrl: './admin-layout.component.html',
  // No styleUrls needed - using Tailwind
})
export class AdminLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  
  user = this.authService.getCurrentUser();
  isSidebarOpen = signal(true);
  currentRoute = signal('Dashboard');

  navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/admin/dashboard' },
    { label: 'Fields', icon: 'crop', route: '/admin/fields' },
    { label: 'Workers', icon: 'people', route: '/admin/workers' },
    { label: 'Worker Fields', icon: 'assignment_ind', route: '/admin/worker-fields' },
    { label: 'Observations', icon: 'visibility', route: '/admin/observations' },
    { label: 'Sensor Data', icon: 'sensors', route: '/admin/sensors' },
    { label: 'Weather', icon: 'cloud', route: '/admin/weather' },
    { label: 'Worker Tasks', icon: 'task', route: '/admin/tasks' },
    { label: 'Harvests', icon: 'inventory_2', route: '/admin/harvests' },
    { label: 'Quality Checks', icon: 'verified', route: '/admin/quality-checks' },
    { label: 'Yield Reports', icon: 'analytics', route: '/admin/yield-reports' }
  ];

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }
    
    if (!this.authService.isAdmin()) {
      this.router.navigate(['/unauthorized']);
      return;
    }
    
    this.user = this.authService.getCurrentUser();
  }

  toggleSidebar(): void {
    this.isSidebarOpen.update(value => !value);
  }

  logout(): void {
    this.authService.logout(true).subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.authService.forceLogout();
      }
    });
  }
}