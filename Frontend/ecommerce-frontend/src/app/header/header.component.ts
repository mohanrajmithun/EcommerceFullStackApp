import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterModule, Router } from '@angular/router';
import { ProductDataApiService } from '../services/product-data-api.service';
import { ProductCategory } from '../model/ProductCategory';
import { TokenService } from '../services/token.service';
import { CustomerService } from '../services/customer.service';

@Component({
  selector: 'app-header',
  standalone: true, 
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  categoriesVisible = false;
  categories: ProductCategory[] = [];
  error: string | null = null;
  isLoggedIn = false;
  isUserRoleadmin = false;

  constructor(
    private productService: ProductDataApiService, 
    private tokenService: TokenService,
    private router: Router,
    private customerService:CustomerService
  ) {}

  ngOnInit(): void {
    this.fetchcategories();
    this.tokenService.isAuthenticated$.subscribe((status) => {
      this.isLoggedIn = status; // Update the login status dynamically
    });
    this.tokenService.userRole$.subscribe((role) => {
      this.isUserRoleadmin = role === 'Admin';
    });  }

  fetchcategories() {
    this.productService.getProductCategories().subscribe({
      next: (categories) => {
        this.categories = categories.map(category => ({
          ...category
        }));
      },
      error: (err) => {
        this.error = 'Failed to load categories';
      }
    });
  }

  toggleCategories() {
    this.categoriesVisible = !this.categoriesVisible;
  }

  selectCategory(id: number) {
    console.log('Selected category:', id);
    // this.router.navigate([$'/products/category/${categoryId}']); // Redirect to login page

  }

  checkLoginStatus() {
    this.isLoggedIn = this.tokenService.isLoggedIn();
    this.isUserRoleadmin = this.tokenService.isUserRoleAdmin();

  }

  logout() {
    this.tokenService.logout(); // Clear the token from TokenService
    this.customerService.clearCustomerId();
    this.isLoggedIn = false;
    this.isUserRoleadmin = false;
    this.router.navigate(['/login']); // Redirect to login page
  }
}
