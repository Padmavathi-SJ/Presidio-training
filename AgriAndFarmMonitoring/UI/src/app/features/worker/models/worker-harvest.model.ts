// src/app/features/worker/models/worker-harvest.model.ts

export interface HarvestDto {
  id: number;
  farmId: number;
  farmName: string;
  fieldId: number;
  fieldName: string;
  cropCycleId: number;
  cropType: string;
  harvestedBy?: number;
  harvesterName?: string;
  submittedBy?: number;
  submitterName?: string;
  harvestDate: string;
  quantityKg: number;
  qualityGrade?: string;
  harvestMethod?: string;

  // Approval workflow
  approvalStatus: string;
  approvedBy?: number;
  approverName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  adminNotes?: string;
  workerResponse?: string;

  // Financial
  pricePerKg?: number;
  totalValue?: number;
  batchNumber?: string;
  notes?: string;

  // Image attachments
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
  imageMetadata?: string;

  // Audit
  createdAt: string;
  updatedAt?: string;
  createdBy?: number;

  // Computed
  statusBadgeColor: string;
  formattedQuantity: string;
  formattedTotalValue: string;
}

export interface CreateHarvestDto {
  fieldId: number;
  cropCycleId: number;
  harvestDate: string;
  quantityKg: number;
  qualityGrade?: string;
  harvestMethod?: string;
  notes?: string;
  pricePerKg?: number;
  batchNumber?: string;
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
  imageMetadata?: string;
}

export interface UpdateHarvestDto {
  harvestDate?: string;
  quantityKg?: number;
  qualityGrade?: string;
  harvestMethod?: string;
  notes?: string;
  pricePerKg?: number;
  batchNumber?: string;
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
  imageMetadata?: string;
}

export interface HarvestWorkerResponseDto {
  responseNotes: string;
}

export interface HarvestFilterDto {
  workerId?: number;
  fieldId?: number;
  cropCycleId?: number;
  fromDate?: string;
  toDate?: string;
  approvalStatus?: string;
  qualityGrade?: string;
  harvestMethod?: string;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}
