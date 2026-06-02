import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-banner.component.html',
  styleUrls: ['./home-banner.component.css']
})
export class HomeBannerComponent implements OnInit, OnDestroy {
  displayText: string = '';
  private currentIndex: number = 0;
  private isDeleting: boolean = false;
  private intervalId: any;
  
  private fullText: string = 'If you want to shine like a Sun, first burn like a Sun!';
  private typeSpeed: number = 80;
  private deleteSpeed: number = 40;
  private delaySpeed: number = 2000;

  ngOnInit() {
    this.startTypewriter();
  }

  ngOnDestroy() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  private startTypewriter() {
    this.intervalId = setInterval(() => {
      if (!this.isDeleting && this.currentIndex <= this.fullText.length) {
        // Typing
        this.displayText = this.fullText.substring(0, this.currentIndex);
        this.currentIndex++;
        
        if (this.currentIndex > this.fullText.length) {
          this.isDeleting = true;
        }
      } else if (this.isDeleting && this.currentIndex >= 0) {
        // Deleting
        this.displayText = this.fullText.substring(0, this.currentIndex);
        this.currentIndex--;
        
        if (this.currentIndex < 0) {
          this.isDeleting = false;
          this.currentIndex = 0;
        }
      }
    }, this.isDeleting ? this.deleteSpeed : this.typeSpeed);
  }
}