// src/app/features/admin/services/weather.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from './task.service';
import {
  WeatherData,
  WeatherAlert,
  WeatherForecast,
  ManualWeatherEntry,
  WeatherAlertCreate,
  WeatherAlertUpdate,
  WeatherAlertFilter,
  WeatherHistoryFilter,
  WeatherApiSettings,
  WeatherStatistics
} from '../models/weather.model';

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // =============================================
  // WEATHER DATA ENDPOINTS
  // =============================================

  getCurrentWeather(farmId: number, fieldId: number): Observable<ApiResponse<WeatherData>> {
    return this.http.get<ApiResponse<WeatherData>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/current/${fieldId}`
    );
  }

  getForecast(farmId: number, fieldId: number): Observable<ApiResponse<WeatherForecast>> {
    return this.http.get<ApiResponse<WeatherForecast>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/forecast/${fieldId}`
    );
  }

  getWeatherHistory(farmId: number, filter: WeatherHistoryFilter): Observable<ApiResponse<PagedResult<WeatherData>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 30).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);

    return this.http.get<ApiResponse<PagedResult<WeatherData>>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/history`,
      { params }
    );
  }

  addManualWeatherEntry(farmId: number, data: ManualWeatherEntry): Observable<ApiResponse<WeatherData>> {
    return this.http.post<ApiResponse<WeatherData>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/manual`,
      data
    );
  }

  updateWeatherData(farmId: number, id: number, data: ManualWeatherEntry): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/${id}`,
      data
    );
  }

  deleteWeatherData(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/${id}`
    );
  }

  refreshWeatherData(farmId: number, fieldId: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/refresh/${fieldId}`,
      {}
    );
  }

  refreshAllWeather(farmId: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/refresh-all`,
      {}
    );
  }

  // =============================================
  // WEATHER ALERT ENDPOINTS
  // =============================================

  getWeatherAlerts(farmId: number, filter: WeatherAlertFilter): Observable<ApiResponse<PagedResult<WeatherAlert>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 20).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.severity) params = params.set('severity', filter.severity);
    if (filter.isAcknowledged !== undefined && filter.isAcknowledged !== null) {
      params = params.set('isAcknowledged', filter.isAcknowledged.toString());
    }
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WeatherAlert>>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts`,
      { params }
    );
  }

  getActiveWeatherAlerts(farmId: number): Observable<ApiResponse<WeatherAlert[]>> {
    return this.http.get<ApiResponse<WeatherAlert[]>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/active`
    ).pipe(
      catchError((error: any) => {
        console.warn('Active alerts endpoint not available:', error);
        // Return empty array instead of failing
        return of({ 
          success: true, 
          data: [],
          message: 'No active alerts' 
        } as ApiResponse<WeatherAlert[]>);
      })
    );
  }

  getWeatherAlertById(farmId: number, id: number): Observable<ApiResponse<WeatherAlert>> {
    return this.http.get<ApiResponse<WeatherAlert>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/${id}`
    );
  }

  createWeatherAlert(farmId: number, data: WeatherAlertCreate): Observable<ApiResponse<WeatherAlert>> {
    return this.http.post<ApiResponse<WeatherAlert>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts`,
      data
    );
  }

  updateWeatherAlert(farmId: number, id: number, data: WeatherAlertUpdate): Observable<ApiResponse<WeatherAlert>> {
    return this.http.put<ApiResponse<WeatherAlert>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/${id}`,
      data
    );
  }

  deleteWeatherAlert(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/${id}`
    );
  }

  acknowledgeWeatherAlert(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/${id}/acknowledge`,
      {}
    );
  }

  acknowledgeAllAlertsForField(farmId: number, fieldId: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/alerts/acknowledge-all/${fieldId}`,
      {}
    );
  }

  // =============================================
  // WEATHER STATISTICS
  // =============================================

  getWeatherStatistics(farmId: number): Observable<ApiResponse<WeatherStatistics>> {
    return this.http.get<ApiResponse<WeatherStatistics>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/statistics`
    ).pipe(
      catchError((error: any) => {
        console.warn('Statistics endpoint not available:', error);
        // Return default statistics
        return of({
          success: true,
          data: {
            totalRecords: 0,
            fieldsWithData: 0,
            averageTemperature: 0,
            averageHumidity: 0,
            totalRainfall: 0,
            activeAlerts: 0,
            criticalAlerts: 0,
            lastUpdated: new Date().toISOString()
          },
          message: 'Statistics not available'
        } as ApiResponse<WeatherStatistics>);
      })
    );
  }

  // =============================================
  // WEATHER SETTINGS
  // =============================================

  getWeatherSettings(farmId: number): Observable<ApiResponse<WeatherApiSettings>> {
    return this.http.get<ApiResponse<WeatherApiSettings>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/settings`
    );
  }

  updateWeatherSettings(farmId: number, settings: WeatherApiSettings): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.API_URL}/admin/farms/${farmId}/weather/settings`,
      settings
    );
  }
}