import { Component, OnInit, inject, ViewChild, TemplateRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { finalize, forkJoin } from 'rxjs';

import { MaterialModule } from '../../../shared/material.module';
import { AdminObservationService } from '../services/admin-observation.service';
import { FieldService } from '../services/field.service';
import { WorkerService } from '../services/worker.service';
import { CropCycleService } from '../services/crop-cycle.service';
import { AuthService } from '../../../core/services/auth.service';
import { ReportGeneratorService } from '../../../core/services/report-generator.service';
import { 
  ObservationDto, 
  ObservationFilterDto, 
  UpdateObservationDto, 
  ObservationValidationDto, 
  ObservationValidationSummaryDto 
} from '../models/admin-observation.model';

@Component({
  selector: 'app-observations',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
    MatDialogModule,
    MatCheckboxModule,
    MatTabsModule,
    MatIconModule,
    MatDividerModule,
    DatePipe
  ],
  templateUrl: './observations.component.html',
  styleUrls: ['./observations.component.scss']
})
export class Observations implements OnInit {
  private observationService = inject(AdminObservationService);
  private fieldService = inject(FieldService);
  private workerService = inject(WorkerService);
  private cropCycleService = inject(CropCycleService);
  private authService = inject(AuthService);
  private reportService = inject(ReportGeneratorService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);

  farmId = 0;
  observations = new MatTableDataSource<ObservationDto>([]);
  fields: any[] = [];
  workers: any[] = [];
  allCropCycles: any[] = [];
  filteredCropCycles: any[] = [];
  editFilteredCropCycles: any[] = [];
  
  selectedTabIndex = 0;
  displayedColumns: string[] = ['observationDate', 'fieldName', 'workerName', 'cropHealth', 'pestDetected', 'images', 'validationStatus', 'actions'];
  
  totalObservations = 0;
  pageSize = 10;
  pageIndex = 0;
  isLoading = false;

  filterForm: FormGroup;
  observationForm!: FormGroup;
  validationForm!: FormGroup;
  
  editingId: number | null = null;
  selectedObservation: ObservationDto | null = null;
  summary: ObservationValidationSummaryDto = { total: 0, pending: 0, questioned: 0, verified: 0, invalid: 0 };

  // Photo uploading states
  isMainUploading = false;
  isRefsUploading = false;

  // Photo previews
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { fileName: string, url: string }[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('editDialog') editDialogTemplate!: TemplateRef<any>;
  @ViewChild('validationDialog') validationDialogTemplate!: TemplateRef<any>;
  @ViewChild('viewDetailsDialog') viewDetailsDialogTemplate!: TemplateRef<any>;

  constructor() {
    this.farmId = this.authService.getFarmId();
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      workerId: [''],
      validationStatus: ['pending'],
      cropHealth: [''],
      includeDeleted: [false],
      fromDate: [''],
      toDate: ['']
    });

