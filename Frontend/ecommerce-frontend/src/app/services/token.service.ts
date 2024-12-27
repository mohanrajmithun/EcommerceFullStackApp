import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {jwtDecode} from 'jwt-decode';


@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private tokenKey = 'authToken';
  private roleKey = 'userRole'; // New key for the role
  private userRole = new BehaviorSubject<string | null>(this.getRoleFromLocalStorage());

  private isAuthenticated = new BehaviorSubject<boolean>(this.hasToken());
  public userRole$ = this.userRole.asObservable();

  private hasToken(): boolean {
    if (this.isLocalStorageAvailable()) {
      return !!localStorage.getItem(this.tokenKey);
    }
    return false;
  }


  private getRoleFromLocalStorage(): string | null {
    if (this.isLocalStorageAvailable()) {

    return localStorage.getItem(this.roleKey) || 'User'; // Default to 'User' if not set
    }
    return 'User'; // Return default role when localStorage is not available

  }
  // Expose the authentication status as an observable
  public isAuthenticated$ = this.isAuthenticated.asObservable();

  public getRole(): string | null {
    return this.userRole.getValue(); // Get the latest value
  }

  public SetUserRole(role: string): void {
    localStorage.setItem(this.roleKey, role); // Save the role in localStorage
  }

  private isLocalStorageAvailable(): boolean {
    return typeof window !== 'undefined' && !!window.localStorage;
  }
  // Save token

  public saveToken(token: string) {
    if (!this.isTokenExpired(token)) {
      localStorage.setItem(this.tokenKey, token);
      this.isAuthenticated.next(true);
    } else {
      this.logout();
    }
  }

  login(token: string, role: string): void {
    if (this.isLocalStorageAvailable()) {
      localStorage.setItem(this.tokenKey, token);
      this.SetUserRole(role); // Set the role when logging in
      this.isAuthenticated.next(true); // Update the authentication status
      this.userRole.next(role); // Notify subscribers of the role change

    }
  }

  // Get token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout(): void {
    if (this.isLocalStorageAvailable()) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.roleKey); // Remove role on logout

      this.isAuthenticated.next(false); // Update the authentication status
      this.userRole.next('User'); // Reset to 'User' role or null

    }
  }
  isLoggedIn(): boolean {
    return this.isAuthenticated.getValue();
  }

  public isUserRoleAdmin(): boolean {
    return this.userRole.getValue() === 'Admin';
  }

  // Remove token
  clearToken(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.roleKey); // Clear role as well
  }

  public getTokenExpiry(token: string): number {
    try {
      const decoded: any = jwtDecode(token);
      return decoded.exp ? decoded.exp * 1000 : 0; // `exp` is in seconds
    } catch (error) {
      console.error('Failed to decode token:', error);
      return 0;
    }
  }
  
  public isTokenExpired(token: string): boolean {
    const expiryTime = this.getTokenExpiry(token);
    return expiryTime < Date.now();
  }
}