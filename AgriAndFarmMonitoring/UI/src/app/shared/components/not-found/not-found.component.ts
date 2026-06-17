import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        <h1 class="text-6xl font-bold text-gray-700">404</h1>
        <h2 class="text-2xl font-semibold text-gray-700 mt-4">Page Not Found</h2>
        <p class="text-gray-500 mt-2">The page you're looking for doesn't exist</p>
        <button mat-raised-button color="primary" class="mt-6" routerLink="/auth/login">
          Go Back to Login
        </button>
      </div>
    </div>
  `
})
export class NotFoundComponent {}