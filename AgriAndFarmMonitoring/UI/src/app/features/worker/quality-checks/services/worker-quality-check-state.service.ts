import { Injectable, computed, inject, signal } from '@angular/core';
import { WorkerQualityCheckService } from './worker-quality-check.service';
import { QualityCheckDto, QualityCheckFilterDto, QualityStatisticsDto } from '../models/worker-quality-check.model';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WorkerQualityCheckStateService {
  private service = inject(WorkerQualityCheckService);

  private state = signal<{
    qualityChecks: QualityCheckDto[];
    totalCount: number;
    loading: boolean;
    error: string | null;
    filter: QualityCheckFilterDto;
    statistics: QualityStatisticsDto | null;
  }>({
    qualityChecks: [],
    totalCount: 0,
    loading: false,
    error: null,
    filter: {
      page: 1,
      pageSize: 10,
      sortBy: 'CheckDate',
      isDescending: true,
      approvalStatus: 'PENDING'
    },
    statistics: null
  });

  // Selectors
  readonly qualityChecks = computed(() => this.state().qualityChecks);
  readonly totalCount = computed(() => this.state().totalCount);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);
  readonly filter = computed(() => this.state().filter);

  readonly pendingCount = computed(() => this.state().statistics?.pendingChecks || 0);
  readonly approvedCount = computed(() => this.state().statistics?.approvedChecks || 0);
  readonly rejectedCount = computed(() => this.state().statistics?.rejectedChecks || 0);
  readonly totalCountGlobal = computed(() => this.state().statistics?.totalChecks || 0);
  
  readonly passRate = computed(() => {
    const totalFinished = this.approvedCount() + this.rejectedCount();
    if (totalFinished === 0) return 0;
    return (this.approvedCount() / totalFinished) * 100;
  });

  async loadQualityChecks() {
    this.state.update(s => ({ ...s, loading: true, error: null }));
    try {
      const response = await firstValueFrom(this.service.getMyQualityChecks(this.state().filter));
      if (response.success) {
        this.state.update(s => ({
          ...s,
          qualityChecks: response.data.items,
          totalCount: response.data.totalCount,
          loading: false
        }));
      }
    } catch (err: any) {
      this.state.update(s => ({ ...s, error: err.message, loading: false }));
    }
  }

  async loadStatistics() {
    try {
      const response = await firstValueFrom(this.service.getStatistics());
      if (response.success) {
        this.state.update(s => ({
          ...s,
          statistics: response.data
        }));
      }
    } catch (err: any) {
      console.error('Failed to load statistics', err);
    }
  }

  updateFilter(newFilter: Partial<QualityCheckFilterDto>) {
    this.state.update(s => ({
      ...s,
      filter: { ...s.filter, ...newFilter, page: newFilter.page || 1 }
    }));
    this.loadQualityChecks();
  }

  refresh() {
    this.loadQualityChecks();
    this.loadStatistics();
  }
}
