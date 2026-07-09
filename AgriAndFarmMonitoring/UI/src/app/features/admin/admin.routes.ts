// src/app/features/admin/admin.routes.ts
import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AuthGuard } from '../../core/guards/auth.guard';
import { AdminGuard } from '../../core/guards/admin.guard';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [AuthGuard, AdminGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component')
          .then(c => c.DashboardComponent)
      },
      {
        path: 'fields',
        loadComponent: () => import('./fields/fields.component')
          .then(c => c.FieldsComponent)
      },
      {
        path: 'workers',
        loadComponent: () => import('./workers/workers.component')
          .then(c => c.WorkersComponent)
      },
      {
        path: 'worker-fields',
        loadComponent: () => import('./worker-fields/worker-fields.component')
          .then(c => c.WorkerFieldsComponent)
      },
      {
        path: 'observations',
        loadComponent: () => import('./observations/observations.component')
          .then(c => c.Observations)
      },
      {
        path: 'harvests',
        loadComponent: () => import('./harvests/harvests.component')
          .then(c => c.Harvests)
      },
      {
        path: 'quality-checks',
        loadComponent: () => import('./quality-checks/quality-checks.component')
          .then(c => c.QualityChecksComponent)
      },
      // ✅ Sensor Module Routes
      {
        path: 'sensors',
        loadComponent: () => import('./sensors/sensors.component')
          .then(c => c.SensorsComponent),
        children: [
          {
            path: '',
            redirectTo: 'dashboard',
            pathMatch: 'full'
          },
          {
            path: 'dashboard',
            loadComponent: () => import('./sensors/sensor-dashboard/sensor-dashboard.component')
              .then(c => c.SensorDashboardComponent)
          },
          {
            path: 'readings',
            loadComponent: () => import('./sensors/sensor-readings/sensor-readings.component')
              .then(c => c.SensorReadingsComponent)
          },
          {
            path: 'statistics',
            loadComponent: () => import('./sensors/sensor-statistics/sensor-statistics.component')
              .then(c => c.SensorStatisticsComponent)
          },
          {
            path: 'alerts',
            loadComponent: () => import('./sensors/alerts/alerts.component')
              .then(c => c.AlertsComponent)
          },
          {
            path: 'thresholds',
            loadComponent: () => import('./sensors/alert-thresholds/alert-thresholds.component')
              .then(c => c.AlertThresholdsComponent)
          },
          {
            path: 'field/:fieldId',
            loadComponent: () => import('./sensors/field-sensor-details/field-sensor-details.component')
              .then(c => c.FieldSensorDetailsComponent)
          }
        ]
      },
      // Weather Module Routes
      {
        path: 'weather',
        loadComponent: () => import('./weather/weather.component')
          .then(c => c.WeatherComponent),
        children: [
          {
            path: '',
            redirectTo: 'dashboard',
            pathMatch: 'full'
          },
          {
            path: 'dashboard',
            loadComponent: () => import('./weather/weather-dashboard/weather-dashboard.component')
              .then(c => c.WeatherDashboardComponent)
          },
          {
            path: 'alerts',
            loadComponent: () => import('./weather/weather-alerts/weather-alerts.component')
              .then(c => c.WeatherAlertsComponent)
          },
          {
            path: 'history',
            loadComponent: () => import('./weather/weather-data-history/weather-data-history.component')
              .then(c => c.WeatherDataHistoryComponent)
          },
        ]
      },
      {
        path: 'tasks',
        loadComponent: () => import('./tasks/tasks.component')
          .then(c => c.TasksComponent)
      },
      {
        path: 'yield-reports',
        loadComponent: () => import('./yield-reports/yield-reports.component')
          .then(c => c.YieldReportsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component')
          .then(c => c.Profile)
      },
      {
        path: 'settings',
        loadComponent: () => import('./settings/settings.component')
          .then(c => c.Settings)
      },

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];