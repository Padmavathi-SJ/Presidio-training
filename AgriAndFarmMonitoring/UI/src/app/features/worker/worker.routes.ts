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
          .then(c => c.Weather)
      },
      {
        path: 'sensors',
        loadComponent: () => import('./sensors/sensors.component')
          .then(c => c.Sensors)
      },
      {
        path: 'harvests',
        loadComponent: () => import('./harvests/harvests.component')
          .then(c => c.Harvests)
      },
      {
        path: 'quality-checks',
        loadComponent: () => import('./quality-checks/quality-checks.component')
          .then(c => c.QualityChecks)
      },
      {
        path: 'yield-reports',
        loadComponent: () => import('./yield-reports/yield-reports')
          .then(c => c.YieldReports)
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