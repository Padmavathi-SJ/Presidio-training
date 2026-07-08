import { Component, EventEmitter, OnInit, Output, ViewChild, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { QualityCheckDto } from '../models/worker-quality-check.model';
import { WorkerQualityCheckStateService } from '../services/worker-quality-check-state.service';

@Component({
  selector: 'app-quality-check-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatCheckboxModule
  ],
  templateUrl: './quality-check-list.component.html',
  styleUrl: './quality-check-list.component.scss'
})
export class QualityCheckListComponent implements OnInit {
  @Output() createCheck = new EventEmitter<void>();
  @Output() editCheck = new EventEmitter<QualityCheckDto>();
  @Output() viewCheck = new EventEmitter<QualityCheckDto>();
  @Output() respondCheck = new EventEmitter<QualityCheckDto>();
  @Output() deleteCheck = new EventEmitter<number>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  public state = inject(WorkerQualityCheckStateService);
  private fb = inject(FormBuilder);

  filterForm!: FormGroup;
  dataSource = new MatTableDataSource<QualityCheckDto>([]);
  displayedColumns = ['checkDate', 'harvestBatchNumber', 'moisturePct', 'defectPct', 'finalGrade', 'approvalStatus', 'actions'];

  selectedTabIndex = 0;
  grades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];

  constructor() {
    effect(() => {
      this.dataSource.data = this.state.qualityChecks();
      
      const filterStatus = this.state.filter().approvalStatus;
      if (filterStatus === 'PENDING') this.selectedTabIndex = 0;
      else if (filterStatus === 'APPROVED') this.selectedTabIndex = 1;
      else if (filterStatus === 'REJECTED') this.selectedTabIndex = 2;
      else if (filterStatus === 'REQUEST_CHANGES') this.selectedTabIndex = 3;
      else this.selectedTabIndex = -1;
    });
  }

  ngOnInit() {
    this.filterForm = this.fb.group({
      finalGrade: [''],
      fromDate: [''],
      toDate: [''],
      includeDeleted: [false]
    });

    this.filterForm.valueChanges.subscribe(val => {
      this.state.updateFilter({
        finalGrade: val.finalGrade,
        fromDate: val.fromDate,
        toDate: val.toDate,
        includeDeleted: val.includeDeleted,
        page: 1
      });
    });

    this.state.updateFilter({ approvalStatus: 'PENDING' });
  }

  onTabChange(index: number) {
    let status = '';
    if (index === 0) status = 'PENDING';
    else if (index === 1) status = 'APPROVED';
    else if (index === 2) status = 'REJECTED';
    else if (index === 3) status = 'REQUEST_CHANGES';

    this.state.updateFilter({ approvalStatus: status, page: 1 });
  }

  onSortChange(event: any) {
    this.state.updateFilter({ 
      sortBy: event.active,
      isDescending: event.direction === 'desc',
      page: 1
    });
  }

  onPageChange(event: any) {
    this.state.updateFilter({
      page: event.pageIndex + 1,
      pageSize: event.pageSize
    });
  }

  getApprovalIcon(status: string): string {
    switch (status) {
      case 'PENDING': return 'hourglass_empty';
      case 'APPROVED': return 'check_circle';
      case 'REJECTED': return 'cancel';
      case 'REQUEST_CHANGES': return 'edit_note';
      default: return 'help';
    }
  }

  canEdit(check: QualityCheckDto): boolean {
    return check.approvalStatus === 'PENDING' || check.approvalStatus === 'REQUEST_CHANGES';
  }

  canRespond(check: QualityCheckDto): boolean {
    return check.approvalStatus === 'REQUEST_CHANGES';
  }

  canDelete(check: QualityCheckDto): boolean {
    return check.approvalStatus === 'PENDING';
  }
}
