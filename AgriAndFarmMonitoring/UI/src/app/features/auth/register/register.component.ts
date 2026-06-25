// src/app/features/auth/register/register.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { MaterialModule } from '../../../shared/material.module';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MaterialModule
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = false;
  errorMessage = '';
  hidePassword = true;
  hideConfirmPassword = true;

  // ✅ Combine into a single form
  registerForm = this.fb.group({
    // Farm Information
    farmName: ['', [Validators.required, Validators.maxLength(200)]],
    farmEmail: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    farmPhone: ['', [Validators.maxLength(20)]],
    farmAddress: [''],
    farmCity: [''],
    farmState: [''],
    farmCountry: [''],
    farmPostalCode: [''],
    totalLandHectares: [null, [Validators.min(0.01)]],
    // Admin Information
    adminName: ['', [Validators.required, Validators.maxLength(100)]],
    adminEmail: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    adminPhone: ['', [Validators.maxLength(20)]],
    adminPassword: ['', [
      Validators.required,
      Validators.minLength(6),
      Validators.maxLength(50),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$/)
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    this.registerForm.updateValueAndValidity();
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('adminPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password === confirm ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formValue = this.registerForm.value;
    const registrationData = {
      farmName: formValue.farmName,
      farmEmail: formValue.farmEmail,
      farmPhone: formValue.farmPhone,
      farmAddress: formValue.farmAddress,
      farmCity: formValue.farmCity,
      farmState: formValue.farmState,
      farmCountry: formValue.farmCountry,
      farmPostalCode: formValue.farmPostalCode,
      totalLandHectares: formValue.totalLandHectares,
      adminName: formValue.adminName,
      adminEmail: formValue.adminEmail,
      adminPhone: formValue.adminPhone,
      adminPassword: formValue.adminPassword,
      confirmPassword: undefined
    };

    this.authService.register(registrationData)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          this.router.navigate(['/admin/dashboard']);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
        }
      });
  }

  getPasswordErrorMessage(): string {
    const control = this.registerForm.get('adminPassword');
    if (control?.hasError('required')) {
      return 'Password is required';
    }
    if (control?.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }
    if (control?.hasError('pattern')) {
      return 'Password must contain uppercase, lowercase and number';
    }
    return '';
  }
}