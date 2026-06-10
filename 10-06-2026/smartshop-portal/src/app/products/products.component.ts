import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ProductService, Product } from '../services/product.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})
export class ProductsComponent implements OnInit {
  // Signals for reactive state
  products = signal<Product[]>([]);
  loading = signal<boolean>(true);
  errorMessage = signal<string>('');
  
  // Computed signals (derived values)
  productCount = computed(() => this.products().length);
  hasProducts = computed(() => this.products().length > 0);
  showError = computed(() => this.errorMessage() !== '');
  
  // Dependency injection using inject() function (new way)
  private productService = inject(ProductService);
  private router = inject(Router);

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error.message);
        this.loading.set(false);
      }
    });
  }

  viewProductDetails(productId: number): void {
    this.router.navigate(['/dashboard/products', productId]);
  }
}