// src/app/features/admin/services/worker.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  Worker,
  CreateWorkerDto,
  UpdateWorkerDto,
  WorkerFilterDto,
  WorkerLoginHistoryDto,
  WorkerStatisticsDto
} from '../models/worker.model';

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
export class WorkerService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // Get all workers with pagination and filtering
  getWorkers(farmId: number, filter: WorkerFilterDto): Observable<ApiResponse<PagedResult<Worker>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.name) params = params.set('name', filter.name);
    if (filter.email) params = params.set('email', filter.email);
    if (filter.role) params = params.set('role', filter.role);
    if (filter.isActive !== undefined && filter.isActive !== null) params = params.set('isActive', filter.isActive.toString());
    if (filter.hireDateFrom) params = params.set('hireDateFrom', filter.hireDateFrom);
    if (filter.hireDateTo) params = params.set('hireDateTo', filter.hireDateTo);
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<Worker>>>(`${this.API_URL}/farms/${farmId}/workers`, { params });
  }

  // Get worker by ID
  getWorkerById(farmId: number, workerId: number): Observable<ApiResponse<Worker>> {
    return this.http.get<ApiResponse<Worker>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}`);
  }

  // Create new worker
  createWorker(farmId: number, data: CreateWorkerDto): Observable<ApiResponse<Worker>> {
    return this.http.post<ApiResponse<Worker>>(`${this.API_URL}/farms/${farmId}/workers`, data);
  }

  // Update worker
  updateWorker(farmId: number, workerId: number, data: UpdateWorkerDto): Observable<ApiResponse<Worker>> {
    return this.http.put<ApiResponse<Worker>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}`, data);
  }

  // Activate worker
  activateWorker(farmId: number, workerId: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}/activate`, {});
  }

  // Deactivate worker
  deactivateWorker(farmId: number, workerId: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}/deactivate`, {});
  }

  // Reset worker password
resetWorkerPassword(farmId: number, workerId: number, newPassword: string, confirmPassword: string): Observable<ApiResponse<boolean>> {
  return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}/reset-password`, { 
    newPassword,
    confirmPassword
  });
}

  // Soft delete worker
  deleteWorker(farmId: number, workerId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}`);
  }

  // Get worker login history
  getLoginHistory(farmId: number, workerId: number): Observable<ApiResponse<WorkerLoginHistoryDto>> {
    return this.http.get<ApiResponse<WorkerLoginHistoryDto>>(`${this.API_URL}/farms/${farmId}/workers/${workerId}/login-history`);
  }
}