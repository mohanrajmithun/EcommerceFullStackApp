import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { ProductDetailsComponent } from './components/product-details/product-details.component';
import { AuthComponent } from './components/auth/auth.component';
import { CartComponent } from './components/cart/cart.component';
import { OrderStatusComponent } from './components/order-status/order-status.component';
import { AdminComponent } from './components/admin/admin.component';
import { ViewSaleOrdersComponent } from './components/view-sale-orders/view-sale-orders.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'home', redirectTo: '/products', pathMatch: 'full' },
  { path: 'products', component: ProductListComponent },
  { path: 'product/:id', component: ProductDetailsComponent },
  { path: 'login', component: AuthComponent },
  { path: 'register', component: AuthComponent },
  { path: 'cart', component: CartComponent },
  { path: 'products/category/:categoryId', component: ProductListComponent },
  { path: 'order-status', component: OrderStatusComponent }, 
  {path: 'AdminPage', component: AdminComponent},
  {path: 'myorders', component:ViewSaleOrdersComponent},// Products by category
];
