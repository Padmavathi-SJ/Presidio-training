// src/app/core/services/auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError, map, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { 
  LoginRequest, 
  AuthResponse, 
  RegisterRequest, 
  RefreshTokenRequest,
  ChangePasswordRequest
} from '../models/auth.model';
import { User } from '../models/user.model';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private tokenService = inject(TokenService);

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private readonly API_URL = environment.apiUrl;
  private isLoggingIn = false;

  constructor() {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    if (this.isLoggingIn) {
      return throwError(() => new Error('Login already in progress'));
    }
    
    this.isLoggingIn = true;
    this.clearUser();
    
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.success && response.data) {
            console.log('✅ Login API success, storing user data');
            const user = this.mapToUser(response.data);
            this.setUser(user);
          }
        }),
        map((response) => {
          this.isLoggingIn = false;
          return response;
        }),
        catchError((error) => {
          this.isLoggingIn = false;
          this.clearUser();
          return this.handleError(error);
        })
      );
  }

  // ✅ Get redirect URL based on user role
getRedirectUrl(user: User | null): string {
  if (!user) return '/auth/login';
  
  // ✅ Normalize role for comparison
  const role = (user.role || user.userType || '').toLowerCase();
  
  if (role === 'admin') {
    return '/admin/dashboard';
  } else if (role === 'worker') {
    return '/worker/dashboard';
  }
  
  return '/auth/login';
}

  // ✅ Redirect user based on role
  redirectBasedOnRole(user: User | null): void {
    const redirectUrl = this.getRedirectUrl(user);
    console.log(`🔄 Redirecting to: ${redirectUrl}`);
    this.router.navigate([redirectUrl]);
  }

  register(registrationData: RegisterRequest): Observable<AuthResponse> {
    this.clearUser();
    
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, registrationData)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.success && response.data) {
            const user = this.mapToUser(response.data);
            this.setUser(user);
          }
        }),
        catchError((error) => {
          this.clearUser();
          return this.handleError(error);
        })
      );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenService.getRefreshToken();
    const accessToken = this.tokenService.getAccessToken();

    if (!refreshToken || !accessToken) {
      this.forceLogout();
      return throwError(() => new Error('No refresh token available'));
    }

    const request: RefreshTokenRequest = { accessToken, refreshToken };

    return this.http.post<AuthResponse>(`${this.API_URL}/auth/refresh-token`, request)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.success && response.data) {
            const user = this.mapToUser(response.data);
            this.updateUser(user);
          }
        }),
        catchError((error) => {
          this.forceLogout();
          return throwError(() => error);
        })
      );
  }

  logout(revokeToken?: boolean): Observable<any> {
    const refreshToken = this.tokenService.getRefreshToken();
    
    if (refreshToken && revokeToken !== false) {
      return this.http.post(`${this.API_URL}/auth/revoke-token`, { refreshToken })
        .pipe(
          finalize(() => {
            this.forceLogout();
          }),
          catchError(() => {
            this.forceLogout();
            return throwError(() => new Error('Logout failed'));
          })
        );
    } else {
      this.forceLogout();
      return new Observable(subscriber => {
        subscriber.next({ success: true });
        subscriber.complete();
      });
    }
  }

  forceLogout(): void {
    console.log('🔴 Force logout - clearing all data');
    this.isLoggingIn = false;
    this.clearUser();
    this.router.navigate(['/auth/login'], { 
      queryParams: { t: Date.now() }
    });
  }

  validateToken(): Observable<boolean> {
    return this.http.get(`${this.API_URL}/auth/validate`)
      .pipe(
        map(() => true),
        catchError(() => {
          this.forceLogout();
          return throwError(() => false);
        })
      );
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/change-password`, request)
      .pipe(
        tap(() => {
          this.forceLogout();
        }),
        catchError(this.handleError)
      );
  }

  isLoggedIn(): boolean {
    const token = this.tokenService.getAccessToken();
    const user = this.currentUserSubject.value;
    return !!token && !!user && !this.tokenService.isTokenExpired() && !this.isLoggingIn;
  }

isAdmin(): boolean {
  const user = this.currentUserSubject.value;
  const role = (user?.role || user?.userType || '').toLowerCase();
  return role === 'admin';
}

isWorker(): boolean {
  const user = this.currentUserSubject.value;
  const role = (user?.role || user?.userType || '').toLowerCase();
  return role === 'worker';
}


  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getFarmId(): number {
    return this.currentUserSubject.value?.farmId || 0;
  }

  getAuthToken(): string | null {
    return this.tokenService.getAccessToken();
  }

  private setUser(user: User): void {
    console.log('✅ Setting user data');
    console.log('📝 User Role:', user.role || user.userType);
    this.tokenService.setTokens(user.accessToken, user.refreshToken);
    this.tokenService.setUser(user);
    this.currentUserSubject.next(user);
  }

  private updateUser(user: User): void {
    console.log('🔄 Updating user data');
    this.tokenService.setTokens(user.accessToken, user.refreshToken);
    this.tokenService.setUser(user);
    this.currentUserSubject.next(user);
  }

  private clearUser(): void {
    console.log('🗑️ Clearing user data');
    this.tokenService.clearAll();
    this.currentUserSubject.next(null);
  }

  private loadUserFromStorage(): void {
    const user = this.tokenService.getUser();
    const token = this.tokenService.getAccessToken();
    
    if (user && token && !this.tokenService.isTokenExpired()) {
      console.log('📂 Loading user from storage');
      this.currentUserSubject.next(user);
    } else {
      console.log('🗑️ No valid user in storage, clearing');
      this.clearUser();
    }
  }

  private mapToUser(data: any): User {
    // Get role from either role or userType field
    const role = data.role || data.userType || 'Unknown';
    
    const normalizedRole = role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();

     console.log(`📝 Mapping user with role: ${normalizedRole}`);
  
    return {
      id: data.id,
      name: data.name,
      email: data.email,
      farmId: data.farmId,
      farmName: data.farmName,
      role: role,
      userType: data.userType || role,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      accessTokenExpiresAt: new Date(data.accessTokenExpiresAt),
      refreshTokenExpiresAt: new Date(data.refreshTokenExpiresAt)
    };
  }

  private handleError(error: any): Observable<never> {
    console.error('❌ Auth error:', error);
    return throwError(() => error);
  }
}