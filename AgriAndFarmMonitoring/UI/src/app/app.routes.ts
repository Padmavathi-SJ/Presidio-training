// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';
import { WorkerGuard } from './core/guards/worker.guard';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
    canActivate: [AuthGuard, AdminGuard]  // ✅ Only use guards at parent level
  },
  {
    path: 'worker',
    loadChildren: () => import('./features/worker/worker.routes').then(m => m.WORKER_ROUTES),
    canActivate: [AuthGuard, WorkerGuard]  // ✅ Only use guards at parent level
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized.component')
      .then(c => c.UnauthorizedComponent)
  },
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found.component')
      .then(c => c.NotFoundComponent)
  }
];