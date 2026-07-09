import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar } from '@angular/material/snack-bar';
import { filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { WorkerSensorService } from '../services/worker-sensor.service';
import { SensorSignalRService } from '../../admin/services/sensor-signalr.service';
import { Alert } from '../../admin/models/sensor.model';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatIconModule,
    MatBadgeModule
  ],
  template: `
    <div class="sensors-module">
      <!-- Professional sub-nav bar -->
      <div class="sensors-subnav">
        <nav mat-tab-nav-bar [tabPanel]="tabPanel" class="!border-b-0">
          <a mat-tab-link [routerLink]="'dashboard'" routerLinkActive #rla1="routerLinkActive" [active]="rla1.isActive">
            <mat-icon class="tab-icon">dashboard</mat-icon>
            Dashboard
          </a>
          <a mat-tab-link [routerLink]="'readings'" routerLinkActive #rla2="routerLinkActive" [active]="rla2.isActive">
            <mat-icon class="tab-icon">list</mat-icon>
            Readings
          </a>
          <a mat-tab-link [routerLink]="'statistics'" routerLinkActive #rla3="routerLinkActive" [active]="rla3.isActive">
            <mat-icon class="tab-icon">analytics</mat-icon>
            Statistics
          </a>
          <a mat-tab-link [routerLink]="'alerts'" routerLinkActive #rla4="routerLinkActive" [active]="rla4.isActive">
            <mat-icon class="tab-icon">notifications_active</mat-icon>
            Alerts
            @if (unreadAlertCount() > 0) {
              <span class="alert-badge">{{ unreadAlertCount() }}</span>
            }
          </a>
          <a mat-tab-link [routerLink]="'thresholds'" routerLinkActive #rla5="routerLinkActive" [active]="rla5.isActive">
            <mat-icon class="tab-icon">rule</mat-icon>
            Thresholds
          </a>
        </nav>
      </div>

      <mat-tab-nav-panel #tabPanel class="block">
        <router-outlet></router-outlet>
      </mat-tab-nav-panel>
    </div>
  `,
  styles: [`
    .sensors-module {
      min-height: calc(100vh - 58px);
      background: #f5f8f5;
    }

    .sensors-subnav {
      background: white;
      border-bottom: 1.5px solid #d5e1da;
      padding: 0 16px;
      position: sticky;
      top: 0;
      z-index: 10;
      box-shadow: 0 1px 4px rgba(45,106,79,0.06);
    }

    .tab-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      margin-right: 5px;
      vertical-align: middle;
    }

    .alert-badge {
      margin-left: 6px;
      background: #ef4444;
      color: white;
      font-size: 11px;
      font-weight: 700;
      border-radius: 9999px;
      padding: 1px 7px;
      min-width: 20px;
      text-align: center;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }

    ::ng-deep {
      .mat-mdc-tab-nav-bar .mat-mdc-tab-link {
        height: 48px;
        font-size: 13px;
        font-weight: 500;
        color: #5a7a6a;
        opacity: 1;
        min-width: auto;
        padding: 0 16px;
        transition: color 0.15s ease;

        &:hover { color: #2d6a4f; }

        &.mdc-tab--active {
          color: #2d6a4f;
          font-weight: 700;
        }
      }

      .mdc-tab-indicator .mdc-tab-indicator__content {
        border-color: #2d6a4f !important;
      }
    }

    @media (max-width: 640px) {
      ::ng-deep .mat-mdc-tab-nav-bar .mat-mdc-tab-link {
        padding: 0 10px;
        font-size: 12px;
        .tab-icon { display: none; }
      }
    }
  `]
})
export class SensorsComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private workerSensorService = inject(WorkerSensorService);
  private sensorSignalR = inject(SensorSignalRService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  private destroy$ = new Subject<void>();
  unreadAlertCount = signal(0);

  ngOnInit(): void {
    this.loadAlertCounts();
    this.setupSignalR();
  }

  private loadAlertCounts(): void {
    this.workerSensorService.getUnresolvedAlerts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (alerts) => {
          this.unreadAlertCount.set(alerts?.length || 0);
        },
        error: () => console.error('Error loading alert count')
      });
  }

  private setupSignalR(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.sensorSignalR.joinFarmGroup(farmId);

    this.sensorSignalR.alert$
      .pipe(takeUntil(this.destroy$))
      .subscribe((alert: any) => {
        if (alert) {
          this.loadAlertCounts();
          this.showAlertNotification(alert);
        }
      });

    this.sensorSignalR.alertResolved$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data: any) => {
        if (data) {
          this.loadAlertCounts();
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
      this.router.navigate(['/worker/sensors/alerts']);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
