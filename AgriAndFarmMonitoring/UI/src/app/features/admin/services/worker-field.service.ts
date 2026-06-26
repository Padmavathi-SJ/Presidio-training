// src/app/features/admin/services/worker-field.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  WorkerFieldAssignment,
  AssignFieldToWorkerDto,
  WorkerFieldFilterDto
} from '../models/worker-field.model';

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
export class WorkerFieldService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // Get all worker field assignments with pagination and filtering
  getAssignments(farmId: number, filter: WorkerFieldFilterDto): Observable<ApiResponse<PagedResult<WorkerFieldAssignment>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.workerId) params = params.set('workerId', filter.workerId.toString());
    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.isActive !== undefined && filter.isActive !== null) params = params.set('isActive', filter.isActive.toString());
    
    // ✅ Date filters
    if (filter.assignedDateFrom) {
      params = params.set('assignedDateFrom', filter.assignedDateFrom);
    }
    if (filter.assignedDateTo) {
      params = params.set('assignedDateTo', filter.assignedDateTo);
    }
    // ✅ End Date filters
    if (filter.endDateFrom) {
      params = params.set('endDateFrom', filter.endDateFrom);
    }
    if (filter.endDateTo) {
      params = params.set('endDateTo', filter.endDateTo);
    }
    
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerFieldAssignment>>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields`, { params });
  }

  // Assign field to worker
  assignFieldToWorker(farmId: number, data: AssignFieldToWorkerDto): Observable<ApiResponse<WorkerFieldAssignment>> {
    return this.http.post<ApiResponse<WorkerFieldAssignment>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields`, data);
  }

  // Update assignment (Edit)
  updateAssignment(farmId: number, assignmentId: number, data: AssignFieldToWorkerDto): Observable<ApiResponse<WorkerFieldAssignment>> {
    return this.http.put<ApiResponse<WorkerFieldAssignment>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields/${assignmentId}`, data);
  }

  // Remove assignment (soft delete)
  removeAssignment(farmId: number, assignmentId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields/${assignmentId}`);
  }

  // Get assignments by worker
  getAssignmentsByWorker(farmId: number, workerId: number, filter: WorkerFieldFilterDto): Observable<ApiResponse<PagedResult<WorkerFieldAssignment>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.isActive !== undefined && filter.isActive !== null) params = params.set('isActive', filter.isActive.toString());
    if (filter.assignedDateFrom) params = params.set('assignedDateFrom', filter.assignedDateFrom);
    if (filter.assignedDateTo) params = params.set('assignedDateTo', filter.assignedDateTo);
    if (filter.endDateFrom) params = params.set('endDateFrom', filter.endDateFrom);
    if (filter.endDateTo) params = params.set('endDateTo', filter.endDateTo);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerFieldAssignment>>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields/worker/${workerId}`, { params });
  }

  // Get assignments by field
  getAssignmentsByField(farmId: number, fieldId: number, filter: WorkerFieldFilterDto): Observable<ApiResponse<PagedResult<WorkerFieldAssignment>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.isActive !== undefined && filter.isActive !== null) params = params.set('isActive', filter.isActive.toString());
    if (filter.assignedDateFrom) params = params.set('assignedDateFrom', filter.assignedDateFrom);
    if (filter.assignedDateTo) params = params.set('assignedDateTo', filter.assignedDateTo);
    if (filter.endDateFrom) params = params.set('endDateFrom', filter.endDateFrom);
    if (filter.endDateTo) params = params.set('endDateTo', filter.endDateTo);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerFieldAssignment>>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields/field/${fieldId}`, { params });
  }

  // Get active assignments
  getActiveAssignments(farmId: number, filter: WorkerFieldFilterDto): Observable<ApiResponse<PagedResult<WorkerFieldAssignment>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.workerId) params = params.set('workerId', filter.workerId.toString());
    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.assignedDateFrom) params = params.set('assignedDateFrom', filter.assignedDateFrom);
    if (filter.assignedDateTo) params = params.set('assignedDateTo', filter.assignedDateTo);
    if (filter.endDateFrom) params = params.set('endDateFrom', filter.endDateFrom);
    if (filter.endDateTo) params = params.set('endDateTo', filter.endDateTo);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerFieldAssignment>>>(`${this.API_URL}/admin/farms/${farmId}/worker-fields/active`, { params });
  }
}