    this.initForms();
  }

  ngOnInit(): void {
    this.loadFields();
    this.loadWorkers();
    this.loadCropCycles();
    this.loadSummary();
    this.loadObservations();

    // Reset cropCycleId filter and filter available cycles if fieldId changes
    this.filterForm.get('fieldId')?.valueChanges.subscribe(fieldId => {
      this.filterForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
      this.filterCropCyclesForFilter();
    });

    // Reset cropCycleId and filter edit cycles if edit fieldId changes
    this.observationForm.get('fieldId')?.valueChanges.subscribe(fieldId => {
      if (fieldId) {
        const parsedId = parseInt(fieldId, 10);
        this.editFilteredCropCycles = this.allCropCycles.filter(c => c.fieldId === parsedId);
        
        const currentCycleId = this.observationForm.get('cropCycleId')?.value;
        if (currentCycleId) {
          const match = this.editFilteredCropCycles.find(c => c.id === parseInt(currentCycleId, 10));
          if (!match) {
            this.observationForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
          }
        }
      } else {
        this.editFilteredCropCycles = [];
        this.observationForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
      }
    });

    this.filterForm.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadObservations();
      });
  }

  initForms(): void {
    this.observationForm = this.fb.group({
      fieldId: ['', Validators.required],
      cropCycleId: [''],
      observationDate: ['', Validators.required],
      cropHealth: [''],
      pestType: [''],
      notes: [''],
      imagePath: [''],
      thumbnailPath: [''],
      imageCaption: [''],
      additionalImagePaths: [[]],
      imageMetadata: ['']
    });

    this.validationForm = this.fb.group({
      validationStatus: ['verified', Validators.required],
      adminNotes: [''],
      flagReason: ['']
    });

    // Conditional validator for flagReason
    this.validationForm.get('validationStatus')?.valueChanges.subscribe(status => {
      const flagReasonCtrl = this.validationForm.get('flagReason');
      if (status === 'questioned' || status === 'invalid') {
        flagReasonCtrl?.setValidators([Validators.required]);
      } else {
        flagReasonCtrl?.clearValidators();
      }
      flagReasonCtrl?.updateValueAndValidity();
    });
  }

  loadFields(): void {
    this.fieldService.getFields(this.farmId, { page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data?.items) {
          this.fields = res.data.items;
        }
      },
      error: (err) => console.error('Failed to load fields', err)
    });
  }

  loadWorkers(): void {
    this.workerService.getWorkers(this.farmId, { page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data?.items) {
          this.workers = res.data.items;
        }
      },
      error: (err) => console.error('Failed to load workers', err)
    });
  }

  loadCropCycles(): void {
    this.cropCycleService.getCropCycles(this.farmId, { page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data?.items) {
          this.allCropCycles = res.data.items;
          this.filterCropCyclesForFilter();
        }
      },
      error: (err) => console.error('Failed to load crop cycles', err)
    });
  }

  filterCropCyclesForFilter(): void {
    const selectedFieldId = this.filterForm.get('fieldId')?.value;
    if (selectedFieldId) {
      const parsedId = parseInt(selectedFieldId, 10);
      this.filteredCropCycles = this.allCropCycles.filter(c => c.fieldId === parsedId);
    } else {
      this.filteredCropCycles = [...this.allCropCycles];
    }
  }

  loadSummary(): void {
    this.observationService.getValidationSummary(this.farmId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.summary = res.data;
        }
      },
      error: (err) => console.error('Failed to load validation summary', err)
    });
  }

  loadObservations(): void {
    this.isLoading = true;
    const rawFilters = this.filterForm.value;
    const filters: ObservationFilterDto = {
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sort?.active || 'observationDate',
      isDescending: this.sort?.direction === 'desc',
      fieldId: rawFilters.fieldId ? parseInt(rawFilters.fieldId, 10) : undefined,
      cropCycleId: rawFilters.cropCycleId ? parseInt(rawFilters.cropCycleId, 10) : undefined,
      workerId: rawFilters.workerId ? parseInt(rawFilters.workerId, 10) : undefined,
      validationStatus: rawFilters.validationStatus || undefined,
      cropHealth: rawFilters.cropHealth || undefined,
      includeDeleted: rawFilters.includeDeleted,
      fromDate: rawFilters.fromDate ? new Date(rawFilters.fromDate).toISOString() : undefined,
      toDate: rawFilters.toDate ? new Date(rawFilters.toDate).toISOString() : undefined
    };

    this.observationService.getObservations(this.farmId, filters)
      .pipe(finalize(() => { this.isLoading = false; this.cdr.detectChanges(); }))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.observations.data = res.data.items;
            this.totalObservations = res.data.totalCount;
          }
        },
        error: (err) => {
          this.snackBar.open('Error loading observations: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['pending', 'questioned', 'verified', 'invalid'];
    this.filterForm.patchValue({ validationStatus: statuses[index] });
  }

  onSortChange(sortState: Sort): void {
    this.pageIndex = 0;
    this.loadObservations();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadObservations();
  }

  private getExportFilter(): ObservationFilterDto {
    const rawFilters = this.filterForm.value;
    return {
      page: 1,
      pageSize: this.totalObservations || 100000,
      sortBy: this.sort?.active || 'observationDate',
      isDescending: this.sort?.direction === 'desc',
      fieldId: rawFilters.fieldId ? parseInt(rawFilters.fieldId, 10) : undefined,
      cropCycleId: rawFilters.cropCycleId ? parseInt(rawFilters.cropCycleId, 10) : undefined,
      workerId: rawFilters.workerId ? parseInt(rawFilters.workerId, 10) : undefined,
      validationStatus: rawFilters.validationStatus || undefined,
      cropHealth: rawFilters.cropHealth || undefined,
      includeDeleted: rawFilters.includeDeleted,
      fromDate: rawFilters.fromDate ? new Date(rawFilters.fromDate).toISOString() : undefined,
      toDate: rawFilters.toDate ? new Date(rawFilters.toDate).toISOString() : undefined
    };
  }

  exportPdf(): void {
    if (!this.observations.data.length) {
      this.snackBar.open('No data to export', 'Close', { duration: 3000 });
      return;
    }
    this.isLoading = true;
    this.observationService.getObservations(this.farmId, this.getExportFilter())
      .pipe(finalize(() => { this.isLoading = false; this.cdr.detectChanges(); }))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const columns = [
              { header: 'Date', dataKey: 'observationDate' },
              { header: 'Field', dataKey: 'fieldName' },
              { header: 'Worker', dataKey: 'workerName' },
              { header: 'Health', dataKey: 'cropHealth' },
              { header: 'Pest/Disease', dataKey: 'pestType' },
              { header: 'Status', dataKey: 'validationStatus' }
            ];
            this.reportService.exportToPdf(res.data.items, columns, 'Farm Observations Report', 'Farm_Observations');
          } else {
            this.snackBar.open('Error loading data for export', 'Close', { duration: 3000 });
          }
        },
        error: (err) => {
          this.snackBar.open('Error loading data for export', 'Close', { duration: 3000 });
        }
      });
  }

  exportCsv(): void {
    if (!this.observations.data.length) {
      this.snackBar.open('No data to export', 'Close', { duration: 3000 });
      return;
    }
    this.isLoading = true;
    this.observationService.getObservations(this.farmId, this.getExportFilter())
      .pipe(finalize(() => { this.isLoading = false; this.cdr.detectChanges(); }))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const cleanData = res.data.items.map((d: any) => ({
              Date: d.observationDate,
              Field: d.fieldName,
              Worker: d.workerName,
              Health: d.cropHealth,
              Pest_Disease: d.pestType || 'None',
              Status: d.validationStatus,
              Notes: d.notes || ''
            }));
            this.reportService.exportToCsv(cleanData, 'Farm_Observations');
          } else {
            this.snackBar.open('Error loading data for export', 'Close', { duration: 3000 });
          }
        },
        error: (err) => {
          this.snackBar.open('Error loading data for export', 'Close', { duration: 3000 });
        }
      });
  }

  viewDetails(obs: ObservationDto): void {
    this.selectedObservation = obs;
    this.dialog.open(this.viewDetailsDialogTemplate, {
      width: '750px',
      maxWidth: '90vw',
      panelClass: 'modern-dialog'
    });
  }

  openValidateDialog(obs: ObservationDto, event?: Event): void {
    if (event) event.stopPropagation();
    this.selectedObservation = obs;
    this.validationForm.reset({
      validationStatus: 'verified',
      adminNotes: '',
      flagReason: ''
    });
    this.dialog.open(this.validationDialogTemplate, {
      width: '500px',
      panelClass: 'modern-dialog'
    });
  }

  submitValidation(): void {
    if (this.validationForm.invalid || !this.selectedObservation) return;

    this.isLoading = true;
    const payload: ObservationValidationDto = this.validationForm.value;

    this.observationService.validateObservation(this.farmId, this.selectedObservation.id, payload)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Validation status submitted successfully', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadSummary();
            this.loadObservations();
          }
        },
        error: (err) => {
          this.snackBar.open('Failed to validate: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  openEditDialog(obs: ObservationDto, event?: Event): void {
    if (event) event.stopPropagation();
    this.selectedObservation = obs;
    this.editingId = obs.id;
    
    if (obs.fieldId) {
      this.editFilteredCropCycles = this.allCropCycles.filter(c => c.fieldId === obs.fieldId);
    } else {
      this.editFilteredCropCycles = [];
    }
    
    this.observationForm.patchValue({
      fieldId: obs.fieldId,
      cropCycleId: obs.cropCycleId || '',
      observationDate: new Date(obs.observationDate),
      cropHealth: obs.cropHealth || '',
      pestType: obs.pestType || '',
      notes: obs.notes || '',
      imagePath: obs.imagePath || '',
      thumbnailPath: obs.thumbnailPath || '',
      imageCaption: obs.imageCaption || '',
      additionalImagePaths: obs.additionalImagePaths || [],
      imageMetadata: obs.imageMetadata || ''
    });

    this.mainPhotoPreviewUrl = obs.imagePath || null;
    this.referencePhotoPreviews = (obs.additionalImagePaths || []).map(p => ({
      fileName: this.getRelativePathFromUrl(p) || 'photo.jpg',
      url: p
    }));

    this.dialog.open(this.editDialogTemplate, {
      width: '650px',
      maxWidth: '95vw',
      panelClass: 'modern-dialog'
    });
  }

  saveObservation(): void {
    if (this.observationForm.invalid || !this.editingId) return;

    this.isLoading = true;
    const val = { ...this.observationForm.value };

    // Clean empty values to null
    if (val.cropCycleId === '') val.cropCycleId = null;
    if (val.cropHealth === '') val.cropHealth = null;
    if (val.pestType === '') val.pestType = null;
    if (val.notes === '') val.notes = null;

    // Convert date to ISO string
    if (val.observationDate) {
      val.observationDate = new Date(val.observationDate).toISOString();
    }

    const payload: UpdateObservationDto = {
      fieldId: val.fieldId,
      cropCycleId: val.cropCycleId,
      observationDate: val.observationDate,
      cropHealth: val.cropHealth,
      pestType: val.pestType,
      notes: val.notes,
      imagePath: this.getRelativePathFromUrl(val.imagePath),
      thumbnailPath: this.getRelativePathFromUrl(val.thumbnailPath),
      imageCaption: val.imageCaption || null,
      additionalImagePaths: (val.additionalImagePaths || []).map((p: string) => this.getRelativePathFromUrl(p)),
      imageMetadata: val.imageMetadata || null
    };

    this.observationService.updateObservation(this.farmId, this.editingId, payload)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation updated successfully', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadObservations();
            this.loadSummary();
          }
        },
        error: (err) => {
          this.snackBar.open('Failed to update: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  deleteObservation(obs: ObservationDto, event?: Event): void {
    if (event) event.stopPropagation();
    if (!confirm(`Are you sure you want to delete this observation from field "${obs.fieldName}"?`)) return;

    this.isLoading = true;
    this.observationService.deleteObservation(this.farmId, obs.id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation deleted successfully', 'Close', { duration: 3000 });
            this.loadObservations();
            this.loadSummary();
          }
        },
        error: (err) => {
          this.snackBar.open('Failed to delete: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  // Upload Logic
  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (!file) return;

    this.isMainUploading = true;
    this.observationForm.disable();

    this.observationService.uploadImage(this.farmId, file)
      .pipe(finalize(() => {
        this.isMainUploading = false;
        this.observationForm.enable();
      }))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.mainPhotoPreviewUrl = res.data.url;
            this.observationForm.patchValue({
              imagePath: res.data.url,
              thumbnailPath: res.data.url // Using standard image url for local dev thumbnail path
            });
            this.snackBar.open('Main photo uploaded successfully', 'Close', { duration: 2000 });
          }
        },
        error: (err) => {
          this.snackBar.open('Main photo upload failed: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  onMultipleFilesSelected(event: any): void {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;

    this.isRefsUploading = true;
    this.observationForm.disable();

    const uploadTasks = Array.from(files).map(file => this.observationService.uploadImage(this.farmId, file));

    forkJoin(uploadTasks)
      .pipe(finalize(() => {
        this.isRefsUploading = false;
        this.observationForm.enable();
      }))
      .subscribe({
        next: (results) => {
          const currentPaths = this.observationForm.get('additionalImagePaths')?.value || [];
          
          results.forEach(res => {
            if (res.success && res.data) {
              currentPaths.push(res.data.url);
              this.referencePhotoPreviews.push({
                fileName: this.getRelativePathFromUrl(res.data.url) || 'photo.jpg',
                url: res.data.url
              });
            }
          });

          this.observationForm.patchValue({ additionalImagePaths: currentPaths });
          this.snackBar.open(`${results.length} photo(s) uploaded successfully`, 'Close', { duration: 2000 });
        },
        error: (err) => {
          this.snackBar.open('One or more reference photo uploads failed: ' + (err.error?.message || err.message), 'Close', { duration: 3000 });
        }
      });
  }

  removeMainPhoto(): void {
    this.mainPhotoPreviewUrl = null;
    this.observationForm.patchValue({
      imagePath: '',
      thumbnailPath: ''
    });
  }

  removeReferencePhoto(index: number): void {
    const currentPaths = this.observationForm.get('additionalImagePaths')?.value || [];
    currentPaths.splice(index, 1);
    this.referencePhotoPreviews.splice(index, 1);
    this.observationForm.patchValue({ additionalImagePaths: currentPaths });
  }

  public getRelativePathFromUrl(url: string | null | undefined): string | null {
    if (!url) return null;
    if (!url.startsWith('http://') && !url.startsWith('https://')) return url;
    
    try {
      const parsed = new URL(url);
     
      const match = parsed.pathname.match(/\/uploads\/(.+)$/);
      return match ? match[1] : parsed.pathname;
    } catch {
      return url;
    }
  }
}
