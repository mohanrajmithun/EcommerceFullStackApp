import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductDataApiService } from '../../services/product-data-api.service';
import { Product } from '../../model/product.model';
import { CommonModule } from '@angular/common'; // Use CommonModule instead of BrowserModule
import { mapProductColour, mapProductSize } from '../../Enums/Product/Product_enums';
import { CartService } from '../../services/cart.service';
import { CustomerService } from '../../services/customer.service';
import { AddProductRequest } from '../../model/cart.model';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-product-details',
  standalone: true, // Mark as standalone component
  imports: [CommonModule],
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css']
})
export class ProductDetailsComponent implements OnInit {

  product: Product | null = null;
  loading = true;
  error: string | null = null;
  customerId: number | null = null;
  IsProductAdded: boolean = false; // Boolean flag to show success message
  IsErrorAddingProduct:boolean = false;
  IsLoading:boolean = false;
  IsOutOfStock:boolean = false;


  constructor(
    private route: ActivatedRoute,
    private productService: ProductDataApiService,
    private cartservice: CartService,
    private customerService : CustomerService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.cdr.detectChanges();
    this.productService.getProductDetails(id).subscribe({
      next: (product) => {
        this.product = product;
        this.loading = false;
        this.product.productColor = mapProductColour(this.product.productColor); // Assuming 'color' is the property
      this.product.productSize = mapProductSize(this.product.productSize);
      },
      error: () => {
        this.error = "Product not found";
        this.loading = false;
      }
      
    });

}

mapProductColour(value: string): string {
    
  return mapProductColour(value);
}

mapProductSize(value: string): string {
  return mapProductSize(value);
}

AddProductToCart(productId: number, quantity: number) {
  // Retrieve the customer ID from the CustomerService
  this.customerId = this.customerService.getCustomerId();

  if (this.customerId !== null) {
    // Create the AddProductRequest object
    const request: AddProductRequest = {
      customerId: this.customerId, // Use the customerId from the service
      productId: productId,
      quantity: quantity
    };

    this.IsProductAdded = false;
  this.IsErrorAddingProduct = false;
  this.IsLoading = true; // Start loading


    // Send the request to the CartService
    this.cartservice.addProductToCart(request).subscribe({
      next: (response) => {
        this.IsProductAdded = true; // Set to true on success

        console.log('Product added to cart successfully', response);
        // Handle successful addition here (e.g., updating UI)

        setTimeout(() => {
          this.IsProductAdded = false;
        }, 3000);
      },
      error: (error) => {
        this.IsLoading = false; // Stop loading

        console.error('Error adding product to cart', error);
        this.IsErrorAddingProduct = true;

        setTimeout(() => {
          this.IsErrorAddingProduct = false;
        }, 3000);
        // Handle error here (e.g., show error message)
      }
    });
  } else {
    this.IsProductAdded = false; // Set to true on success

    console.error('Customer ID not found');
    // Handle the case where customer ID is null
  }
}
}
