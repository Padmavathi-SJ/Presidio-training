export interface HarvestDto {
  id: number;
  farmId: number;
  farmName?: string;
  fieldId: number;
  fieldName?: string;
  cropCycleId: number;
  cropType?: string;
  harvestedBy?: number;
  harvesterName?: string;
  submittedBy?: number;
  submitterName?: string;
  harvestDate: string;
  quantityKg: number;
  qualityGrade?: string;
  harvestMethod?: string;
  
  approvalStatus: string;
  approvedBy?: number;
  approverName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  adminNotes?: string;
  workerResponse?: string;
  
  notes?: string;
  pricePerKg?: number;
  totalValue?: number;
  batchNumber?: string;
  
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
  imageMetadata?: string;
  
  hasImages: boolean;
  imageCount: number;
  isPending: boolean;
  isApproved: boolean;
  isRejected: boolean;
  needsChanges: boolean;
  statusBadgeColor: string;
  
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
  deletedAt?: string;
}

export interface UpdateHarvestDto {
  harvestDate?: string | null;
  quantityKg?: number | null;
  qualityGrade?: string | null;
  harvestMethod?: string | null;
  notes?: string | null;
  pricePerKg?: number | null;
  batchNumber?: string | null;
  
  imagePath?: string | null;
  thumbnailPath?: string | null;
  imageCaption?: string | null;
  additionalImagePaths?: string[] | null;
  imageMetadata?: string | null;
}

export interface HarvestApprovalDto {
  approvalStatus: string;
  adminNotes?: string;
  rejectionReason?: string;
}

export interface HarvestFilterDto {
  fieldId?: number;
  cropCycleId?: number;
  workerId?: number;
  fromDate?: string;
  toDate?: string;
  qualityGrade?: string;
  harvestMethod?: string;
  approvalStatus?: string;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface YieldStatisticsDto {
  totalYieldKg: number;
  averageYieldPerHectare: number;
  totalHarvests: number;
  yieldByField: { [key: string]: number };
  yieldByCropType: { [key: string]: number };
  monthlyTrend: MonthlyYieldDto[];
  qualityDistribution: { [key: string]: number };
  harvestMethodDistribution: { [key: string]: number };
  totalValue: number;
  averagePricePerKg: number;
  previousSeasonYield: number;
  yieldGrowthPercentage: number;
}

export interface MonthlyYieldDto {
  month: string;
  year: number;
  yieldKg: number;
  harvestCount: number;
  averagePrice: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
