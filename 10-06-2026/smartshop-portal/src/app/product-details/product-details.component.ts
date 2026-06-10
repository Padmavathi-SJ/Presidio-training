import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService, Product } from '../services/product.service';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css']
})
export class ProductDetailsComponent implements OnInit {
  // Signals for reactive state
  product = signal<Product | null>(null);
  loading = signal<boolean>(true);
  errorMessage = signal<string>('');
  selectedImageIndex = signal<number>(0);
  
  // Computed signals
  hasProduct = computed(() => this.product() !== null);
  showError = computed(() => this.errorMessage() !== '');
  currentImage = computed(() => {
    const currentProduct = this.product();
    if (!currentProduct) return '';
    const images = currentProduct.images;
    return images && images.length > 0 ? images[this.selectedImageIndex()] : currentProduct.thumbnail;
  });
  allImages = computed(() => {
    const currentProduct = this.product();
    if (!currentProduct) return [];
    return currentProduct.images && currentProduct.images.length > 0 ? currentProduct.images : [currentProduct.thumbnail];
  });
  
  // Dependency injection
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  
  private productId: number = 0;

  ngOnInit(): void {
    console.log('ProductDetailsComponent initialized with Signals');
    this.productId = Number(this.route.snapshot.paramMap.get('id'));
    console.log('Product ID:', this.productId);
    
    if (this.productId) {
      this.loadProductDetails();
    } else {
      this.errorMessage.set('Product not found');
      this.loading.set(false);
    }
  }

  loadProductDetails(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    console.log('Loading product details for ID:', this.productId);
    
    this.productService.getProductById(this.productId).subscribe({
      next: (product) => {
        console.log('Product received:', product);
        console.log('Product images:', product.images);
        this.product.set(product);
        this.loading.set(false);
        this.selectedImageIndex.set(0); // Reset to first image
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.errorMessage.set('Failed to load product details');
        this.loading.set(false);
      }
    });
  }

  selectImage(index: number): void {
    this.selectedImageIndex.set(index);
    console.log('Selected image index:', index);
  }

  goBack(): void {
    console.log('Going back to products');
    this.router.navigate(['/dashboard/products']);
  }
}