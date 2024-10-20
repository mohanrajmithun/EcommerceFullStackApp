import { Component, OnInit } from '@angular/core';
import { Product } from '../../model/product.model';
import { CommonModule } from '@angular/common'; // Use CommonModule instead of BrowserModule
import { ProductDataApiService } from '../../services/product-data-api.service';
import { RouterModule } from '@angular/router';  // Import RouterModule
import { mapProductColour, mapProductSize } from '../../Enums/Product/Product_enums';
import { ActivatedRoute } from '@angular/router';





@Component({
  selector: 'app-product-list',
  standalone: true, // Mark as standalone component
  imports: [CommonModule,RouterModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {

  products: Product[] = [];
  loading = true;
  error: string | null = null;
  categoryId: number | null = null;


  constructor(private productService: ProductDataApiService,    private route: ActivatedRoute // Inject ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.categoryId = params['categoryId'] ? +params['categoryId'] : null;
      if (this.categoryId) {
        this.fetchProductsByCategory(this.categoryId);
      } else {
        this.fetchProducts();
      }
    });  }

  fetchProducts() {
    this.productService.getAllProducts().subscribe({
      next: (products) => {
        this.products = products.map(product => ({ 
          ...product
        }));
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load products';
        this.loading = false;
      }
    });



  }
  fetchProductsByCategory(categoryId: number) {
    this.productService.getProductsByCategories(categoryId).subscribe({
      next: (products) => {
        this.products = products;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load products by category';
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

  getColor(productColor: string): string {
    productColor = productColor.toString()
    switch (productColor) {
      case "0": return 'black';
      case "1": return 'white';
      case "2": return 'green';
      case "3": return 'blue';
      case "4": return 'brown';
      case "5": return 'yellow';
      case "6": return 'red';
      default: return 'black'; // fallback color if value is out of range
    }
  }

  }

