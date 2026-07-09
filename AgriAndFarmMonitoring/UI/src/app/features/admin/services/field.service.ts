// src/app/features/admin/services/field.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  Field,
  CreateFieldDto,
  UpdateFieldDto,
  UpdateLocationDto,
  FieldFilterDto,
  FieldStatisticsDto,
  PagedResult,
  ApiResponse,
  BulkImportResult
} from '../models/field.model';

@Injectable({
  providedIn: 'root'
})
export class FieldService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // Get all fields with pagination and filtering
  getFields(farmId: number, filter: FieldFilterDto): Observable<ApiResponse<PagedResult<Field>>> {
    let params = new HttpParams()
      .set('page', filter.page?.toString() || '1')
      .set('pageSize', filter.pageSize?.toString() || '10')
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldName) params = params.set('fieldName', filter.fieldName);
    if (filter.location) params = params.set('location', filter.location);
    if (filter.soilType) params = params.set('soilType', filter.soilType);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<Field>>>(`${this.API_URL}/farms/${farmId}/fields`, { params });
  }

  // Get field by ID
  getFieldById(farmId: number, fieldId: number): Observable<ApiResponse<Field>> {
    return this.http.get<ApiResponse<Field>>(`${this.API_URL}/farms/${farmId}/fields/${fieldId}`);
  }

  // Create new field
  createField(farmId: number, data: CreateFieldDto): Observable<ApiResponse<Field>> {
    return this.http.post<ApiResponse<Field>>(`${this.API_URL}/farms/${farmId}/fields`, data);
  }

  // Update field
  updateField(farmId: number, fieldId: number, data: UpdateFieldDto): Observable<ApiResponse<Field>> {
    return this.http.put<ApiResponse<Field>>(`${this.API_URL}/farms/${farmId}/fields/${fieldId}`, data);
  }

  // Update field location
  updateFieldLocation(farmId: number, fieldId: number, data: UpdateLocationDto): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/fields/${fieldId}/location`, data);
  }

  // Soft delete field
  deleteField(farmId: number, fieldId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/fields/${fieldId}`);
  }

  // Bulk soft delete fields
  bulkDeleteFields(farmId: number, ids: number[]): Observable<ApiResponse<BulkImportResult>> {
    return this.http.post<ApiResponse<BulkImportResult>>(`${this.API_URL}/farms/${farmId}/fields/bulk-soft-delete`, ids);
  }

  // Get field statistics
  getStatistics(farmId: number): Observable<ApiResponse<FieldStatisticsDto>> {
    return this.http.get<ApiResponse<FieldStatisticsDto>>(`${this.API_URL}/farms/${farmId}/fields/statistics`);
  }

  // Bulk import fields from Excel
  bulkImport(farmId: number, file: File): Observable<ApiResponse<BulkImportResult>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<BulkImportResult>>(`${this.API_URL}/farms/${farmId}/fields/bulk-import`, formData);
  }

  // Export fields to Excel
  exportFields(farmId: number): Observable<Blob> {
    return this.http.get(`${this.API_URL}/farms/${farmId}/fields/export`, { responseType: 'blob' });
  }

  // Download import template
  downloadTemplate(farmId: number): Observable<Blob> {
    return this.http.get(`${this.API_URL}/farms/${farmId}/fields/template`, { responseType: 'blob' });
  }

  // Upload field image
  uploadImage(farmId: number, file: File): Observable<ApiResponse<{ fileName: string, fileUrl: string }>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<{ fileName: string, fileUrl: string }>>(`${this.API_URL}/farms/${farmId}/fields/upload-image`, formData);
  }
}