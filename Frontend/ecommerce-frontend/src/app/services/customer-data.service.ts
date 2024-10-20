import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, throwError } from 'rxjs';
import {TokenService } from './token.service'
import { catchError } from 'rxjs/operators';
import { Console } from 'console';
import { CustomerService } from './customer.service'; 
import { Customer } from '../model/Customer.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerDataService {
  private baseUrl = 'https://localhost:7276/api/CustomerData'; // Adjust the URL to your backend
  DuplicateEmail = false;
  DuplicateUserName = false;
  BadCredentials = false;

  constructor(private http: HttpClient) { }

  fetchCustomerById(customerId: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.baseUrl}/customer?id=${customerId}`);
  }

}
