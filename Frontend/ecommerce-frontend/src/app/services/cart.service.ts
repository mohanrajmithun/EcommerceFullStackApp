import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { AddProductRequest, CartDetailsInfoDTO, CheckoutRequest, RemoveProductRequest, UpdateProductQuantityRequest } from '../model/cart.model';
import { catchError, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = 'http://localhost:5003/api/Cart'; // Base URL of Cart API

  constructor(private http: HttpClient) {}

  // Fetch Cart
  getCart(customerId: number|null): Observable<CartDetailsInfoDTO> {
      return this.http.get<CartDetailsInfoDTO>(`${this.apiUrl}/${customerId}`).pipe(
        tap((response) => {
          console.log('Cart details fetched successfully:', response);
        }),
        catchError((error) => {
          console.error('Error fetching cart details:', error);
          return throwError(() => error); // Re-throw error after logging
        })
      );
    
    
  }

  // Add Product to Cart
  addProductToCart(request: AddProductRequest): Observable<CartDetailsInfoDTO> {
    return this.http.post<CartDetailsInfoDTO>(`${this.apiUrl}/add`, request);
  }

  // Remove Product from Cart
  removeProductFromCart(request: RemoveProductRequest): Observable<CartDetailsInfoDTO> {
    return this.http.post<CartDetailsInfoDTO>(`${this.apiUrl}/remove`, request);
  }

  // Update Product Quantity
  updateProductQuantity(request: UpdateProductQuantityRequest): Observable<CartDetailsInfoDTO> {
    return this.http.put<CartDetailsInfoDTO>(`${this.apiUrl}/update`, request);
  }

  // Checkout
  checkout(request: CheckoutRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/checkout`, request).pipe(
      tap((response) => {
        console.log('Checkout successful:', response);
      }),
      catchError((error) => {
        console.error('Checkout error:', error);
        return throwError(() => error);
      })
    );
  }
}
