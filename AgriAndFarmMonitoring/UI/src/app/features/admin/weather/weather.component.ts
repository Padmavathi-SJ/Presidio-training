// src/app/features/admin/weather/weather.component.ts
import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { WeatherService } from '../services/weather.service';
import { WeatherSignalRService } from '../services/weather-signalr.service';
import { WeatherAlert } from '../models/weather.model';

@Component({
  selector: 'app-weather',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatIconModule,
    MatBadgeModule,
    MatTooltipModule
  ],
  template: `
    <div class="weather-module">
      <!-- Sub-header with tabs -->
      <div class="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div class="container-fluid px-4 sm:px-6">
          <nav mat-tab-nav-bar [tabPanel]="tabPanel" class="!border-b-0">
            <a mat-tab-link
               [routerLink]="'dashboard'"
               routerLinkActive #rla1="routerLinkActive"
               [active]="rla1.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">dashboard</mat-icon>
              Dashboard
            </a>
            <a mat-tab-link
               [routerLink]="'alerts'"
               routerLinkActive #rla2="routerLinkActive"
               [active]="rla2.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">notifications_active</mat-icon>
              Alerts
              @if (unreadAlertCount() > 0) {
                <span class="ml-1 bg-red-500 text-white text-xs rounded-full px-2 py-0.5 min-w-[20px] text-center">
                  {{ unreadAlertCount() }}
                </span>
              }
            </a>
            <a mat-tab-link
               [routerLink]="'history'"
               routerLinkActive #rla3="routerLinkActive"
               [active]="rla3.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">history</mat-icon>
              History
            </a>
            <!-- ✅ Settings tab removed -->
          </nav>
        </div>
      </div>

      <!-- Tab Content -->
      <mat-tab-nav-panel #tabPanel class="block">
        <router-outlet></router-outlet>
      </mat-tab-nav-panel>
    </div>
  `,
  styles: [`
    .weather-module {
      min-height: calc(100vh - 64px);
      background-color: #f8f9fa;
    }

    ::ng-deep {
      .mat-mdc-tab-nav-bar {
        .mat-mdc-tab-link {
          height: 56px;
          font-size: 14px;
          font-weight: 500;
          color: #6b7280;
          opacity: 1;
          min-width: auto;
          padding: 0 20px;

          &:hover {
            color: #2d6a4f;
          }

          &.mdc-tab--active {
            color: #2d6a4f;
            font-weight: 600;
          }

          .mat-icon {
            margin-right: 4px;
          }
        }

        .mdc-tab-indicator {
          .mdc-tab-indicator__content {
            border-color: #2d6a4f;
          }
        }
      }
    }

    @media (max-width: 640px) {
      ::ng-deep .mat-mdc-tab-nav-bar .mat-mdc-tab-link {
        padding: 0 12px;
        font-size: 12px;
        
        .mat-icon {
          font-size: 18px;
          width: 18px;
          height: 18px;
        }
      }
    }
  `]
})
export class WeatherComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private weatherService = inject(WeatherService);
  private weatherSignalR = inject(WeatherSignalRService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  private destroy$ = new Subject<void>();
  unreadAlertCount = signal(0);

  ngOnInit(): void {
    this.loadUnreadAlertCount();
    this.setupSignalR();
    this.trackNavigation();
  }

  private loadUnreadAlertCount(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.weatherService.getActiveWeatherAlerts(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            const unread = response.data?.filter((a: WeatherAlert) => !a.isAcknowledged) || [];
            this.unreadAlertCount.set(unread.length);
          }
        },
        error: (error: any) => console.error('Error loading alert count:', error)
      });
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Listen for new alerts to update the badge
    this.weatherSignalR.alertUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert: WeatherAlert | null) => {
        if (alert && !alert.isAcknowledged) {
          this.unreadAlertCount.update(count => count + 1);
          this.showAlertNotification(alert);
        }
      });

    // Listen for alert acknowledgments
    this.weatherSignalR.alertUpdate$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert: WeatherAlert | null) => {
        if (alert && alert.isAcknowledged) {
          this.unreadAlertCount.update(count => Math.max(0, count - 1));
        }
      });
  }

  private trackNavigation(): void {
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        // Handle navigation if needed
      });
  }

  private showAlertNotification(alert: WeatherAlert): void {
    const severityEmoji: Record<string, string> = {
      'ADVISORY': 'ℹ️',
      'WATCH': '👀',
      'WARNING': '⚠️',
      'EMERGENCY': '🚨'
    };

    const emoji = severityEmoji[alert.severity] || '🔔';
    const message = `${emoji} New Alert: ${alert.title}`;
    
    this.snackBar.open(message, 'View', {
      duration: 10000,
      panelClass: ['alert-snackbar', `alert-${alert.severity.toLowerCase()}`],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    }).onAction().subscribe(() => {
      this.router.navigate(['/admin/weather/alerts']);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}