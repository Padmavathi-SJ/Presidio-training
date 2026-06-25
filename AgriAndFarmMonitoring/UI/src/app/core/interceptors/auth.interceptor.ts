// src/app/core/interceptors/auth.interceptor.ts
import { inject } from '@angular/core';
import { 
  HttpInterceptorFn, 
  HttpRequest, 
  HttpHandlerFn, 
  HttpEvent,
  HttpErrorResponse 
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap, finalize } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { environment } from '../../../environments/environment';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);

  // ✅ Skip auth and public endpoints
  const publicEndpoints = [
    '/auth/login',
    '/auth/register',
    '/auth/refresh-token',
    '/auth/validate',
    '/worker/auth/login',  // ✅ Added worker auth endpoints
    '/worker/auth/refresh-token',
    '/worker/auth/validate'
  ];
  
  const isPublicEndpoint = publicEndpoints.some(endpoint => req.url.includes(endpoint));
  
  if (isPublicEndpoint) {
    return next(req);
  }

  // ✅ Get token from service
  const accessToken = tokenService.getAccessToken();
  
  // ✅ Log for debugging (only in development)
  if (!environment.production) {
    console.log('🔑 Interceptor - Request:', req.url);
    console.log('🔑 Interceptor - Token exists:', !!accessToken);
    if (accessToken) {
      console.log('🔑 Interceptor - Token:', accessToken.substring(0, 30) + '...');
    }
  }

  let authReq = req;
  if (accessToken) {
    authReq = addTokenToRequest(req, accessToken);
  } else {
    console.warn('⚠️ No token found for request:', req.url);
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error('❌ HTTP Error:', error.status, error.message);
      if (error.status === 401 && !req.url.includes('/auth/refresh-token') && !req.url.includes('/worker/auth/refresh-token')) {
        return handle401Error(authReq, next, authService, tokenService);
      }
      return throwError(() => error);
    })
  );
};

function addTokenToRequest(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

function handle401Error(
  request: HttpRequest<any>, 
  next: HttpHandlerFn,
  authService: AuthService,
  tokenService: TokenService
): Observable<HttpEvent<any>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((response: any) => {
        isRefreshing = false;
        const newAccessToken = response.data.accessToken;
        console.log('🔄 Token refreshed successfully');
        refreshTokenSubject.next(newAccessToken);
        return next(addTokenToRequest(request, newAccessToken));
      }),
      catchError((error) => {
        isRefreshing = false;
        console.error('❌ Refresh token failed:', error);
        authService.forceLogout();
        return throwError(() => error);
      }),
      finalize(() => {
        isRefreshing = false;
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        return next(addTokenToRequest(request, token!));
      })
    );
  }
}