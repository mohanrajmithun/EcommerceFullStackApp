export enum OrderStatus {
    Created = 'Created',
    Processing = 'Processing',
    Shipped = 'Shipped',
    Delivered = 'Delivered',
    Cancelled = 'Cancelled',
    Completed = 'Completed',
    Pending = 'Pending'
  }
  
  export function mapOrderStatus(status: number | string): string {
    // Map the number values to corresponding enum values
    const statusMap: { [key: number]: OrderStatus } = {
      0: OrderStatus.Created,
      1: OrderStatus.Processing,
      2: OrderStatus.Shipped,
      3: OrderStatus.Delivered,
      4: OrderStatus.Cancelled,
      5: OrderStatus.Completed,
      6: OrderStatus.Pending
    };
  
    // Ensure status is interpreted as a number if it's a string
    const statusNumber = typeof status === 'string' ? parseInt(status, 10) : status;
  
    // Map and return the corresponding status, or 'Unknown' if invalid
    return statusMap[statusNumber] || 'Unknown';
  }
  