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

import { AdminQualityCheckStateService } from '../services/admin-quality-check-state.service';
import { QualityCheckDto } from '../models/admin-quality-check.model';
import { QualityCheckDetailsComponent } from '../quality-check-details/quality-check-details.component';
import { QualityCheckApprovalComponent } from '../quality-check-approval/quality-check-approval.component';
import { ReportGeneratorService } from '../../../../core/services/report-generator.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-quality-check-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatPaginatorModule,
    MatSortModule, MatIconModule, MatButtonModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatDatepickerModule, MatNativeDateModule,
    MatProgressSpinnerModule, MatMenuModule, MatDialogModule, MatDividerModule,
    MatTooltipModule, TitleCasePipe, DatePipe, DecimalPipe
  ],
  templateUrl: './quality-check-list.component.html',
  styleUrl: './quality-check-list.component.scss'
})
export class QualityCheckListComponent implements OnInit, AfterViewInit, OnDestroy {
  private qualityState = inject(AdminQualityCheckStateService);
  private fb = inject(FormBuilder);
  private reportService = inject(ReportGeneratorService);
  private snackBar = inject(MatSnackBar);
  public dialog = inject(MatDialog);
  
  private destroy$ = new Subject<void>();
  
  dataSource = new MatTableDataSource<QualityCheckDto>([]);
  displayedColumns = ['checkDate', 'harvestBatchNumber', 'checkerName', 'moisturePct', 'defectPct', 'finalGrade', 'approvalStatus', 'actions'];
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  filterForm!: FormGroup;
  selectedTabIndex = 0;
  
  checks = this.qualityState.qualityChecks;
  totalCount = this.qualityState.totalCount;
  isLoading = this.qualityState.loading;
  currentFilter = this.qualityState.filter;
  
  constructor() {
    effect(() => {
      this.dataSource.data = this.checks();
    });
  }

  ngOnInit() {
    this.initForm();
    
    this.qualityState.updateFilter({ approvalStatus: 'PENDING' });

    this.filterForm.valueChanges
      .pipe(debounceTime(500), takeUntil(this.destroy$))
      .subscribe(val => {
        const filterUpdate = {
          finalGrade: val.finalGrade || (undefined as any),
          fromDate: val.fromDate ? new Date(val.fromDate).toISOString() : (undefined as any),
          toDate: val.toDate ? new Date(val.toDate).toISOString() : (undefined as any),
          page: 1
        };
        this.qualityState.updateFilter(filterUpdate);
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
      finalGrade: [''],
      fromDate: [''],
      toDate: ['']
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
    this.qualityState.updateFilter({ approvalStatus: status, page: 1 });
  }

  onPageChange(event: PageEvent) {
    this.qualityState.updateFilter({
      page: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }

  onSortChange(sortState: Sort) {
    if (sortState.direction) {
      this.qualityState.updateFilter({
        sortBy: sortState.active,
        isDescending: sortState.direction === 'desc',
        page: 1
      });
    } else {
      this.qualityState.updateFilter({
        sortBy: (undefined as any),
        isDescending: true,
        page: 1
      });
    }
  }

  viewDetails(check: QualityCheckDto) {
    this.dialog.open(QualityCheckDetailsComponent, {
      data: { check },
      width: '560px',
      panelClass: 'custom-dialog-container'
    });
  }

  openApproveDialog(check: QualityCheckDto) {
    const dialogRef = this.dialog.open(QualityCheckApprovalComponent, {
      data: { checkId: check.id },
      width: '500px',
      panelClass: 'custom-dialog-container'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.qualityState.refresh();
      }
    });
  }

  exportPdf(): void {
    const data = this.checks();
    if (!data.length) {
      this.snackBar.open('No data to export', 'Close', { duration: 3000 });
      return;
    }
    const columns = [
      { header: 'Date', dataKey: 'checkDate' },
      { header: 'Batch', dataKey: 'harvestBatchNumber' },
      { header: 'Checker', dataKey: 'checkerName' },
      { header: 'Moisture %', dataKey: 'moisturePct' },
      { header: 'Defects %', dataKey: 'defectPct' },
      { header: 'Grade', dataKey: 'finalGrade' },
      { header: 'Status', dataKey: 'approvalStatus' }
    ];
    this.reportService.exportToPdf(data, columns, 'Farm Quality Checks Report', 'Farm_QualityChecks');
  }

  exportCsv(): void {
    const data = this.checks();
    if (!data.length) {
      this.snackBar.open('No data to export', 'Close', { duration: 3000 });
      return;
    }
    const cleanData = data.map(d => ({
      Date: d.checkDate,
      Batch: d.harvestBatchNumber,
      Checker: d.checkerName,
      Moisture_Pct: d.moisturePct,
      Defect_Pct: d.defectPct,
      Grade: d.finalGrade,
      Status: d.approvalStatus,
      Notes: d.notes || ''
    }));
    this.reportService.exportToCsv(cleanData, 'Farm_QualityChecks');
  }
}
