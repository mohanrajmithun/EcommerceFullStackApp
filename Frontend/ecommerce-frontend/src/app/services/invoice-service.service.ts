import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private apiBaseUrl = 'https://localhost:7262/api/invoices'; // Update to match your API endpoint

  constructor(private http: HttpClient) {}

  // Fetch the invoice for the specified customer ID
  getInvoiceByNumber(invoiceNumber: string): Observable<Blob> {
    return this.http.get(`${this.apiBaseUrl}/GetInvoiceAsPDF?InvoiceNumber=${invoiceNumber}`, { responseType: 'blob' });
  }
}
