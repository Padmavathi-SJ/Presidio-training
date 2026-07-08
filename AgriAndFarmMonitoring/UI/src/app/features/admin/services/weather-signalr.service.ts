// src/app/core/services/weather-signalr.service.ts
import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { WeatherData, WeatherAlert } from '../models/weather.model';

@Injectable({
  providedIn: 'root'
})
export class WeatherSignalRService {
  private hubConnection!: HubConnection;
  private readonly API_URL = environment.apiUrl;

  // Subjects for real-time updates
  private weatherUpdateSubject = new BehaviorSubject<WeatherData | null>(null);
  private alertUpdateSubject = new BehaviorSubject<WeatherAlert | null>(null);
  private alertCountSubject = new BehaviorSubject<number>(0);

  public weatherUpdate$ = this.weatherUpdateSubject.asObservable();
  public alertUpdate$ = this.alertUpdateSubject.asObservable();
  public alertCount$ = this.alertCountSubject.asObservable();

  constructor() {
    this.startConnection();
  }

  private startConnection(): void {
    const baseUrl = this.API_URL.replace('/api', '');
    const token = localStorage.getItem('token');
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/weatherHub`, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('✅ Weather SignalR connection established'))
      .catch(err => console.error('❌ Error starting Weather SignalR connection:', err));

    this.registerEventHandlers();
  }

  private registerEventHandlers(): void {
    this.hubConnection.on('WeatherUpdated', (data: WeatherData) => {
      console.log('🌤️ Weather updated via SignalR:', data);
      this.weatherUpdateSubject.next(data);
    });

    this.hubConnection.on('AlertCreated', (alert: WeatherAlert) => {
      console.log('🔔 New weather alert via SignalR:', alert);
      this.alertUpdateSubject.next(alert);
      this.alertCountSubject.next(this.alertCountSubject.value + 1);
    });

    this.hubConnection.on('AlertAcknowledged', (alertId: number) => {
      console.log('✅ Alert acknowledged via SignalR:', alertId);
    });

    this.hubConnection.on('AlertCountUpdated', (count: number) => {
      this.alertCountSubject.next(count);
    });
  }

  subscribeToField(fieldId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('SubscribeToField', fieldId)
        .catch(err => console.error('Error subscribing to field:', err));
    }
  }

  unsubscribeFromField(fieldId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('UnsubscribeFromField', fieldId)
        .catch(err => console.error('Error unsubscribing from field:', err));
    }
  }

  joinAdminGroup(farmId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('JoinAdminGroup', farmId)
        .catch(err => console.error('Error joining admin group:', err));
    }
  }

  leaveAdminGroup(farmId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('LeaveAdminGroup', farmId)
        .catch(err => console.error('Error leaving admin group:', err));
    }
  }

  isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('⏹️ Weather SignalR connection stopped'))
        .catch(err => console.error('Error stopping Weather SignalR connection:', err));
    }
  }
}