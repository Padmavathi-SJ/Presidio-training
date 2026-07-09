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
    .notification-menu {
      max-height: 400px;
      width: 350px;
      overflow-y: auto;
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
      padding: 8px 16px;
      border-bottom: 1px solid #ddd;
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
      white-space: normal; /* allow wrapping */
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
  `]
})
export class NotificationMenuComponent implements OnInit {
  notificationService = inject(NotificationService);
  router = inject(Router);

  ngOnInit() {
    this.notificationService.loadNotifications();
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
