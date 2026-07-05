import { Component, OnInit, inject, ViewChild, TemplateRef, ChangeDetectorRef, NgZone } from '@angular/core';
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
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { finalize, forkJoin } from 'rxjs';

import { MaterialModule } from '../../../shared/material.module';
import { WorkerObservationService } from '../services/worker-observation.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { 
  ObservationDto, 
  ObservationFilterDto,
  CreateObservationDto,
  UpdateObservationDto,
  ObservationWorkerResponseDto
} from '../models/worker-observation.model';

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
    DatePipe
  ],
  templateUrl: './observations.component.html',
  styleUrls: ['./observations.component.scss']
})
export class ObservationsComponent implements OnInit {
  private observationService = inject(WorkerObservationService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);

  observations = new MatTableDataSource<ObservationDto>([]);
  fields: any[] = [];
  cropCycles: any[] = [];
  
  selectedTabIndex = 0;
  
  displayedColumns: string[] = ['observationDate', 'fieldName', 'cropHealth', 'pestDetected', 'images', 'validationStatus', 'actions'];
  
  totalObservations = 0;
  pageSize = 10;
  pageIndex = 0;
  isLoading = false;

  filterForm: FormGroup;
  observationForm!: FormGroup;
  responseForm!: FormGroup;
  
  editingId: number | null = null;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('observationDialog') observationDialogTemplate!: TemplateRef<any>;
  @ViewChild('respondDialog') respondDialogTemplate!: TemplateRef<any>;
  @ViewChild('viewDetailsDialog') viewDetailsDialogTemplate!: TemplateRef<any>;

  selectedObservation: ObservationDto | null = null;

  // Photo uploading states
  isMainUploading = false;
  isRefsUploading = false;

  // Photo preview URLs (stored as absolute URLs returned from server)
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { fileName: string, url: string }[] = [];

