import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-worker-weather',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTabsModule, MatIconModule],
  template: `
    <div class="weather-module">
      <div class="weather-subnav">
        <nav mat-tab-nav-bar [tabPanel]="tabPanel" class="!border-b-0">
          <a mat-tab-link [routerLink]="'dashboard'" routerLinkActive #rla1="routerLinkActive" [active]="rla1.isActive">
            <mat-icon class="tab-icon">cloud</mat-icon>
            Dashboard
          </a>
          <a mat-tab-link [routerLink]="'alerts'" routerLinkActive #rla2="routerLinkActive" [active]="rla2.isActive">
            <mat-icon class="tab-icon">warning_amber</mat-icon>
            Alerts
          </a>
          <a mat-tab-link [routerLink]="'history'" routerLinkActive #rla3="routerLinkActive" [active]="rla3.isActive">
            <mat-icon class="tab-icon">history</mat-icon>
            History
          </a>
        </nav>
      </div>
      <mat-tab-nav-panel #tabPanel class="block">
        <router-outlet></router-outlet>
      </mat-tab-nav-panel>
    </div>
  `,
  styles: [`
    .weather-module {
      min-height: calc(100vh - 58px);
      background: #f5f8f5;
    }

    .weather-subnav {
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
export class WorkerWeatherComponent {}
