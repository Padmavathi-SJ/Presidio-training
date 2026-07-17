import { Component, OnInit, inject, ViewChild, EventEmitter, Output, effect, OnDestroy, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

import { WorkerObservationStateService } from '../../services/worker-observation-state.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ObservationDto, ObservationFilterDto } from '../../models/worker-observation.model';

@Component({
  selector: 'app-observation-list',
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
    MatMenuModule,
    MatButtonModule,
    MatCardModule,
    DatePipe
  ],
  templateUrl: './observation-list.html'
})
export class ObservationListComponent implements OnInit, OnDestroy {
  private observationState = inject(WorkerObservationStateService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);

  @Output() createObservation = new EventEmitter<void>();
  @Output() editObservation = new EventEmitter<ObservationDto>();
  @Output() viewObservation = new EventEmitter<ObservationDto>();
  @Output() respondObservation = new EventEmitter<ObservationDto>();
  @Output() deleteObservation = new EventEmitter<number>();

  // ✅ Signals for reactive data
  readonly observations = this.observationState.observations;
  readonly isLoading = this.observationState.isLoading;
  readonly totalCount = this.observationState.totalCount;
  readonly pendingCount = computed(() => this.observationState.statistics()?.pendingObservations || 0);
  readonly verifiedCount = computed(() => this.observationState.statistics()?.verifiedObservations || 0);

  // ✅ MatTableDataSource for the table
  dataSource = new MatTableDataSource<ObservationDto>([]);

  // ✅ Signal for selected tab
  selectedTabIndex = 0;

  displayedColumns: string[] = ['observationDate', 'fieldName', 'cropHealth', 'pestDetected', 'images', 'validationStatus', 'actions'];

  filterForm!: FormGroup;
  fields: any[] = [];
  cropCycles: any[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private destroy$ = new Subject<void>();

  ngAfterViewInit(): void {
    // ✅ Link paginator and sort to data source
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  constructor() {
    this.initFilterForm();
    
    // ✅ Auto-update dataSource when observations signal changes
    effect(() => {
      this.dataSource.data = this.observations();
    });
  }

  ngOnInit(): void {
    this.loadFields();
  }

  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      validationStatus: ['pending'],
      cropHealth: [''],
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
        const filter: ObservationFilterDto = {
          page: 1,
          pageSize: 10,
          ...values,
          fromDate: values.fromDate ? new Date(values.fromDate).toISOString() : undefined,
          toDate: values.toDate ? new Date(values.toDate).toISOString() : undefined
        };
        
        // Convert empty strings to undefined so state merges them properly
        Object.keys(filter).forEach(key => {
          const k = key as keyof ObservationFilterDto;
          if (filter[k] === '') {
            filter[k] = undefined as any;
          }
        });
        
        // ✅ Update state filter - auto triggers reload
        this.observationState.updateFilter(filter);
      });
  }

  loadFields(): void {
    this.fieldService.getMyAssignedFields().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.fields = res.data;
        }
      },
      error: (err: any) => console.error('Failed to load fields', err)
    });
  }

  onFilterFieldSelected(fieldId: number): void {
    this.filterForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
    this.cropCycles = [];
    if (fieldId) {
      this.fieldService.getAssignedFieldDetail(fieldId).subscribe({
        next: (res: any) => {
          if (res.success && res.data && res.data.cropCycles) {
            this.cropCycles = res.data.cropCycles;
          }
        },
        error: (err: any) => console.error('Failed to load crop cycles', err)
      });
    }
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['pending', 'questioned', 'verified', 'invalid'];
    this.filterForm.patchValue({ validationStatus: statuses[index] });
  }

  onPageChange(event: PageEvent): void {
    this.observationState.updateFilter({
      page: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }

  onSortChange(sort: Sort): void {
    if (!sort.active || sort.direction === '') {
      this.observationState.updateFilter({ sortBy: undefined, isDescending: true });
      return;
    }

    this.observationState.updateFilter({
      sortBy: sort.active,
      isDescending: sort.direction === 'desc'
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
