import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; // Use CommonModule instead of BrowserModule
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import {TokenService } from '../../services/token.service'
import { RegistrationRequest } from '../../model/RegistrationRequest'
import { Subscription, timer } from 'rxjs';

@Component({
  selector: 'app-auth',
  standalone: true, // Mark as standalone component
  imports: [CommonModule,RouterModule,ReactiveFormsModule],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  loginForm: FormGroup;
  registerForm: FormGroup;
  sessionTimeout$: Subscription | null = null; // For session timeout
  isLoginMode = true; // Switch between login and register
  errorMessage: string | null = null;
  registrationSuccessful = false;
  registrationMessage : string | null = null;
  registrationrequest: RegistrationRequest = { // Initialize the variable here
    Email: '',
    username: '',
    password: '',
    FullName:'',
    Address:'',
    Phone_Number: '',
    Role:'User'
  };
  duplicateEmail = false;
  duplicateUserName = false;
  badCredentials = false;
  isUserRoleadmin = false;
  private isSessionExpired = false; 




  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router, private tokenservice:TokenService) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
      fullName: ['', [Validators.required]],
      address: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]],


    });


  }
  ngOnInit() {
    // Automatically log out when the session expires
    this.tokenservice.isAuthenticated$.subscribe((isAuthenticated) => {
      if (!isAuthenticated && !this.isSessionExpired) {
        this.isSessionExpired = true; // Set the flag to true
        this.handleSessionExpiry();
      }
    });

    // Optional: Check token expiry periodically
    this.startSessionCheck();
  }

  ngOnDestroy() {
    this.sessionTimeout$?.unsubscribe(); // Clear subscription
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.resetFormAndErrors(); // Reset form and error flags on mode switch

  }

  onSubmit() {
    if (this.isLoginMode) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          this.tokenservice.saveToken(response.token); // Save the token
          this.tokenservice.SetUserRole(response.role); // Set the user role
          this.startSessionCheck(); // Start session expiry check

          // Navigate based on user role
          this.tokenservice.userRole$.subscribe((role) => {
            this.isUserRoleadmin = role === 'Admin';
            this.router.navigate([this.isUserRoleadmin ? '/AdminPage' : '/products']);
          });
        },
        error: (err) => {
          this.errorMessage = 'Invalid credentials. Please try again.';
        },
      });
    } else {

    if(this.registerForm.valid){
      this.registrationrequest.Email =  this.registerForm.value.email,
      this.registrationrequest.username = this.registerForm.value.email,
      this.registrationrequest.password = this.registerForm.value.password,
      this.registrationrequest.FullName = this.registerForm.value.fullName,
      this.registrationrequest.Address = this.registerForm.value.address,
      this.registrationrequest.Phone_Number = this.registerForm.value.phoneNumber
      this.registrationrequest.Role = 'User'

      }
    



      this.authService.register(this.registrationrequest).subscribe({
        next: (response) => {
          this.registrationSuccessful = true;
          this.registrationMessage = 'Registration successful! Please go to login.';
          this.resetErrorFlags(); // Reset error flags
        },
        error: (error) => {
          this.handleErrors(error); // Capture API errors
        }
      });
    }
  }

 

  private handleErrors(error: any) {
    this.resetErrorFlags();
    this.resetSuccessfulResponse();

    // Set flags based on the error response
    this.duplicateEmail = error.DuplicateEmail;
    this.duplicateUserName = error.DuplicateUserName;
    this.badCredentials = error.BadCredentials;
    this.errorMessage = 'An error occurred. Please check the details.';
  }

  private resetErrorFlags() {
    this.duplicateEmail = false;
    this.duplicateUserName = false;
    this.badCredentials = false;
    this.errorMessage = null; // Clear the error message
    
  }

  private resetSuccessfulResponse(){
    this.registrationMessage = null;
    this.registrationSuccessful = false;

  }

  private resetFormAndErrors() {
    this.loginForm.reset(); // Reset the login form
    this.registerForm.reset(); // Reset the registration form
    this.resetErrorFlags(); // Reset all error flags and messages
    this.resetSuccessfulResponse();
  }

  private startSessionCheck() {
    const token = this.tokenservice.getToken();
    if (token) {
      const expiryTime = this.tokenservice.getTokenExpiry(token);
      const currentTime = Date.now();
      const timeoutDuration = expiryTime - currentTime;

      if (timeoutDuration > 0) {
        this.sessionTimeout$?.unsubscribe(); // Clear previous timeout
        this.sessionTimeout$ = timer(timeoutDuration).subscribe(() => {
          this.handleSessionExpiry();
        });
      } else {
        this.handleSessionExpiry(); // If already expired
      }
    }
  }

  private handleSessionExpiry() {
    if (!this.isSessionExpired) {
      this.isSessionExpired = true; // Set the flag to true
      this.tokenservice.logout(); // Clear token and update state
      alert('Your session has expired. Please log in again.');
      this.router.navigate(['/login']); // Redirect to login
    }
  }
}



