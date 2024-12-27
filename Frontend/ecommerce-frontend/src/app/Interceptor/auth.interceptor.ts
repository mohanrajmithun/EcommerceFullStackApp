import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { trace, context, ROOT_CONTEXT } from '@opentelemetry/api';

let isSessionExpired = false; // Global flag to prevent multiple redirects

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);
  const token = tokenService.getToken();

  // Check for token expiry before proceeding
  if (token && tokenService.isTokenExpired(token)) {
    if (!isSessionExpired) {
      isSessionExpired = true; // Set the flag to true
      console.log('Session expired. Logging out...');
      handleSessionExpiry(tokenService, router);
    }
    return throwError(() => new Error('Session expired. Please log in again.'));
  }

  // Set span name dynamically based on request
  const spanName = `HTTP ${req.method} ${req.urlWithParams}`;
  const traceparent = getCurrentTraceParent(spanName);

  const authReq = req.clone({
    setHeaders: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(traceparent ? { traceparent } : {}) // Add traceparent if available
    }
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 and 403 errors as token invalid scenarios
      if ((error.status === 401 || error.status === 403) && !isSessionExpired) {
        isSessionExpired = true; // Set the flag to true
        console.log(`Error ${error.status}: Unauthorized or forbidden access. Logging out...`);
        handleSessionExpiry(tokenService, router);
      }
      return throwError(() => error);
    })
  );
};

// Helper to retrieve traceparent header with dynamic span name
function getCurrentTraceParent(spanName: string): string | null {
  let currentSpan = trace.getSpan(context.active());

  if (!currentSpan) {
    const tracer = trace.getTracer('AngularFrontendApp');
    currentSpan = tracer.startSpan(spanName, undefined, ROOT_CONTEXT);
    context.with(context.active(), () => currentSpan?.end());
    console.log('Created new span to propagate trace context:', currentSpan.spanContext().traceId);
  }

  if (currentSpan) {
    const spanContext = currentSpan.spanContext();
    const traceparent = `00-${spanContext.traceId}-${spanContext.spanId}-01`;
    console.log('Traceparent header added:', traceparent);
    return traceparent;
  }

  console.log('No active span found for traceparent');
  return null;
}

// Handle session expiry
function handleSessionExpiry(tokenService: TokenService, router: Router) {
  tokenService.logout();
  router.navigate(['/login'], {
    queryParams: { sessionExpired: true }, // Optionally pass a message for user feedback
  });
}
