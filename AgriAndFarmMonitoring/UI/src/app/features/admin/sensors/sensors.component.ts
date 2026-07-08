// src/app/features/admin/sensors/sensors.component.ts
import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { SensorService } from '../services/sensor.service';
import { SensorSignalRService } from '../services/sensor-signalr.service';
import { ManualReadingDialogComponent } from './manual-reading-dialog/manual-reading-dialog.component';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatIconModule,
    MatBadgeModule,
    MatTooltipModule,
    MatButtonModule
  ],
  template: `
    <div class="sensors-module">
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
               [routerLink]="'readings'"
               routerLinkActive #rla2="routerLinkActive"
               [active]="rla2.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">list</mat-icon>
              Readings
              @if (unreadAlertCount() > 0) {
                <span class="ml-1 bg-red-500 text-white text-xs rounded-full px-2 py-0.5 min-w-[20px] text-center">
                  {{ unreadAlertCount() }}
                </span>
              }
            </a>
            <a mat-tab-link
               [routerLink]="'statistics'"
               routerLinkActive #rla3="routerLinkActive"
               [active]="rla3.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">analytics</mat-icon>
              Statistics
            </a>
            <a mat-tab-link
               [routerLink]="'alerts'"
               routerLinkActive #rla4="routerLinkActive"
               [active]="rla4.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">notifications_active</mat-icon>
              Alerts
              @if (criticalAlertCount() > 0) {
                <span class="ml-1 bg-red-600 text-white text-xs rounded-full px-2 py-0.5 min-w-[20px] text-center">
                  {{ criticalAlertCount() }}
                </span>
              }
            </a>
            <a mat-tab-link
               [routerLink]="'thresholds'"
               routerLinkActive #rla5="routerLinkActive"
               [active]="rla5.isActive"
               class="flex items-center gap-2">
              <mat-icon class="text-sm">rule</mat-icon>
              Thresholds
            </a>
            
            <div class="flex-grow"></div>
            
            <div class="flex items-center px-4">
              <button mat-raised-button color="accent" (click)="openManualReadingDialog()">
                <mat-icon>add</mat-icon>
                Manual Reading
              </button>
            </div>
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
    .sensors-module {
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
export class SensorsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private sensorService = inject(SensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  private destroy$ = new Subject<void>();
  unreadAlertCount = signal(0);
  criticalAlertCount = signal(0);

  ngOnInit(): void {
    this.loadAlertCounts();
    this.setupSignalR();
    this.trackNavigation();
  }

  openManualReadingDialog() {
    this.dialog.open(ManualReadingDialogComponent, {
      width: '500px'
    });
  }

  private loadAlertCounts(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Get unresolved alert count
    this.sensorService.getUnresolvedAlertCount(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.unreadAlertCount.set(response.data);
          }
        },
        error: () => console.error('Error loading alert count')
      });

    // Get critical alerts count
    this.sensorService.getCriticalAlerts(farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.criticalAlertCount.set(response.data?.length || 0);
          }
        },
        error: () => console.error('Error loading critical alerts')
      });
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    // Join farm group for real-time updates
    this.sensorSignalR.joinFarmGroup(farmId);

    // Listen for new alerts
    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert) => {
        if (alert) {
          this.unreadAlertCount.update(count => count + 1);
          if (alert.severity === 'CRITICAL' || alert.severity === 'HIGH') {
            this.criticalAlertCount.update(count => count + 1);
            this.showAlertNotification(alert);
          }
        }
      });

    // Listen for resolved alerts
    this.sensorSignalR.alertResolved$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => {
        if (data) {
          this.unreadAlertCount.update(count => Math.max(0, count - 1));
          this.criticalAlertCount.update(count => Math.max(0, count - 1));
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
        // Reset badge when navigating to alerts page
        if (this.router.url.includes('/sensors/alerts')) {
          // Don't reset, let the alerts component handle it
        }
      });
  }

  private showAlertNotification(alert: any): void {
    const severityEmoji: Record<string, string> = {
      'LOW': 'ℹ️',
      'MEDIUM': '⚡',
      'HIGH': '⚠️',
      'CRITICAL': '🚨'
    };

    const emoji = severityEmoji[alert.severity] || '🔔';
    const message = `${emoji} ${alert.alertType}: ${alert.message || 'New alert'}`;
    
    this.snackBar.open(message, 'View', {
      duration: 10000,
      panelClass: ['alert-snackbar', `alert-${alert.severity?.toLowerCase()}`],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    }).onAction().subscribe(() => {
      this.router.navigate(['/admin/sensors/alerts']);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}