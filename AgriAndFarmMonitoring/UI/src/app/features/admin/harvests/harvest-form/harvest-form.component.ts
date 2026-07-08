import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { AdminHarvestService } from '../services/admin-harvest.service';
import { ImageCompressorService } from '../../../../core/services/image-compressor.service';
import { AuthService } from '../../../../core/services/auth.service';
import { HarvestDto, UpdateHarvestDto } from '../models/admin-harvest.model';

@Component({
  selector: 'app-harvest-form',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, MatProgressBarModule,
    MatFormFieldModule, MatSelectModule, MatInputModule, MatIconModule,
    MatButtonModule, MatTooltipModule, MatProgressSpinnerModule
  ],
  templateUrl: './harvest-form.component.html'
})
export class HarvestFormComponent implements OnInit {
  private harvestService = inject(AdminHarvestService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<HarvestFormComponent>);
  private data = inject(MAT_DIALOG_DATA);
  private snackBar = inject(MatSnackBar);
  private imageCompressor = inject(ImageCompressorService);
  
  farmId = this.authService.getFarmId();
  harvestForm!: FormGroup;
  isSaving = false;
  editingId: number = this.data.editingId;
  editData: HarvestDto = this.data.editData;

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

  readonly qualityGrades = ['A_PLUS', 'A', 'B', 'C', 'D', 'REJECTED'];
  readonly harvestMethods = ['MANUAL', 'MECHANICAL', 'SEMI_MECHANICAL', 'COMBINE'];

  ngOnInit() {
    let formattedDate = '';
    if (this.editData.harvestDate) {
      formattedDate = new Date(this.editData.harvestDate).toISOString().split('T')[0];
    }

    this.harvestForm = this.fb.group({
      harvestDate: [formattedDate],
      quantityKg: [this.editData.quantityKg, [Validators.required, Validators.min(0.1)]],
      qualityGrade: [this.editData.qualityGrade || ''],
      harvestMethod: [this.editData.harvestMethod || ''],
      pricePerKg: [this.editData.pricePerKg],
      notes: [this.editData.notes || ''],
      batchNumber: [this.editData.batchNumber || ''],
      imageCaption: [this.editData.imageCaption || '']
    });

    this.existingMainImagePath = this.editData.imagePath || null;
    this.existingReferencePaths = this.editData.additionalImagePaths || [];
    this.mainPhotoPreviewUrl = this.editData.imagePath || null;
    this.referencePhotoPreviews = (this.editData.additionalImagePaths || []).map((url: string, index: number) => ({
      id: `existing-${index}`, fileName: 'Image', url, isExisting: true
    }));
  }

  closeDialog() {
    this.dialogRef.close();
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

  async save() {
    if (this.harvestForm.invalid) return;

    this.isSaving = true;
    this.uploadStatus = 'Saving...';
    
    try {
      let mainImagePath = this.existingMainImagePath;
      if (this.selectedMainFile) {
        this.uploadStatus = 'Uploading main photo...';
        const res = await this.harvestService.uploadImage(this.farmId, this.selectedMainFile).toPromise();
        if (res?.success && res.data) mainImagePath = res.data.fileName;
      }

      let referencePaths = this.existingReferencePaths.filter(p => this.referencePhotoPreviews.some(rp => rp.isExisting && rp.url === p));
      const newFiles = this.referencePhotoPreviews.filter(p => !p.isExisting && p.file).map(p => p.file!);
      
      if (newFiles.length > 0) {
        this.uploadStatus = 'Uploading reference photos...';
        for (const file of newFiles) {
          const res = await this.harvestService.uploadImage(this.farmId, file).toPromise();
          if (res?.success && res.data) referencePaths.push(res.data.fileName);
        }
      }

      const formVal = this.harvestForm.value;
      const dto: UpdateHarvestDto = {
        ...formVal,
        harvestDate: formVal.harvestDate ? new Date(formVal.harvestDate).toISOString() : null,
        imagePath: mainImagePath,
        additionalImagePaths: referencePaths.length > 0 ? referencePaths : null
      };

      this.uploadStatus = 'Updating harvest...';
      const updateRes = await this.harvestService.updateHarvest(this.farmId, this.editingId, dto).toPromise();
      if (updateRes?.success) {
        this.snackBar.open('Harvest updated', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      } else {
        this.snackBar.open(updateRes?.message || 'Update failed', 'Close');
      }
    } catch {
      this.snackBar.open('An error occurred', 'Close');
    } finally {
      this.isSaving = false;
    }
  }

  formatQualityGrade(grade: string) {
    const m: any = { 'A_PLUS': 'A+', 'A': 'A', 'B': 'B', 'C': 'C', 'D': 'D', 'REJECTED': 'Rejected' };
    return m[grade] || grade;
  }
  
  formatHarvestMethod(method: string) {
    return method.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }
}
