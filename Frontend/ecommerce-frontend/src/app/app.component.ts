import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common'; // Use CommonModule instead of BrowserModule
import { ProductDataApiService } from './services/product-data-api.service';
import { ProductListComponent } from './components/product-list/product-list.component';
import { ProductDetailsComponent } from './components/product-details/product-details.component';
import { RouterModule } from '@angular/router'; // Import RouterModule for routing
import { HeaderComponent } from './header/header.component';
import { AuthComponent } from './components/auth/auth.component';
import { CartComponent } from './components/cart/cart.component';
import { OrderStatusComponent } from './components/order-status/order-status.component';
import { AdminComponent } from './components/admin/admin.component';
import { ViewSaleOrdersComponent } from './components/view-sale-orders/view-sale-orders.component';





@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule,RouterOutlet,CommonModule,ProductListComponent,ProductDetailsComponent,HeaderComponent,AuthComponent,CartComponent,OrderStatusComponent,AdminComponent, ViewSaleOrdersComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'ecommerce-frontend';

  constructor(private productservice:ProductDataApiService){}
}
