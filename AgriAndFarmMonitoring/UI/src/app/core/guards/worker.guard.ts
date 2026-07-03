// src/app/core/guards/worker.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export const WorkerGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      // ✅ Check both role and userType
      const userRole = (user?.role || user?.userType || '').toLowerCase();
      const isWorker = userRole === 'worker';
      
      if (user && authService.isLoggedIn() && isWorker) {
        return true;
      }
      
      // ✅ If logged in but not worker, redirect to unauthorized
      if (user && authService.isLoggedIn()) {
        router.navigate(['/unauthorized']);
        return false;
      }
      
      // ✅ If not logged in, redirect to login with return URL
      router.navigate(['/auth/login'], {
        queryParams: { returnUrl: '/worker/dashboard' }
      });
      return false;
    })
  );
};