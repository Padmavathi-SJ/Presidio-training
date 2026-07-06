// src/app/features/worker/harvests/components/harvest-list/harvest-list.component.ts
import { Component, OnInit, OnDestroy, inject, ViewChild, EventEmitter, Output, Input } from '@angular/core';
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
import { debounceTime, distinctUntilChanged, finalize, Subject, takeUntil } from 'rxjs';

import { WorkerHarvestService } from '../../services/worker-harvest.service';
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
  private harvestService = inject(WorkerHarvestService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  private destroy$ = new Subject<void>();

  @Input() farmId: number = 0;
  @Output() createHarvest = new EventEmitter<void>();
  @Output() editHarvest = new EventEmitter<HarvestDto>();
  @Output() viewHarvest = new EventEmitter<HarvestDto>();
  @Output() respondHarvest = new EventEmitter<HarvestDto>();
  @Output() deleteHarvest = new EventEmitter<number>();

  harvests = new MatTableDataSource<HarvestDto>([]);
  fields: any[] = [];
  cropCycles: any[] = [];

  selectedTabIndex = 0;
  displayedColumns: string[] = [
    'harvestDate', 'fieldName', 'cropType', 'quantityKg',
    'qualityGrade', 'images', 'approvalStatus', 'actions'
  ];

  totalHarvests = 0;
  pageSize = 10;
  pageIndex = 0;
  isLoading = false;

  filterForm!: FormGroup;

  readonly qualityGrades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];
  readonly harvestMethods = ['MANUAL', 'MECHANICAL', 'SEMI_MECHANICAL', 'COMBINE'];
  readonly approvalStatuses = ['PENDING', 'APPROVED', 'REJECTED', 'REQUEST_CHANGES'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.initFilterForm();
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadHarvests();

    this.filterForm.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.pageIndex = 0;
      this.loadHarvests();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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
    this.loadHarvests();
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

  loadHarvests(): void {
    this.isLoading = true;
    const formVal = this.filterForm.value;
    const filter: HarvestFilterDto = {
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      ...formVal,
      fromDate: formVal.fromDate ? new Date(formVal.fromDate).toISOString() : undefined,
      toDate: formVal.toDate ? new Date(formVal.toDate).toISOString() : undefined
    };

    Object.keys(filter).forEach(key => {
      const k = key as keyof HarvestFilterDto;
      if (filter[k] === '' || filter[k] === null || filter[k] === undefined) {
        delete filter[k];
      }
    });

    this.harvestService.getMyHarvests(filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.harvests.data = res.data.items || [];
            this.totalHarvests = res.data.totalCount;
          }
        },
        error: () => this.showError('Failed to load harvests.')
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadHarvests();
  }

  onSortChange(sortState: Sort): void {
    this.filterForm.patchValue({
      sortBy: sortState.direction ? sortState.active : 'harvestDate',
      isDescending: sortState.direction === 'desc'
    }, { emitEvent: false });
    this.loadHarvests();
  }

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

  get pendingCount(): number {
    return this.harvests.data.filter(h => h.approvalStatus === 'PENDING').length;
  }

  get approvedCount(): number {
    return this.harvests.data.filter(h => h.approvalStatus === 'APPROVED').length;
  }

  get totalQuantityKg(): number {
    return this.harvests.data
      .filter(h => h.approvalStatus === 'APPROVED')
      .reduce((sum, h) => sum + (h.quantityKg || 0), 0);
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
  }
}