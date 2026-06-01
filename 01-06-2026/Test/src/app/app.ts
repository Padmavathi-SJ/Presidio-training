import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Customers } from './customers/customers';
import {FormsModule} from '@angular/forms';
import { Products } from './products/products';

@Component({
  selector: 'app-root',
  imports: [Customers, Products, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Test');
}
