import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { NotificationService, NotificationDto } from '../../../core/services/notification.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-notification-menu',
  standalone: true,
  imports: [
    CommonModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    MatBadgeModule,
    DatePipe
  ],
  templateUrl: './notification-menu.component.html',
  styles: [`
    .notification-backdrop {
      position: fixed;
      top: 52px;
      left: 0;
      right: 0;
      bottom: 0;
      z-index: 1040;
    }
    
    .notification-sidebar {
      position: fixed;
      top: 52px;
      right: 0;
      width: 28vw;
      min-width: 360px;
      height: calc(100vh - 52px);
      z-index: 1050;
      transform: translateX(100%);
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }
    
    .notification-sidebar.open {
      transform: translateX(0);
    }
    
    .custom-scrollbar::-webkit-scrollbar {
      width: 6px;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
      background: transparent;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
      background: #c1d1c8;
      border-radius: 3px;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb:hover {
      background: #a3b8ad;
    }
    
    .unread-item {
      background-color: #f5f8f5;
    }
    .unread-item .notification-title {
      font-weight: 700;
      color: #1b4332;
    }
    
    @media (max-width: 768px) {
      .notification-sidebar {
        width: 100vw;
      }
    }
  `]
})
export class NotificationMenuComponent implements OnInit {
  notificationService = inject(NotificationService);
  router = inject(Router);
  isOpen = false;

  ngOnInit() {
    this.notificationService.loadNotifications();
  }

  toggleMenu(event: Event) {
    event.stopPropagation();
    this.isOpen = !this.isOpen;
  }

  closeMenu() {
    this.isOpen = false;
  }

  getIconForType(type: string): string {
    if (!type) return 'notifications';
    
    if (type.includes('Harvest')) return 'agriculture';
    if (type.includes('Observation')) return 'visibility';
    if (type.includes('QualityCheck')) return 'high_quality';
    if (type.includes('Task')) return 'assignment';
    if (type.includes('Field')) return 'landscape';
    if (type.includes('Sensor')) return 'sensors';
    if (type.includes('Weather')) return 'cloud_queue';
    
    return 'notifications';
  }

  getBgColorForType(type: string): string {
    if (!type) return 'bg-gradient-to-br from-gray-500 to-gray-600';
    
    // Alerts (Professional Danger Colors)
    // Sensor Alert: Deep crimson/red for critical hardware/threshold failures
    if (type.includes('Sensor')) return 'bg-gradient-to-br from-red-600 to-red-800 shadow-red-900/20';
    // Weather Alert: Deep amber/orange for severe weather warnings
    if (type.includes('Weather')) return 'bg-gradient-to-br from-orange-500 to-orange-700 shadow-orange-900/20';
    
    // Harvests (Greens/Teals)
    if (type.includes('Harvest')) return 'bg-gradient-to-br from-green-500 to-green-600';
    
    // Observations (Blues/Indigos)
    if (type.includes('Observation')) return 'bg-gradient-to-br from-blue-500 to-blue-600';
    
    // Quality Checks (Purples/Pinks)
    if (type.includes('QualityCheck')) return 'bg-gradient-to-br from-purple-500 to-purple-600';
    
    // Tasks & Assignments (Oranges/Yellows/Limes)
    if (type.includes('Task')) return 'bg-gradient-to-br from-orange-500 to-orange-600';
    if (type.includes('Field')) return 'bg-gradient-to-br from-amber-500 to-amber-600';

    return 'bg-gradient-to-br from-gray-500 to-gray-600';
  }

  onNotificationClick(notification: NotificationDto) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id);
    }
    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }
    this.closeMenu();
  }

  markAllAsRead(event: Event) {
    event.stopPropagation();
    this.notificationService.markAllAsRead();
  }
}
