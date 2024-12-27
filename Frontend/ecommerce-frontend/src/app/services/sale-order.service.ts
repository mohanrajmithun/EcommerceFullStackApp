import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SaleOrderDTO, SaleOrder } from '../model/SaleOrder.model'; // Adjust import path as needed
import { catchError, tap } from 'rxjs/operators';
import { of } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class SaleOrderDataService {

  private apiUrl = 'http://localhost:5004/api/SaleOrderDataService'; // Adjust to your actual API base URL

  constructor(private http: HttpClient) {}

  // Get Sale Order by Invoice Number
  getSaleOrder(invoicenumber: string): Observable<SaleOrderDTO> {
    return this.http.get<SaleOrderDTO>(`${this.apiUrl}/GetSaleOrder?invoicenumber=${invoicenumber}`)
    .pipe(
      tap(response => {
        // Log the response to the console
        console.log('Sale Order Response:', response);

        // Check if the response is null
        if (!response) {
          console.log('No sale order found for the given invoice number.');
        }
      })
    );
  } 

  // Get All Sale Orders
  getAllSaleOrders(): Observable<SaleOrderDTO[]> {
    return this.http.get<SaleOrderDTO[]>(`${this.apiUrl}/GetAllSaleOrder`);
  }

  // Get Order Total by Invoice Number
  getOrderTotal(invoicenumber: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/GetOrderTotal?invoicenumber=${invoicenumber}`);
  }

  // Get Sale Orders for a Customer by Customer ID
  getSaleOrderForCustomer(customerId: number): Observable<SaleOrder[]> {
    return this.http.get<SaleOrder[]>(`${this.apiUrl}/GetSaleOrderforCustomer?customerid=${customerId}`);
  }

  // Get Product IDs for an Invoice
  getProductIdsforInvoice(invoicenumber: string): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/GetProductIdsforInvoice?invoicenumber=${invoicenumber}`);
  }

  // Create a Sale Order
  createSaleOrder(saleOrder: SaleOrderDTO): Observable<SaleOrder> {
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.getBearerToken()}` });
    return this.http.post<SaleOrder>(`${this.apiUrl}/CreateSaleOrder`, saleOrder, { headers });
  }

  // Add Products to a Sale Order
  addProductsToSaleOrder(invoiceNumber: string, productId: number): Observable<SaleOrderDTO> {
    return this.http.post<SaleOrderDTO>(`${this.apiUrl}/AddProductsToSaleOrder?invoiceNumber=${invoiceNumber}&productid=${productId}`, {});
  }

  // Remove Products from a Sale Order
  removeProductsFromSaleOrder(invoiceNumber: string, productId: number): Observable<SaleOrderDTO> {
    return this.http.delete<SaleOrderDTO>(`${this.apiUrl}/RemoveProductsFromSaleOrder?invoiceNumber=${invoiceNumber}&productid=${productId}`);
  }

  // Update Delivery Address
  updateDeliveryAddress(invoiceNumber: string, deliveryAddress: string): Observable<SaleOrderDTO> {
    return this.http.patch<SaleOrderDTO>(`${this.apiUrl}/UpdateDeliveryAddress?invoiceNumber=${invoiceNumber}&deliveryAddress=${deliveryAddress}`, {});
  }

  // Update Order Status
  updateOrderStatus(invoiceNumber: string, orderStatus: string): Observable<SaleOrderDTO> {
    return this.http.get<SaleOrderDTO>(`${this.apiUrl}/UpdateOrderStatus?invoiceNumber=${invoiceNumber}&orderStatus=${orderStatus}`);
  }

  // Helper method to get the bearer token from storage
  private getBearerToken(): string {
    // Implement logic to get the token, e.g., from localStorage
    return localStorage.getItem('authToken') || '';
  }
}
