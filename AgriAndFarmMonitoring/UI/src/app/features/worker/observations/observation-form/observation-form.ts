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
import { MatCheckboxModule } from '@angular/material/checkbox';

import { WorkerObservationStateService } from '../../services/worker-observation-state.service';
import { WorkerFieldService } from '../../services/worker-field.service';
import { ImageCompressorService } from '../../../../core/services/image-compressor.service';
import { DiseaseDetectionService } from '../../../ai/disease-detection/disease-detection.service';

@Component({
  selector: 'app-observation-form',
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
    MatCheckboxModule,
    DatePipe
  ],
  templateUrl: './observation-form.html',
  styleUrls: ['./observation-form.scss']
})
export class ObservationFormComponent implements OnInit, OnDestroy {
  private observationState = inject(WorkerObservationStateService);
  private fieldService = inject(WorkerFieldService);
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<ObservationFormComponent>);
  private data = inject(MAT_DIALOG_DATA);
  private snackBar = inject(MatSnackBar);
  private imageCompressor = inject(ImageCompressorService);
  private diseaseService = inject(DiseaseDetectionService);

  isSaving = false;
  editingId: number | null = null;
  editData: any | null = null;
  isLoading = false;

  observationForm!: FormGroup;
  fields: any[] = [];
  cropCycles: any[] = [];

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
  
  // AI Disease Detection
  isAnalyzingDisease = false;
  aiDiseaseResult: any = null;

  readonly healthStatuses = ['EXCELLENT', 'GOOD', 'AVERAGE', 'POOR', 'CRITICAL'];

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
      this.observationForm.reset({ 
        observationDate: new Date(),
        pestDetected: false,
        diseaseDetected: false
      });
    }
  }

  ngOnDestroy(): void {}

  private initForm(): void {
    this.observationForm = this.fb.group({
      fieldId: ['', Validators.required],
      cropCycleId: [''],
      observationDate: [new Date(), Validators.required],
      cropHealth: [''],
      soilMoisture: [null, [Validators.min(0), Validators.max(100)]],
      pestDetected: [false],
      pestType: [''],
      diseaseDetected: [false],
      diseaseType: [''],
      notes: [''],
      imageCaption: ['']
    });

    this.observationForm.get('pestDetected')?.valueChanges.subscribe(detected => {
      const pestTypeControl = this.observationForm.get('pestType');
      if (detected) {
        pestTypeControl?.setValidators([Validators.required]);
      } else {
        pestTypeControl?.clearValidators();
        pestTypeControl?.setValue('');
      }
      pestTypeControl?.updateValueAndValidity();
    });

    this.observationForm.get('diseaseDetected')?.valueChanges.subscribe(detected => {
      const diseaseTypeControl = this.observationForm.get('diseaseType');
      if (detected) {
        diseaseTypeControl?.setValidators([Validators.required]);
      } else {
        diseaseTypeControl?.clearValidators();
        diseaseTypeControl?.setValue('');
      }
      diseaseTypeControl?.updateValueAndValidity();
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

  loadCropCyclesAndPopulate(observation: any): void {
    this.isLoading = true;
    
    this.fieldService.getAssignedFieldDetail(observation.fieldId).subscribe({
      next: (res: any) => {
        if (res.success && res.data?.cropCycles) {
          this.cropCycles = res.data.cropCycles;
        }
        this.populateForm(observation);
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load crop cycles:', err);
        this.populateForm(observation);
        this.isLoading = false;
      }
    });
  }

  populateForm(observation: any): void {
    this.originalFormValues = {
      fieldId: observation.fieldId,
      cropCycleId: observation.cropCycleId || '',
      observationDate: observation.observationDate,
      cropHealth: observation.cropHealth || '',
      soilMoisture: observation.soilMoisture || null,
      pestDetected: observation.pestDetected || !!observation.pestType,
      pestType: observation.pestType || '',
      diseaseDetected: observation.diseaseDetected || !!observation.diseaseType,
      diseaseType: observation.diseaseType || '',
      notes: observation.notes || '',
      imageCaption: observation.imageCaption || ''
    };

    this.observationForm.patchValue(this.originalFormValues);

    this.existingMainImagePath = observation.imagePath || null;
    this.existingReferencePaths = observation.additionalImagePaths || [];
    this.originalAdditionalImagePaths = observation.additionalImagePaths ? [...observation.additionalImagePaths] : [];

    this.mainPhotoPreviewUrl = observation.imagePath || null;
    this.referencePhotoPreviews = (observation.additionalImagePaths || []).map((url: string, index: number) => ({
      id: `existing-${index}-${Date.now()}`,
      fileName: this.extractFileNameFromUrl(url) || 'Image',
      url: url,
      isExisting: true
    }));

    if (observation.fieldId) {
      this.cropCycles = [...this.cropCycles];
    }
  }

  onFieldSelected(fieldId: number): void {
    this.cropCycles = [];
    if (this.observationForm) {
      this.observationForm.patchValue({ cropCycleId: '' }, { emitEvent: false });
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

  analyzeForDisease(): void {
    if (!this.selectedMainFile) {
      this.showError('Please select a main photo first.');
      return;
    }
    if (!this.observationForm.get('fieldId')?.value) {
      this.showError('Please select a field first to provide context.');
      return;
    }

    this.isAnalyzingDisease = true;
    this.snackBar.open('Analyzing image with AI...', '', { duration: 2000 });

    this.diseaseService.detectDisease({
      image: this.selectedMainFile,
      farmId: 0, // In real app, get from auth service
      fieldId: this.observationForm.get('fieldId')?.value,
      cropCycleId: this.observationForm.get('cropCycleId')?.value,
      additionalSymptoms: this.observationForm.get('notes')?.value
    }).subscribe({
      next: (res) => {
        this.isAnalyzingDisease = false;
        this.aiDiseaseResult = res;
        this.snackBar.open(`AI Analysis complete: Detected ${res.diseaseName}`, 'Close', { duration: 5000 });
        
        // Auto-fill form based on AI result
        if (res.diseaseName && res.diseaseName.toLowerCase() !== 'healthy') {
          this.observationForm.patchValue({
            diseaseDetected: true,
            diseaseType: res.diseaseName,
            cropHealth: res.severity.toUpperCase() === 'HIGH' ? 'CRITICAL' : (res.severity.toUpperCase() === 'MEDIUM' ? 'POOR' : 'AVERAGE')
          });
        } else {
          this.observationForm.patchValue({
            diseaseDetected: false,
            diseaseType: '',
            cropHealth: 'GOOD'
          });
        }
      },
      error: (err) => {
        this.isAnalyzingDisease = false;
        this.showError('AI Analysis failed. Please try again or fill manually.');
      }
    });
  }

  async onMultipleFilesSelected(event: any): Promise<void> {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    
    try {
      const processedFiles: File[] = [];
      
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        
        if (!file.type.match(/image\/(jpeg|png|webp)/)) {
          this.showError(`Invalid file type: ${file.name}. Only JPG, PNG and WEBP are allowed.`);
          continue;
        }
        
        if (file.size > 10 * 1024 * 1024) {
          this.showError(`File ${file.name} exceeds 10MB limit.`);
          continue;
        }
        
        let processedFile = file;
        if (file.size > 1024 * 1024) {
          processedFile = await this.imageCompressor.compressImage(file);
        }
        
        processedFiles.push(processedFile);
      }
      
      if (processedFiles.length === 0) {
        this.showError('No valid images selected');
        event.target.value = '';
        return;
      }
      
      this.selectedReferenceFiles = [...this.selectedReferenceFiles, ...processedFiles];
      
      processedFiles.forEach((file) => {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.referencePhotoPreviews.push({
            id: `new-${Date.now()}-${Math.random()}`,
            fileName: file.name,
            url: e.target?.result as string,
            isExisting: false,
            file: file
          });
        };
        reader.onerror = () => {
          this.showError(`Failed to read file: ${file.name}`);
        };
        reader.readAsDataURL(file);
      });
      
      event.target.value = '';
    } catch (err) {
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

  async save(): Promise<void> {
    if (this.observationForm.invalid) return;

    this.isSaving = true;
    this.uploadProgress = 0;
    this.uploadStatus = 'Preparing files...';

    const currentValues = { ...this.observationForm.value };
    
    if (currentValues.cropCycleId === '') currentValues.cropCycleId = null;
    if (currentValues.cropHealth === '') currentValues.cropHealth = null;
    if (currentValues.notes === '') currentValues.notes = null;
    if (currentValues.imageCaption === '') currentValues.imageCaption = null;
    if (currentValues.pestType === '') currentValues.pestType = null;
    if (currentValues.diseaseType === '') currentValues.diseaseType = null;
    if (currentValues.soilMoisture === '') currentValues.soilMoisture = null;

    try {
      let mainImagePath: string | null = null;
      let referencePaths: string[] = [];

      if (this.selectedMainFile) {
        this.uploadStatus = 'Uploading main photo...';
        this.uploadProgress = 10;
        const uploadResult = await this.observationState.uploadImage(this.selectedMainFile).toPromise();
        if (uploadResult?.success && uploadResult.data) {
          mainImagePath = uploadResult.data.fileName;
          this.uploadProgress = 40;
        } else {
          throw new Error('Failed to upload main photo');
        }
      } else {
        mainImagePath = this.existingMainImagePath;
      }

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
          return this.observationState.uploadImage(file).toPromise();
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

      this.uploadStatus = 'Saving observation...';
      this.uploadProgress = 90;

      let response;

      if (this.editingId) {
        const updateDto: any = {};

        if (currentValues.fieldId !== this.originalFormValues.fieldId) updateDto.fieldId = currentValues.fieldId;
        if (currentValues.cropCycleId !== this.originalFormValues.cropCycleId) updateDto.cropCycleId = currentValues.cropCycleId;
        if (currentValues.observationDate !== this.originalFormValues.observationDate) updateDto.observationDate = new Date(currentValues.observationDate).toISOString();
        if (currentValues.cropHealth !== this.originalFormValues.cropHealth) updateDto.cropHealth = currentValues.cropHealth;
        if (currentValues.soilMoisture !== this.originalFormValues.soilMoisture) updateDto.soilMoisture = currentValues.soilMoisture;
        if (currentValues.pestDetected !== this.originalFormValues.pestDetected) updateDto.pestDetected = currentValues.pestDetected;
        if (currentValues.pestType !== this.originalFormValues.pestType) updateDto.pestType = currentValues.pestType;
        if (currentValues.diseaseDetected !== this.originalFormValues.diseaseDetected) updateDto.diseaseDetected = currentValues.diseaseDetected;
        if (currentValues.diseaseType !== this.originalFormValues.diseaseType) updateDto.diseaseType = currentValues.diseaseType;
        if (currentValues.notes !== this.originalFormValues.notes) updateDto.notes = currentValues.notes;
        if (currentValues.imageCaption !== this.originalFormValues.imageCaption) updateDto.imageCaption = currentValues.imageCaption;

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

        response = await this.observationState.updateObservation(this.editingId, updateDto).toPromise();
      } else {
        const createDto: any = {
          ...currentValues,
          observationDate: new Date(currentValues.observationDate).toISOString(),
          imagePath: mainImagePath,
          additionalImagePaths: referencePaths.length > 0 ? referencePaths : null
        };
        response = await this.observationState.createObservation(createDto).toPromise();
      }

      if (response?.success) {
        this.uploadProgress = 100;
        this.uploadStatus = 'Complete!';
        this.snackBar.open(
          `Observation ${this.editingId ? 'updated' : 'submitted'} successfully`,
          'Close',
          { duration: 3000 }
        );
        this.dialogRef.close({ saved: true });
      } else {
        throw new Error(response?.message || 'Failed to save observation');
      }
    } catch (error: any) {
      this.showError(error.message || 'Failed to save observation');
    } finally {
      this.isSaving = false;
      this.uploadProgress = 0;
      this.uploadStatus = '';
    }
  }

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

  formatHealthStatus(status: string | undefined): string {
    if (!status) return '—';
    return status.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
  }
}
