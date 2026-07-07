// src/app/features/worker/harvests/components/harvest-form/harvest-form.component.ts
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, finalize } from 'rxjs';

import { HarvestStateService } from '../../services/harvest-state.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ImageCompressorService } from '../../../../core/services/image-compressor.service';
import { WorkerHarvestService } from '../../services/worker-harvest.service';
import { HarvestDto, UpdateHarvestDto } from '../../models/worker-harvest.model';

@Component({
  selector: 'app-harvest-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    DatePipe
  ],
  templateUrl: './harvest-form.component.html',
  styleUrls: ['./harvest-form.component.scss']
})
export class HarvestFormComponent implements OnInit, OnDestroy {
  private harvestService = inject(WorkerHarvestService);
  private harvestState = inject(HarvestStateService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<HarvestFormComponent>);
  private data = inject(MAT_DIALOG_DATA);
  private snackBar = inject(MatSnackBar);
  private imageCompressor = inject(ImageCompressorService);

  // ✅ Local saving state (not using signal to avoid issues)
  isSaving = false;

  editingId: number | null = null;
  editData: HarvestDto | null = null;
  isLoading = false;

  harvestForm!: FormGroup;
  fields: any[] = [];
  cropCycles: any[] = [];

  // Original data for tracking changes
  originalFormValues: any = {};
  originalAdditionalImagePaths: string[] = [];

  // Image states
  selectedMainFile: File | null = null;
  selectedReferenceFiles: File[] = [];
  existingMainImagePath: string | null = null;
  existingReferencePaths: string[] = [];
  
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { 
    id: string;
    fileName: string;
    url: string;
    isExisting: boolean;
    file?: File;
  }[] = [];

  uploadProgress = 0;
  uploadStatus = '';

