import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import * as signalR from '@microsoft/signalr';

export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  actionUrl: string;
  createdAt: string;
  count: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection: signalR.HubConnection | null = null;
  private readonly baseUrl = `${environment.apiUrl}/farms`;
  
  // State
  notifications = signal<NotificationDto[]>([]);
  unreadCount = signal<number>(0);
  
  constructor(
    private http: HttpClient,
    private tokenService: TokenService
  ) {}

  initializeSignalR() {
    const token = this.tokenService.getAccessToken();
    if (!token) return;

    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return; // Already connected
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/hubs/notifications`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      this.handleNewNotification(notification);
    });
    
    this.hubConnection.on('UpdateNotification', (notification: NotificationDto) => {
      this.handleUpdateNotification(notification);
    });

    this.hubConnection.start()
      .then(() => console.log('SignalR Notifications Connected'))
      .catch(err => console.error('Error while starting connection: ' + err));
  }

  stopSignalR() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private getApiUrl(): string {
    const user = this.tokenService.getUser();
    const farmId = user?.farmId || 1; // Fallback to 1 if not found
    return `${this.baseUrl}/${farmId}/notifications`;
  }

  loadNotifications() {
    this.http.get<NotificationDto[]>(`${this.getApiUrl()}/unread`).subscribe(data => {
      this.notifications.set(data);
      this.updateUnreadCount();
    });
  }

  markAsRead(id: number) {
    this.http.put(`${this.getApiUrl()}/${id}/read`, {}).subscribe(() => {
      this.notifications.update(list => 
        list.filter(n => n.id !== id)
      );
      this.updateUnreadCount();
    });
  }

  markAllAsRead() {
    this.http.put(`${this.getApiUrl()}/read-all`, {}).subscribe(() => {
      this.notifications.set([]);
      this.updateUnreadCount();
    });
  }

  private handleNewNotification(notification: NotificationDto) {
    this.notifications.update(list => [notification, ...list]);
    this.updateUnreadCount();
  }
  
  private handleUpdateNotification(notification: NotificationDto) {
    this.notifications.update(list => 
      list.map(n => n.id === notification.id ? notification : n)
    );
    this.updateUnreadCount();
  }

  private updateUnreadCount() {
    const count = this.notifications().filter(n => !n.isRead).length;
    this.unreadCount.set(count);
  }
}
