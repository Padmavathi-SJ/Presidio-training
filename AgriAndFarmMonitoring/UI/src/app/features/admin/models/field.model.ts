// src/app/features/admin/models/field.model.ts
export interface Field {
  id: number;
  farmId: number;
  farmName: string;
  fieldName: string;
  location: string | null;
  areaHectares: number | null;
  soilType: string | null;
  status: string | null;
  activeCropCount: number;
  createdAt: string;
  updatedAt: string | null;
  isDeleted: boolean;
  deletedAt: string | null;
  latitude: number | null;
  longitude: number | null;
  
  // Image attachments
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
}

export interface CreateFieldDto {
  fieldName: string;
  location?: string | null;
  areaHectares?: number | null;
  soilType?: string | null;
  status?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  
  // Image attachments
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
}

export interface UpdateFieldDto {
  fieldName?: string | null;
  location?: string | null;
  areaHectares?: number | null;
  soilType?: string | null;
  status?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  imagePath?: string;
  thumbnailPath?: string;
  imageCaption?: string;
  additionalImagePaths?: string[];
}

export interface UpdateLocationDto {
  latitude?: number;
  longitude?: number;
}

export interface FieldFilterDto {
  fieldName?: string | null;
  location?: string | null;
  soilType?: string | null;
  status?: string | null;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface FieldStatisticsDto {
  totalFields: number;
  activeFields: number;
  deletedFields: number;
  totalAreaHectares: number;
  fallowFields: number;
  preparingFields: number;
  maintenanceFields: number;
  retiredFields: number;
  totalActiveCrops: number;
  soilTypeDistribution: Record<string, number>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export interface BulkImportResult {
  totalRecords: number;
  successCount: number;
  failedCount: number;
  errors: BulkImportError[];
}

export interface BulkImportError {
  rowNumber: number;
  fieldName: string;
  errorMessage: string;
}

// Field status enum
export enum FieldStatus {
  ACTIVE = 'ACTIVE',
  FALLOW = 'FALLOW',
  PREPARING = 'PREPARING',
  MAINTENANCE = 'MAINTENANCE',
  RETIRED = 'RETIRED'
}

// Soil type enum
export enum SoilType {
  CLAY = 'CLAY',
  SANDY = 'SANDY',
  SILTY = 'SILTY',
  LOAMY = 'LOAMY',
  PEATY = 'PEATY',
  CHALKY = 'CHALKY'
}

// Field status options for dropdown
export const FIELD_STATUS_OPTIONS = [
  { value: 'ACTIVE', label: 'Active' },
  { value: 'FALLOW', label: 'Fallow' },
  { value: 'PREPARING', label: 'Preparing' },
  { value: 'MAINTENANCE', label: 'Maintenance' },
  { value: 'RETIRED', label: 'Retired' }
];

// Soil type options for dropdown
export const SOIL_TYPE_OPTIONS = [
  { value: 'CLAY', label: 'Clay' },
  { value: 'SANDY', label: 'Sandy' },
  { value: 'SILTY', label: 'Silty' },
  { value: 'LOAMY', label: 'Loamy' },
  { value: 'PEATY', label: 'Peaty' },
  { value: 'CHALKY', label: 'Chalky' }
];

// Status color mapping
export const STATUS_COLORS: Record<string, string> = {
  'ACTIVE': 'text-green-600 bg-green-50',
  'FALLOW': 'text-yellow-600 bg-yellow-50',
  'PREPARING': 'text-blue-600 bg-blue-50',
  'MAINTENANCE': 'text-orange-600 bg-orange-50',
  'RETIRED': 'text-gray-600 bg-gray-50'
};

// Soil type color mapping
export const SOIL_TYPE_COLORS: Record<string, string> = {
  'CLAY': 'text-red-600 bg-red-50',
  'SANDY': 'text-yellow-600 bg-yellow-50',
  'SILTY': 'text-blue-600 bg-blue-50',
  'LOAMY': 'text-green-600 bg-green-50',
  'PEATY': 'text-brown-600 bg-brown-50',
  'CHALKY': 'text-gray-600 bg-gray-50'
};