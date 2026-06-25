// src/app/core/services/token.service.ts
import { Injectable } from '@angular/core';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user';

  setTokens(accessToken: string, refreshToken: string): void {
    console.log('💾 Storing tokens');
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  setUser(user: User): void {
    console.log('💾 Storing user data');
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  getUser(): User | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        console.warn('⚠️ Failed to parse user data');
        return null;
      }
    }
    return null;
  }

  clearTokens(): void {
    console.log('🗑️ Clearing tokens');
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  clearUser(): void {
    console.log('🗑️ Clearing user data');
    localStorage.removeItem(this.USER_KEY);
  }

  clearAll(): void {
    console.log('🗑️ Clearing all auth data');
    this.clearTokens();
    this.clearUser();
  }

  isTokenExpired(): boolean {
    const user = this.getUser();
    if (!user) {
      return true;
    }
    
    const token = this.getAccessToken();
    if (!token) {
      return true;
    }
    
    const expiryDate = user.accessTokenExpiresAt;
    if (!expiryDate) {
      return true;
    }
    
    // ✅ Add 30 seconds buffer for safety
    const bufferMs = 30 * 1000;
    const expiryTime = new Date(expiryDate).getTime();
    const currentTime = Date.now();
    
    const isExpired = expiryTime - currentTime < bufferMs;
    if (isExpired) {
      console.log('⏰ Token is expired or expiring soon');
    }
    return isExpired;
  }
}