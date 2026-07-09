import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';
import { TokenService } from '../../../core/services/token.service';
import { environment } from '../../../../environments/environment';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-worker-profile',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);
  private authService = inject(AuthService);
  private tokenService = inject(TokenService);

  user = signal<User | null>(null);
  workerProfile = signal<any>(null);
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);

  passwordForm!: FormGroup;

  ngOnInit() {
    this.user.set(this.tokenService.getUser());
    
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.fetchProfile();
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  fetchProfile() {
    this.isLoading.set(true);
    this.http.get<any>(`${environment.apiUrl}/worker/profile`).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.workerProfile.set(res.data);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch profile', err);
        this.snackBar.open('Failed to load profile details', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  onChangePassword() {
    if (this.passwordForm.invalid) return;

    this.isSaving.set(true);
    const dto = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.http.put<any>(`${environment.apiUrl}/worker/profile/change-password`, dto).subscribe({
      next: (res) => {
        this.snackBar.open('Password changed successfully!', 'Close', { duration: 3000, panelClass: ['bg-green-600', 'text-white'] });
        this.passwordForm.reset();
        this.isSaving.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to change password', 'Close', { duration: 4000, panelClass: ['bg-red-600', 'text-white'] });
        this.isSaving.set(false);
      }
    });
  }
}
