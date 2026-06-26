// src/app/features/admin/models/worker.model.ts
export interface Worker {
  id: number;
  farmId: number;
  farmName: string;
  name: string;
  email: string;
  phone: string | null;
  role: string;
  hireDate: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
  lastLoginDaysAgo: number | null;
}

export interface CreateWorkerDto {
  name: string;
  email: string;
  phone?: string | null;
  role?: string;
  password?: string;
  hireDate?: string;
}

export interface UpdateWorkerDto {
  name?: string;
  email?: string;
  phone?: string | null;
  role?: string;
  isActive?: boolean;
}

export interface WorkerFilterDto {
  name?: string | null;
  email?: string | null;
  role?: string | null;
  isActive?: boolean | null;
  hireDateFrom?: string | null;
  hireDateTo?: string | null;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface WorkerLoginHistoryDto {
  workerId: number;
  workerName: string;
  lastLoginAt: string | null;
  lastLoginIp: string | null;
  totalLogins: number;
}

export interface WorkerStatisticsDto {
  totalWorkers: number;
  activeWorkers: number;
  inactiveWorkers: number;
  deletedWorkers: number;
  roleDistribution: Record<string, number>;
}

export interface ResetPasswordDto {
  newPassword: string;
  confirmPassword: string;
}

// ✅ Worker Role Options - Added 'Worker' at the top
export const WORKER_ROLES = [
  'Worker',
  'MANAGER', 
  'SUPERVISOR', 
  'OPERATOR', 
  'LABOR', 
  'TECHNICIAN', 
  'DRIVER'
];

// Status Color Mapping
export const STATUS_COLORS: Record<string, string> = {
  'true': 'bg-green-100 text-green-700',
  'false': 'bg-red-100 text-red-700'
};

// ✅ Role Color Mapping - Added 'Worker'
export const ROLE_COLORS: Record<string, string> = {
  'Worker': 'bg-gray-100 text-gray-700',
  'MANAGER': 'bg-purple-100 text-purple-700',
  'SUPERVISOR': 'bg-blue-100 text-blue-700',
  'OPERATOR': 'bg-green-100 text-green-700',
  'LABOR': 'bg-yellow-100 text-yellow-700',
  'TECHNICIAN': 'bg-orange-100 text-orange-700',
  'DRIVER': 'bg-indigo-100 text-indigo-700'
};