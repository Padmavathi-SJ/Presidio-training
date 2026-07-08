import { Injectable, computed, inject, signal, OnDestroy } from '@angular/core';
import { Subject, takeUntil, catchError, of } from 'rxjs';
import { AdminHarvestService } from './admin-harvest.service';
import { AuthService } from '../../../../core/services/auth.service';
import { 
  HarvestDto, 
  HarvestFilterDto, 
  YieldStatisticsDto 
} from '../models/admin-harvest.model';

export interface AdminHarvestState {
  harvests: HarvestDto[];
  totalCount: number;
  isLoading: boolean;
  error: string | null;
  lastUpdated: Date | null;
  statistics: YieldStatisticsDto | null;
}

@Injectable({
  providedIn: 'root'
})
export class AdminHarvestStateService implements OnDestroy {
  private harvestService = inject(AdminHarvestService);
  private authService = inject(AuthService);
  private destroy$ = new Subject<void>();
  private farmId = this.authService.getFarmId();

  // Primary state signal
  private state = signal<AdminHarvestState>({
    harvests: [],
    totalCount: 0,
    isLoading: false,
    error: null,
    lastUpdated: null,
    statistics: null
  });

  // Filter signal
  private currentFilter = signal<HarvestFilterDto>({
    page: 1,
    pageSize: 10,
    approvalStatus: 'PENDING',
    isDescending: true
  });

  // Computed views for components
  readonly harvests = computed(() => this.state().harvests);
  readonly totalCount = computed(() => this.state().totalCount);
  readonly isLoading = computed(() => this.state().isLoading);
  readonly error = computed(() => this.state().error);
  readonly filter = computed(() => this.currentFilter());
  readonly statistics = computed(() => this.state().statistics);

  constructor() {
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --- Actions ---

  private updateState(newState: Partial<AdminHarvestState>): void {
    this.state.update(current => ({ ...current, ...newState }));
  }

  updateFilter(filter: Partial<HarvestFilterDto>): void {
    this.currentFilter.update(current => ({
      ...current,
      ...filter,
      page: filter.page ?? current.page
    }));
    this.loadHarvests(this.currentFilter());
  }

  refresh(): void {
    this.loadHarvests(this.currentFilter());
    this.loadStatistics();
  }

  loadHarvests(filter: HarvestFilterDto = this.currentFilter()): void {
    this.updateState({ isLoading: true, error: null });
    
    this.harvestService.getHarvests(this.farmId, filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response?.success && response.data) {
            this.updateState({
              harvests: response.data.items || [],
              totalCount: response.data.totalCount,
              isLoading: false,
              error: null,
              lastUpdated: new Date()
            });
          } else {
            this.updateState({
              harvests: [],
              totalCount: 0,
              isLoading: false,
              error: response?.message || 'Failed to load harvests'
            });
          }
        },
        error: (err) => {
          this.updateState({ 
            isLoading: false, 
            error: err.message || 'Failed to load harvests' 
          });
        }
      });
  }

  loadStatistics(): void {
    this.harvestService.getYieldStatistics(this.farmId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.updateState({ statistics: res.data });
          }
        },
        error: (err) => console.error('Failed to load yield statistics', err)
      });
  }
}
