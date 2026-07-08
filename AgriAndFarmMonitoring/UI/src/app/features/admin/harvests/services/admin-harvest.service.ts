import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { 
  HarvestDto, 
  UpdateHarvestDto, 
  HarvestApprovalDto, 
  HarvestFilterDto,
  YieldStatisticsDto,
  ApiResponse, 
  PagedResult 
} from '../models/admin-harvest.model';

@Injectable({
  providedIn: 'root'
})
export class AdminHarvestService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getHarvests(farmId: number, filter: HarvestFilterDto): Observable<ApiResponse<PagedResult<HarvestDto>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.workerId) params = params.set('workerId', filter.workerId.toString());
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.qualityGrade) params = params.set('qualityGrade', filter.qualityGrade);
    if (filter.harvestMethod) params = params.set('harvestMethod', filter.harvestMethod);
    if (filter.approvalStatus) params = params.set('approvalStatus', filter.approvalStatus);
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<HarvestDto>>>(`${this.API_URL}/admin/farms/${farmId}/harvests`, { params });
  }

  getHarvestById(farmId: number, id: number): Observable<ApiResponse<HarvestDto>> {
    return this.http.get<ApiResponse<HarvestDto>>(`${this.API_URL}/admin/farms/${farmId}/harvests/${id}`);
  }

  updateHarvest(farmId: number, id: number, data: UpdateHarvestDto): Observable<ApiResponse<HarvestDto>> {
    return this.http.put<ApiResponse<HarvestDto>>(`${this.API_URL}/admin/farms/${farmId}/harvests/${id}`, data);
  }

  deleteHarvest(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/admin/farms/${farmId}/harvests/${id}`);
  }

  approveHarvest(farmId: number, id: number, data: HarvestApprovalDto): Observable<ApiResponse<HarvestDto>> {
    return this.http.post<ApiResponse<HarvestDto>>(`${this.API_URL}/admin/farms/${farmId}/harvests/${id}/approve`, data);
  }

  getYieldStatistics(farmId: number, cropCycleId?: number, fromDate?: string, toDate?: string): Observable<ApiResponse<YieldStatisticsDto>> {
    let params = new HttpParams();
    if (cropCycleId) params = params.set('cropCycleId', cropCycleId.toString());
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<YieldStatisticsDto>>(`${this.API_URL}/admin/farms/${farmId}/harvests/statistics/yield`, { params });
  }

  uploadImage(farmId: number, file: File): Observable<ApiResponse<{ fileName: string, url: string }>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<{ fileName: string, url: string }>>(`${this.API_URL}/admin/farms/${farmId}/harvests/upload`, formData);
  }
}
