// src/app/features/admin/models/crop-cycle.model.ts

export interface CropCycle {
  id: number;
  farmId: number;
  farmName: string;
  fieldId: number;
  fieldName: string;
  cropType: string;
  plantingDate: string;
  expectedHarvestDate: string | null;
  actualHarvestDate: string | null;
  growthStage: string;
  previousGrowthStage: string | null;
  lastStageUpdate: string | null;
  status: string;
  autoUpdateGrowthStage: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string | null;
  
  growthPercentage?: number;
  daysUntilHarvest?: number;
  isOverdue?: boolean;
  isReadyForHarvest?: boolean;
}

export interface CreateCropCycleDto {
  fieldId: number;
  cropType: string;
  plantingDate: string;
  expectedHarvestDate?: string | null;
  growthStage?: string;
  status?: string;
  autoUpdateGrowthStage?: boolean;
}

export interface UpdateCropCycleDto {
  cropType?: string;
  plantingDate?: string;
  expectedHarvestDate?: string | null;
  growthStage?: string;
  status?: string;
  autoUpdateGrowthStage?: boolean;
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

// ✅ ADD THIS - Crop Types (was missing)
export const CROP_TYPES = [
  'WHEAT',
  'MAIZE',
  'RICE',
  'BARLEY',
  'SOYBEAN',
  'COTTON',
  'HAZELNUT',
  'POTATO',
  'TOMATO',
  'ONION',
  'GRAPE',
  'APPLE'
];

// Growth Stage Options
export const GROWTH_STAGES = [
  'PLANTED',
  'GERMINATION',
  'SEEDLING',
  'VEGETATIVE',
  'FLOWERING',
  'FRUITING',
  'MATURE',
  'READY_FOR_HARVEST',
  'HARVESTED',
  'OVERRIPE'
];

// Status Options
export const CROP_STATUSES = ['ACTIVE', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED', 'PENDING'];

// Color mappings
export const GROWTH_STAGE_COLORS: Record<string, string> = {
  'PLANTED': 'bg-purple-200 text-purple-800',
  'GERMINATION': 'bg-indigo-200 text-indigo-800',
  'SEEDLING': 'bg-blue-200 text-blue-800',
  'VEGETATIVE': 'bg-green-200 text-green-800',
  'FLOWERING': 'bg-yellow-200 text-yellow-800',
  'FRUITING': 'bg-orange-200 text-orange-800',
  'MATURE': 'bg-red-200 text-red-800',
  'READY_FOR_HARVEST': 'bg-emerald-200 text-emerald-800',
  'HARVESTED': 'bg-gray-200 text-gray-800',
  'OVERRIPE': 'bg-rose-200 text-rose-800'
};

export const STATUS_COLORS: Record<string, string> = {
  'ACTIVE': 'bg-green-100 text-green-700',
  'IN_PROGRESS': 'bg-blue-100 text-blue-700',
  'COMPLETED': 'bg-gray-100 text-gray-700',
  'CANCELLED': 'bg-red-100 text-red-700',
  'PENDING': 'bg-yellow-100 text-yellow-700'
};

// Growth stage icons
export const GROWTH_STAGE_ICONS: Record<string, string> = {
  'PLANTED': 'grass',
  'GERMINATION': 'sprout',
  'SEEDLING': 'grass',
  'VEGETATIVE': 'forest',
  'FLOWERING': 'flower',
  'FRUITING': 'apple',
  'MATURE': 'crop',
  'READY_FOR_HARVEST': 'harvest',
  'HARVESTED': 'check_circle',
  'OVERRIPE': 'warning'
};

// Growth stage descriptions
export const GROWTH_STAGE_DESCRIPTIONS: Record<string, string> = {
  'PLANTED': 'Just planted, not yet germinated',
  'GERMINATION': 'Germinated, early growth',
  'SEEDLING': 'Seedling stage',
  'VEGETATIVE': 'Vegetative growth phase',
  'FLOWERING': 'Flowering/pollination phase',
  'FRUITING': 'Fruit/seed development',
  'MATURE': 'Fully grown, not yet ready for harvest',
  'READY_FOR_HARVEST': 'Ready for harvest',
  'HARVESTED': 'Already harvested',
  'OVERRIPE': 'Past optimal harvest time'
};

// ✅ ADD THIS - Helper function to get stage progress percentage
export function getGrowthProgress(stage: string): number {
  const progressMap: Record<string, number> = {
    'PLANTED': 0,
    'GERMINATION': 10,
    'SEEDLING': 25,
    'VEGETATIVE': 50,
    'FLOWERING': 65,
    'FRUITING': 80,
    'MATURE': 90,
    'READY_FOR_HARVEST': 98,
    'HARVESTED': 100,
    'OVERRIPE': 100
  };
  return progressMap[stage] || 0;
}