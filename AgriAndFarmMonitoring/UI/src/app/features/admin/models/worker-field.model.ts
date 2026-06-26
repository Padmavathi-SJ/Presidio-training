// src/app/features/admin/models/worker-field.model.ts
export interface WorkerFieldAssignment {
  id: number;
  workerId: number;
  workerName: string;
  workerEmail: string;
  fieldId: number;
  fieldName: string;
  fieldLocation: string | null;
  fieldAreaHectares: number | null;
  fieldSoilType: string | null;
  isActive: boolean;
  assignedDate: string;
  endDate: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface AssignFieldToWorkerDto {
  workerId: number;
  fieldId: number;
  assignedDate?: string;
  endDate?: string;
  notes?: string;
}

export interface WorkerFieldFilterDto {
  workerId?: number | null;
  fieldId?: number | null;
  isActive?: boolean | null;
  assignedDateFrom?: string | null;
  assignedDateTo?: string | null;
  endDateFrom?: string | null;      // ✅ Added
  endDateTo?: string | null;        // ✅ Added
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface WorkerFieldListDto {
  assignmentId: number;
  fieldId: number;
  fieldName: string;
  location: string | null;
  areaHectares: number | null;
  soilType: string | null;
  status: string | null;
  assignedDate: string | null;
  activeCropCount: number;
}

// Status color mapping
export const ASSIGNMENT_STATUS_COLORS: Record<string, string> = {
  'true': 'bg-green-100 text-green-700',
  'false': 'bg-red-100 text-red-700'
};