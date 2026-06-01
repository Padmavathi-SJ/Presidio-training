import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loader.html',
  styleUrls: ['./loader.css']
})
export class LoaderComponent implements OnInit, OnDestroy {
  loading: boolean = true;
  private timer: any;
  
  displayText: string = '<Padmavathi />';
  characters: string[] = [];
  
  ngOnInit() {
    this.characters = this.displayText.split('');
    
    this.timer = setTimeout(() => {
      this.loading = false;
    }, 2000);
  }
  
  ngOnDestroy() {
    if (this.timer) {
      clearTimeout(this.timer);
    }
  }
}