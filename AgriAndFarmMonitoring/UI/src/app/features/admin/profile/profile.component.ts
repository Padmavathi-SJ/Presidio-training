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
  selector: 'app-admin-profile',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class Profile implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);
  private tokenService = inject(TokenService);

  user = signal<User | null>(null);
  farmStats = signal<any>(null);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  editMode = signal<boolean>(false);
  adminProfile = signal<any>(null);

  ngOnInit() {
    this.user.set(this.tokenService.getUser());
    
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      phone: [''],
      farmName: ['', Validators.required],
      farmPhone: [''],
      farmAddress: ['']
    });

    this.fetchProfile();
    this.fetchFarmStats();
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmNewPassword')?.value
      ? null : { mismatch: true };
  }

  fetchProfile() {
    this.isLoading.set(true);
    this.http.get<any>(`${environment.apiUrl}/admin/profile`).subscribe({
      next: (res) => {
        if (res.success) {
          this.adminProfile.set(res.data);
          this.profileForm.patchValue({
            name: res.data.name,
            phone: res.data.phone,
            farmName: res.data.farmName,
            farmPhone: res.data.farmPhone,
            farmAddress: res.data.farmAddress
          });
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.snackBar.open('Failed to load profile details', 'Close', { duration: 3000 });
      }
    });
  }

  toggleEditMode() {
    if (this.editMode()) {
      // cancel edit
      const profile = this.adminProfile();
      if (profile) {
        this.profileForm.patchValue(profile);
      }
    }
    this.editMode.set(!this.editMode());
  }

  saveProfile() {
    if (this.profileForm.invalid) return;
    
    this.isSaving.set(true);
    this.http.put<any>(`${environment.apiUrl}/admin/profile`, this.profileForm.value).subscribe({
      next: (res) => {
        if (res.success) {
          this.adminProfile.set(res.data);
          this.editMode.set(false);
          this.snackBar.open('Profile updated successfully!', 'Close', { duration: 3000, panelClass: ['bg-green-600', 'text-white'] });
        }
        this.isSaving.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to update profile', 'Close', { duration: 4000, panelClass: ['bg-red-600', 'text-white'] });
        this.isSaving.set(false);
      }
    });
  }

  fetchFarmStats() {
    if (!this.user()?.farmId) return;
    
    this.http.get<any>(`${environment.apiUrl}/farms/${this.user()?.farmId}/fields/statistics`).subscribe({
      next: (res) => {
        const stats = res.data || res;
        this.farmStats.set({
          totalFields: stats.totalFields || 0,
          activeCrops: stats.totalActiveCrops || stats.activeFields || 0
        });
      },
      error: () => console.log('Could not load farm fields stats')
    });
  }

  onChangePassword() {
    if (this.passwordForm.invalid) return;

    this.isSaving.set(true);
    const dto = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword,
      confirmNewPassword: this.passwordForm.value.confirmNewPassword
    };

    this.http.post<any>(`${environment.apiUrl}/auth/change-password`, dto).subscribe({
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
