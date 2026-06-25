// src/app/features/admin/admin.routes.ts
import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
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
          .then(c => c.Workers)
      },
      {
        path: 'worker-fields',
        loadComponent: () => import('./worker-fields/worker-fields.component')
          .then(c => c.WorkerFields)
      },
      {
        path: 'observations',
        loadComponent: () => import('./observations/observations.component')
          .then(c => c.Observations)
      },
      {
        path: 'sensors',
        loadComponent: () => import('./sensors/sensors.component')
          .then(c => c.Sensors)
      },
      {
        path: 'weather',
        loadComponent: () => import('./weather/weather.component')
          .then(c => c.Weather)
      },
      {
        path: 'tasks',
        loadComponent: () => import('./tasks/tasks.component')
          .then(c => c.Tasks)
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
        loadComponent: () => import('./yield-reports/yield-reports.component')
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