import { Product } from "./product.model";

export interface CartDetailsInfoDTO {
  cartId: number;
  customerId: number;
  products: CartDetailsProductInfo[];
  totalPrice: number;
  deliveryAddress: string;
}

export interface CartDetailsProductInfo {
  product: Product;
  quantity: number;
  subtotal: number;
}


export interface AddProductRequest {
  customerId: number;
  productId: number;
  quantity: number;
}

export interface RemoveProductRequest {
  customerId: number;
  productId: number;
}

export interface UpdateProductQuantityRequest {
  customerId: number;
  productId: number;
  quantity: number;
}

export interface CheckoutRequest {
  customerId: number;
  deliveryAddress: string;
}
