// src/app/core/guards/auth-check.guard.ts (FIXED)
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export const AuthCheckGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      const isLoggedIn = user && authService.isLoggedIn();
      
      if (isLoggedIn) {
        return true;
      }
      
      // ✅ Redirect to login if not authenticated
      router.navigate(['/auth/login'], {
        queryParams: { returnUrl: state.url }
      });
      return false;
    })
  );
};