// src/app/features/admin/services/sensor.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from './task.service';
import {
  SensorReading,
  SensorReadingFilter,
  SensorStatistics,
  Alert,
  AlertFilter,
  AlertDashboard,
  ResolveAlert,
  AlertThreshold,
  CreateAlertThreshold,
  UpdateAlertThreshold,
  CreateManualSensorReading
} from '../models/sensor.model';

@Injectable({
  providedIn: 'root'
})
export class SensorService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // =============================================
  // SENSOR READING ENDPOINTS
  // =============================================

  getSensorReadings(farmId: number, filter: SensorReadingFilter): Observable<ApiResponse<PagedResult<SensorReading>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 20).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.sensorType) params = params.set('sensorType', filter.sensorType);
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.latestOnly !== undefined && filter.latestOnly !== null) {
      params = params.set('latestOnly', filter.latestOnly.toString());
    }
    if (filter.groupBy) params = params.set('groupBy', filter.groupBy);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<SensorReading>>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors`,
      { params }
    );
  }

  getLatestReadings(farmId: number): Observable<ApiResponse<SensorReading[]>> {
    return this.http.get<ApiResponse<SensorReading[]>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/latest`
    );
  }

  getFieldHistory(
    farmId: number,
    fieldId: number,
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResponse<SensorReading[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<SensorReading[]>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/field/${fieldId}/history`,
      { params }
    );
  }

  getThresholdViolations(
    farmId: number,
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResponse<SensorReading[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<SensorReading[]>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/threshold-violations`,
      { params }
    );
  }

  getSensorStatistics(
    farmId: number,
    groupBy: string = 'day',
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResponse<SensorStatistics>> {
    let params = new HttpParams().set('groupBy', groupBy);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<SensorStatistics>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/statistics`,
      { params }
    );
  }

  exportSensorData(
    farmId: number,
    fieldId?: number,
    fromDate?: string,
    toDate?: string
  ): Observable<Blob> {
    let params = new HttpParams();
    if (fieldId) params = params.set('fieldId', fieldId.toString());
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get(
      `${this.API_URL}/admin/farms/${farmId}/sensors/export`,
      { params, responseType: 'blob' }
    );
  }

  // =============================================
  // ALERT ENDPOINTS
  // =============================================

  getAlerts(farmId: number, filter: AlertFilter): Observable<ApiResponse<PagedResult<Alert>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 20).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.alertType) params = params.set('alertType', filter.alertType);
    if (filter.severity) params = params.set('severity', filter.severity);
    if (filter.isResolved !== undefined && filter.isResolved !== null) {
      params = params.set('isResolved', filter.isResolved.toString());
    }
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<Alert>>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts`,
      { params }
    );
  }

  getAlertDashboard(farmId: number): Observable<ApiResponse<AlertDashboard>> {
    return this.http.get<ApiResponse<AlertDashboard>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts/dashboard`
    );
  }

  getCriticalAlerts(farmId: number): Observable<ApiResponse<Alert[]>> {
    return this.http.get<ApiResponse<Alert[]>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts/critical`
    );
  }

  getAlertStatistics(
    farmId: number,
    fromDate?: string,
    toDate?: string
  ): Observable<ApiResponse<any>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<any>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts/statistics`,
      { params }
    );
  }

  getUnresolvedAlertCount(farmId: number): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts/unresolved-count`
    );
  }

  resolveAlert(farmId: number, alertId: number, data: ResolveAlert): Observable<ApiResponse<Alert>> {
    return this.http.put<ApiResponse<Alert>>(
      `${this.API_URL}/admin/farms/${farmId}/alerts/${alertId}/resolve`,
      data
    );
  }

  generateHourlyAlert(farmId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.API_URL}/admin/farms/${farmId}/test/generate-hourly-alert`,
      {}
    );
  }

  // =============================================
  // ALERT THRESHOLD ENDPOINTS
  // =============================================

  getAlertThresholds(farmId: number): Observable<ApiResponse<AlertThreshold[]>> {
    return this.http.get<ApiResponse<AlertThreshold[]>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/thresholds`
    );
  }

  createAlertThreshold(farmId: number, data: CreateAlertThreshold): Observable<ApiResponse<AlertThreshold>> {
    return this.http.post<ApiResponse<AlertThreshold>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/thresholds`,
      data
    );
  }

  updateAlertThreshold(farmId: number, id: number, data: UpdateAlertThreshold): Observable<ApiResponse<AlertThreshold>> {
    return this.http.put<ApiResponse<AlertThreshold>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/thresholds/${id}`,
      data
    );
  }

  deleteAlertThreshold(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/thresholds/${id}`
    );
  }

  // =============================================
  // MANUAL SENSOR READING ENDPOINTS
  // =============================================

  addManualReading(farmId: number, data: CreateManualSensorReading): Observable<ApiResponse<SensorReading>> {
    return this.http.post<ApiResponse<SensorReading>>(
      `${this.API_URL}/admin/farms/${farmId}/sensors/manual`,
      data
    );
  }
}