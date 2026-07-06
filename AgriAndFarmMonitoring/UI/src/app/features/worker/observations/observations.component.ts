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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { debounceTime, distinctUntilChanged, finalize, forkJoin } from 'rxjs';

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
    MatDatepickerModule,
    MatNativeDateModule,
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

  // ✅ Store selected files in memory, NOT uploaded to server yet
  selectedMainFile: File | null = null;
  selectedReferenceFiles: File[] = [];
  
  // ✅ Local preview URLs for display (blob URLs)
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { file: File | null; url: string }[] = [];

  // Upload states
  isSaving = false;

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

  // =============================================
  // TAB AND FILTER METHODS
  // =============================================

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['pending', 'questioned', 'verified', 'invalid'];
    this.filterForm.patchValue({ validationStatus: statuses[index] });
  }

  onFieldSelected(fieldId: number): void {
    this.cropCycles = [];
    if (this.observationForm) {
      this.observationForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
    }
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

  onFilterFieldSelected(fieldId: number): void {
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

  // =============================================
  // ✅ FIXED: Store files in memory, don't upload yet
  // =============================================

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      // ✅ Store file in memory
      this.selectedMainFile = file;
      
      // ✅ Create local blob URL for preview
      this.mainPhotoPreviewUrl = URL.createObjectURL(file);
      
      // ✅ Update form with temporary file name (will be replaced after upload)
      this.observationForm.patchValue({ imagePath: file.name });
      
      this.cdr.detectChanges();
      event.target.value = '';
    }
  }

  onMultipleFilesSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      // ✅ Store files in memory
      const newFiles = Array.from(files);
      this.selectedReferenceFiles = [...this.selectedReferenceFiles, ...newFiles];
      
      // ✅ Create blob URLs for previews
      const newPreviews = newFiles.map(file => ({
        file: file,
        url: URL.createObjectURL(file)
      }));
      
      this.referencePhotoPreviews = [...this.referencePhotoPreviews, ...newPreviews];
      
      // ✅ Update form with temporary file names
      const currentFiles = this.observationForm.get('additionalImagePaths')?.value || [];
      const updatedFiles = [...currentFiles, ...newFiles.map(f => f.name)];
      this.observationForm.patchValue({ additionalImagePaths: updatedFiles });
      
      this.cdr.detectChanges();
      event.target.value = '';
    }
  }

  removeReferencePhoto(index: number): void {
    const removedPreview = this.referencePhotoPreviews[index];
    if (removedPreview) {
      // ✅ Revoke blob URL to free memory
      URL.revokeObjectURL(removedPreview.url);
    }
    
    // Remove from arrays
    this.referencePhotoPreviews.splice(index, 1);
    this.selectedReferenceFiles.splice(index, 1);
    
    // Update form
    const currentFiles: string[] = this.observationForm.get('additionalImagePaths')?.value || [];
    currentFiles.splice(index, 1);
    this.observationForm.patchValue({ additionalImagePaths: [...currentFiles] });
    
    this.cdr.detectChanges();
  }

  removeMainPhoto(): void {
    if (this.mainPhotoPreviewUrl) {
      // ✅ Revoke blob URL to free memory
      URL.revokeObjectURL(this.mainPhotoPreviewUrl);
    }
    
    this.selectedMainFile = null;
    this.mainPhotoPreviewUrl = null;
    this.observationForm.patchValue({ imagePath: null });
    
    this.cdr.detectChanges();
  }

  private cleanupPreviews(): void {
    // ✅ Revoke all blob URLs to free memory
    if (this.mainPhotoPreviewUrl) {
      URL.revokeObjectURL(this.mainPhotoPreviewUrl);
    }
    this.referencePhotoPreviews.forEach(p => URL.revokeObjectURL(p.url));
    
    this.selectedMainFile = null;
    this.selectedReferenceFiles = [];
    this.mainPhotoPreviewUrl = null;
    this.referencePhotoPreviews = [];
  }

  private isValidImageFile(file: File): boolean {
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    return validTypes.includes(file.type);
  }

  // =============================================
  // ✅ FIXED: Upload images ONLY on form submission
  // =============================================

  saveObservation(): void {
    if (this.observationForm.invalid) return;

    this.isSaving = true;
    this.cdr.detectChanges();

    const val = { ...this.observationForm.value };
    
    // Clean up empty strings
    if (val.cropCycleId === '') val.cropCycleId = null;
    if (val.cropHealth === '') val.cropHealth = null;
    if (val.pestType === '') val.pestType = null;
    if (val.notes === '') val.notes = null;
    if (val.imagePath === '') val.imagePath = null;
    if (val.imageCaption === '') val.imageCaption = null;
    if (val.additionalImagePaths && val.additionalImagePaths.length === 0) {
      val.additionalImagePaths = null;
    }
    
    const dto: any = {
      ...val,
      observationDate: new Date(val.observationDate).toISOString()
    };

    // ✅ Determine if we need to upload images
    const hasMainImage = this.selectedMainFile !== null;
    const hasRefImages = this.selectedReferenceFiles.length > 0;

    if (this.editingId) {
      // ✅ For edit: First update the observation
      this.observationService.updateObservation(this.editingId, dto).subscribe({
        next: (res) => {
          if (res.success) {
            // ✅ If there are new images, upload them
            if (hasMainImage || hasRefImages) {
              this.uploadImagesAndUpdate(res.data.id);
            } else {
              this.handleSaveSuccess('Observation updated successfully');
            }
          } else {
            this.handleSaveError(res.errors?.join(', ') || 'Failed to update observation');
          }
        },
        error: (err) => this.handleSaveError(err)
      });
    } else {
      // ✅ For create: First create the observation
      this.observationService.createObservation(dto).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            // ✅ Upload images after successful creation
            if (hasMainImage || hasRefImages) {
              this.uploadImagesAndUpdate(res.data.id);
            } else {
              this.handleSaveSuccess('Observation created successfully');
            }
          } else {
            this.handleSaveError(res.errors?.join(', ') || 'Failed to create observation');
          }
        },
        error: (err) => this.handleSaveError(err)
      });
    }
  }

  // ✅ Upload images after observation is created/updated
  private uploadImagesAndUpdate(observationId: number): void {
    const uploadObservables: any[] = [];

    // Upload main image
    if (this.selectedMainFile) {
      uploadObservables.push(this.observationService.uploadImage(this.selectedMainFile));
    }

    // Upload reference images
    for (const file of this.selectedReferenceFiles) {
      uploadObservables.push(this.observationService.uploadImage(file));
    }

    if (uploadObservables.length === 0) {
      this.handleSaveSuccess('Saved successfully');
      return;
    }

    // ✅ Upload all images in parallel
    forkJoin(uploadObservables).pipe(
      finalize(() => {
        this.isSaving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (responses: any[]) => {
        // ✅ Update observation with image paths
        const imagePaths = responses.map(r => r.data.fileName);
        const mainImagePath = this.selectedMainFile ? imagePaths[0] : null;
        const refImagePaths = this.selectedReferenceFiles.length > 0 
          ? imagePaths.slice(this.selectedMainFile ? 1 : 0) 
          : [];

        // ✅ Update the observation with image paths
        const updateDto: any = {};
        if (mainImagePath) {
          updateDto.imagePath = mainImagePath;
        }
        if (refImagePaths.length > 0) {
          updateDto.additionalImagePaths = refImagePaths;
        }

        if (Object.keys(updateDto).length > 0) {
          this.observationService.updateObservation(observationId, updateDto).subscribe({
            next: () => {
              this.handleSaveSuccess('Observation saved with images');
            },
            error: (err) => {
              console.error('Failed to update observation with images:', err);
              this.handleSaveSuccess('Observation saved but images may not be attached');
            }
          });
        } else {
          this.handleSaveSuccess('Observation saved successfully');
        }
      },
      error: (err) => {
        console.error('Failed to upload images:', err);
        this.handleSaveSuccess('Observation saved but images failed to upload');
      }
    });
  }

  private handleSaveSuccess(message: string): void {
    // ✅ Clean up blob URLs
    this.cleanupPreviews();
    
    this.isSaving = false;
    this.snackBar.open(message, 'Close', { duration: 3000 });
    this.dialog.closeAll();
    this.loadObservations();
    this.cdr.detectChanges();
  }

  private handleSaveError(err: any): void {
    this.isSaving = false;
    this.cdr.detectChanges();
    const message = err?.error?.message || err?.message || 'Failed to save observation';
    this.showError(message);
  }

  // =============================================
  // DIALOG METHODS
  // =============================================

  openCreateDialog(): void {
    this.editingId = null;
    this.cleanupPreviews();
    this.observationForm.reset({ observationDate: new Date() });
    this.cropCycles = [];
    this.dialog.open(this.observationDialogTemplate, { width: '600px' });
  }

  openEditDialog(obs: ObservationDto): void {
    if (!obs.isPending && !obs.isQuestioned) {
      this.showError('You can only edit pending or questioned observations.');
      return;
    }
    
    // Fetch crop cycles for this field
    this.onFieldSelected(obs.fieldId);
    
    this.editingId = obs.id;
    this.cleanupPreviews();

    this.observationForm.patchValue({
      fieldId: obs.fieldId,
      cropCycleId: obs.cropCycleId,
      observationDate: obs.observationDate,
      cropHealth: obs.cropHealth,
      pestType: obs.pestType,
      notes: obs.notes,
      imagePath: obs.imagePath,
      additionalImagePaths: obs.additionalImagePaths || [],
      imageCaption: obs.imageCaption
    });

    // ✅ For existing images, use server URLs directly (not blob URLs)
    this.mainPhotoPreviewUrl = obs.imagePath || null;
    this.referencePhotoPreviews = (obs.additionalImagePaths || []).map(url => ({
      file: null,
      url: url
    }));

    this.dialog.open(this.observationDialogTemplate, { width: '600px' });
  }

  openEditDialogFromDetails(obs: ObservationDto): void {
    this.dialog.closeAll();
    setTimeout(() => {
      this.openEditDialog(obs);
    }, 150);
  }

  viewObservationDetails(obs: ObservationDto): void {
    this.selectedObservation = obs;
    this.dialog.open(this.viewDetailsDialogTemplate, { width: '550px' });
  }

  closeDetails(): void {
    this.dialog.closeAll();
  }

  closeDialog(): void {
    this.cleanupPreviews();
    this.dialog.closeAll();
  }

  // =============================================
  // RESPOND TO ADMIN
  // =============================================

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

  // =============================================
  // CRUD OPERATIONS
  // =============================================

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

  // =============================================
  // DATA LOADING METHODS
  // =============================================

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

  // =============================================
  // FORM INITIALIZATION
  // =============================================

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

  // =============================================
  // HELPER METHODS FOR TEMPLATE
  // =============================================

  canEdit(obs: ObservationDto): boolean {
    return obs.isPending || obs.isQuestioned;
  }

  canRespond(obs: ObservationDto): boolean {
    return obs.isQuestioned;
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

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'verified': return 'primary';
      case 'questioned': return 'accent';
      case 'invalid': return 'warn';
      default: return 'default';
    }
  }

  // =============================================
  // UTILITY METHODS
  // =============================================

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { 
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}