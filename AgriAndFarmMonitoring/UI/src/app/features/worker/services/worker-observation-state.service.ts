import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { Observable, of, tap, catchError, finalize } from 'rxjs';
import { WorkerObservationService } from './worker-observation.service';
import { 
  ObservationDto, 
  ObservationFilterDto, 
  CreateObservationDto, 
  UpdateObservationDto,
  ObservationWorkerResponseDto,
  ObservationStatisticsDto
} from '../models/worker-observation.model';
import { ApiResponse, PagedResult } from '../../../features/admin/services/task.service';

export interface ObservationState {
  observations: ObservationDto[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  selectedObservation: ObservationDto | null;
  isSubmitting: boolean;
  lastUpdated: Date | null;
  statistics: ObservationStatisticsDto | null;
}

@Injectable({
  providedIn: 'root'
})
export class WorkerObservationStateService {
  private observationService = inject(WorkerObservationService);

  // ✅ State using signals (reactive state)
  private state = signal<ObservationState>({
    observations: [],
    totalCount: 0,
    isLoading: false,
    error: null,
    selectedObservation: null,
    isSubmitting: false,
    lastUpdated: null,
    statistics: null
  });

  // ✅ Computed values (derived state)
  public readonly observations = computed(() => this.state().observations);
  public readonly totalCount = computed(() => this.state().totalCount);
  public readonly isLoading = computed(() => this.state().isLoading);
  public readonly error = computed(() => this.state().error);
  public readonly selectedObservation = computed(() => this.state().selectedObservation);
  public readonly isSubmitting = computed(() => this.state().isSubmitting);
  public readonly lastUpdated = computed(() => this.state().lastUpdated);
  public readonly statistics = computed(() => this.state().statistics);

  // ✅ Current filter state
  private currentFilter = signal<ObservationFilterDto>({
    page: 1,
    pageSize: 10,
    validationStatus: 'pending',
    isDescending: true
  });

  constructor() {
    // ✅ Auto-load on filter change
    effect(() => {
      const filter = this.currentFilter();
      this.loadObservations(filter);
    });
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.observationService.getObservationStatistics().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.updateState({ statistics: res.data });
        }
      },
      error: (err) => console.error('Failed to load observation statistics', err)
    });
  }

  // =============================================
  // LOAD OBSERVATIONS (Reactive)
  // =============================================

  loadObservations(filter: ObservationFilterDto): void {
    this.updateState({ isLoading: true, error: null });
    
    this.observationService.getMyObservations(filter)
      .subscribe({
        next: (response: ApiResponse<PagedResult<ObservationDto>> | null) => {
          if (response?.success && response.data) {
            this.updateState({
              observations: response.data.items || [],
              totalCount: response.data.totalCount,
              isLoading: false,
              error: null,
              lastUpdated: new Date()
            });
          } else if (response?.success) {
            this.updateState({
              observations: [],
              totalCount: 0,
              isLoading: false,
              error: null,
              lastUpdated: new Date()
            });
          } else {
            this.updateState({
              isLoading: false,
              error: response?.message || 'Failed to load observations'
            });
          }
        },
        error: (err) => {
          this.updateState({ 
            isLoading: false, 
            error: err.message || 'Failed to load observations' 
          });
        }
      });
  }

  // ✅ Public method to update filter (triggers auto-reload)
  updateFilter(filter: Partial<ObservationFilterDto>): void {
    this.currentFilter.update(current => ({
      ...current,
      ...filter,
      page: filter.page ?? current.page
    }));
  }

  // ✅ Reload current filter
  refresh(): void {
    this.loadObservations(this.currentFilter());
  }

  // =============================================
  // CRUD OPERATIONS
  // =============================================

  createObservation(data: CreateObservationDto): Observable<ApiResponse<ObservationDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.observationService.createObservation(data)
      .pipe(
        tap((response: ApiResponse<ObservationDto>) => {
          if (response.success) {
            this.refresh();
          }
        }),
        catchError(error => {
          this.updateState({ error: error.message || 'Failed to create observation' });
          throw error;
        }),
        finalize(() => this.updateState({ isSubmitting: false }))
      );
  }

  updateObservation(id: number, data: UpdateObservationDto): Observable<ApiResponse<ObservationDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.observationService.updateObservation(id, data)
      .pipe(
        tap((response: ApiResponse<ObservationDto>) => {
          if (response.success) {
            this.refresh();
          }
        }),
        catchError(error => {
          this.updateState({ error: error.message || 'Failed to update observation' });
          throw error;
        }),
        finalize(() => this.updateState({ isSubmitting: false }))
      );
  }

  deleteObservation(id: number): Observable<ApiResponse<any>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.observationService.deleteObservation(id)
      .pipe(
        tap((response: ApiResponse<any>) => {
          if (response.success) {
            this.refresh();
          }
        }),
        catchError(error => {
          this.updateState({ error: error.message || 'Failed to delete observation' });
          throw error;
        }),
        finalize(() => this.updateState({ isSubmitting: false }))
      );
  }

  respondToAdmin(id: number, responseDto: ObservationWorkerResponseDto): Observable<ApiResponse<ObservationDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.observationService.respondToAdmin(id, responseDto)
      .pipe(
        tap((response: ApiResponse<ObservationDto>) => {
          if (response.success) {
            this.refresh();
          }
        }),
        catchError(error => {
          this.updateState({ error: error.message || 'Failed to respond to observation' });
          throw error;
        }),
        finalize(() => this.updateState({ isSubmitting: false }))
      );
  }

  uploadImage(file: File): Observable<ApiResponse<{ fileName: string }>> {
    return this.observationService.uploadImage(file);
  }

  // =============================================
  // STATE HELPERS
  // =============================================

  private updateState(newState: Partial<ObservationState>): void {
    this.state.update(current => ({ ...current, ...newState }));
  }
}
