import { HttpInterceptorFn, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

export const notificationInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    tap((event) => {
      if (event instanceof HttpResponse) {
        // Only trigger popups for mutation requests (POST, PUT, DELETE)
        if (['POST', 'PUT', 'DELETE'].includes(req.method)) {
          let message = 'Operation completed successfully.';
          const body = event.body as any;
          
          if (body && body.message) {
            message = body.message;
          } else if (req.method === 'POST') {
            message = 'Record created successfully.';
          } else if (req.method === 'PUT') {
            message = 'Record updated successfully.';
          } else if (req.method === 'DELETE') {
            message = 'Record deleted successfully.';
          }

          if (body && body.success === false) {
             snackBar.open(body.message || 'Operation failed.', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar'],
              horizontalPosition: 'end',
              verticalPosition: 'top'
            });
          } else {
            snackBar.open(message, 'Close', {
              duration: 3000,
              panelClass: ['success-snackbar'],
              horizontalPosition: 'end',
              verticalPosition: 'top'
            });
          }
        }
      }
    }),
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';
      let panelClass = 'error-snackbar';

      if (error.error instanceof ErrorEvent) {
        errorMessage = error.error.message;
      } else {
        errorMessage = error.error?.message || error.message || `Error ${error.status}`;
        if (error.status === 400 || error.status === 409 || error.status === 422) {
          panelClass = 'warning-snackbar';
        }
      }

      snackBar.open(errorMessage, 'Close', {
        duration: 5000,
        panelClass: [panelClass],
        horizontalPosition: 'end',
        verticalPosition: 'top'
      });

      console.error('API Error:', error);
      return throwError(() => error);
    })
  );
};
