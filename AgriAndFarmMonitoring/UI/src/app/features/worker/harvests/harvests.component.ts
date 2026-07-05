// src/app/features/worker/harvests/harvests.component.ts
import {
  Component, OnInit, inject, ViewChild, TemplateRef,
  ChangeDetectorRef, NgZone
} from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
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
import { debounceTime, distinctUntilChanged, finalize, forkJoin } from 'rxjs';

import { MaterialModule } from '../../../shared/material.module';
import { WorkerHarvestService } from '../services/worker-harvest.service';
import { WorkerFieldService } from '../services/worker-field.service';
import {
  HarvestDto,
  HarvestFilterDto,
  CreateHarvestDto,
  UpdateHarvestDto,
  HarvestWorkerResponseDto
} from '../models/worker-harvest.model';

@Component({
  selector: 'app-harvests',
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
    DatePipe,
    DecimalPipe,
    CurrencyPipe
  ],
  templateUrl: './harvests.component.html',
  styleUrls: ['./harvests.component.scss']
})
export class Harvests implements OnInit {
  private harvestService = inject(WorkerHarvestService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);

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
  harvestForm!: FormGroup;
  responseForm!: FormGroup;

  editingId: number | null = null;
  selectedHarvest: HarvestDto | null = null;

  // Photo upload states
  isMainUploading = false;
  isRefsUploading = false;
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { fileName: string; url: string }[] = [];

