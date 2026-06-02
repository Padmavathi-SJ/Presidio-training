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
  private fullText: string = 'If you want to shine like a Sun, first burn like a Sun!';
  private index: number = 0;
  private isTyping: boolean = true;
  private intervalId: any;
  private timeoutId: any;

  ngOnInit() {
    this.startTyping();
  }

  ngOnDestroy() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
  }

  private startTyping() {
    this.intervalId = setInterval(() => {
      if (this.isTyping) {
        if (this.index < this.fullText.length) {
          this.displayText += this.fullText.charAt(this.index);
          this.index++;
        } else {
          // Completed typing, wait then delete
          this.isTyping = false;
          this.timeoutId = setTimeout(() => {
            this.startDeleting();
          }, 2000);
          clearInterval(this.intervalId);
        }
      }
    }, 80);
  }

  private startDeleting() {
    this.intervalId = setInterval(() => {
      if (this.displayText.length > 0) {
        this.displayText = this.displayText.slice(0, -1);
      } else {
        // Finished deleting, restart typing
        this.isTyping = true;
        this.index = 0;
        clearInterval(this.intervalId);
        this.startTyping();
      }
    }, 50);
  }
}