import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { WeatherData, WeatherForecast, WeatherAlert, WeatherHistoryFilter } from '../../admin/models/weather.model';

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
export class WorkerWeatherService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  
  private get baseUrl(): string {
    const farmId = this.authService.getFarmId();
    return `${environment.apiUrl}/worker/farms/${farmId}/weather`;
  }

  getCurrentWeather(fieldId: number): Observable<ApiResponse<WeatherData>> {
    return this.http.get<ApiResponse<WeatherData>>(`${this.baseUrl}/current/${fieldId}`);
  }

  getForecast(fieldId: number): Observable<ApiResponse<WeatherForecast>> {
    return this.http.get<ApiResponse<WeatherForecast>>(`${this.baseUrl}/forecast/${fieldId}`);
  }

  getWeatherHistory(filter: WeatherHistoryFilter): Observable<ApiResponse<PagedResult<WeatherData>>> {
    let params = new HttpParams();
    Object.keys(filter).forEach(key => {
      const value = (filter as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value);
      }
    });
    
    return this.http.get<ApiResponse<PagedResult<WeatherData>>>(`${this.baseUrl}/history`, { params });
  }

  getWeatherAlerts(filter?: any): Observable<ApiResponse<PagedResult<WeatherAlert>>> {
    let params = new HttpParams();
    if (filter) {
      Object.keys(filter).forEach(key => {
        if (filter[key] !== null && filter[key] !== undefined && filter[key] !== '') {
          params = params.set(key, filter[key]);
        }
      });
    }
    return this.http.get<ApiResponse<PagedResult<WeatherAlert>>>(`${this.baseUrl}/alerts`, { params });
  }

  resolveAlert(alertId: number, resolutionNotes: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/alerts/${alertId}/resolve`, { resolutionNotes });
  }
}
