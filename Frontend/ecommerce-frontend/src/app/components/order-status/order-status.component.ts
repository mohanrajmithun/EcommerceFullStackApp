import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { SaleOrder } from '../../model/SaleOrder.model';
import { CommonModule } from '@angular/common';
import { SaleOrderDTO } from '../../model/SaleOrder.model'; // Adjust import path as needed
import { SaleOrderDataService } from '../../services/sale-order.service'; // Import the SaleOrderDataService
import { mapOrderStatus, OrderStatus } from '../../Enums/OrderStatus';
import { Product } from '../../model/product.model';
import { ProductDataApiService } from '../../services/product-data-api.service';
import { Observable, forkJoin } from 'rxjs';
import { InvoiceService } from '../../services/invoice-service.service';




@Component({
  selector: 'app-order-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-status.component.html',
  styleUrl: './order-status.component.css'
})
export class OrderStatusComponent implements OnInit {
  saleOrder: SaleOrderDTO | null = null;

  invoiceNumber!: string;
  customerId!: number;
  orderStatus: string  | undefined;
  errorMessage: string = '';
  orderStatusDisplay!: string; // For displaying the mapped order status
  isProcessing: boolean = false;
  processingMessage: string = 'Your order is being Cancelled. Please do not refresh the page.'; // Message to display
  productsWithQuantity: { product: Product, quantity: number }[] = []; // Array to store products with quantity




  constructor(private route: ActivatedRoute,private saleOrderService: SaleOrderDataService,
    private productService: ProductDataApiService,
    private invoiceService: InvoiceService // Inject the InvoiceService


  ) {}

  ngOnInit(): void {
    // Fetch invoice number from query parameters
    this.route.queryParams.subscribe(params => {
      this.invoiceNumber = params['invoiceNumber'];
      
      if (this.invoiceNumber) {
        this.fetchOrderStatus(this.invoiceNumber);


      }
    });
  }

  // Method to fetch the order status from the database using SaleOrderDataService
  fetchOrderStatus(invoiceNumber: string): void {
    this.saleOrderService.getSaleOrder(invoiceNumber).subscribe({
      next: (saleOrder) => {
        this.saleOrder = saleOrder; // Store the fetched sale order

        if (this.saleOrder) {
          this.orderStatus = this.saleOrder.status; // Adjust property name if necessary
          this.orderStatusDisplay = mapOrderStatus(this.orderStatus) || 'Unknown';
        }

        this.fetchProductDetails(this.saleOrder.productIDs, this.saleOrder.quantities);

        this.isProcessing = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to fetch order details. Please try again later.';
        console.error('Error fetching order details:', err);
        this.isProcessing = false;

      }
    });
  }

  fetchProductDetails(productIDs: number[], quantities: number[]): void {
    const productDetailRequests = productIDs.map(id => 
      this.productService.getProductDetails(id)
    );

    // Combine all product detail fetch calls
    forkJoin(productDetailRequests).subscribe({
      next: (products) => {
        // Combine product details with quantities
        this.productsWithQuantity = products.map((product, index) => {
          return { product, quantity: quantities[index] };
        });
      },
      error: (err) => {
        console.error('Error fetching product details:', err);
      }
    });
  }

  // In your component TypeScript file
  cancelOrder() {
    if (!this.saleOrder || !this.saleOrder.invoiceNumber) {
      this.errorMessage = 'Unable to cancel order: invalid order data.';
      return;
    }

    this.isProcessing = true;


    this.saleOrderService.updateOrderStatus(this.saleOrder.invoiceNumber, 'Cancelled').subscribe(
      (updatedOrder) => {
        // Update the order status to "Cancelled" in the UI
        this.orderStatusDisplay = mapOrderStatus(updatedOrder.status) || 'Unknown';
        this.isProcessing = false;

                console.log('Order has been cancelled successfully');
      },
      (error) => {
        // Handle any errors during the cancellation process
        console.error('Error cancelling order:', error);
        this.errorMessage = 'Failed to cancel the order. Please try again later.';
      }
    );
  }


  downloadInvoice(invoiceNumber: string): void {
    this.invoiceService.getInvoiceByNumber(invoiceNumber).subscribe(response => {
      // Blob response already contains the PDF file
      const blob = response;
      const url = URL.createObjectURL(blob);

      // Create a link element and simulate a click to start the download
      const link = document.createElement('a');
      link.href = url;
      link.download = `${invoiceNumber}.pdf`;
      link.click();

      // Release the URL object after the download starts
      URL.revokeObjectURL(url);
    }, error => {
      console.error('Error downloading PDF', error);
    });
  }
  

}