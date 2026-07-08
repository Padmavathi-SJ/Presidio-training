import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { 
  QualityCheckDto, 
  CreateQualityCheckDto, 
  UpdateQualityCheckDto, 
  QualityCheckFilterDto, 
  QualityCheckWorkerResponseDto 
} from '../models/worker-quality-check.model';

@Injectable({
  providedIn: 'root'
})
export class WorkerQualityCheckService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/worker/quality-checks`;

  getMyQualityChecks(filter?: QualityCheckFilterDto): Observable<{ data: { items: QualityCheckDto[], totalCount: number }, success: boolean }> {
    let params = new HttpParams();
    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = (filter as any)[key];
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }
    return this.http.get<any>(`${this.apiUrl}/my`, { params });
  }

  getById(id: number): Observable<{ data: QualityCheckDto, success: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateQualityCheckDto): Observable<{ data: QualityCheckDto, success: boolean }> {
    return this.http.post<any>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateQualityCheckDto): Observable<{ data: QualityCheckDto, success: boolean }> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<{ success: boolean }> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  respondToAdmin(id: number, response: QualityCheckWorkerResponseDto): Observable<{ success: boolean }> {
    return this.http.post<any>(`${this.apiUrl}/${id}/respond`, response);
  }

  getPendingCount(): Observable<{ hasPendingApprovals: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/pending-count`);
  }

  getStatistics(fromDate?: string, toDate?: string): Observable<{ data: any, success: boolean }> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<any>(`${this.apiUrl}/statistics/quality`, { params });
  }
}
