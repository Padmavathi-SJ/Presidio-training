import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-panel.component.html',
  styleUrls: ['./profile-panel.component.css']
})
export class ProfilePanelComponent {
  @Input() isOpen: boolean = false;
  @Output() close = new EventEmitter<void>();

  // Resume PDF path
  resumePDFPath: string = 'assets/resume.pdf';

  closePanel(): void {
    this.close.emit();
  }

  downloadResume(): void {
    const link = document.createElement('a');
    link.href = this.resumePDFPath;
    link.download = 'Padmavathi_SJ_Resume.pdf';
    link.click();
  }
}