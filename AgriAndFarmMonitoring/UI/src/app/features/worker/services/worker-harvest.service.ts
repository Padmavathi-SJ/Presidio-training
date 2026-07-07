// src/app/features/worker/services/worker-harvest.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../features/admin/services/task.service';
import {
  HarvestDto,
  CreateHarvestDto,
  UpdateHarvestDto,
  HarvestFilterDto,
  HarvestWorkerResponseDto
} from '../models/worker-harvest.model';

@Injectable({
  providedIn: 'root'
})
export class WorkerHarvestService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getMyHarvests(filter: HarvestFilterDto): Observable<ApiResponse<PagedResult<HarvestDto>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.approvalStatus) params = params.set('approvalStatus', filter.approvalStatus);
    if (filter.qualityGrade) params = params.set('qualityGrade', filter.qualityGrade);
    if (filter.harvestMethod) params = params.set('harvestMethod', filter.harvestMethod);
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<HarvestDto>>>(`${this.API_URL}/worker/harvests/my`, { params });
  }

  getHarvestById(id: number): Observable<ApiResponse<HarvestDto>> {
    return this.http.get<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests/${id}`);
  }

  createHarvest(data: CreateHarvestDto): Observable<ApiResponse<HarvestDto>> {
    return this.http.post<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests`, data);
  }

updateHarvest(id: number, data: UpdateHarvestDto): Observable<ApiResponse<HarvestDto>> {
  // ✅ Use PATCH for partial updates
  return this.http.patch<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests/${id}`, data);
}
  deleteHarvest(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/worker/harvests/${id}`);
  }

  respondToAdmin(id: number, data: HarvestWorkerResponseDto): Observable<ApiResponse<HarvestDto>> {
    return this.http.post<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests/${id}/respond`, data);
  }

  uploadImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.API_URL}/worker/harvests/upload`, formData);
  }

  deleteUploadedImage(fileName: string): Observable<any> {
    return this.http.delete<any>(`${this.API_URL}/worker/harvests/upload/${encodeURIComponent(fileName)}`);
  }
}