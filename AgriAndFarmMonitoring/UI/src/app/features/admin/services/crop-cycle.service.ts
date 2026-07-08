// src/app/features/admin/services/crop-cycle.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CropCycle,
  CreateCropCycleDto,
  UpdateCropCycleDto,
  CropCycleFilterDto
} from '../models/crop-cycle.model';

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CropCycleService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getCropCycles(farmId: number, filter: CropCycleFilterDto): Observable<ApiResponse<PagedResult<CropCycle>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropType) params = params.set('cropType', filter.cropType);
    if (filter.growthStage) params = params.set('growthStage', filter.growthStage);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.activeOnly !== undefined) params = params.set('activeOnly', filter.activeOnly.toString());
    if (filter.overdueOnly !== undefined) params = params.set('overdueOnly', filter.overdueOnly.toString());
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<CropCycle>>>(`${this.API_URL}/farms/${farmId}/crop-cycles`, { params });
  }

  getCropCycleById(farmId: number, id: number): Observable<ApiResponse<CropCycle>> {
    return this.http.get<ApiResponse<CropCycle>>(`${this.API_URL}/farms/${farmId}/crop-cycles/${id}`);
  }

  createCropCycle(farmId: number, data: CreateCropCycleDto): Observable<ApiResponse<CropCycle>> {
    return this.http.post<ApiResponse<CropCycle>>(`${this.API_URL}/farms/${farmId}/crop-cycles`, data);
  }

  updateCropCycle(farmId: number, id: number, data: UpdateCropCycleDto): Observable<ApiResponse<CropCycle>> {
    return this.http.put<ApiResponse<CropCycle>>(`${this.API_URL}/farms/${farmId}/crop-cycles/${id}`, data);
  }

  deleteCropCycle(farmId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/crop-cycles/${id}`);
  }

  getOverdueCropCycles(farmId: number): Observable<ApiResponse<CropCycle[]>> {
    return this.http.get<ApiResponse<CropCycle[]>>(`${this.API_URL}/farms/${farmId}/crop-cycles/overdue`);
  }

  // ✅ NEW: Manually update growth stage
  updateGrowthStage(farmId: number, id: number): Observable<ApiResponse<CropCycle>> {
    return this.http.post<ApiResponse<CropCycle>>(`${this.API_URL}/farms/${farmId}/crop-cycles/${id}/update-growth-stage`, {});
  }
}