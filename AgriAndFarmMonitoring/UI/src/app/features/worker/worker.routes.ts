// src/app/features/worker/worker.routes.ts
import { Routes } from '@angular/router';
import { WorkerLayoutComponent } from './worker-layout/worker-layout.component';

export const WORKER_ROUTES: Routes = [
  {
    path: '',
    component: WorkerLayoutComponent,
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
        path: 'observations',
        loadComponent: () => import('./observations/observations.component')
          .then(c => c.ObservationsComponent)
      },
      {
        path: 'tasks',
        loadComponent: () => import('./tasks/tasks.component')
          .then(c => c.TasksComponent)
      },
      {
        path: 'weather',
        loadComponent: () => import('./weather/weather.component')
          .then(c => c.WorkerWeatherComponent),
        children: [
          {
            path: 'dashboard',
            loadComponent: () => import('./weather/weather-dashboard/weather-dashboard.component')
              .then(c => c.WorkerWeatherDashboardComponent)
          },
          {
            path: 'alerts',
            loadComponent: () => import('./weather/weather-alerts/weather-alerts.component')
              .then(c => c.WorkerWeatherAlertsComponent)
          },
          {
            path: 'history',
            loadComponent: () => import('./weather/weather-history/weather-history.component')
              .then(c => c.WorkerWeatherHistoryComponent)
          },
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
      },
      {
        path: 'sensors',
        loadComponent: () => import('./sensors/sensors.component')
          .then(c => c.SensorsComponent),
        children: [
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
            loadComponent: () => import('./sensors/sensor-alerts/sensor-alerts.component')
              .then(c => c.SensorAlertsComponent)
          },
          {
            path: 'thresholds',
            loadComponent: () => import('./sensors/sensor-thresholds/sensor-thresholds.component')
              .then(c => c.SensorThresholdsComponent)
          },
          {
            path: '',
            redirectTo: 'dashboard',
            pathMatch: 'full'
          }
        ]
      },
      {
        path: 'harvests',
        loadComponent: () => import('./harvests/harvests.component')
          .then(c => c.HarvestsComponent)
      },
      {
        path: 'quality-checks',
        loadComponent: () => import('./quality-checks/quality-checks.component')
          .then(c => c.QualityChecksComponent)
      },

      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component')
          .then(c => c.ProfileComponent)
      },
      {
        path: 'disease-detection',
        loadComponent: () => import('../ai/disease-detection/disease-detection.component')
          .then(c => c.DiseaseDetectionComponent)
      },
      {
        path: 'ai-chat',
        loadComponent: () => import('../ai/ai-chat/ai-chat.component')
          .then(c => c.AiChatComponent)
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