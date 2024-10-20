import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterModule, Router } from '@angular/router';
import { ProductDataApiService } from '../../services/product-data-api.service';
import { SaleOrderDataService } from '../../services/sale-order.service'; 
import { SaleOrderDTO } from '../../model/SaleOrder.model';
import { ProductCategory } from '../../model/ProductCategory';
import { TokenService } from '../../services/token.service';
import { CustomerService } from '../../services/customer.service';
import { Product } from '../../model/product.model';
import { FormsModule } from '@angular/forms';
import { mapOrderStatus, OrderStatus } from '../../Enums/OrderStatus';
import { CustomerDataService } from '../../services/customer-data.service';
import { mapProductColour, mapProductSize, ProductColour, ProductSize } from '../../Enums/Product/Product_enums';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  MapOrderStatus(status: string) {
    return mapOrderStatus(status);
    }

  orders: SaleOrderDTO[] = []; 
  products: Product[] = []; 
  categories: ProductCategory[] = []; 
  isUserRoleadmin = false;
  updatedOrderId: string | null = null; 
  showUpdatePopup = false; 
  customers: { [key: number]: string } = {}; 
  activeTab: 'salesOrders' | 'productInventory' = 'salesOrders'; // Default tab

  productColours = Object.values(ProductColour);
  productSizes = Object.values(ProductSize);


  newProduct: Product = {
    productId: 0, 
    productName: '',
    productCode: '',
    price: 0,
    stockQuantity: 0,
    categoryId: 0, 
    productColor: '', 
    productSize: '',  
    imageName: ''     
  };

  isAddProductDisabled = true;

  
  orderStatuses = ['Created', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Completed', 'Pending'];

  constructor(
    private productService: ProductDataApiService,
    private orderService: SaleOrderDataService,
    private customerService: CustomerService,
    private tokenService: TokenService,
    private customerdataservice: CustomerDataService,
  ) {}

  ngOnInit(): void {
    this.fetchOrders();
    this.tokenService.userRole$.subscribe((role) => {
      this.isUserRoleadmin = role === 'Admin';
    });
    this.fetchProducts();
    this.fetchCategories();
  }

  selectTab(tab: 'salesOrders' | 'productInventory') {
    this.activeTab = tab;
    this.fetchProducts();
    this.fetchCategories();
  }

  fetchOrders() {
    this.orderService.getAllSaleOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.orders.forEach(order => {
          this.fetchCustomerName(order.customerId);
        });
      },
      error: (err) => {
        console.error('Error fetching orders:', err);
      }
    });
  }

  onColourChange(event: Event) {
    const target = event.target as HTMLSelectElement; // Cast to HTMLSelectElement
    this.newProduct.productColor = mapProductColour(target.value);
  }

  // Update the method to cast the event target
  onSizeChange(event: Event) {
    const target = event.target as HTMLSelectElement; // Cast to HTMLSelectElement
    this.newProduct.productSize = mapProductSize(target.value);
  }

  fetchCustomerName(customerId: number) {
    if (!this.customers[customerId]) { 
      this.customerdataservice.fetchCustomerById(customerId).subscribe({
        next: (customer) => {
          this.customers[customerId] = `${customer.customerName}`; 
        },
        error: (err) => {
          console.error('Error fetching customer:', err);
        }
      });
    }
  }

  fetchProducts() {
    this.productService.getAllProducts().subscribe({
      next: (data) => {
        this.products = data;
        console.log('products:',data)
      },
      error: (err) => {
        console.error('Error fetching products:', err);
      }
    });
  }

  fetchCategories() {
    this.productService.getProductCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Error fetching categories:', err);
      }
    });
  }

  updateOrderStatus(invoiceNumber: string, status: string) {
    this.orderService.updateOrderStatus(invoiceNumber, status).subscribe(
      (response) => {
        console.log(`Order ${invoiceNumber} status updated to ${status}`);
        this.updatedOrderId = invoiceNumber; 
        this.showUpdatePopup = true; 

        setTimeout(() => {
          this.showUpdatePopup = false;
        }, 3000);
        
        this.fetchOrders(); // Refresh orders after update
      },
      (error) => {
        console.error('Error updating order status:', error);
      }
    );
  }

  updateStock(productId: number, newStockQuantity: number) {
    this.productService.updateStockQuantity(productId, newStockQuantity).subscribe({
      next: () => {
        console.log(`Product ${productId} stock updated to ${newStockQuantity}`);
        this.fetchProducts(); // Refresh products after update
      },
      error: (err) => {
        console.error('Error updating stock:', err);
      }
    });
  }

  selectCategory(event: Event) {
    const selectElement = event.target as HTMLSelectElement; // Cast to HTMLSelectElement
    const selectedCategoryId = Number(selectElement.value); // Get the value as a number
    this.newProduct.categoryId = selectedCategoryId; // Set the category ID
  }
  

  resetNewProduct() {
    this.newProduct = {
      productId: 0,
      productName: '',
      productCode: '',
      price: 0,
      stockQuantity: 0,
      categoryId: 0,
      productColor: '',
      productSize: '',
      imageName: ''
    };
  }

  addNewProduct() {
    this.productService.addProduct(this.newProduct).subscribe({
      next: (addedProduct) => {
        console.log('New Product Added:', addedProduct);
        this.resetNewProduct(); // Reset after successful addition
        this.fetchProducts(); // Reload products
      },
      error: (err) => {
        console.error('Error adding new product:', err);
      }
    });
  }
}
