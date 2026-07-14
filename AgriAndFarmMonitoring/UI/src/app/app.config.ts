// src/app/app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { notificationInterceptor } from './core/interceptors/notification.interceptor';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { MAT_PAGINATOR_DEFAULT_OPTIONS } from '@angular/material/paginator';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor, notificationInterceptor])
    ),
    provideAnimations(),  // ✅ This is critical
    provideCharts(withDefaultRegisterables()),
    {
      provide: MAT_PAGINATOR_DEFAULT_OPTIONS,
      useValue: { formFieldAppearance: 'outline', pageSize: 10, pageSizeOptions: [10] }
    }
  ]
};