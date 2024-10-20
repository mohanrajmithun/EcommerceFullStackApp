export interface Product {
  productId: number;
  productName: string;
  productCode: string;
  productColor: string;
  productSize: string;
  price: number;
  categoryId: number;
  stockQuantity: number;  // assuming there's stock data
  imageName: string;
}
