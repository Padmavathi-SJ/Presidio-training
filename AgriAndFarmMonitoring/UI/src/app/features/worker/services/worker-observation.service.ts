import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../features/admin/services/task.service';
import { 
  ObservationDto, 
  CreateObservationDto, 
  UpdateObservationDto, 
  ObservationFilterDto,
  ObservationWorkerResponseDto 
} from '../models/worker-observation.model';

@Injectable({
  providedIn: 'root'
})
export class WorkerObservationService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  getMyObservations(filter: ObservationFilterDto): Observable<ApiResponse<PagedResult<ObservationDto>>> {
    let params = new HttpParams()
      .set('page', (filter.page || 1).toString())
      .set('pageSize', (filter.pageSize || 10).toString())
      .set('isDescending', (filter.isDescending ?? true).toString());

    if (filter.fieldId) params = params.set('fieldId', filter.fieldId.toString());
    if (filter.cropCycleId) params = params.set('cropCycleId', filter.cropCycleId.toString());
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);
    if (filter.cropHealth) params = params.set('cropHealth', filter.cropHealth);
    if (filter.validationStatus) params = params.set('validationStatus', filter.validationStatus);
    if (filter.includeDeleted !== undefined) params = params.set('includeDeleted', filter.includeDeleted.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<ApiResponse<PagedResult<ObservationDto>>>(`${this.API_URL}/worker/observations/my`, { params });
  }

  getObservationById(id: number): Observable<ApiResponse<ObservationDto>> {
    return this.http.get<ApiResponse<ObservationDto>>(`${this.API_URL}/worker/observations/${id}`);
  }

  createObservation(data: CreateObservationDto): Observable<ApiResponse<ObservationDto>> {
    return this.http.post<ApiResponse<ObservationDto>>(`${this.API_URL}/worker/observations`, data);
  }

  updateObservation(id: number, data: UpdateObservationDto): Observable<ApiResponse<ObservationDto>> {
    return this.http.put<ApiResponse<ObservationDto>>(`${this.API_URL}/worker/observations/${id}`, data);
  }

  deleteObservation(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/worker/observations/${id}`);
  }

  respondToAdmin(id: number, data: ObservationWorkerResponseDto): Observable<ApiResponse<ObservationDto>> {
    return this.http.post<ApiResponse<ObservationDto>>(`${this.API_URL}/worker/observations/${id}/respond`, data);
  }

  uploadImage(file: File): Observable<ApiResponse<{ fileName: string, url: string }>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<{ fileName: string, url: string }>>(`${this.API_URL}/worker/observations/upload`, formData);
  }
}