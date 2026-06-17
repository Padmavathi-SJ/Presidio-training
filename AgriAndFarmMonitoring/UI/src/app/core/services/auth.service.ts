import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { environment } from '@env/environment';
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

  constructor() {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.success && response.data) {
            const user = this.mapToUser(response.data);
            this.setUser(user);
          }
        }),
        catchError(this.handleError)
      );
  }

  register(registrationData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, registrationData)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.success && response.data) {
            const user = this.mapToUser(response.data);
            this.setUser(user);
          }
        }),
        catchError(this.handleError)
      );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenService.getRefreshToken();
    const accessToken = this.tokenService.getAccessToken();

    if (!refreshToken || !accessToken) {
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
          this.logout();
          return throwError(() => error);
        })
      );
  }

  logout(revokeToken?: string): Observable<any> {
    const refreshToken = revokeToken || this.tokenService.getRefreshToken();
    
    if (refreshToken) {
      return this.http.post(`${this.API_URL}/auth/revoke-token`, { refreshToken })
        .pipe(
          tap(() => this.clearUser()),
          catchError(() => {
            this.clearUser();
            return throwError(() => new Error('Logout failed'));
          })
        );
    } else {
      this.clearUser();
      return new Observable(subscriber => subscriber.next({ success: true }));
    }
  }

  validateToken(): Observable<boolean> {
    return this.http.get(`${this.API_URL}/auth/validate`)
      .pipe(
        map(() => true),
        catchError(() => {
          this.logout();
          return throwError(() => false);
        })
      );
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/change-password`, request)
      .pipe(
        tap(() => {
          this.logout();
        }),
        catchError(this.handleError)
      );
  }

  isLoggedIn(): boolean {
    return !!this.tokenService.getAccessToken() && !!this.currentUserSubject.value;
  }

  isAdmin(): boolean {
    return this.currentUserSubject.value?.role === 'Admin';
  }

  isWorker(): boolean {
    return this.currentUserSubject.value?.role === 'Worker';
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
    this.tokenService.setTokens(user.accessToken, user.refreshToken);
    this.tokenService.setUser(user);
    this.currentUserSubject.next(user);
  }

  private updateUser(user: User): void {
    this.tokenService.setTokens(user.accessToken, user.refreshToken);
    this.tokenService.setUser(user);
    this.currentUserSubject.next(user);
  }

  private clearUser(): void {
    this.tokenService.clearTokens();
    this.tokenService.clearUser();
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private loadUserFromStorage(): void {
    const user = this.tokenService.getUser();
    if (user && this.tokenService.getAccessToken()) {
      this.currentUserSubject.next(user);
    }
  }

  private mapToUser(data: any): User {
    return {
      id: data.id,
      name: data.name,
      email: data.email,
      farmId: data.farmId,
      farmName: data.farmName,
      role: data.userType === 'Admin' ? 'Admin' : 'Worker',
      userType: data.userType,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      accessTokenExpiresAt: new Date(data.accessTokenExpiresAt),
      refreshTokenExpiresAt: new Date(data.refreshTokenExpiresAt)
    };
  }

  private handleError(error: any): Observable<never> {
    console.error('Auth error:', error);
    return throwError(() => error);
  }
}