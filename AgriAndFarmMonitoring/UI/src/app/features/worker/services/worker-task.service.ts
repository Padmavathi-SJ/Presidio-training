import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../features/admin/services/task.service';
import { 
  WorkerTaskDto, 
  WorkerTaskStatisticsDto, 
  UpdateWorkerTaskStatusDto, 
  WorkerTaskFilterDto 
} from '../models/worker-task.model';

@Injectable({
  providedIn: 'root'
})
export class WorkerTaskService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getMyTasks(filter: WorkerTaskFilterDto): Observable<ApiResponse<PagedResult<WorkerTaskDto>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? false).toString());

    if (filter.status) params = params.set('status', filter.status);
    if (filter.priority) params = params.set('priority', filter.priority);
    if (filter.taskName) params = params.set('taskName', filter.taskName);
    if (filter.dueDateFrom) params = params.set('dueDateFrom', filter.dueDateFrom);
    if (filter.dueDateTo) params = params.set('dueDateTo', filter.dueDateTo);
    if (filter.isOverdue !== undefined && filter.isOverdue !== null) params = params.set('isOverdue', filter.isOverdue.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerTaskDto>>>(`${this.API_URL}/worker/tasks`, { params });
  }

  getStatistics(): Observable<ApiResponse<WorkerTaskStatisticsDto>> {
    return this.http.get<ApiResponse<WorkerTaskStatisticsDto>>(`${this.API_URL}/worker/tasks/statistics`);
  }

  getTaskHistory(filter: WorkerTaskFilterDto): Observable<ApiResponse<PagedResult<WorkerTaskDto>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString()); // usually history is descending

    if (filter.taskName) params = params.set('taskName', filter.taskName);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<WorkerTaskDto>>>(`${this.API_URL}/worker/tasks/history`, { params });
  }

  getTaskById(taskId: number): Observable<ApiResponse<WorkerTaskDto>> {
    return this.http.get<ApiResponse<WorkerTaskDto>>(`${this.API_URL}/worker/tasks/${taskId}`);
  }

  updateTaskStatus(taskId: number, data: UpdateWorkerTaskStatusDto): Observable<ApiResponse<WorkerTaskDto>> {
    return this.http.put<ApiResponse<WorkerTaskDto>>(`${this.API_URL}/worker/tasks/${taskId}/status`, data);
  }
}
