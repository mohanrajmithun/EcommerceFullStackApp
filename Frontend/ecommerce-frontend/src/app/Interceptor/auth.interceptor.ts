import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Use inject to get the TokenService and Router in a functional interceptor
  const tokenService = inject(TokenService);
  const router = inject(Router);

  // Get the token from the TokenService
  const token = tokenService.getToken();

  // Clone the request and add the Authorization header if the token exists
  const authReq = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  // Pass the cloned request to the next handler
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Check for token expiration or invalidation
     {
        // Token is invalid or expired, handle the error
        handleAuthError(tokenService, router);
      }
      return throwError(() => error); // Re-throw the error for further handling
    })
  );
}

// Helper function to handle token expiration/invalidation
function handleAuthError(tokenService: TokenService, router: Router) {
  // Clear the token from local storage or memory
  tokenService.logout();

  // Redirect to the login page
  router.navigate(['/login']);
}
