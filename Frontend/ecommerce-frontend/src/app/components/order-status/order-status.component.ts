import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { SaleOrder } from '../../model/SaleOrder.model';
import { CommonModule } from '@angular/common';
import { SaleOrderDTO } from '../../model/SaleOrder.model'; // Adjust import path as needed
import { SaleOrderDataService } from '../../services/sale-order.service'; // Import the SaleOrderDataService
import { mapOrderStatus, OrderStatus } from '../../Enums/OrderStatus';



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
  isProcessing: boolean = true; // New property to track processing state
  processingMessage: string = 'Your order is being processed. Please do not refresh the page.'; // Message to display



  constructor(private route: ActivatedRoute,private saleOrderService: SaleOrderDataService) {}

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
        this.isProcessing = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to fetch order details. Please try again later.';
        console.error('Error fetching order details:', err);
        this.isProcessing = false;

      }
    });
  }
}