import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatListModule } from '@angular/material/list';
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
    MatListModule,
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
      width: 25vw;
      min-width: 320px;
      height: calc(100vh - 52px);
      background: white;
      box-shadow: -4px 0 24px rgba(0, 0, 0, 0.1);
      z-index: 1050;
      display: flex;
      flex-direction: column;
      transform: translateX(100%);
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }
    
    .notification-sidebar.open {
      transform: translateX(0);
    }

    .notification-list {
      flex: 1;
      overflow-y: auto;
    }
    
    .custom-scrollbar::-webkit-scrollbar {
      width: 6px;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
      background: transparent;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
      background: #cbd5e1;
      border-radius: 3px;
    }
    
    .unread {
      background-color: #f0f7ff;
    }
    .notification-item {
      cursor: pointer;
      border-bottom: 1px solid #eee;
    }
    .notification-item:hover {
      background-color: #f9f9f9;
    }
    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      border-bottom: 1px solid #ddd;
      background: #f8fafc;
    }
    .notification-content {
      display: flex;
      flex-direction: column;
    }
    .notification-title {
      font-weight: 500;
      font-size: 14px;
    }
    .notification-message {
      font-size: 13px;
      color: #666;
      margin-top: 4px;
      white-space: normal;
      line-height: 1.4;
    }
    .notification-time {
      font-size: 11px;
      color: #999;
      margin-top: 4px;
    }
    .notification-count {
      background-color: #007bff;
      color: white;
      border-radius: 50%;
      padding: 2px 6px;
      font-size: 11px;
      margin-left: 8px;
    }
    .empty-state {
      padding: 24px;
      text-align: center;
      color: #777;
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
    switch(type) {
      case 'NewHarvest': return 'agriculture';
      case 'HarvestUpdated': return 'edit';
      case 'HarvestStatus': return 'fact_check';
      case 'NewObservation': return 'visibility';
      case 'ObservationUpdated': return 'edit';
      case 'ObservationValidation': return 'verified';
      case 'NewQualityCheck': return 'high_quality';
      case 'QualityCheckUpdated': return 'edit';
      case 'QualityCheckStatus': return 'fact_check';
      case 'NewTask': return 'assignment';
      case 'TaskReassigned': return 'assignment_ind';
      case 'TaskStatus': return 'task_alt';
      case 'FieldAssigned': return 'landscape';
      case 'SensorAlert': return 'sensors';
      case 'WeatherAlert': return 'cloud_queue';
      default: return 'notifications';
    }
  }

  getColorForType(type: string): string {
    if (type.includes('Alert')) return 'warn';
    if (type.includes('Status') || type.includes('Validation')) return 'primary';
    return 'accent';
  }

  onNotificationClick(notification: NotificationDto) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id);
    }
    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }
  }

  markAllAsRead(event: Event) {
    event.stopPropagation();
    this.notificationService.markAllAsRead();
  }
}
