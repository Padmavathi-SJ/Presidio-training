import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { WorkerGuard } from './core/guards/worker.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(r => r.AUTH_ROUTES)
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then(r => r.ADMIN_ROUTES),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'worker',
    loadChildren: () => import('./features/worker/worker.routes').then(r => r.WORKER_ROUTES),
    canActivate: [AuthGuard, WorkerGuard]
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