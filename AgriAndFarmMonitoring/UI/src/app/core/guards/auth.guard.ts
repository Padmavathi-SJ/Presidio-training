// src/app/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export const AuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      if (user && authService.isLoggedIn()) {
        return true;
      }
      
      // ✅ Redirect to login with return URL
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url }
      });
      return false;
    })
  );
};