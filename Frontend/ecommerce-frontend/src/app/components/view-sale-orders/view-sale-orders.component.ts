import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterModule, Router } from '@angular/router';
import { ProductDataApiService } from '../../services/product-data-api.service';
import { SaleOrderDataService } from '../../services/sale-order.service'; // Assuming you have this service for orders
import { SaleOrder, SaleOrderDTO } from '../../model/SaleOrder.model';
import { ProductCategory } from '../../model/ProductCategory';
import { TokenService } from '../../services/token.service';
import { CustomerService } from '../../services/customer.service';
import { Product } from '../../model/product.model';
import { FormsModule } from '@angular/forms';
import { mapOrderStatus, OrderStatus } from '../../Enums/OrderStatus';
import { CustomerDataService } from '../../services/customer-data.service';
import { Customer } from '../../model/Customer.model';
@Component({
  selector: 'app-view-sale-orders',
  standalone: true,
  imports: [CommonModule,RouterModule,FormsModule],
  templateUrl: './view-sale-orders.component.html',
  styleUrl: './view-sale-orders.component.css'
})
export class ViewSaleOrdersComponent implements OnInit{
  MapOrderStatus(status: string) {
    return mapOrderStatus(status);
    }

  orders: SaleOrder[] = []; // Fetch these from your backend
  products: Product[] = []; // Fetch these from your backend
  isUserRoleadmin = false;
  customerId!: number;
  customer: Customer | undefined;


  orderStatuses = ['Created', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Completed', 'Pending'];

  constructor(
    private productService: ProductDataApiService,
    private orderService: SaleOrderDataService,
    private customerService: CustomerService,
    private tokenService: TokenService,
    private customerdataservice: CustomerDataService,
  ) {}

  ngOnInit(): void {
    this.customerId = this.customerService.getCustomerId();
    this.fetchOrders();
    this.tokenService.userRole$.subscribe((role) => {
      this.isUserRoleadmin = role === 'Admin';
    });
    this.fetchCustomerDetails();
    // this.fetchProducts();
    // this.fetchCategories();
  }

  fetchOrders() {
    // Call API to fetch orders

    this.orderService.getSaleOrderForCustomer(this.customerId).subscribe({
      next: (data) => {
        this.orders = data;
        
      },
      error: (err) => {
        console.error('Error fetching orders:', err);
      }
    });
  }

  fetchCustomerDetails()
  {
    this.customerdataservice.fetchCustomerById(this.customerId).subscribe({
      next:(customer)=>
      {
        this.customer = customer;
      },error: (err) => {
        console.error('Error fetching customer details:', err);
      }
    });
  }



}

