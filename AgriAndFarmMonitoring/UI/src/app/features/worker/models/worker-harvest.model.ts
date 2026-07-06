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

// src/app/features/worker/models/worker-harvest.model.ts

export interface UpdateHarvestDto {
  // ✅ Add missing fields that can be updated
  fieldId?: number;        // ✅ Added
  cropCycleId?: number;    // ✅ Added
  harvestDate?: string;
  quantityKg?: number;
  qualityGrade?: string | null;
  harvestMethod?: string | null;
  notes?: string | null;
  pricePerKg?: number | null;
  batchNumber?: string | null;
  imagePath?: string | null;  // ✅ Changed to allow null
  thumbnailPath?: string | null;
  imageCaption?: string | null;
  additionalImagePaths?: string[] | null;  // ✅ Changed to allow null
  imageMetadata?: string | null;
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
