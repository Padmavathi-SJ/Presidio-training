// src/app/features/auth/login/login.component.ts
import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { MaterialModule } from '../../../shared/material.module';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MaterialModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  isLoading = false;
  errorMessage = '';
  hidePassword = true;
  returnUrl = '/admin/dashboard';

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      const user = this.authService.getCurrentUser();
      if (user) {
        this.authService.redirectBasedOnRole(user);
        return;
      }
    }

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';
    
    if (!this.returnUrl) {
      const user = this.authService.getCurrentUser();
      this.returnUrl = this.authService.getRedirectUrl(user);
    }

    const logoutParam = this.route.snapshot.queryParams['logout'];
    if (logoutParam) {
      console.log('ℹ️ User was logged out');
    }

    const emailParam = this.route.snapshot.queryParams['email'];
    if (emailParam) {
      this.loginForm.patchValue({ email: emailParam });
    }
  }

  ngOnDestroy(): void {}

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const credentials = this.loginForm.value as { email: string; password: string };

    this.authService.login(credentials)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            console.log(`✅ Login successful as ${response.userType}`);
            
            const user = this.authService.getCurrentUser();
            
            if (user) {
              this.authService.redirectBasedOnRole(user);
            } else {
              console.error('❌ User data missing after login');
              this.errorMessage = 'Login successful but user data is missing. Please try again.';
            }
          }
        },
        error: (error) => {
          console.error('❌ Login error:', error);
          this.errorMessage = error.error?.message || 'Invalid email or password. Please try again.';
          this.loginForm.patchValue({ password: '' });
        }
      });
  }
}