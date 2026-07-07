// src/app/features/worker/harvests/components/harvest-list/harvest-list.component.ts
import { Component, OnInit, inject, ViewChild, EventEmitter, Output, effect, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

import { HarvestStateService } from '../../services/harvest-state.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { HarvestDto, HarvestFilterDto } from '../../models/worker-harvest.model';

@Component({
  selector: 'app-harvest-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatTabsModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatProgressSpinnerModule,
    DatePipe,
    DecimalPipe,
    CurrencyPipe
  ],
  templateUrl: './harvest-list.component.html',
  styleUrls: ['./harvest-list.component.scss']
})
export class HarvestListComponent implements OnInit, OnDestroy {
  private harvestState = inject(HarvestStateService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  @Output() createHarvest = new EventEmitter<void>();
  @Output() editHarvest = new EventEmitter<HarvestDto>();
  @Output() viewHarvest = new EventEmitter<HarvestDto>();
  @Output() respondHarvest = new EventEmitter<HarvestDto>();
  @Output() deleteHarvest = new EventEmitter<number>();

  // ✅ Signals for reactive data
  readonly harvests = this.harvestState.harvests;
  readonly totalCount = this.harvestState.totalCount;
  readonly isLoading = this.harvestState.isLoading;
  readonly pendingCount = this.harvestState.pendingCount;
  readonly approvedCount = this.harvestState.approvedCount;
  readonly totalQuantityKg = this.harvestState.totalQuantityKg;

  // ✅ MatTableDataSource for the table
  dataSource = new MatTableDataSource<HarvestDto>([]);

  // ✅ Signal for selected tab
  selectedTabIndex = 0;

  displayedColumns: string[] = [
    'harvestDate', 'fieldName', 'cropType', 'quantityKg',
    'qualityGrade', 'images', 'approvalStatus', 'actions'
  ];

  filterForm!: FormGroup;
  fields: any[] = [];
  cropCycles: any[] = [];

  readonly qualityGrades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];
  readonly harvestMethods = ['MANUAL', 'MECHANICAL', 'SEMI_MECHANICAL', 'COMBINE'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private destroy$ = new Subject<void>();

  constructor() {
    this.initFilterForm();
    
    // ✅ Auto-update dataSource when harvests signal changes
    effect(() => {
      this.dataSource.data = this.harvests();
    });
  }

  ngOnInit(): void {
    this.loadFields();
  }

  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      approvalStatus: ['PENDING'],
      qualityGrade: [''],
      harvestMethod: [''],
      includeDeleted: [false],
      fromDate: [''],
      toDate: ['']
    });

    // ✅ Auto-trigger filter changes
    this.filterForm.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((values) => {
        const filter: HarvestFilterDto = {
          page: 1,
          pageSize: 10,
          ...values,
          fromDate: values.fromDate ? new Date(values.fromDate).toISOString() : undefined,
          toDate: values.toDate ? new Date(values.toDate).toISOString() : undefined
        };
        
        // Remove empty values
        Object.keys(filter).forEach(key => {
          const k = key as keyof HarvestFilterDto;
          if (filter[k] === '' || filter[k] === null || filter[k] === undefined) {
            delete filter[k];
          }
        });
        
        // ✅ Update state filter - auto triggers reload
        this.harvestState.updateFilter(filter);
      });
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['PENDING', 'APPROVED', 'REJECTED', 'REQUEST_CHANGES'];
    this.filterForm.patchValue({ approvalStatus: statuses[index] });
  }

  onFilterFieldSelected(fieldId: number): void {
    this.filterForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
    if (fieldId) {
      this.fieldService.getAssignedFieldDetail(fieldId).subscribe({
        next: (res: any) => {
          if (res.success && res.data?.cropCycles) {
            this.cropCycles = res.data.cropCycles;
          }
        },
        error: () => {}
      });
    }
  }

  loadFields(): void {
    this.fieldService.getMyAssignedFields().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.fields = res.data;
        }
      },
      error: () => console.error('Failed to load fields')
    });
  }

  onPageChange(event: PageEvent): void {
    this.harvestState.updateFilter({
      page: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }

  onSortChange(sortState: Sort): void {
    if (sortState.direction) {
      this.harvestState.updateFilter({
        sortBy: sortState.active,
        isDescending: sortState.direction === 'desc'
      });
    }
  }

  // ✅ Helper methods for template
  getApprovalBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED': return 'badge-approved';
      case 'REJECTED': return 'badge-rejected';
      case 'REQUEST_CHANGES': return 'badge-changes';
      default: return 'badge-pending';
    }
  }

  getApprovalIcon(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED': return 'check_circle';
      case 'REJECTED': return 'cancel';
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'hourglass_empty';
    }
  }

  getQualityClass(grade: string | undefined): string {
    switch (grade?.toUpperCase()) {
      case 'A_PLUS': return 'quality-premium';
      case 'A': return 'quality-a';
      case 'B': return 'quality-b';
      case 'C': return 'quality-c';
      case 'D': return 'quality-d';
      case 'REJECTED': return 'quality-reject';
      default: return 'quality-none';
    }
  }

  formatQualityGrade(grade: string | undefined): string {
    if (!grade) return '—';
    const displayMap: { [key: string]: string } = {
      'A_PLUS': 'A+',
      'A': 'A',
      'B': 'B',
      'C': 'C',
      'D': 'D',
      'REJECTED': 'Rejected'
    };
    return displayMap[grade] || grade.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  formatHarvestMethod(method: string | undefined): string {
    if (!method) return '—';
    return method.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  canEdit(harvest: HarvestDto): boolean {
    return harvest.approvalStatus === 'PENDING' || harvest.approvalStatus === 'REQUEST_CHANGES';
  }

  canDelete(harvest: HarvestDto): boolean {
    return harvest.approvalStatus === 'PENDING' || harvest.approvalStatus === 'REJECTED';
  }

  canRespond(harvest: HarvestDto): boolean {
    return harvest.approvalStatus === 'REQUEST_CHANGES';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}