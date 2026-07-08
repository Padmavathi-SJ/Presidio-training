export interface QualityCheckDto {
  id: number;
  farmId: number;
  farmName: string;
  harvestId: number;
  harvestBatchNumber?: string;
  harvestQuantity?: number;
  checkedBy?: number;
  checkerName?: string;
  checkDate: string;
  moisturePct?: number;
  defectPct?: number;
  finalGrade?: string;
  notes?: string;
  approvalStatus: string;
  approvedBy?: number;
  approverName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  adminNotes?: string;
  workerResponse?: string;
  statusBadgeColor?: string;
  isPass?: boolean;
  qualityStatus?: string;
}

export interface UpdateQualityCheckDto {
  checkDate: string;
  moisturePct?: number | null;
  defectPct?: number | null;
  finalGrade?: string | null;
  notes?: string | null;
}

export interface QualityCheckApprovalDto {
  approvalStatus: string;
  rejectionReason?: string | null;
  adminNotes?: string | null;
}

export interface QualityCheckFilterDto {
  harvestId?: number;
  workerId?: number;
  approvalStatus?: string;
  finalGrade?: string;
  fromDate?: string;
  toDate?: string;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface MonthlyQualityTrendDto {
  month: string;
  year: number;
  totalChecks: number;
  passCount: number;
  failCount: number;
  passRate: number;
}

export interface QualityStatisticsDto {
  totalChecks: number;
  approvedChecks: number;
  rejectedChecks: number;
  pendingChecks: number;
  passRate: number;
  rejectionRate: number;
  gradeDistribution: Record<string, number>;
  monthlyTrend: MonthlyQualityTrendDto[];
  averageMoisturePct: number;
  averageDefectPct: number;
  minMoisturePct: number;
  maxMoisturePct: number;
  minDefectPct: number;
  maxDefectPct: number;
  qualityByWorker: Record<string, number>;
  qualityByHarvest: Record<string, number>;
}
