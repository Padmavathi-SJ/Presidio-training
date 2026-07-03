// src/app/features/worker/services/worker-field.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../features/admin/services/task.service';
import { WorkerFieldList, WorkerFieldDetail } from '../models/worker-field.model';

@Injectable({
  providedIn: 'root'
})
export class WorkerFieldService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  /**
   * Get all fields assigned to the current worker
   */
  getMyAssignedFields(): Observable<ApiResponse<WorkerFieldList[]>> {
    return this.http.get<ApiResponse<WorkerFieldList[]>>(
      `${this.API_URL}/worker/fields`
    );
  }

  /**
   * Get detailed information about a specific assigned field
   */
  getAssignedFieldDetail(fieldId: number): Observable<ApiResponse<WorkerFieldDetail>> {
    return this.http.get<ApiResponse<WorkerFieldDetail>>(
      `${this.API_URL}/worker/fields/${fieldId}`
    );
  }
}