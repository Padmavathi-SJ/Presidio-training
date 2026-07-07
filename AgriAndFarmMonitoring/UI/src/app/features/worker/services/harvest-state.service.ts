// src/app/features/worker/services/harvest-state.service.ts
import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, tap, catchError, finalize } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { 
  HarvestDto, 
  HarvestFilterDto, 
  CreateHarvestDto, 
  UpdateHarvestDto,
  HarvestWorkerResponseDto,
  ApiResponse,
  PagedResult
} from '../models/worker-harvest.model';

export interface HarvestState {
  harvests: HarvestDto[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  selectedHarvest: HarvestDto | null;
  isSubmitting: boolean;
  lastUpdated: Date | null;
}

@Injectable({
  providedIn: 'root'
})
export class HarvestStateService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // ✅ State using signals (reactive state)
  private state = signal<HarvestState>({
    harvests: [],
    totalCount: 0,
    isLoading: false,
    error: null,
    selectedHarvest: null,
    isSubmitting: false,
    lastUpdated: null
  });

  // ✅ Computed values (derived state)
  public readonly harvests = computed(() => this.state().harvests);
  public readonly totalCount = computed(() => this.state().totalCount);
  public readonly isLoading = computed(() => this.state().isLoading);
  public readonly error = computed(() => this.state().error);
  public readonly selectedHarvest = computed(() => this.state().selectedHarvest);
  public readonly isSubmitting = computed(() => this.state().isSubmitting);
  public readonly lastUpdated = computed(() => this.state().lastUpdated);

  // ✅ Computed stats
  public readonly pendingCount = computed(() => 
    this.state().harvests.filter(h => h.approvalStatus === 'PENDING').length
  );
  
  public readonly approvedCount = computed(() => 
    this.state().harvests.filter(h => h.approvalStatus === 'APPROVED').length
  );
  
  public readonly totalQuantityKg = computed(() => 
    this.state().harvests
      .filter(h => h.approvalStatus === 'APPROVED')
      .reduce((sum, h) => sum + (h.quantityKg || 0), 0)
  );

  // ✅ Current filter state
  private currentFilter = signal<HarvestFilterDto>({
    page: 1,
    pageSize: 10,
    approvalStatus: 'PENDING',
    isDescending: true
  });

  constructor() {
    // ✅ Auto-load on filter change
    effect(() => {
      const filter = this.currentFilter();
      this.loadHarvests(filter);
    });
  }

  // =============================================
  // LOAD HARVESTS (Reactive)
  // =============================================

  loadHarvests(filter: HarvestFilterDto): void {
    this.updateState({ isLoading: true, error: null });
    
    const params = this.buildParams(filter);
    
    this.http.get<ApiResponse<PagedResult<HarvestDto>>>(`${this.API_URL}/worker/harvests/my`, { params })
      .pipe(
        catchError(error => {
          this.updateState({ 
            isLoading: false, 
            error: error.message || 'Failed to load harvests' 
          });
          return of(null);
        })
      )
      .subscribe((response: ApiResponse<PagedResult<HarvestDto>> | null) => {
        if (response?.success && response.data) {
          this.updateState({
            harvests: response.data.items || [],
            totalCount: response.data.totalCount,
            isLoading: false,
            error: null,
            lastUpdated: new Date()
          });
        } else if (response?.success) {
          this.updateState({
            harvests: [],
            totalCount: 0,
            isLoading: false,
            error: null,
            lastUpdated: new Date()
          });
        }
      });
  }

  // ✅ Public method to update filter (triggers auto-reload)
  updateFilter(filter: Partial<HarvestFilterDto>): void {
    this.currentFilter.update(current => ({
      ...current,
      ...filter,
      page: filter.page ?? current.page
    }));
  }

  // ✅ Reload current filter
  refresh(): void {
    this.loadHarvests(this.currentFilter());
  }

  // =============================================
  // CRUD OPERATIONS
  // =============================================

  createHarvest(data: CreateHarvestDto): Observable<ApiResponse<HarvestDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.http.post<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests`, data)
      .pipe(
        tap((response: ApiResponse<HarvestDto>) => {
          if (response.success) {
            // ✅ Auto-refresh after successful creation
            this.refresh();
            this.updateState({ isSubmitting: false });
          }
        }),
        catchError((error: any) => {
          this.updateState({ 
            isSubmitting: false, 
            error: error.message || 'Failed to create harvest' 
          });
          throw error;
        }),
        finalize(() => {
          this.updateState({ isSubmitting: false });
        })
      );
  }

  updateHarvest(id: number, data: UpdateHarvestDto): Observable<ApiResponse<HarvestDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.http.patch<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests/${id}`, data)
      .pipe(
        tap((response: ApiResponse<HarvestDto>) => {
          if (response.success) {
            // ✅ Auto-refresh after successful update
            this.refresh();
            this.updateState({ isSubmitting: false });
          }
        }),
        catchError((error: any) => {
          this.updateState({ 
            isSubmitting: false, 
            error: error.message || 'Failed to update harvest' 
          });
          throw error;
        }),
        finalize(() => {
          this.updateState({ isSubmitting: false });
        })
      );
  }

  deleteHarvest(id: number): Observable<ApiResponse<boolean>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/worker/harvests/${id}`)
      .pipe(
        tap((response: ApiResponse<boolean>) => {
          if (response.success) {
            // ✅ Auto-refresh after successful deletion
            this.refresh();
            this.updateState({ isSubmitting: false });
          }
        }),
        catchError((error: any) => {
          this.updateState({ 
            isSubmitting: false, 
            error: error.message || 'Failed to delete harvest' 
          });
          throw error;
        }),
        finalize(() => {
          this.updateState({ isSubmitting: false });
        })
      );
  }

  respondToAdmin(id: number, data: HarvestWorkerResponseDto): Observable<ApiResponse<HarvestDto>> {
    this.updateState({ isSubmitting: true, error: null });
    
    return this.http.post<ApiResponse<HarvestDto>>(`${this.API_URL}/worker/harvests/${id}/respond`, data)
      .pipe(
        tap((response: ApiResponse<HarvestDto>) => {
          if (response.success) {
            this.refresh();
            this.updateState({ isSubmitting: false });
          }
        }),
        catchError((error: any) => {
          this.updateState({ 
            isSubmitting: false, 
            error: error.message || 'Failed to respond' 
          });
          throw error;
        }),
        finalize(() => {
          this.updateState({ isSubmitting: false });
        })
      );
  }

  // =============================================
  // SELECTION & UI STATE
  // =============================================

  selectHarvest(harvest: HarvestDto): void {
    this.updateState({ selectedHarvest: harvest });
  }

  clearSelection(): void {
    this.updateState({ selectedHarvest: null });
  }

  // =============================================
  // PRIVATE HELPERS
  // =============================================

  private updateState(updates: Partial<HarvestState>): void {
    this.state.update(current => ({
      ...current,
      ...updates
    }));
  }

  private buildParams(filter: HarvestFilterDto): HttpParams {
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

    return params;
  }
}