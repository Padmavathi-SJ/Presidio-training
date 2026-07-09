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

  passwordForm!: FormGroup;

  ngOnInit() {
    this.user.set(this.tokenService.getUser());
    
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.fetchFarmStats();
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmNewPassword')?.value
      ? null : { mismatch: true };
  }

  fetchFarmStats() {
    if (!this.user()?.farmId) return;
    
    // We can fetch basic farm stats to show some farm details
    this.http.get<any>(`${environment.apiUrl}/admin/farms/${this.user()?.farmId}/fields`).subscribe({
      next: (res) => {
        const fields = Array.isArray(res) ? res : (res.data || []);
        this.farmStats.set({
          totalFields: fields.length,
          activeCrops: fields.filter((f: any) => f.currentCropCycleId != null).length
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
