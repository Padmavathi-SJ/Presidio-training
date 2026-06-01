import { Component } from '@angular/core';
import { ProductModel } from '../models/product.model';

@Component({
  selector: 'app-products',
  imports: [],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products {
   product1: ProductModel = new ProductModel(
    "Baleno I5",
    "Latest Apple smartphone with A17 Pro chip and amazing camera system",
    599999,
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRcV5KnayG5c2Cb77FKvHOcmn1bT19BY8YppA&s"
  );

}
