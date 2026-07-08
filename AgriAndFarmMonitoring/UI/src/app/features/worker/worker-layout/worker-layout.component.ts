import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
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
  selector: 'app-worker-layout',
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
  templateUrl: './worker-layout.component.html',
  // No styleUrls needed - using Tailwind
})
export class WorkerLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  
  user = this.authService.getCurrentUser();
  isSidebarOpen = signal(true);
  currentRoute = signal('Dashboard');
  currentRouteIcon = signal('dashboard');

  navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/worker/dashboard' },
    { label: 'Fields', icon: 'crop', route: '/worker/fields' },
    { label: 'Observations', icon: 'visibility', route: '/worker/observations' },
    { label: 'Tasks', icon: 'assignment', route: '/worker/tasks' },
    { label: 'Weather', icon: 'cloud', route: '/worker/weather' },
    { label: 'Sensor Data', icon: 'sensors', route: '/worker/sensors' },
    { label: 'Harvests', icon: 'inventory_2', route: '/worker/harvests' },
    { label: 'Quality Checks', icon: 'verified', route: '/worker/quality-checks' },
    { label: 'Yield Reports', icon: 'analytics', route: '/worker/yield-reports' }
  ];

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }
    
    if (!this.authService.isWorker()) {
      this.router.navigate(['/unauthorized']);
      return;
    }
    
    this.user = this.authService.getCurrentUser();
    
    // Sync title on initial load and route changes
    this.syncRouteTitle(this.router.url);
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((event: any) => {
        this.syncRouteTitle(event.urlAfterRedirects);
      });
  }

  private syncRouteTitle(url: string): void {
    const matchingNav = this.navItems.find(item => url.includes(item.route));
    if (matchingNav) {
      this.currentRoute.set(matchingNav.label);
      this.currentRouteIcon.set(matchingNav.icon);
    } else if (url.includes('/worker/profile')) {
      this.currentRoute.set('Profile');
      this.currentRouteIcon.set('person');
    } else if (url.includes('/worker/settings')) {
      this.currentRoute.set('Settings');
      this.currentRouteIcon.set('settings');
    }
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