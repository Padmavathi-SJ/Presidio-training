// src/app/features/admin/field-form/field-form.component.ts
import { Component, Inject, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { ImageCompressorService } from '../../../core/services/image-compressor.service';
import { FieldService } from '../services/field.service';
import { Field, CreateFieldDto, UpdateFieldDto, FIELD_STATUS_OPTIONS, SOIL_TYPE_OPTIONS } from '../models/field.model';

interface DialogData {
  mode: 'create' | 'edit';
  field?: Field;
}

@Component({
  selector: 'app-field-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDividerModule
  ],
  templateUrl: './field-form.component.html'
})
export class FieldFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private fieldService = inject(FieldService);
  private dialogRef = inject(MatDialogRef<FieldFormComponent>);
  private snackBar = inject(MatSnackBar);
  private imageCompressor = inject(ImageCompressorService);
  
  data = inject<DialogData>(MAT_DIALOG_DATA);

  farmId = this.authService.getFarmId()!;


  fieldForm: FormGroup;
  isLoading = false;
  mode: 'create' | 'edit' = 'create';
  statusOptions = FIELD_STATUS_OPTIONS;
  soilTypeOptions = SOIL_TYPE_OPTIONS;

  uploadProgress = 0;
  uploadStatus = '';

  selectedMainFile: File | null = null;
  selectedReferenceFiles: File[] = [];
  existingMainImagePath: string | null = null;
  existingReferencePaths: string[] = [];
  
  mainPhotoPreviewUrl: string | null = null;
  referencePhotoPreviews: { 
    id: string; fileName: string; url: string; isExisting: boolean; file?: File;
  }[] = [];

  constructor() {
    this.mode = this.data.mode || 'create';

    this.fieldForm = this.fb.group({
      fieldName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[a-zA-Z0-9\s\-_]+$/)]],
      location: ['', [Validators.maxLength(200)]],
      areaHectares: ['', [Validators.min(0.01), Validators.max(10000)]],
      soilType: [''],
      status: [''],
      latitude: ['', [Validators.min(-90), Validators.max(90)]],
      longitude: ['', [Validators.min(-180), Validators.max(180)]],
      imageCaption: ['']
    });

    // If both latitude and longitude are provided, they must be provided together
    this.fieldForm.get('latitude')?.valueChanges.subscribe(() => {
      this.validateCoordinates();
    });
    this.fieldForm.get('longitude')?.valueChanges.subscribe(() => {
      this.validateCoordinates();
    });
  }

  ngOnInit(): void {
    if (this.mode === 'edit' && this.data.field) {
      const field = this.data.field;
      this.fieldForm.patchValue({
        fieldName: field.fieldName,
        location: field.location,
        areaHectares: field.areaHectares,
        soilType: field.soilType,
        status: field.status,
        latitude: field.latitude,
        longitude: field.longitude,
        imageCaption: field.imageCaption
      });

      this.existingMainImagePath = field.imagePath || null;
      this.existingReferencePaths = field.additionalImagePaths || [];
      this.mainPhotoPreviewUrl = field.imagePath || null;
      this.referencePhotoPreviews = (field.additionalImagePaths || []).map((url: string, index: number) => ({
        id: `existing-${index}`, fileName: 'Image', url, isExisting: true
      }));
    }
  }

  validateCoordinates(): void {
    const lat = this.fieldForm.get('latitude')?.value;
    const lng = this.fieldForm.get('longitude')?.value;

    if ((lat && !lng) || (!lat && lng)) {
      this.fieldForm.get('longitude')?.setErrors({ coordinateMismatch: true });
      this.fieldForm.get('latitude')?.setErrors({ coordinateMismatch: true });
    } else {
      const latErrors = this.fieldForm.get('latitude')?.errors;
      const lngErrors = this.fieldForm.get('longitude')?.errors;
      
      if (latErrors) {
        delete latErrors['coordinateMismatch'];
        if (Object.keys(latErrors).length === 0) {
          this.fieldForm.get('latitude')?.setErrors(null);
        }
      }
      
      if (lngErrors) {
        delete lngErrors['coordinateMismatch'];
        if (Object.keys(lngErrors).length === 0) {
          this.fieldForm.get('longitude')?.setErrors(null);
        }
      }
    }
  }

  async onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    try {
      let processedFile = file;
      if (file.size > 1024 * 1024) processedFile = await this.imageCompressor.compressImage(file);
      this.selectedMainFile = processedFile;
      const reader = new FileReader();
      reader.onload = (e) => this.mainPhotoPreviewUrl = e.target?.result as string;
      reader.readAsDataURL(processedFile);
    } catch {
      this.snackBar.open('Failed to process image', 'Close');
    }
  }

  async onMultipleFilesSelected(event: any) {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    try {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        let processedFile = file;
        if (file.size > 1024 * 1024) processedFile = await this.imageCompressor.compressImage(file);
        this.selectedReferenceFiles.push(processedFile);
        const reader = new FileReader();
        reader.onload = (e) => {
          this.referencePhotoPreviews.push({
            id: `new-${Date.now()}-${Math.random()}`, fileName: file.name, url: e.target?.result as string, isExisting: false, file: processedFile
          });
        };
        reader.readAsDataURL(processedFile);
      }
    } catch {
      this.snackBar.open('Failed to process images', 'Close');
    }
    event.target.value = '';
  }

  removeMainPhoto() {
    this.selectedMainFile = null;
    this.mainPhotoPreviewUrl = null;
  }

  removeReferencePhoto(index: number) {
    const removedPreview = this.referencePhotoPreviews[index];
    if (!removedPreview.isExisting) {
      const fileIndex = this.selectedReferenceFiles.findIndex(f => f.name === removedPreview.fileName);
      if (fileIndex > -1) this.selectedReferenceFiles.splice(fileIndex, 1);
    }
    this.referencePhotoPreviews.splice(index, 1);
  }

  async onSubmit(): Promise<void> {
    if (this.fieldForm.invalid) {
      this.fieldForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.snackBar.open('Farm ID not found', 'Close', { duration: 3000 });
      this.isLoading = false;
      return;
    }

    const formValue = this.fieldForm.value;
    
    // Clean empty strings and nulls
    const cleanedData: any = {};
    Object.keys(formValue).forEach(key => {
      const value = formValue[key];
      if (value !== '' && value !== null && value !== undefined) {
        cleanedData[key] = value;
      }
    });

    // Parse numeric values
    if (cleanedData.areaHectares) cleanedData.areaHectares = parseFloat(cleanedData.areaHectares);
    if (cleanedData.latitude) cleanedData.latitude = parseFloat(cleanedData.latitude);
    if (cleanedData.longitude) cleanedData.longitude = parseFloat(cleanedData.longitude);

    this.uploadStatus = 'Saving...';
    try {
      let mainImagePath = this.existingMainImagePath;
      if (this.selectedMainFile) {
        this.uploadStatus = 'Uploading main photo...';
        const res = await this.fieldService.uploadImage(farmId, this.selectedMainFile).toPromise();
        if (res?.success && res.data) mainImagePath = res.data.fileName;
      }

      let referencePaths = this.existingReferencePaths.filter(p => this.referencePhotoPreviews.some(rp => rp.isExisting && rp.url === p));
      const newFiles = this.referencePhotoPreviews.filter(p => !p.isExisting && p.file).map(p => p.file!);
      
      if (newFiles.length > 0) {
        this.uploadStatus = 'Uploading reference photos...';
        for (const file of newFiles) {
          const res = await this.fieldService.uploadImage(farmId, file).toPromise();
          if (res?.success && res.data) referencePaths.push(res.data.fileName);
        }
      }

      cleanedData.imagePath = mainImagePath;
      cleanedData.additionalImagePaths = referencePaths.length > 0 ? referencePaths : null;

      let request;
      if (this.mode === 'create') {
        request = this.fieldService.createField(farmId, cleanedData as CreateFieldDto);
      } else {
        request = this.fieldService.updateField(farmId, this.data.field!.id, cleanedData as UpdateFieldDto);
      }

      const response = await request.toPromise();
      if (response?.success) {
        this.dialogRef.close(true);
        this.snackBar.open(
          this.mode === 'create' ? 'Field created successfully' : 'Field updated successfully',
          'Close',
          { duration: 3000, panelClass: ['bg-green-600', 'text-white'] }
        );
      } else {
        this.snackBar.open(response?.message || 'Operation failed', 'Close', {
          duration: 5000,
          panelClass: ['bg-red-600', 'text-white']
        });
      }
    } catch (error) {
      console.error('Error saving field:', error);
      this.snackBar.open('Failed to save field', 'Close', {
        duration: 5000,
        panelClass: ['bg-red-600', 'text-white']
      });
    } finally {
      this.isLoading = false;
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // Helper to check if field has error
  hasError(controlName: string, errorName: string): boolean {
    const control = this.fieldForm.get(controlName);
    return !!control && control.hasError(errorName) && control.touched;
  }
}