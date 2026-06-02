import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loader.html',
  styleUrl: './loader.css'
})
export class LoaderComponent implements OnInit, OnDestroy {
  displayText: string = '<Padmavathi />';
  characters: string[] = [];
  
  ngOnInit() {
    this.characters = this.displayText.split('');
  }
  
  ngOnDestroy() {
  }
}