  readonly qualityGrades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];
  readonly harvestMethods = ['MANUAL', 'MECHANICAL', 'SEMI_MECHANICAL', 'COMBINE'];

  constructor() {
    this.initForm();
    this.editingId = this.data?.editingId || null;
    this.editData = this.data?.editData || null;
  }

  ngOnInit(): void {
    this.loadFields();
    if (this.editData) {
      this.loadCropCyclesAndPopulate(this.editData);
    } else {
      this.harvestForm.reset({ harvestDate: new Date() });
    }
  }

  ngOnDestroy(): void {
    // Cleanup
  }

  private initForm(): void {
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
      imageCaption: ['']
    });
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

  loadCropCyclesAndPopulate(harvest: HarvestDto): void {
    this.isLoading = true;
    
    this.fieldService.getAssignedFieldDetail(harvest.fieldId).subscribe({
      next: (res: any) => {
        if (res.success && res.data?.cropCycles) {
          this.cropCycles = res.data.cropCycles;
        }
        this.populateForm(harvest);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load crop cycles:', err);
        this.populateForm(harvest);
        this.isLoading = false;
      }
    });
  }

  populateForm(harvest: HarvestDto): void {
    this.originalFormValues = {
      fieldId: harvest.fieldId,
      cropCycleId: harvest.cropCycleId,
      harvestDate: harvest.harvestDate,
      quantityKg: harvest.quantityKg,
      qualityGrade: harvest.qualityGrade || '',
      harvestMethod: harvest.harvestMethod || '',
      notes: harvest.notes || '',
      pricePerKg: harvest.pricePerKg || null,
      batchNumber: harvest.batchNumber || '',
      imageCaption: harvest.imageCaption || ''
    };

    this.harvestForm.patchValue(this.originalFormValues);

    this.existingMainImagePath = harvest.imagePath || null;
    this.existingReferencePaths = harvest.additionalImagePaths || [];
    this.originalAdditionalImagePaths = harvest.additionalImagePaths ? [...harvest.additionalImagePaths] : [];

    this.mainPhotoPreviewUrl = harvest.imagePath || null;
    this.referencePhotoPreviews = (harvest.additionalImagePaths || []).map((url, index) => ({
      id: `existing-${index}-${Date.now()}`,
      fileName: this.extractFileNameFromUrl(url) || 'Image',
      url: url,
      isExisting: true
    }));

    if (harvest.fieldId) {
      this.cropCycles = [...this.cropCycles];
    }
  }

  onFieldSelected(fieldId: number): void {
    this.cropCycles = [];
    if (this.harvestForm) {
      this.harvestForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
    }
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

  closeDialog(): void {
    this.resetImageState();
    this.dialogRef.close({ saved: false });
  }

  // =============================================
  // IMAGE HANDLING
  // =============================================

  async onFileSelected(event: any): Promise<void> {
    const file: File = event.target.files[0];
    if (!file) return;
    
    try {
      let processedFile = file;
      if (file.size > 1024 * 1024) {
        processedFile = await this.imageCompressor.compressImage(file);
      }
      this.selectedMainFile = processedFile;
      
      const reader = new FileReader();
      reader.onload = (e) => {
        this.mainPhotoPreviewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(processedFile);
    } catch (err) {
      this.showError('Failed to process image');
    }
    event.target.value = '';
  }


// src/app/features/worker/harvests/components/harvest-form/harvest-form.component.ts
// Update the onMultipleFilesSelected method

async onMultipleFilesSelected(event: any): Promise<void> {
  const files: FileList = event.target.files;
  if (!files || files.length === 0) return;
  
  console.log(`📸 Selected ${files.length} reference photos`);
  
  try {
    const processedFiles: File[] = [];
    
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      console.log(`📸 Processing file ${i + 1}: ${file.name} (${file.size} bytes)`);
      
      // Validate file type
      if (!file.type.match(/image\/(jpeg|png|webp)/)) {
        this.showError(`Invalid file type: ${file.name}. Only JPG, PNG and WEBP are allowed.`);
        continue;
      }
      
      // Validate file size (max 10MB)
      if (file.size > 10 * 1024 * 1024) {
        this.showError(`File ${file.name} exceeds 10MB limit.`);
        continue;
      }
      
      let processedFile = file;
      if (file.size > 1024 * 1024) { // > 1MB
        processedFile = await this.imageCompressor.compressImage(file);
        console.log(`✅ Compressed ${file.name}: ${file.size} -> ${processedFile.size} bytes`);
      }
      
      processedFiles.push(processedFile);
    }
    
    if (processedFiles.length === 0) {
      this.showError('No valid images selected');
      event.target.value = '';
      return;
    }
    
    // Add all processed files
    this.selectedReferenceFiles = [...this.selectedReferenceFiles, ...processedFiles];
    console.log(`✅ Added ${processedFiles.length} reference photos, total: ${this.selectedReferenceFiles.length}`);
    
    // Create previews for all files
    processedFiles.forEach((file, index) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.referencePhotoPreviews.push({
          id: `new-${Date.now()}-${Math.random()}`,
          fileName: file.name,
          url: e.target?.result as string,
          isExisting: false,
          file: file
        });
        console.log(`✅ Preview created for ${file.name}`);
      };
      reader.onerror = (error) => {
        console.error(`❌ Error reading file ${file.name}:`, error);
        this.showError(`Failed to read file: ${file.name}`);
      };
      reader.readAsDataURL(file);
    });
    
    // Reset the input so the same files can be selected again
    event.target.value = '';
    
  } catch (err) {
    console.error('❌ Error processing images:', err);
    this.showError('Failed to process images');
    event.target.value = '';
  }
}

  removeMainPhoto(): void {
    this.selectedMainFile = null;
    this.mainPhotoPreviewUrl = null;
  }

  removeReferencePhoto(index: number): void {
    const removedPreview = this.referencePhotoPreviews[index];
    if (removedPreview) {
      if (!removedPreview.isExisting) {
        const fileIndex = this.selectedReferenceFiles.findIndex(f => f.name === removedPreview.fileName);
        if (fileIndex > -1) {
          this.selectedReferenceFiles.splice(fileIndex, 1);
        }
      }
      this.referencePhotoPreviews.splice(index, 1);
    }
  }

  private resetImageState(): void {
    this.selectedMainFile = null;
    this.selectedReferenceFiles = [];
    this.mainPhotoPreviewUrl = null;
    this.referencePhotoPreviews = [];
    this.uploadProgress = 0;
    this.uploadStatus = '';
    this.existingMainImagePath = null;
    this.existingReferencePaths = [];
    this.originalAdditionalImagePaths = [];
    this.originalFormValues = {};
    this.isSaving = false;
  }

  // =============================================
  // SAVE HARVEST - Uses State Service
  // =============================================

  async save(): Promise<void> {
    if (this.harvestForm.invalid) return;

    // ✅ Set local saving state
    this.isSaving = true;
    this.uploadProgress = 0;
    this.uploadStatus = 'Preparing files...';

    const currentValues = { ...this.harvestForm.value };
    
    if (currentValues.cropCycleId === '') currentValues.cropCycleId = null;
    if (currentValues.qualityGrade === '') currentValues.qualityGrade = null;
    if (currentValues.harvestMethod === '') currentValues.harvestMethod = null;
    if (currentValues.notes === '') currentValues.notes = null;
    if (currentValues.imageCaption === '') currentValues.imageCaption = null;
    if (currentValues.batchNumber === '') currentValues.batchNumber = null;
    if (currentValues.pricePerKg === '') currentValues.pricePerKg = null;

    try {
      let mainImagePath: string | null = null;
      let referencePaths: string[] = [];

      // Handle main photo
      if (this.selectedMainFile) {
        this.uploadStatus = 'Uploading main photo...';
        this.uploadProgress = 10;
        const uploadResult = await this.harvestService.uploadImage(this.selectedMainFile).toPromise();
        if (uploadResult?.success && uploadResult.data) {
          mainImagePath = uploadResult.data.fileName;
          this.uploadProgress = 40;
        } else {
          throw new Error('Failed to upload main photo');
        }
      } else {
        mainImagePath = this.existingMainImagePath;
      }

      // Handle reference photos
      const keptExistingPaths = this.existingReferencePaths.filter(path =>
        this.referencePhotoPreviews.some(p => p.isExisting && p.url === path)
      );
      referencePaths = [...keptExistingPaths];

      const newFilesToUpload = this.referencePhotoPreviews
        .filter(p => !p.isExisting && p.file)
        .map(p => p.file!);

      if (newFilesToUpload.length > 0) {
        this.uploadStatus = `Uploading ${newFilesToUpload.length} new reference photos...`;
        this.uploadProgress = 50;
        
        const uploads = newFilesToUpload.map((file, index) => {
          this.uploadStatus = `Uploading reference photo ${index + 1}/${newFilesToUpload.length}...`;
          return this.harvestService.uploadImage(file).toPromise();
        });
        
        const results = await Promise.all(uploads);
        for (const result of results) {
          if (result?.success && result.data) {
            referencePaths.push(result.data.fileName);
          } else {
            throw new Error('Failed to upload one or more reference photos');
          }
        }
        this.uploadProgress = 80;
      }

      this.uploadStatus = 'Saving harvest...';
      this.uploadProgress = 90;

      let response;

      if (this.editingId) {
        const updateDto: UpdateHarvestDto = {};

        if (currentValues.fieldId !== this.originalFormValues.fieldId) {
          updateDto.fieldId = currentValues.fieldId;
        }
        if (currentValues.cropCycleId !== this.originalFormValues.cropCycleId) {
          updateDto.cropCycleId = currentValues.cropCycleId;
        }
        if (currentValues.harvestDate !== this.originalFormValues.harvestDate) {
          updateDto.harvestDate = new Date(currentValues.harvestDate).toISOString();
        }
        if (currentValues.quantityKg !== this.originalFormValues.quantityKg) {
          updateDto.quantityKg = currentValues.quantityKg;
        }
        if (currentValues.qualityGrade !== this.originalFormValues.qualityGrade) {
          updateDto.qualityGrade = currentValues.qualityGrade;
        }
        if (currentValues.harvestMethod !== this.originalFormValues.harvestMethod) {
          updateDto.harvestMethod = currentValues.harvestMethod;
        }
        if (currentValues.notes !== this.originalFormValues.notes) {
          updateDto.notes = currentValues.notes;
        }
        if (currentValues.pricePerKg !== this.originalFormValues.pricePerKg) {
          updateDto.pricePerKg = currentValues.pricePerKg;
        }
        if (currentValues.batchNumber !== this.originalFormValues.batchNumber) {
          updateDto.batchNumber = currentValues.batchNumber;
        }
        if (currentValues.imageCaption !== this.originalFormValues.imageCaption) {
          updateDto.imageCaption = currentValues.imageCaption;
        }

        const mainImageChanged = this.selectedMainFile !== null;
        const mainImageRemoved = mainImagePath === null && this.existingMainImagePath !== null;
        
        if (mainImageChanged || mainImageRemoved) {
          updateDto.imagePath = mainImagePath;
        }

        const referencePathsChanged = 
          referencePaths.length !== this.originalAdditionalImagePaths.length ||
          referencePaths.some((path, index) => path !== this.originalAdditionalImagePaths[index]);

        if (referencePathsChanged) {
          updateDto.additionalImagePaths = referencePaths.length > 0 ? referencePaths : null;
        }

        if (Object.keys(updateDto).length === 0) {
          this.snackBar.open('No changes to update', 'Close', { duration: 3000 });
          this.dialogRef.close({ saved: false });
          this.isSaving = false;
          return;
        }

        // ✅ Use state service - auto refreshes list
        response = await this.harvestState.updateHarvest(this.editingId, updateDto).toPromise();
      } else {
        const createDto = {
          ...currentValues,
          harvestDate: new Date(currentValues.harvestDate).toISOString(),
          imagePath: mainImagePath,
          additionalImagePaths: referencePaths.length > 0 ? referencePaths : null
        };
        // ✅ Use state service - auto refreshes list
        response = await this.harvestState.createHarvest(createDto).toPromise();
      }

      if (response?.success) {
        this.uploadProgress = 100;
        this.uploadStatus = 'Complete!';
        this.snackBar.open(
          `Harvest ${this.editingId ? 'updated' : 'submitted for approval'} successfully`,
          'Close',
          { duration: 3000 }
        );
        this.dialogRef.close({ saved: true });
      } else {
        throw new Error(response?.message || 'Failed to save harvest');
      }
    } catch (error: any) {
      this.showError(error.message || 'Failed to save harvest');
    } finally {
      // ✅ Reset saving state
      this.isSaving = false;
      this.uploadProgress = 0;
      this.uploadStatus = '';
    }
  }

  // =============================================
  // HELPERS
  // =============================================

  private extractFileNameFromUrl(url: string | null | undefined): string | null {
    if (!url) return null;
    
    if (url.startsWith('http://') || url.startsWith('https://')) {
      try {
        const parsedUrl = new URL(url);
        const pathname = parsedUrl.pathname;
        const cleanPath = pathname.replace('/uploads/', '');
        const segments = cleanPath.split('/').filter(s => s);
        return segments.length > 0 ? segments[segments.length - 1] : null;
      } catch {
        return url;
      }
    }
    
    const cleanPath = url.replace('uploads/', '');
    const segments = cleanPath.split('/').filter(s => s);
    return segments.length > 0 ? segments[segments.length - 1] : url;
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

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
  }
}