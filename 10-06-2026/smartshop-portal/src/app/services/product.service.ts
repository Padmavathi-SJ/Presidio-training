import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';

export interface Product {
  id: number;
  title: string;
  description: string;
  price: number;
  discountPercentage: number;
  rating: number;
  stock: number;
  brand: string;
  category: string;
  thumbnail: string;
  images: string[];
}

export interface ProductResponse {
  products: Product[];
  total: number;
  skip: number;
  limit: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = 'https://dummyjson.com/products';

  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    console.log('Fetching products from API...');
    return this.http.get<ProductResponse>(this.apiUrl).pipe(
      tap(response => {
        console.log('Products API called, received:', response.products.length, 'products');
      }),
      map(response => {
        // Transform the response to extract only needed data
        return response.products.map(product => ({
          ...product,
          price: product.price,
          rating: product.rating
        }));
      }),
      catchError(error => {
        console.error('Error fetching products:', error);
        return throwError(() => new Error('Failed to load products'));
      })
    );
  }

  getProductById(id: number): Observable<Product> {
    console.log(`Fetching product with id: ${id}`);
    return this.http.get<Product>(`${this.apiUrl}/${id}`).pipe(
      tap(product => {
        console.log('Product API called for:', product.title);
      }),
      map(product => {
        // Transform product data
        return {
          ...product,
          price: product.price,
          rating: product.rating
        };
      }),
      catchError(error => {
        console.error('Error fetching product:', error);
        return throwError(() => new Error('Failed to load product details'));
      })
    );
  }
}