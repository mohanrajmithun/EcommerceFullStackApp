import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, throwError } from 'rxjs';
import {TokenService } from './token.service'
import { catchError } from 'rxjs/operators';
import { Console } from 'console';
import { CustomerService } from './customer.service'; 

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = 'https://localhost:7276/api/users'; // Adjust the URL to your backend
  DuplicateEmail = false;
  DuplicateUserName = false;
  BadCredentials = false;

  constructor(private http: HttpClient,private tokenService: TokenService, private customerService:CustomerService) {}

  login(userData: any): Observable<any> {
    console.log('Login request body:', userData); // Log the request body

    return this.http.post(`${this.baseUrl}/login`, userData).pipe(
      tap((response: any) => {
        // Assuming the token is in the 'token' field of the response
        const token = response.token;
        const Role = response.role;
        if (token) {
          this.tokenService.login(token,Role); // Save the token using TokenService
        }

        const customerId = response.customerId
        if(customerId){
          this.customerService.setCustomerId(customerId);

        }
      }),
      catchError((error) => {
        console.error('Error occurred during login:', error.error);
        return this.handleError(error);  // Call your error handling function
      })
    );
  }

  register(userData: any): Observable<any> {
    console.log('Registration request body:', userData); // Log the request body

    return this.http.post(`${this.baseUrl}/Register`, userData).pipe(
      tap((response: any) => {
        console.log('Registration response:', response); 
      }),
      catchError((error) => {
        console.error('Error occurred during registration:', error);
        return this.handleError(error);  // Call your error handling function
      })
    );  }

    private handleError(error: any): Observable<never> {
      if (error.error) {
        // Check if custom errors (DuplicateEmail, DuplicateUserName) exist in the error object
        console.log('Registration/login error:',error);
        
        const errorResponse = {
          DuplicateEmail: false,
          DuplicateUserName: false,
          BadCredentials: false
        };
    
        if (error.error) {
          console.log('Error response:', error);
    
          if (error.error.DuplicateEmail) {
            errorResponse.DuplicateEmail = true;
          }
    
          if (error.error.DuplicateUserName) {
            errorResponse.DuplicateUserName = true;
          }
    
          if (error.error.message === 'Bad Credentials') {
            errorResponse.BadCredentials = true;
          }
        }

        return throwError(errorResponse);

  
       
      }
  
      return throwError('An error occurred. Please try again.');
    }
}
