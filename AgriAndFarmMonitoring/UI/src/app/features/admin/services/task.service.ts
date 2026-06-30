// src/app/features/admin/services/task.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  Task,
  CreateTaskDto,
  UpdateTaskDto,
  TaskFilterDto,
  TaskStatisticsDto,
  BulkAssignTaskDto,
  BulkAssignResultDto,
  BulkStatusUpdateDto,
  BulkReassignDto,
  UpdateTaskStatusDto,
  ReassignTaskDto
} from '../models/task.model';

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
export class TaskService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getTasks(farmId: number, filter: TaskFilterDto): Observable<ApiResponse<PagedResult<Task>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.workerId) params = params.set('workerId', filter.workerId.toString());
    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.status) params = params.set('status', filter.status);
    if (filter.priority) params = params.set('priority', filter.priority);
    if (filter.taskName) params = params.set('taskName', filter.taskName);
    if (filter.assignedDateFrom) params = params.set('assignedDateFrom', filter.assignedDateFrom);
    if (filter.assignedDateTo) params = params.set('assignedDateTo', filter.assignedDateTo);
    if (filter.dueDateFrom) params = params.set('dueDateFrom', filter.dueDateFrom);
    if (filter.dueDateTo) params = params.set('dueDateTo', filter.dueDateTo);
    if (filter.isOverdue !== undefined && filter.isOverdue !== null) params = params.set('isOverdue', filter.isOverdue.toString());
    if (filter.activeOnly !== undefined && filter.activeOnly !== null) params = params.set('activeOnly', filter.activeOnly.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<Task>>>(`${this.API_URL}/admin/farms/${farmId}/tasks`, { params });
  }

  getTaskById(farmId: number, taskId: number): Observable<ApiResponse<Task>> {
    return this.http.get<ApiResponse<Task>>(`${this.API_URL}/admin/farms/${farmId}/tasks/${taskId}`);
  }

  createTask(farmId: number, data: CreateTaskDto): Observable<ApiResponse<Task>> {
    return this.http.post<ApiResponse<Task>>(`${this.API_URL}/admin/farms/${farmId}/tasks`, data);
  }

  updateTask(farmId: number, taskId: number, data: UpdateTaskDto): Observable<ApiResponse<Task>> {
    return this.http.put<ApiResponse<Task>>(`${this.API_URL}/admin/farms/${farmId}/tasks/${taskId}`, data);
  }

  updateTaskStatus(farmId: number, taskId: number, data: UpdateTaskStatusDto): Observable<ApiResponse<Task>> {
    return this.http.put<ApiResponse<Task>>(`${this.API_URL}/admin/farms/${farmId}/tasks/${taskId}/status`, data);
  }

  reassignTask(farmId: number, taskId: number, data: ReassignTaskDto): Observable<ApiResponse<Task>> {
    return this.http.put<ApiResponse<Task>>(`${this.API_URL}/admin/farms/${farmId}/tasks/${taskId}/reassign`, data);
  }

  deleteTask(farmId: number, taskId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/admin/farms/${farmId}/tasks/${taskId}`);
  }

  getTasksByWorker(farmId: number, workerId: number): Observable<ApiResponse<Task[]>> {
    return this.http.get<ApiResponse<Task[]>>(`${this.API_URL}/admin/farms/${farmId}/tasks/worker/${workerId}`);
  }

  getTasksByField(farmId: number, fieldId: number): Observable<ApiResponse<Task[]>> {
    return this.http.get<ApiResponse<Task[]>>(`${this.API_URL}/admin/farms/${farmId}/tasks/field/${fieldId}`);
  }

  getOverdueTasks(farmId: number): Observable<ApiResponse<Task[]>> {
    return this.http.get<ApiResponse<Task[]>>(`${this.API_URL}/admin/farms/${farmId}/tasks/overdue`);
  }

  getActiveTasks(farmId: number): Observable<ApiResponse<Task[]>> {
    return this.http.get<ApiResponse<Task[]>>(`${this.API_URL}/admin/farms/${farmId}/tasks/active`);
  }

  getTaskStatistics(farmId: number): Observable<ApiResponse<TaskStatisticsDto>> {
    return this.http.get<ApiResponse<TaskStatisticsDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/statistics`);
  }

  getTaskCompletionHistory(farmId: number, fromDate?: string, toDate?: string): Observable<ApiResponse<Task[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<Task[]>>(`${this.API_URL}/admin/farms/${farmId}/tasks/completion-history`, { params });
  }

  bulkAssignTasks(farmId: number, data: BulkAssignTaskDto): Observable<ApiResponse<BulkAssignResultDto>> {
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-assign`, data);
  }

  bulkUpdateStatus(farmId: number, data: BulkStatusUpdateDto): Observable<ApiResponse<BulkAssignResultDto>> {
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-status`, data);
  }

  bulkReassignTasks(farmId: number, data: BulkReassignDto): Observable<ApiResponse<BulkAssignResultDto>> {
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-reassign`, data);
  }

  bulkAssignFromExcel(farmId: number, file: File): Observable<ApiResponse<BulkAssignResultDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-assign-excel`, formData);
  }

  bulkUpdateStatusFromExcel(farmId: number, file: File): Observable<ApiResponse<BulkAssignResultDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-status-excel`, formData);
  }

  bulkReassignFromExcel(farmId: number, file: File): Observable<ApiResponse<BulkAssignResultDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<BulkAssignResultDto>>(`${this.API_URL}/admin/farms/${farmId}/tasks/bulk-reassign-excel`, formData);
  }

  downloadBulkAssignTemplate(farmId: number): Observable<Blob> {
    return this.http.get(`${this.API_URL}/admin/farms/${farmId}/tasks/templates/bulk-assign`, { responseType: 'blob' });
  }

  downloadStatusUpdateTemplate(farmId: number): Observable<Blob> {
    return this.http.get(`${this.API_URL}/admin/farms/${farmId}/tasks/templates/status-update`, { responseType: 'blob' });
  }

  downloadReassignTemplate(farmId: number): Observable<Blob> {
    return this.http.get(`${this.API_URL}/admin/farms/${farmId}/tasks/templates/reassign`, { responseType: 'blob' });
  }
}