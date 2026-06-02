import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ProfilePanelComponent } from '../profile-panel/profile-panel.component';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, ProfilePanelComponent],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  isOpen: boolean = false;
  mobileMenuOpen: boolean = false;

  constructor(public router: Router) {}

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  openProfilePanel(): void {
    this.isOpen = true;
  }

  closeProfilePanel(): void {
    this.isOpen = false;
  }

  navigateTo(sectionId: string): void {
    // Scroll to section instead of routing
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
    this.closeMobileMenu();
  }
}