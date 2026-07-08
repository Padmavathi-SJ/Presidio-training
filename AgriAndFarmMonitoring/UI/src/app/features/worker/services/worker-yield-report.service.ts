// src/app/features/worker/services/worker-yield-report.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface YieldReportDto {
  id: number;
  reportName: string;
  reportType: string;
  startDate: string;
  endDate: string;
  totalYieldKg: number;
  totalHarvests: number;
  totalValue: number;
  averageQualityGrade: string;
  exportFormat?: string;
  exportedAt?: string;
  downloadUrl?: string;
}

export interface GenerateYieldReportDto {
  cropCycleId?: number;
  fieldId?: number;
  startDate: string;
  endDate: string;
  reportName?: string;
  exportFormat: string;
}

@Injectable({
  providedIn: 'root'
})
export class WorkerYieldReportService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/worker/yield-reports`;

  getReports(params: any): Observable<any> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.set(key, params[key]);
      }
    });
    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  generateReport(data: GenerateYieldReportDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/generate`, data);
  }

  exportReport(id: number, format: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/export`, { format });
  }
}
