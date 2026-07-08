import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { AuthService } from '../../../../core/services/auth.service';
import { 
  QualityCheckDto, 
  UpdateQualityCheckDto, 
  QualityCheckApprovalDto,
  QualityCheckFilterDto,
  QualityStatisticsDto
} from '../models/admin-quality-check.model';

@Injectable({
  providedIn: 'root'
})
export class AdminQualityCheckService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private getHeaders(): HttpHeaders {
    const user = this.authService.getCurrentUser();
    let headers = new HttpHeaders();
    if (user && user.accessToken) {
      headers = headers.set('Authorization', `Bearer ${user.accessToken}`);
    }
    return headers;
  }

  private get apiUrl(): string {
    const user = this.authService.getCurrentUser();
    const farmId = user?.farmId || 0;
    return `${environment.apiUrl}/admin/farms/${farmId}/quality-checks`;
  }

  getAll(filter?: QualityCheckFilterDto): Observable<{ data: { items: QualityCheckDto[], totalCount: number }, success: boolean }> {
    let params = new HttpParams();
    if (filter) {
      Object.keys(filter).forEach(key => {
        const value = (filter as any)[key];
        if (value !== undefined && value !== null && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }
    return this.http.get<any>(this.apiUrl, { params });
  }

  getById(id: number): Observable<{ data: QualityCheckDto, success: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getByHarvest(harvestId: number): Observable<{ data: QualityCheckDto[], success: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/harvest/${harvestId}`);
  }

  getByWorker(workerId: number): Observable<{ data: QualityCheckDto[], success: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/worker/${workerId}`);
  }

  getPendingApprovals(page: number = 1, pageSize: number = 20): Observable<{ data: { items: QualityCheckDto[], totalCount: number }, success: boolean }> {
    return this.http.get<any>(`${this.apiUrl}/pending-approvals?page=${page}&pageSize=${pageSize}`);
  }

  approve(id: number, approval: QualityCheckApprovalDto): Observable<{ success: boolean, message: string }> {
    return this.http.post<any>(`${this.apiUrl}/${id}/approve`, approval);
  }

  update(id: number, dto: UpdateQualityCheckDto): Observable<{ success: boolean }> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<{ success: boolean }> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  getStatistics(fromDate?: string, toDate?: string): Observable<{ data: QualityStatisticsDto, success: boolean }> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<any>(`${this.apiUrl}/statistics/quality`, { params });
  }
}