  constructor() {
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      validationStatus: [''],
      cropHealth: [''],
      includeDeleted: [false],
      fromDate: [''],
      toDate: ['']
    });

    this.initForms();
  }

  ngOnInit(): void {
    this.loadFields();
    this.filterForm.patchValue({ validationStatus: 'pending' });
    this.loadObservations();

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

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['pending', 'questioned', 'verified', 'invalid'];
    this.filterForm.patchValue({ validationStatus: statuses[index] });
  }

  onFieldSelected(fieldId: number): void {
    this.cropCycles = [];
    this.filterForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
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

  private getRelativePathFromUrl(url: string | null | undefined): string | null {
    if (!url) return null;
    if (!url.startsWith('http://') && !url.startsWith('https://')) return url;
    
    try {
      const parsedUrl = new URL(url);
      const path = parsedUrl.pathname;
      if (path.includes('/uploads/')) {
        return path.substring(path.indexOf('/uploads/') + 9);
      }
      const segments = path.split('/').filter(s => s);
      if (segments.length >= 2) {
        return segments.slice(1).join('/');
      }
      return path;
    } catch (e) {
      return url;
    }
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.isMainUploading = true;
      this.cdr.detectChanges();

      this.observationService.uploadImage(file).pipe(
        finalize(() => {
          this.isMainUploading = false;
          this.cdr.detectChanges();
        })
      ).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.ngZone.run(() => {
              this.observationForm.patchValue({ imagePath: res.data.fileName });
              this.mainPhotoPreviewUrl = res.data.url;
              this.cdr.detectChanges();
            });
          }
        },
        error: (err) => this.showError('Failed to upload main photo')
      });
      event.target.value = '';
    }
  }

  onMultipleFilesSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      const uploadObservables = Array.from(files).map(file => this.observationService.uploadImage(file));
      this.isRefsUploading = true;
      this.cdr.detectChanges();

      forkJoin(uploadObservables).pipe(
        finalize(() => {
          this.isRefsUploading = false;
          this.cdr.detectChanges();
        })
      ).subscribe({
        next: (responses) => {
          const newFiles = responses.map(r => ({
            fileName: r.data.fileName,
            url: r.data.url
          }));

          this.ngZone.run(() => {
            this.referencePhotoPreviews = [...this.referencePhotoPreviews, ...newFiles];
            const currentFiles = this.observationForm.get('additionalImagePaths')?.value || [];
            const updatedFiles = [...currentFiles, ...newFiles.map(nf => nf.fileName)];
            const uniqueFiles = Array.from(new Set(updatedFiles));
            
            this.observationForm.patchValue({ additionalImagePaths: uniqueFiles });
            this.cdr.detectChanges();
          });
        },
        error: (err) => this.showError('Failed to upload reference photos')
      });
      event.target.value = '';
    }
  }

  removeReferencePhoto(index: number): void {
    const currentFiles: string[] = this.observationForm.get('additionalImagePaths')?.value || [];
    const removedFileName = currentFiles[index];
    currentFiles.splice(index, 1);
    
    this.ngZone.run(() => {
      this.observationForm.patchValue({ additionalImagePaths: [...currentFiles] });
      this.referencePhotoPreviews = this.referencePhotoPreviews.filter(p => p.fileName !== removedFileName);
      this.cdr.detectChanges();
    });
  }

  removeMainPhoto(): void {
    this.ngZone.run(() => {
      this.observationForm.patchValue({ imagePath: null });
      this.mainPhotoPreviewUrl = null;
      this.cdr.detectChanges();
    });
  }

  viewObservationDetails(obs: ObservationDto): void {
    this.selectedObservation = obs;
    this.dialog.open(this.viewDetailsDialogTemplate, { width: '550px' });
  }

  closeDetails(): void {
    this.dialog.closeAll();
  }

  openEditDialogFromDetails(obs: ObservationDto): void {
    this.dialog.closeAll();
    setTimeout(() => {
      this.openEditDialog(obs);
    }, 150);
  }

  getStatusBadgeClass(status: string | undefined): string {
    switch (status?.toLowerCase()) {
      case 'verified':
        return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-900';
      case 'questioned':
        return 'bg-amber-100 text-amber-800 dark:bg-amber-950/30 dark:text-amber-300 border border-amber-200 dark:border-amber-900';
      case 'invalid':
        return 'bg-rose-100 text-rose-800 dark:bg-rose-950/30 dark:text-rose-300 border border-rose-200 dark:border-rose-900';
      default:
        return 'bg-slate-100 text-slate-800 dark:bg-slate-950/30 dark:text-slate-300 border border-slate-200 dark:border-slate-800';
    }
  }

  getHealthColorClass(health: string | undefined): string {
    switch (health?.toUpperCase()) {
      case 'EXCELLENT':
        return 'text-emerald-500';
      case 'GOOD':
        return 'text-teal-500';
      case 'AVERAGE':
        return 'text-amber-500';
      case 'POOR':
        return 'text-orange-500';
      case 'CRITICAL':
        return 'text-rose-500';
      default:
        return 'text-slate-400';
    }
  }

  initForms() {
    this.observationForm = this.fb.group({
      fieldId: ['', Validators.required],
      cropCycleId: [''],
      observationDate: [new Date(), Validators.required],
      cropHealth: [''],
      pestType: [''],
      notes: [''],
      imagePath: [''],
      additionalImagePaths: [[]],
      imageCaption: ['']
    });

    this.responseForm = this.fb.group({
      responseNotes: ['', Validators.required],
      additionalImagePath: ['']
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

  loadObservations(): void {
    this.isLoading = true;
    const formVal = this.filterForm.value;
    const filter: ObservationFilterDto = {
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      ...formVal,
      fromDate: formVal.fromDate ? new Date(formVal.fromDate).toISOString() : undefined,
      toDate: formVal.toDate ? new Date(formVal.toDate).toISOString() : undefined
    };

    Object.keys(filter).forEach(key => {
      const k = key as keyof ObservationFilterDto;
      if (filter[k] === '' || filter[k] === null || filter[k] === undefined) {
        delete filter[k];
      }
    });

    this.observationService.getMyObservations(filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.observations.data = res.data.items || [];
            this.totalObservations = res.data.totalCount;
          }
        },
        error: (err) => this.showError('Failed to load observations.')
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadObservations();
  }

  onSortChange(sortState: Sort): void {
    if (sortState.direction) {
      this.filterForm.patchValue({
        sortBy: sortState.active,
        isDescending: sortState.direction === 'desc'
      }, { emitEvent: false });
    } else {
      this.filterForm.patchValue({
        sortBy: 'ObservationDate',
        isDescending: true
      }, { emitEvent: false });
    }
    this.loadObservations();
  }

  openCreateDialog(): void {
    this.editingId = null;
    this.mainPhotoPreviewUrl = null;
    this.referencePhotoPreviews = [];
    this.observationForm.reset({ observationDate: new Date() });
    this.dialog.open(this.observationDialogTemplate, { width: '600px' });
  }

  openEditDialog(obs: ObservationDto): void {
    if (!obs.isPending && !obs.isQuestioned) {
      this.showError('You can only edit pending or questioned observations.');
      return;
    }
    
    // Fetch crop cycles for this field so the dropdown populates
    this.onFieldSelected(obs.fieldId);
    
    this.editingId = obs.id;
    
    const relativeImagePath = this.getRelativePathFromUrl(obs.imagePath);
    const relativeRefs = (obs.additionalImagePaths || []).map(path => this.getRelativePathFromUrl(path) || '');

    this.observationForm.patchValue({
      fieldId: obs.fieldId,
      cropCycleId: obs.cropCycleId,
      observationDate: obs.observationDate,
      cropHealth: obs.cropHealth,
      pestType: obs.pestType,
      notes: obs.notes,
      imagePath: relativeImagePath,
      additionalImagePaths: relativeRefs,
      imageCaption: obs.imageCaption
    });

    this.mainPhotoPreviewUrl = obs.imagePath || null;
    this.referencePhotoPreviews = (obs.additionalImagePaths || []).map((url, i) => ({
      fileName: relativeRefs[i],
      url: url
    }));

    this.dialog.open(this.observationDialogTemplate, { width: '600px' });
  }

  saveObservation(): void {
    if (this.observationForm.invalid) return;

    const val = { ...this.observationForm.value };
    
    // Clean up empty strings to null for the backend
    if (val.cropCycleId === '') val.cropCycleId = null;
    if (val.cropHealth === '') val.cropHealth = null;
    if (val.pestType === '') val.pestType = null;
    if (val.notes === '') val.notes = null;
    if (val.imagePath === '') val.imagePath = null;
    if (val.imageCaption === '') val.imageCaption = null;
    if (val.additionalImagePaths && val.additionalImagePaths.length === 0) {
      val.additionalImagePaths = null;
    }
    
    if (this.editingId) {
      const dto: UpdateObservationDto = {
        ...val,
        observationDate: new Date(val.observationDate).toISOString()
      };
      
      this.observationService.updateObservation(this.editingId, dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation updated successfully', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadObservations();
          }
        },
        error: (err) => this.showError(err.error?.message || 'Failed to update observation')
      });
    } else {
      const dto: CreateObservationDto = {
        ...val,
        observationDate: new Date(val.observationDate).toISOString()
      };
      
      this.observationService.createObservation(dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation created successfully', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadObservations();
          }
        },
        error: (err) => this.showError(err.error?.message || 'Failed to create observation')
      });
    }
  }

  deleteObservation(id: number): void {
    if (confirm('Are you sure you want to delete this observation?')) {
      this.observationService.deleteObservation(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Observation deleted successfully', 'Close', { duration: 3000 });
            this.loadObservations();
          }
        },
        error: (err) => this.showError(err.error?.message || 'Failed to delete observation')
      });
    }
  }

  openRespondDialog(obs: ObservationDto): void {
    this.editingId = obs.id;
    this.responseForm.reset();
    this.dialog.open(this.respondDialogTemplate, { width: '500px' });
  }

  sendResponse(): void {
    if (this.responseForm.invalid || !this.editingId) return;

    const dto: ObservationWorkerResponseDto = this.responseForm.value;
    
    this.observationService.respondToAdmin(this.editingId, dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Response sent successfully', 'Close', { duration: 3000 });
          this.dialog.closeAll();
          this.loadObservations();
        }
      },
      error: (err) => this.showError(err.error?.message || 'Failed to send response')
    });
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'verified': return 'primary';
      case 'questioned': return 'accent';
      case 'invalid': return 'warn';
      default: return 'default'; // pending
    }
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { 
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
