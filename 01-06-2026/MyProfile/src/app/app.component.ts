import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { LoaderComponent } from './components/loader/loader.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, LoaderComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  loading: boolean = true;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    console.log('AppComponent initialized');
    
    setTimeout(() => {
      this.loading = false;
      console.log('Loading finished, loading =', this.loading);
      this.cdr.detectChanges(); // Force view update
    }, 3000);
  }
}