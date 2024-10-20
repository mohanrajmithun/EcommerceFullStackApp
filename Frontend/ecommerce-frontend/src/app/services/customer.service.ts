import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  // Private customerId property, not exposed to the UI
  private customerId: number  = 0;

  constructor() { }

  // Method to store customerId securely
  setCustomerId(customerId: number): void {
    this.customerId = customerId;
    localStorage.setItem('customerId', customerId.toString()); // Store it in localStorage for persistence
  }

  // Method to retrieve customerId internally within the app
  getCustomerId(): number  {
    if (this.customerId === 0) {
      const storedId = localStorage.getItem('customerId');
      this.customerId = storedId ? +storedId : 0;
    }
    return this.customerId;
  }

  // Method to clear customerId upon logout
  clearCustomerId(): void {
    this.customerId = 0;
    localStorage.removeItem('customerId'); // Clear it from storage
  }
}
