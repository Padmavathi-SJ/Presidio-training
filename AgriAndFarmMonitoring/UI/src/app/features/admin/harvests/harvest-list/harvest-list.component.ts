import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit, inject, effect } from '@angular/core';
import { CommonModule, TitleCasePipe, DatePipe, DecimalPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subject, takeUntil, debounceTime } from 'rxjs';

import { AdminHarvestStateService } from '../services/admin-harvest-state.service';
import { FieldService } from '../../services/field.service';
import { CropCycleService } from '../../services/crop-cycle.service';
import { AuthService } from '../../../../core/services/auth.service';
import { HarvestDto } from '../models/admin-harvest.model';

import { HarvestDetailsComponent } from '../harvest-details/harvest-details.component';
import { HarvestFormComponent } from '../harvest-form/harvest-form.component';
import { HarvestApprovalComponent } from '../harvest-approval/harvest-approval.component';

@Component({
  selector: 'app-harvest-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatPaginatorModule,
    MatSortModule, MatIconModule, MatButtonModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatDatepickerModule, MatNativeDateModule,
    MatProgressSpinnerModule, MatMenuModule, MatDialogModule, MatDividerModule,
    MatTooltipModule,
    TitleCasePipe, DatePipe, DecimalPipe
  ],
  templateUrl: './harvest-list.component.html',
  styleUrl: './harvest-list.component.scss'
})
export class HarvestListComponent implements OnInit, AfterViewInit, OnDestroy {
  private harvestState = inject(AdminHarvestStateService);
  private fieldService = inject(FieldService);
  private cropCycleService = inject(CropCycleService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  public dialog = inject(MatDialog);
  
  private destroy$ = new Subject<void>();
  
  dataSource = new MatTableDataSource<HarvestDto>([]);
  displayedColumns = ['harvestDate', 'fieldName', 'cropType', 'quantityKg', 'qualityGrade', 'submitterName', 'status', 'actions'];
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  filterForm!: FormGroup;
  selectedTabIndex = 0; // 0: PENDING, 1: APPROVED, 2: REJECTED, 3: NEEDS_CHANGES
  
  fields: any[] = [];
  cropCycles: any[] = [];
  filteredCropCycles: any[] = [];
  workers: any[] = [];

  harvests = this.harvestState.harvests;
  totalCount = this.harvestState.totalCount;
  isLoading = this.harvestState.isLoading;
  currentFilter = this.harvestState.filter;
  
  constructor() {
    effect(() => {
      this.dataSource.data = this.harvests();
    });
  }

  ngOnInit() {
    this.initForm();
    this.loadDropdownData();
    
    // Set initial filter status to match tab
    this.harvestState.updateFilter({ approvalStatus: 'PENDING' });

    this.filterForm.valueChanges
      .pipe(debounceTime(500), takeUntil(this.destroy$))
      .subscribe(val => {
        const filterUpdate = {
          fieldId: val.fieldId || (undefined as any),
          cropCycleId: val.cropCycleId || (undefined as any),
          qualityGrade: val.qualityGrade || (undefined as any),
          fromDate: val.fromDate ? new Date(val.fromDate).toISOString() : (undefined as any),
          toDate: val.toDate ? new Date(val.toDate).toISOString() : (undefined as any),
          page: 1
        };
        this.harvestState.updateFilter(filterUpdate);
      });
      
    this.filterForm.get('fieldId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(fieldId => {
        if (fieldId) {
          this.filteredCropCycles = this.cropCycles.filter(c => c.fieldId === fieldId);
        } else {
          this.filteredCropCycles = [...this.cropCycles];
        }
        this.filterForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
      });
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  initForm() {
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      qualityGrade: [''],
      fromDate: [''],
      toDate: ['']
    });
  }

  loadDropdownData() {
    const farmId = this.authService.getFarmId();
    
    this.fieldService.getFields(farmId, { page: 1, pageSize: 100 }).subscribe(res => {
      if (res.success && res.data) {
        this.fields = res.data.items;
      }
    });

    this.cropCycleService.getCropCycles(farmId, { page: 1, pageSize: 100 }).subscribe(res => {
      if (res.success && res.data) {
        this.cropCycles = res.data.items;
        this.filteredCropCycles = [...this.cropCycles];
      }
    });
  }

  onTabChange(index: number) {
    this.selectedTabIndex = index;
    let status = 'PENDING';
    switch (index) {
      case 0: status = 'PENDING'; break;
      case 1: status = 'APPROVED'; break;
      case 2: status = 'REJECTED'; break;
      case 3: status = 'REQUEST_CHANGES'; break;
    }
    this.harvestState.updateFilter({ approvalStatus: status, page: 1 });
  }

  onPageChange(event: PageEvent) {
    this.harvestState.updateFilter({
      page: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }

  onSortChange(sortState: Sort) {
    if (sortState.direction) {
      this.harvestState.updateFilter({
        sortBy: sortState.active,
        isDescending: sortState.direction === 'desc',
        page: 1
      });
    } else {
      this.harvestState.updateFilter({
        sortBy: (undefined as any),
        isDescending: true,
        page: 1
      });
    }
  }

  viewDetails(harvest: HarvestDto) {
    this.dialog.open(HarvestDetailsComponent, {
      data: { harvest },
      width: '560px',
      panelClass: 'custom-dialog-container'
    });
  }

  openApproveDialog(harvest: HarvestDto) {
    const dialogRef = this.dialog.open(HarvestApprovalComponent, {
      data: { harvestId: harvest.id },
      width: '500px',
      panelClass: 'custom-dialog-container'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.harvestState.refresh();
      }
    });
  }

  openEditDialog(harvest: HarvestDto) {
    const dialogRef = this.dialog.open(HarvestFormComponent, {
      data: { editingId: harvest.id, editData: harvest },
      width: '620px',
      panelClass: 'custom-dialog-container',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.harvestState.refresh();
      }
    });
  }

  deleteHarvest(harvest: HarvestDto) {
    if (confirm(`Are you sure you want to delete this harvest record?`)) {
      // Not implemented in this subagent code directly, but typically you call a service
    }
  }
}
