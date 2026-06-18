import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';  // ✅ For ngModel
import { HttpClient, HttpClientModule } from '@angular/common/http';  // ✅ For API calls

export interface Customer {
  id?: number;
  name: string;
  email: string;
}

@Component({
  selector: 'app-root',
  standalone: true,  // ✅ Standalone component
  imports: [
    CommonModule,
    FormsModule,      // ✅ For ngModel
    HttpClientModule  // ✅ For HTTP requests
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Customer Management';
  customers: Customer[] = [];
  newCustomer: Customer = { name: '', email: '' };
  private apiUrl = 'http://localhost:8080/customers';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.http.get<Customer[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.customers = data;
      },
      error: (error) => {
        console.error('Error loading customers:', error);
      }
    });
  }

  createCustomer() {
    if (this.newCustomer.name && this.newCustomer.email) {
      this.http.post<Customer>(this.apiUrl, this.newCustomer).subscribe({
        next: (customer) => {
          this.customers.push(customer);
          this.newCustomer = { name: '', email: '' };
        },
        error: (error) => {
          console.error('Error creating customer:', error);
        }
      });
    }
  }
}