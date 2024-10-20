import { Component, OnInit } from '@angular/core';
import { CartService } from '../../services/cart.service';
import { CartDetailsInfoDTO, AddProductRequest, RemoveProductRequest, UpdateProductQuantityRequest, CheckoutRequest } from '../../model/cart.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';  // Import FormsModule
import { CustomerService } from '../../services/customer.service';
import { ProductDataApiService } from '../../services/product-data-api.service';
import { forkJoin } from 'rxjs'; // Import forkJoin
import { Router } from '@angular/router';


@Component({
  selector: 'app-cart',
  standalone:true,
  imports:[CommonModule,FormsModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cart: CartDetailsInfoDTO | null = null;
  customerId: number  = 0; // Hardcoded customer ID for demonstration
  outOfStockStatus: boolean[] = []; // Array to track out-of-stock status
 isProcessing: boolean = false;
 processingMessage: string = 'Your order is being processed. Please do not refresh the page.'; // Message to display


  constructor(private cartService: CartService, private customerService: CustomerService, private productService: ProductDataApiService,private router: Router) {}

  ngOnInit(): void {
    this.customerId = this.customerService.getCustomerId();
    this.loadCart();
  }

  loadCart() {
    this.customerId = this.customerService.getCustomerId();

    this.cartService.getCart(this.customerId).subscribe(cart => {
      this.cart = cart
      this.checkProductStock(cart.products);
    });
  }

  checkProductStock(products: { product: { productId: number } }[]) {
    const requests = products.map(productInfo =>
      this.productService.getProductDetails(productInfo.product.productId)
    );

    forkJoin(requests).subscribe(productsDetails => {
      // Clear previous status
      this.outOfStockStatus = productsDetails.map(product => product.stockQuantity < 1);
    });
  }

  removeProduct(productId: number) {
    this.customerId = this.customerService.getCustomerId();

    const request: RemoveProductRequest = {
      customerId: this.customerId,
      productId: productId
    };
    this.cartService.removeProductFromCart(request).subscribe(cart => {
      this.cart = cart;
    });
  }

  updateQuantity(productId: number, quantity: number) {
    this.customerId = this.customerService.getCustomerId();

    const request: UpdateProductQuantityRequest = {
      customerId: this.customerId,
      productId: productId,
      quantity: quantity
    };
    this.cartService.updateProductQuantity(request).subscribe(cart => {
      this.cart = cart;
    });
  }

  checkout() {
    if (this.cart) {
      this.customerId = this.customerService.getCustomerId();

      const request: CheckoutRequest = {
        customerId: this.customerId,
        deliveryAddress: this.cart.deliveryAddress || 'Default Address'
      };

      this.isProcessing = true;

      this.cartService.checkout(request).subscribe(response => {
        if (response.isCheckedOut === 1) {
          // Pass the SaleOrder object through router state
          this.router.navigate(['/order-status'], {
            queryParams: {
              invoiceNumber: response.invoiceNumber
              // customerId: response.customerId,
              // orderStatus: response.orderStatus
            }
          });
        } else {
          alert('Checkout failed');
        }
      });
    }
  }
}
