// src/app/features/admin/fields/fields.component.ts
import { Component, inject, OnInit, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { finalize, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { FieldService } from '../services/field.service';
import { Field, FieldFilterDto, FIELD_STATUS_OPTIONS, SOIL_TYPE_OPTIONS, STATUS_COLORS, SOIL_TYPE_COLORS } from '../models/field.model';
import { FieldFormComponent } from '../field-form/field-form.component';
import { FieldLocationComponent } from '../field-location/field-location.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { CropCyclesComponent } from '../crop-cycles/crop-cycles.component';

@Component({
  selector: 'app-fields',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatBadgeModule,
    CropCyclesComponent
  ],
  templateUrl: './fields.component.html'
})
export class FieldsComponent implements OnInit {
  private authService = inject(AuthService);
  private fieldService = inject(FieldService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // State
  isLoading = signal(true);
  isImporting = signal(false);
  fields = signal<Field[]>([]);
  totalCount = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  sortField = signal('CreatedAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  selectedFields = signal<number[]>([]);
  statistics = signal<any>(null);
  
  // ✅ Crop Cycles State - Moved here
  selectedField = signal<Field | null>(null);

  // Filter form
  filterForm: FormGroup;

  // Table columns - ✅ Updated with crop cycles button
  displayedColumns = [
    'select',
    'fieldName',
    'location',
    'areaHectares',
    'soilType',
    'status',
    'activeCropCount',
    'createdAt',
    'actions'
  ];

  // Options
  statusOptions = FIELD_STATUS_OPTIONS;
  soilTypeOptions = SOIL_TYPE_OPTIONS;
  statusColors = STATUS_COLORS;
  soilTypeColors = SOIL_TYPE_COLORS;

  constructor() {
    this.filterForm = this.fb.group({
      fieldName: [''],
      location: [''],
      soilType: [''],
      status: ['']
    });
  }

  ngOnInit(): void {
    this.loadFields();

    this.filterForm.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr))
      )
      .subscribe(() => {
        this.pageIndex.set(0);
        this.loadFields();
      });
  }

  // ✅ Crop Cycles Methods - Moved here
  openCropCycles(field: Field): void {
    this.selectedField.set(field);
  }

  closeCropCycles(): void {
    this.selectedField.set(null);
  }

  // Load fields with filters and pagination
  loadFields(): void {
    this.isLoading.set(true);

    const farmId = this.authService.getFarmId();
    
    console.log('=== DEBUG: Frontend FarmId ===');
    console.log('FarmId from AuthService:', farmId);
    console.log('Current User:', this.authService.getCurrentUser());

    if (!farmId) {
      this.isLoading.set(false);
      this.showError('No farm found. Please login again.');
      return;
    }

    const filterValues = this.filterForm.value;
    
    const filter: FieldFilterDto = {
      fieldName: filterValues.fieldName || null,
      location: filterValues.location || null,
      soilType: filterValues.soilType || null,
      status: filterValues.status || null,
      includeDeleted: false,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      isDescending: this.sortDirection() === 'desc'
    };
    
    console.log('📤 Sending filter:', filter);

    this.fieldService.getFields(farmId, filter)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          if (response.success) {
            console.log(`✅ Received ${response.data.items.length} fields for Farm ${farmId}`);
            this.fields.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.showError(response.message || 'Failed to load fields');
          }
        },
        error: (error) => {
          console.error('Error loading fields:', error);
          this.showError('Failed to load fields');
        }
      });

    this.loadStatistics();
  }

  loadStatistics(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) return;

    this.fieldService.getStatistics(farmId).subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics.set(response.data);
        }
      },
      error: (error) => console.error('Error loading statistics:', error)
    });
  }

  // Pagination
  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
    this.loadFields();
  }

  // Sorting
  onSortChange(sort: Sort): void {
    this.sortField.set(sort.active);
    this.sortDirection.set(sort.direction || 'desc');
    this.pageIndex.set(0);
    this.loadFields();
  }

  // Selection
  toggleSelection(fieldId: number): void {
    const current = this.selectedFields();
    if (current.includes(fieldId)) {
      this.selectedFields.set(current.filter(id => id !== fieldId));
    } else {
      this.selectedFields.set([...current, fieldId]);
    }
  }

  toggleAllSelection(): void {
    const currentFields = this.fields();
    const selected = this.selectedFields();
    if (selected.length === currentFields.length && currentFields.length > 0) {
      this.selectedFields.set([]);
    } else {
      this.selectedFields.set(currentFields.map(f => f.id));
    }
  }

  isSelected(fieldId: number): boolean {
    return this.selectedFields().includes(fieldId);
  }

  // CRUD Operations
  openCreateDialog(): void {
    const dialogRef = this.dialog.open(FieldFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadFields();
        this.showSuccess('Field created successfully');
      }
    });
  }

  openEditDialog(field: Field): void {
    const dialogRef = this.dialog.open(FieldFormComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { mode: 'edit', field }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadFields();
        this.showSuccess('Field updated successfully');
      }
    });
  }

  openLocationDialog(field: Field): void {
    const dialogRef = this.dialog.open(FieldLocationComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { field }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadFields();
        this.showSuccess('Field location updated successfully');
      }
    });
  }

  deleteField(field: Field): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Delete Field',
        message: `Are you sure you want to delete "${field.fieldName}"? This action can be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        this.fieldService.deleteField(farmId, field.id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.loadFields();
                this.showSuccess('Field deleted successfully');
              } else {
                this.showError(response.message || 'Failed to delete field');
              }
            },
            error: (error) => {
              console.error('Error deleting field:', error);
              this.showError('Failed to delete field');
            }
          });
      }
    });
  }

  bulkDelete(): void {
    const selectedIds = this.selectedFields();
    if (selectedIds.length === 0) {
      this.showError('Please select fields to delete');
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      maxWidth: '90vw',
      data: {
        title: 'Bulk Delete Fields',
        message: `Are you sure you want to delete ${selectedIds.length} selected field(s)? This action can be undone.`,
        confirmText: 'Delete All',
        cancelText: 'Cancel',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        const farmId = this.authService.getFarmId();
        if (!farmId) return;

        this.isLoading.set(true);
        this.fieldService.bulkDeleteFields(farmId, selectedIds)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe({
            next: (response) => {
              if (response.success) {
                this.selectedFields.set([]);
                this.loadFields();
                this.showSuccess(`${response.data.successCount} fields deleted successfully`);
              } else {
                this.showError(response.message || 'Failed to delete fields');
              }
            },
            error: (error) => {
              console.error('Error bulk deleting fields:', error);
              this.showError('Failed to delete fields');
            }
          });
      }
    });
  }

  // Import/Export
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      const validExtensions = ['.xlsx', '.xls'];
      const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      
      if (!validExtensions.includes(extension)) {
        this.showError('Please upload a valid Excel file (.xlsx or .xls)');
        input.value = '';
        return;
      }
      
      if (file.size > 10 * 1024 * 1024) {
        this.showError('File size must be less than 10MB');
        input.value = '';
        return;
      }
      
      this.importFields(file);
    }
    input.value = '';
  }

  importFields(file: File): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found');
      return;
    }

    this.isImporting.set(true);
    this.isLoading.set(true);
    
    this.fieldService.bulkImport(farmId, file)
      .pipe(finalize(() => {
        this.isLoading.set(false);
        this.isImporting.set(false);
      }))
      .subscribe({
        next: (response) => {
          if (response.success) {
            const result = response.data;
            if (result.failedCount === 0) {
              this.showSuccess(`Successfully imported ${result.successCount} fields`);
            } else {
              this.showWarning(`Imported ${result.successCount} fields, ${result.failedCount} failed`);
              console.warn('Import errors:', result.errors);
            }
            this.loadFields();
          } else {
            this.showError(response.message || 'Failed to import fields');
          }
        },
        error: (error) => {
          console.error('Error importing fields:', error);
          this.showError('Failed to import fields');
        }
      });
  }

  downloadTemplate(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found');
      return;
    }

    this.isLoading.set(true);
    this.fieldService.downloadTemplate(farmId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'fields_import_template.xlsx';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
          this.showSuccess('Template downloaded successfully');
        },
        error: (error) => {
          console.error('Error downloading template:', error);
          this.showError('Failed to download template');
        }
      });
  }

  exportFields(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.showError('No farm found');
      return;
    }

    this.isLoading.set(true);
    this.fieldService.exportFields(farmId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `fields_export_${new Date().toISOString().split('T')[0]}.xlsx`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
          this.showSuccess('Fields exported successfully');
        },
        error: (error) => {
          console.error('Error exporting fields:', error);
          this.showError('Failed to export fields');
        }
      });
  }

  resetFilters(): void {
    this.filterForm.patchValue({
      fieldName: '',
      location: '',
      soilType: '',
      status: ''
    });
    this.pageIndex.set(0);
    this.loadFields();
  }

  // Helper methods
  getStatusColor(status: string): string {
    return this.statusColors[status] || 'text-gray-600 bg-gray-50';
  }

  getSoilTypeColor(soilType: string): string {
    return this.soilTypeColors[soilType] || 'text-gray-600 bg-gray-50';
  }

  getStatusLabel(status: string): string {
    const option = this.statusOptions.find(opt => opt.value === status);
    return option ? option.label : status;
  }

  getSoilTypeLabel(soilType: string): string {
    const option = this.soilTypeOptions.find(opt => opt.value === soilType);
    return option ? option.label : soilType;
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  // Notification helpers
  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  private showWarning(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['bg-yellow-600', 'text-white']
    });
  }
}