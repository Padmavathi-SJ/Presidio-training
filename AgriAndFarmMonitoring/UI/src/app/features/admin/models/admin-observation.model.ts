export interface ObservationDto {
  id: number;
  farmId: number;
  farmName: string;
  fieldId: number;
  fieldName: string;
  cropCycleId?: number;
  cropType?: string;
  workerId?: number;
  workerName?: string;
  observationDate: string;
  cropHealth?: string;
  pestType?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
  
  validationStatus: string;
  adminNotes?: string;
  workerResponse?: string;
  validatedBy?: number;
  validatorName?: string;
  validatedAt?: string;
  flagReason?: string;
  
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
  imageMetadata?: string;
  isImageVerified: boolean;
  imageVerificationNotes?: string;
  
  pestDetected: boolean;
  hasImages: boolean;
  imageCount: number;
  isPending: boolean;
  isVerified: boolean;
  isQuestioned: boolean;
  isInvalid: boolean;
  statusBadgeColor: string;
}

export interface UpdateObservationDto {
  fieldId?: number | null;
  cropCycleId?: number | null;
  observationDate: string;
  cropHealth?: string | null;
  pestType?: string | null;
  notes?: string | null;
  imagePath?: string | null;
  thumbnailPath?: string | null;
  imageCaption?: string | null;
  additionalImagePaths?: string[] | null;
  imageMetadata?: string | null;
}

export interface ObservationValidationDto {
  validationStatus: string;
  adminNotes?: string;
  flagReason?: string;
}

export interface ObservationValidationSummaryDto {
  total: number;
  pending: number;
  questioned: number;
  verified: number;
  invalid: number;
}

export interface ObservationFilterDto {
  workerId?: number;
  fieldId?: number;
  cropCycleId?: number;
  fromDate?: string;
  toDate?: string;
  cropHealth?: string;
  includeDeleted?: boolean;
  validationStatus?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}
