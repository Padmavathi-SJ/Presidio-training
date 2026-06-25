// src/app/features/admin/models/crop-cycle.model.ts
export interface CropCycle {
  id: number;
  fieldId: number;
  fieldName: string;
  cropType: string;
  plantingDate: string;
  expectedHarvestDate: string | null;
  growthStage: string;
  status: string;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCropCycleDto {
  fieldId: number;
  cropType: string;
  plantingDate: string;
  expectedHarvestDate?: string | null;
  growthStage?: string;
  status?: string;
}

export interface UpdateCropCycleDto {
  cropType?: string;
  plantingDate?: string;
  expectedHarvestDate?: string | null;
  growthStage?: string;
  status?: string;
}

export interface CropCycleFilterDto {
  fieldId?: number;
  cropType?: string;
  growthStage?: string;
  status?: string;
  expectedHarvestDateFrom?: string;
  expectedHarvestDateTo?: string;
  includeDeleted?: boolean;
  activeOnly?: boolean;
  overdueOnly?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

// Crop Type Options
export const CROP_TYPES = [
  'WHEAT', 'MAIZE', 'RICE', 'BARLEY', 'SOYBEAN', 
  'COTTON', 'HAZELNUT', 'POTATO', 'TOMATO', 'ONION', 
  'GRAPE', 'APPLE'
];

// Growth Stage Options
export const GROWTH_STAGES = [
  'GERMINATION', 'SEEDLING', 'VEGETATIVE', 
  'FLOWERING', 'FRUITING', 'MATURITY', 'HARVESTED'
];

// Status Options
export const CROP_STATUSES = ['ACTIVE', 'COMPLETED', 'CANCELLED'];

// Color mappings
export const GROWTH_STAGE_COLORS: Record<string, string> = {
  'GERMINATION': 'bg-purple-100 text-purple-700',
  'SEEDLING': 'bg-blue-100 text-blue-700',
  'VEGETATIVE': 'bg-green-100 text-green-700',
  'FLOWERING': 'bg-yellow-100 text-yellow-700',
  'FRUITING': 'bg-orange-100 text-orange-700',
  'MATURITY': 'bg-red-100 text-red-700',
  'HARVESTED': 'bg-gray-100 text-gray-700'
};

export const STATUS_COLORS: Record<string, string> = {
  'ACTIVE': 'bg-green-100 text-green-700',
  'COMPLETED': 'bg-blue-100 text-blue-700',
  'CANCELLED': 'bg-red-100 text-red-700'
};