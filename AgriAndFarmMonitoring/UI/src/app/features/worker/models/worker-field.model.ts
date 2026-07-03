// src/app/features/worker/models/worker-field.model.ts

export interface WorkerFieldList {
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

export interface WorkerFieldDetail {
  assignmentId: number;
  fieldId: number;
  fieldName: string;
  location: string | null;
  areaHectares: number | null;
  soilType: string | null;
  status: string | null;
  assignedDate: string | null;
  notes: string | null;
  createdAt: string;
  latitude: number | null;
  longitude: number | null;
  cropCycles: WorkerCropCycle[];
}

export interface WorkerCropCycle {
  id: number;
  cropType: string | null;
  plantingDate: string | null;
  expectedHarvestDate: string | null;
  growthStage: string | null;
  status: string | null;
  daysSincePlanting: number;
  daysToHarvest: number;
  growthProgressPercent: number | null;
}

// Field Status Colors
export const FIELD_STATUS_COLORS: Record<string, string> = {
  'ACTIVE': 'bg-green-100 text-green-700 border-green-400',
  'FALLOW': 'bg-yellow-100 text-yellow-700 border-yellow-400',
  'PREPARING': 'bg-blue-100 text-blue-700 border-blue-400',
  'MAINTENANCE': 'bg-orange-100 text-orange-700 border-orange-400',
  'RETIRED': 'bg-gray-100 text-gray-700 border-gray-400'
};

// Growth Stage Progress
export const GROWTH_STAGE_PROGRESS: Record<string, number> = {
  'GERMINATION': 10,
  'SEEDLING': 25,
  'VEGETATIVE': 50,
  'FLOWERING': 65,
  'FRUITING': 80,
  'MATURITY': 95,
  'HARVESTED': 100
};

export const GROWTH_STAGE_COLORS: Record<string, string> = {
  'GERMINATION': 'bg-purple-100 text-purple-700',
  'SEEDLING': 'bg-blue-100 text-blue-700',
  'VEGETATIVE': 'bg-green-100 text-green-700',
  'FLOWERING': 'bg-pink-100 text-pink-700',
  'FRUITING': 'bg-orange-100 text-orange-700',
  'MATURITY': 'bg-yellow-100 text-yellow-700',
  'HARVESTED': 'bg-gray-100 text-gray-700'
};