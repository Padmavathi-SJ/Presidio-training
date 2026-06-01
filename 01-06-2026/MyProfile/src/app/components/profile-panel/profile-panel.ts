import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile-panel',
  standalone: true,
  imports: [CommonModule], // Import CommonModule for ngIf
  templateUrl: './profile-panel.html',
  styleUrls: ['./profile-panel.css']
})
export class ProfilePanelComponent {
  @Input() isOpen: boolean = false;
  @Output() close = new EventEmitter<void>();

  closePanel(): void {
    this.close.emit();
  }
}