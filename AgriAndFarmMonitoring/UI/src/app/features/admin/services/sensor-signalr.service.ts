// src/app/core/services/sensor-signalr.service.ts
import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SensorReading, Alert } from '../models/sensor.model';

@Injectable({
  providedIn: 'root'
})
export class SensorSignalRService {
  private hubConnection!: HubConnection;
  private readonly API_URL = environment.apiUrl;

  // Subjects for real-time updates
  private sensorReadingSubject = new BehaviorSubject<SensorReading | null>(null);
  private alertSubject = new BehaviorSubject<Alert | null>(null);
  private alertResolvedSubject = new BehaviorSubject<{ alertId: number } | null>(null);
  private alertCountSubject = new BehaviorSubject<number>(0);

  public sensorReading$ = this.sensorReadingSubject.asObservable();
  public alert$ = this.alertSubject.asObservable();
  public alertResolved$ = this.alertResolvedSubject.asObservable();
  public alertCount$ = this.alertCountSubject.asObservable();

  constructor() {
    this.startConnection();
  }

  private startConnection(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.API_URL}/monitoringHub`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('✅ Sensor SignalR connection established'))
      .catch(err => console.error('❌ Error starting Sensor SignalR connection:', err));

    this.registerEventHandlers();
  }

  private registerEventHandlers(): void {
    // New sensor reading
    this.hubConnection.on('ReceiveSensorReading', (data: SensorReading) => {
      console.log('📊 New sensor reading via SignalR:', data);
      this.sensorReadingSubject.next(data);
    });

    // New alert
    this.hubConnection.on('NewAlert', (alert: Alert) => {
      console.log('🔔 New alert via SignalR:', alert);
      this.alertSubject.next(alert);
      this.alertCountSubject.next(this.alertCountSubject.value + 1);
    });

    // Alert resolved
    this.hubConnection.on('AlertResolved', (data: { alertId: number }) => {
      console.log('✅ Alert resolved via SignalR:', data);
      this.alertResolvedSubject.next(data);
      this.alertCountSubject.next(Math.max(0, this.alertCountSubject.value - 1));
    });

    // Alert acknowledged
    this.hubConnection.on('AlertAcknowledged', (data: { alertId: number }) => {
      console.log('👀 Alert acknowledged via SignalR:', data);
    });
  }

  // Join farm group for real-time updates
  joinFarmGroup(farmId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('JoinFarmGroup', farmId)
        .catch(err => console.error('Error joining farm group:', err));
    }
  }

  // Join field group
  joinFieldGroup(fieldId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('JoinFieldGroup', fieldId)
        .catch(err => console.error('Error joining field group:', err));
    }
  }

  leaveFieldGroup(fieldId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('LeaveFieldGroup', fieldId)
        .catch(err => console.error('Error leaving field group:', err));
    }
  }

  // Acknowledge alert via SignalR
  acknowledgeAlert(alertId: number): void {
    if (this.hubConnection?.state === 'Connected') {
      this.hubConnection.invoke('AcknowledgeAlert', alertId)
        .catch(err => console.error('Error acknowledging alert:', err));
    }
  }

  isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('⏹️ Sensor SignalR connection stopped'))
        .catch(err => console.error('Error stopping Sensor SignalR connection:', err));
    }
  }
}