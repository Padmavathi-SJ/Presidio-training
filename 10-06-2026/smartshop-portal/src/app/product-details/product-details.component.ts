import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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
  product: Product | null = null;
  loading: boolean = true;
  errorMessage: string = '';
  private productId: number = 0;

  constructor(
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    console.log('ProductDetailsComponent initialized');
    this.productId = Number(this.route.snapshot.paramMap.get('id'));
    console.log('Product ID:', this.productId);
    
    if (this.productId) {
      this.loadProductDetails();
    } else {
      this.errorMessage = 'Product not found';
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  loadProductDetails(): void {
    this.loading = true;
    this.errorMessage = '';
    console.log('Loading product details for ID:', this.productId);
    
    this.productService.getProductById(this.productId).subscribe({
      next: (product) => {
        console.log('Product received:', product);
        this.product = product;
        this.loading = false;
        this.cdr.detectChanges();
        console.log('Product assigned, loading:', this.loading);
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.errorMessage = 'Failed to load product details';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    console.log('Going back to products');
    this.router.navigate(['/dashboard/products']);
  }
}