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
      <div class="weather-subnav">
        <nav mat-tab-nav-bar [tabPanel]="tabPanel" class="!border-b-0">
          <a mat-tab-link [routerLink]="'dashboard'" routerLinkActive #rla1="routerLinkActive" [active]="rla1.isActive">
            <mat-icon class="tab-icon">cloud</mat-icon>Dashboard
          </a>
          <a mat-tab-link [routerLink]="'alerts'" routerLinkActive #rla2="routerLinkActive" [active]="rla2.isActive">
            <mat-icon class="tab-icon">warning_amber</mat-icon>
            Alerts
            @if (unreadAlertCount() > 0) {
              <span class="alert-badge">{{ unreadAlertCount() }}</span>
            }
          </a>
          <a mat-tab-link [routerLink]="'history'" routerLinkActive #rla3="routerLinkActive" [active]="rla3.isActive">
            <mat-icon class="tab-icon">history</mat-icon>History
          </a>
        </nav>
      </div>
      <mat-tab-nav-panel #tabPanel class="block">
        <router-outlet></router-outlet>
      </mat-tab-nav-panel>
    </div>
  `,
  styles: [`
    .weather-module { min-height: calc(100vh - 58px); background: #f5f8f5; }
    .weather-subnav {
      background: white; border-bottom: 1.5px solid #d5e1da;
      padding: 0 16px; position: sticky; top: 0; z-index: 10;
      box-shadow: 0 1px 4px rgba(45,106,79,0.06);
    }
    .tab-icon { font-size: 16px; width: 16px; height: 16px; margin-right: 5px; vertical-align: middle; }
    .alert-badge {
      margin-left: 6px; background: #ef4444; color: white; font-size: 11px; font-weight: 700;
      border-radius: 9999px; padding: 1px 7px; min-width: 20px; text-align: center;
      display: inline-flex; align-items: center; justify-content: center;
    }
    ::ng-deep {
      .mat-mdc-tab-nav-bar .mat-mdc-tab-link {
        height: 48px; font-size: 13px; font-weight: 500; color: #5a7a6a; opacity: 1;
        min-width: auto; padding: 0 16px; transition: color 0.15s ease;
        &:hover { color: #2d6a4f; }
        &.mdc-tab--active { color: #2d6a4f; font-weight: 700; }
      }
      .mdc-tab-indicator .mdc-tab-indicator__content { border-color: #2d6a4f !important; }
    }
    @media (max-width: 640px) {
      ::ng-deep .mat-mdc-tab-nav-bar .mat-mdc-tab-link { padding: 0 10px; font-size: 12px; }
      .tab-icon { display: none; }
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