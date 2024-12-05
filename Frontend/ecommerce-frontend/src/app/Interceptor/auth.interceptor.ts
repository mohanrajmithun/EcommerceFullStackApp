import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { trace, context, ROOT_CONTEXT, Span } from '@opentelemetry/api';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);
  const token = tokenService.getToken();

  // Set span name dynamically based on request
  const spanName = `HTTP ${req.method} ${req.urlWithParams}`;
  const traceparent = getCurrentTraceParent(spanName); // Pass spanName to getCurrentTraceParent
  const authReq = req.clone({
    setHeaders: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(traceparent ? { traceparent } : {}) // Add traceparent if available
    }
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        handleAuthError(tokenService, router);
      }
      return throwError(() => error);
    })
  );
};

// Helper to retrieve traceparent header with dynamic span name
function getCurrentTraceParent(spanName: string): string | null {
  let currentSpan = trace.getSpan(context.active());

  // If no active span, create a new one with the dynamic name
  if (!currentSpan) {
    const tracer = trace.getTracer('AngularFrontendApp');
    currentSpan = tracer.startSpan(spanName, undefined, ROOT_CONTEXT);
    context.with(context.active(), () => currentSpan?.end()); // End span immediately after creation
    console.log("Created new span to propagate trace context:", currentSpan.spanContext().traceId);
  }

  if (currentSpan) {
    const spanContext = currentSpan.spanContext();
    const traceparent = `00-${spanContext.traceId}-${spanContext.spanId}-01`;
    console.log("Traceparent header added:", traceparent);
    return traceparent;
  }

  console.log("No active span found for traceparent");
  return null;
}

// Handle authorization errors
function handleAuthError(tokenService: TokenService, router: Router) {
  tokenService.logout();
  router.navigate(['/login']);
}
