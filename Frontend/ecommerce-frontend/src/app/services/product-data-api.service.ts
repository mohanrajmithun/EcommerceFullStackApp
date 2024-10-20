import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../model/product.model';
import { tap, catchError, throwError } from 'rxjs';
import { ProductCategory } from '../model/ProductCategory';


@Injectable({
  providedIn: 'root'
})
export class ProductDataApiService {

  private apiUrl = 'https://localhost:7101/api/ProductDataAPI'; // Update this with your API base URL

  constructor(private http: HttpClient) { }

  getAllProducts(): Observable<Product[]> {
    console.log('Fetching all products from API...');

    return this.http.get<Product[]>(`${this.apiUrl}/Products`).pipe(
      tap(products => {
        console.log('Products fetched successfully:', products);
      }),
      catchError(err => {
        console.error('Error fetching products:', err);
        return throwError(() => err);
      })
    );
  }
  getProductDetails(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/Product?id=${id}`).pipe(
      tap(product => {
        console.log('product fetched successfully:', product);
      }),
      catchError(err => {
        console.error('Error fetching product:', err);
        return throwError(() => err);
      })
    );
  }

  getProductCategories() : Observable<ProductCategory[]>{
    console.log('Fetching all product categories from API...');

    return this.http.get<ProductCategory[]>(`${this.apiUrl}/GetProductCategories`).pipe(
      tap(productcategories => {
        console.log('product categories fetched successfully:', productcategories);
      }),
      catchError(err => {
        console.error('Error fetching product categories:', err);
        return throwError(() => err);
      })
    );
  }

  getProductsByCategories(CategoryId:number) : Observable<Product[]>{
    console.log('Fetching all products for categories from API...');

    return this.http.get<Product[]>(`${this.apiUrl}/GetProductsByCategories?CategoryId=${CategoryId}`).pipe(
      tap(products => {
        console.log('product categories fetched successfully:', products);
      }),
      catchError(err => {
        console.error('Error fetching product categories:', err);
        return throwError(() => err);
      })
    );
  }

  checkStockAvailability(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/IsProductOutOfStock?id=${id}`);
  }


  updateStockQuantity(id: number, count: number): Observable<Product> {
    console.log(`Updating stock quantity for product ID: ${id} to ${count}...`);
    return this.http.put<Product>(`${this.apiUrl}/UpdateStockQuantity?id=${id}&count=${count}`, null).pipe(
      tap(product => {
        console.log('Stock quantity updated successfully:', product);
      }),
      catchError(err => {
        console.error('Error updating stock quantity:', err);
        return throwError(() => err);
      })
    );
  }

  // Add a new product
  addProduct(product: Product): Observable<Product> {
    console.log('Adding new product:', product);
    return this.http.post<Product>(`${this.apiUrl}/AddProduct`, product).pipe(
      tap(newProduct => {
        console.log('Product added successfully:', newProduct);
      }),
      catchError(err => {
        console.error('Error adding product:', err);
        return throwError(() => err);
      })
    );
  }
}
