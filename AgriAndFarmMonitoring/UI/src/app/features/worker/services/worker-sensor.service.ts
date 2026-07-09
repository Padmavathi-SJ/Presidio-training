import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { SensorReading, SensorStatistics, AlertThreshold, Alert } from '../../admin/models/sensor.model';
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
@Injectable({
  providedIn: 'root'
})
export class WorkerSensorService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  
  private get baseUrl(): string {
    const farmId = this.authService.getFarmId();
    return `${environment.apiUrl}/worker/farms/${farmId}/sensors`;
  }

  // Dashboard & Latest
  getLatestReadings(): Observable<SensorReading[]> {
    return this.http.get<SensorReading[]>(`${this.baseUrl}/latest`);
  }

  getStatistics(groupBy: string = 'day', fromDate?: string, toDate?: string): Observable<SensorStatistics> {
    let params = new HttpParams().set('groupBy', groupBy);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    
    return this.http.get<SensorStatistics>(`${this.baseUrl}/statistics`, { params });
  }

  // Readings List
  getAllReadings(filters: any): Observable<ApiResponse<PagedResult<SensorReading>>> {
    let params = new HttpParams();
    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key]);
      }
    });
    return this.http.get<ApiResponse<PagedResult<SensorReading>>>(this.baseUrl, { params });
  }

  // Thresholds
  getThresholds(): Observable<ApiResponse<AlertThreshold[]>> {
    return this.http.get<ApiResponse<AlertThreshold[]>>(`${this.baseUrl}/thresholds`);
  }

  // Alerts
  getAlerts(filters: any): Observable<ApiResponse<PagedResult<Alert>>> {
    let params = new HttpParams();
    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key]);
      }
    });
    return this.http.get<ApiResponse<PagedResult<Alert>>>(`${this.baseUrl}/alerts`, { params });
  }

  getUnresolvedAlerts(): Observable<Alert[]> {
    return this.http.get<Alert[]>(`${this.baseUrl}/alerts/unresolved`);
  }

  resolveAlert(alertId: number, dto: { resolutionNotes: string }): Observable<ApiResponse<Alert>> {
    return this.http.put<ApiResponse<Alert>>(`${this.baseUrl}/alerts/${alertId}/resolve`, dto);
  }
}
