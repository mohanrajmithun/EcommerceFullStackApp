import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { HttpClientModule } from '@angular/common/http';
import { provideHttpClient, withFetch, withInterceptors  } from '@angular/common/http';
import { authInterceptor  } from './app/Interceptor/auth.interceptor';


import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideHttpClient(withInterceptors([authInterceptor ])), // Provide routes here
    importProvidersFrom(HttpClientModule) // Provide HttpClientModule here

  ]
}).catch(err => console.error(err));