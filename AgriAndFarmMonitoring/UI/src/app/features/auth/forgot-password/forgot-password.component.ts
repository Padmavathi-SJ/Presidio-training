import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent implements OnInit, OnDestroy {
  step: 1 | 2 | 3 = 1;
  isLoading = false;
  email = '';
  otp = '';

  emailForm: FormGroup;
  otpForm: FormGroup;
  passwordForm: FormGroup;
  
  countdown = 600; // 10 minutes in seconds
  timer: any;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });

    this.passwordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  get formatTime(): string {
    const min = Math.floor(this.countdown / 60);
    const sec = this.countdown % 60;
    return `${min}:${sec < 10 ? '0' : ''}${sec}`;
  }

  startTimer() {
    this.countdown = 600;
    if (this.timer) clearInterval(this.timer);
    this.timer = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(this.timer);
        this.snackBar.open('OTP expired. Please try again.', 'Close', { duration: 3000 });
        this.step = 1;
      }
    }, 1000);
  }

  onRequestOtp() {
    if (this.emailForm.invalid) return;
    this.isLoading = true;
    this.email = this.emailForm.value.email;
    
    this.authService.forgotPassword(this.email).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.snackBar.open('OTP sent successfully (if email exists).', 'Close', { duration: 3000 });
          this.step = 2;
          this.startTimer();
        } else {
          this.snackBar.open(res.message || 'Error sending OTP', 'Close', { duration: 3000 });
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open(err.error?.message || 'Error connecting to server', 'Close', { duration: 3000 });
      }
    });
  }

  onVerifyOtp() {
    if (this.otpForm.invalid) return;
    this.isLoading = true;
    this.otp = this.otpForm.value.otp.toUpperCase();
    
    this.authService.verifyOtp(this.email, this.otp).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          if (this.timer) clearInterval(this.timer);
          this.step = 3;
        } else {
          this.snackBar.open(res.message || 'Invalid OTP', 'Close', { duration: 3000 });
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open(err.error?.message || 'Invalid OTP', 'Close', { duration: 3000 });
      }
    });
  }

  onResetPassword() {
    if (this.passwordForm.invalid) return;
    this.isLoading = true;
    const newPassword = this.passwordForm.value.newPassword;

    this.authService.resetPassword(this.email, this.otp, newPassword).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.snackBar.open('Password reset successfully! Please login.', 'Close', { duration: 5000 });
          this.router.navigate(['/auth/login']);
        } else {
          this.snackBar.open(res.message || 'Error resetting password', 'Close', { duration: 3000 });
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open(err.error?.message || 'Error resetting password', 'Close', { duration: 3000 });
      }
    });
  }
}
