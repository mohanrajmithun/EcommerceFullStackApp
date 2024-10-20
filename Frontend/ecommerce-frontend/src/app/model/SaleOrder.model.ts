export interface SaleOrderDTO {
  invoiceNumber: string;
  customerId: number;
  productIDs: number[];
  netTotal: number;
  tax:number;
  invoiceDate: Date;
    deliveryAddress: string;
    status: string;
    quantities: number[]
  }
  
  // export interface SaleOrderProduct {
  //   productId: number;
  //   productName: string;
  //   quantity: number;
  //   price: number;
  // }
  
  export interface SaleOrder {
    invoiceNumber: string;
    customerId: number;
    invoiceDate: Date;

    netTotal: number;
    deliveryAddress: string;
    status: string;
    // products: SaleOrderProduct[];
  }
  