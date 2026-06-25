// src/app/core/guards/admin.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export const AdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      // ✅ Check both role and userType
      const userRole = user?.role || user?.userType;
      const isAdmin = userRole === 'Admin' || userRole === 'admin';
      
      if (user && authService.isLoggedIn() && isAdmin) {
        return true;
      }
      
      // ✅ If logged in but not admin, redirect to unauthorized
      if (user && authService.isLoggedIn()) {
        router.navigate(['/unauthorized']);
        return false;
      }
      
      // ✅ If not logged in, redirect to login
      router.navigate(['/auth/login']);
      return false;
    })
  );
};