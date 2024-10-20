import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; // Use CommonModule instead of BrowserModule
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import {TokenService } from '../../services/token.service'
import { RegistrationRequest } from '../../model/RegistrationRequest'

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

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.resetFormAndErrors(); // Reset form and error flags on mode switch

  }

  onSubmit() {


    if (this.isLoginMode) {

      this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.errorMessage = null;
        this.tokenservice.userRole$.subscribe((role) => {
          this.isUserRoleadmin = role === 'Admin';
        });
        if(this.isUserRoleadmin){
          this.router.navigate(['/AdminPage']); // Or the route for your login page

        }
        else{
        this.router.navigate(['/products']);
        } // Or the route for your login page

      },
      error: (err) => {
        this.handleErrors(err);
        console.error(err);
      }
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
}