  readonly qualityGrades = ['PREMIUM', 'GRADE_A', 'GRADE_B', 'GRADE_C', 'REJECT'];
  readonly harvestMethods = ['MANUAL', 'MECHANICAL', 'SEMI_MECHANICAL', 'STRIP_HARVESTING'];
  readonly approvalStatuses = ['PENDING', 'APPROVED', 'REJECTED', 'REQUEST_CHANGES'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('harvestDialog') harvestDialogTemplate!: TemplateRef<any>;
  @ViewChild('respondDialog') respondDialogTemplate!: TemplateRef<any>;
  @ViewChild('viewDetailsDialog') viewDetailsDialogTemplate!: TemplateRef<any>;

  constructor() {
    this.initForms();
  }

  ngOnInit(): void {
    this.loadFields();
    this.filterForm.patchValue({ approvalStatus: 'PENDING' });
    this.loadHarvests();

    this.filterForm.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageIndex = 0;
      this.loadHarvests();
    });
  }

  initForms(): void {
    this.filterForm = this.fb.group({
      fieldId: [''],
      cropCycleId: [''],
      approvalStatus: [''],
      qualityGrade: [''],
      harvestMethod: [''],
      includeDeleted: [false],
      fromDate: [''],
      toDate: ['']
    });

    this.harvestForm = this.fb.group({
      fieldId: ['', Validators.required],
      cropCycleId: ['', Validators.required],
      harvestDate: [new Date(), Validators.required],
      quantityKg: [null, [Validators.required, Validators.min(0.1)]],
      qualityGrade: [''],
      harvestMethod: [''],
      notes: [''],
      pricePerKg: [null],
      batchNumber: [''],
      imagePath: [''],
      thumbnailPath: [''],
      imageCaption: [''],
      additionalImagePaths: [[]]
    });

    this.responseForm = this.fb.group({
      responseNotes: ['', Validators.required]
    });
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const statuses = ['PENDING', 'APPROVED', 'REJECTED', 'REQUEST_CHANGES'];
    this.filterForm.patchValue({ approvalStatus: statuses[index] });
  }

  onFieldSelected(fieldId: number): void {
    this.cropCycles = [];
    if (this.harvestForm) this.harvestForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
    if (fieldId) {
      this.fieldService.getAssignedFieldDetail(fieldId).subscribe({
        next: (res: any) => {
          if (res.success && res.data?.cropCycles) {
            this.cropCycles = res.data.cropCycles;
          }
        },
        error: () => console.error('Failed to load crop cycles')
      });
    }
  }

  onFilterFieldSelected(fieldId: number): void {
    // Only reset cropCycleId in filter form
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
        if (res.success && res.data) this.fields = res.data;
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

    // Remove empty/null values
    (Object.keys(filter) as (keyof HarvestFilterDto)[]).forEach(key => {
      if (filter[key] === '' || filter[key] === null || filter[key] === undefined) {
        delete filter[key];
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

  // ── Create / Edit Dialog ──────────────────────────────────────
  openCreateDialog(): void {
    this.editingId = null;
    this.mainPhotoPreviewUrl = null;
    this.referencePhotoPreviews = [];
    this.harvestForm.reset({ harvestDate: new Date() });
    this.cropCycles = [];
    this.dialog.open(this.harvestDialogTemplate, { width: '620px', maxHeight: '90vh' });
  }

  openEditDialog(harvest: HarvestDto): void {
    if (harvest.approvalStatus !== 'PENDING' && harvest.approvalStatus !== 'REQUEST_CHANGES') {
      this.showError('Only PENDING or REQUEST_CHANGES harvests can be edited.');
      return;
    }

    this.onFieldSelected(harvest.fieldId);
    this.editingId = harvest.id;

    const relativeImagePath = this.getRelativePathFromUrl(harvest.imagePath);
    const relativeRefs = (harvest.additionalImagePaths || []).map(p => this.getRelativePathFromUrl(p) || '');

    this.harvestForm.patchValue({
      fieldId: harvest.fieldId,
      cropCycleId: harvest.cropCycleId,
      harvestDate: harvest.harvestDate,
      quantityKg: harvest.quantityKg,
      qualityGrade: harvest.qualityGrade || '',
      harvestMethod: harvest.harvestMethod || '',
      notes: harvest.notes || '',
      pricePerKg: harvest.pricePerKg || null,
      batchNumber: harvest.batchNumber || '',
      imagePath: relativeImagePath,
      imageCaption: harvest.imageCaption || '',
      additionalImagePaths: relativeRefs
    });

    this.mainPhotoPreviewUrl = harvest.imagePath || null;
    this.referencePhotoPreviews = (harvest.additionalImagePaths || []).map((url, i) => ({
      fileName: relativeRefs[i],
      url
    }));

    this.dialog.open(this.harvestDialogTemplate, { width: '620px', maxHeight: '90vh' });
  }

  saveHarvest(): void {
    if (this.harvestForm.invalid) return;
    const val = { ...this.harvestForm.value };

    // Normalize nulls
    if (!val.cropCycleId) val.cropCycleId = null;
    if (!val.qualityGrade) val.qualityGrade = null;
    if (!val.harvestMethod) val.harvestMethod = null;
    if (!val.notes) val.notes = null;
    if (!val.imagePath) val.imagePath = null;
    if (!val.imageCaption) val.imageCaption = null;
    if (!val.batchNumber) val.batchNumber = null;
    if (!val.pricePerKg) val.pricePerKg = null;
    if (!val.additionalImagePaths?.length) val.additionalImagePaths = null;

    if (this.editingId) {
      const dto: UpdateHarvestDto = {
        ...val,
        harvestDate: new Date(val.harvestDate).toISOString()
      };
      this.harvestService.updateHarvest(this.editingId, dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Harvest updated successfully', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadHarvests();
          }
        },
        error: (err) => this.showError(err.error?.message || 'Failed to update harvest')
      });
    } else {
      const dto: CreateHarvestDto = {
        ...val,
        harvestDate: new Date(val.harvestDate).toISOString()
      };
      this.harvestService.createHarvest(dto).subscribe({
        next: (res) => {
          if (res.success) {
            this.snackBar.open('Harvest submitted for approval', 'Close', { duration: 3000 });
            this.dialog.closeAll();
            this.loadHarvests();
          }
        },
        error: (err) => this.showError(err.error?.message || 'Failed to create harvest')
      });
    }
  }

  deleteHarvest(id: number): void {
    if (!confirm('Are you sure you want to delete this harvest?')) return;
    this.harvestService.deleteHarvest(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Harvest deleted successfully', 'Close', { duration: 3000 });
          this.loadHarvests();
        }
      },
      error: (err) => this.showError(err.error?.message || 'Failed to delete harvest')
    });
  }

  // ── View Details ──────────────────────────────────────────────
  viewHarvestDetails(harvest: HarvestDto): void {
    this.selectedHarvest = harvest;
    this.dialog.open(this.viewDetailsDialogTemplate, { width: '560px', maxHeight: '90vh' });
  }

  closeDetails(): void {
    this.dialog.closeAll();
  }

  openEditFromDetails(harvest: HarvestDto): void {
    this.dialog.closeAll();
    setTimeout(() => this.openEditDialog(harvest), 150);
  }

  // ── Respond to Admin ─────────────────────────────────────────
  openRespondDialog(harvest: HarvestDto): void {
    this.editingId = harvest.id;
    this.responseForm.reset();
    this.dialog.open(this.respondDialogTemplate, { width: '500px' });
  }

  sendResponse(): void {
    if (this.responseForm.invalid || !this.editingId) return;
    const dto: HarvestWorkerResponseDto = this.responseForm.value;
    this.harvestService.respondToAdmin(this.editingId, dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Response sent successfully', 'Close', { duration: 3000 });
          this.dialog.closeAll();
          this.loadHarvests();
        }
      },
      error: (err) => this.showError(err.error?.message || 'Failed to send response')
    });
  }

  // ── Image upload ──────────────────────────────────────────────
  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (!file) return;
    this.isMainUploading = true;
    this.cdr.detectChanges();

    this.harvestService.uploadImage(file).pipe(
      finalize(() => { this.isMainUploading = false; this.cdr.detectChanges(); })
    ).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.ngZone.run(() => {
            this.harvestForm.patchValue({ imagePath: res.data.fileName });
            this.mainPhotoPreviewUrl = res.data.url;
            this.cdr.detectChanges();
          });
        }
      },
      error: () => this.showError('Failed to upload main photo')
    });
    event.target.value = '';
  }

  onMultipleFilesSelected(event: any): void {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    const uploads = Array.from(files).map(f => this.harvestService.uploadImage(f));
    this.isRefsUploading = true;
    this.cdr.detectChanges();

    forkJoin(uploads).pipe(
      finalize(() => { this.isRefsUploading = false; this.cdr.detectChanges(); })
    ).subscribe({
      next: (responses) => {
        const newFiles = responses.map(r => ({ fileName: r.data.fileName, url: r.data.url }));
        this.ngZone.run(() => {
          this.referencePhotoPreviews = [...this.referencePhotoPreviews, ...newFiles];
          const current: string[] = this.harvestForm.get('additionalImagePaths')?.value || [];
          const updated = Array.from(new Set([...current, ...newFiles.map(f => f.fileName)]));
          this.harvestForm.patchValue({ additionalImagePaths: updated });
          this.cdr.detectChanges();
        });
      },
      error: () => this.showError('Failed to upload reference photos')
    });
    event.target.value = '';
  }

  removeMainPhoto(): void {
    this.ngZone.run(() => {
      this.harvestForm.patchValue({ imagePath: null });
      this.mainPhotoPreviewUrl = null;
      this.cdr.detectChanges();
    });
  }

  removeReferencePhoto(index: number): void {
    const current: string[] = this.harvestForm.get('additionalImagePaths')?.value || [];
    const removed = current[index];
    current.splice(index, 1);
    this.ngZone.run(() => {
      this.harvestForm.patchValue({ additionalImagePaths: [...current] });
      this.referencePhotoPreviews = this.referencePhotoPreviews.filter(p => p.fileName !== removed);
      this.cdr.detectChanges();
    });
  }

  // ── Helpers ───────────────────────────────────────────────────
  private getRelativePathFromUrl(url: string | null | undefined): string | null {
    if (!url) return null;
    if (!url.startsWith('http://') && !url.startsWith('https://')) return url;
    try {
      const parsedUrl = new URL(url);
      const path = parsedUrl.pathname;
      if (path.includes('/uploads/')) return path.substring(path.indexOf('/uploads/') + 9);
      const segments = path.split('/').filter(s => s);
      return segments.length >= 2 ? segments.slice(1).join('/') : path;
    } catch {
      return url;
    }
  }

  getApprovalBadgeClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'APPROVED':
        return 'badge-approved';
      case 'REJECTED':
        return 'badge-rejected';
      case 'REQUEST_CHANGES':
        return 'badge-changes';
      default:
        return 'badge-pending';
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
      case 'PREMIUM': return 'quality-premium';
      case 'GRADE_A': return 'quality-a';
      case 'GRADE_B': return 'quality-b';
      case 'GRADE_C': return 'quality-c';
      case 'REJECT': return 'quality-reject';
      default: return 'quality-none';
    }
  }

  formatQualityGrade(grade: string | undefined): string {
    if (!grade) return '—';
    return grade.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
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
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
