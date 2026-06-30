// src/app/features/admin/models/task.model.ts
export interface Task {
  id: number;
  farmId: number;
  workerId: number;
  workerName: string;
  fieldId: number | null;
  fieldName: string | null;
  cropCycleId: number | null;
  cropType: string | null;
  taskName: string;
  assignedDate: string;
  dueDate: string | null;
  status: string;
  priority: string;
  notes: string | null;
  isOverdue: boolean;
  completedDaysAgo: number | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTaskDto {
  workerId: number;
  fieldId?: number | null;
  cropCycleId?: number | null;
  taskName: string;
  dueDate?: string | null;
  priority?: string;
  notes?: string;
}

export interface UpdateTaskDto {
  workerId?: number | null;
  fieldId?: number | null;
  cropCycleId?: number | null;
  taskName?: string;
  dueDate?: string | null;
  status?: string;
  priority?: string;
  notes?: string;
}

export interface TaskFilterDto {
  workerId?: number | null;
  fieldId?: number | null;
  cropCycleId?: number | null;
  status?: string | null;
  priority?: string | null;
  taskName?: string | null;
  assignedDateFrom?: string | null;
  assignedDateTo?: string | null;
  dueDateFrom?: string | null;
  dueDateTo?: string | null;
  isOverdue?: boolean | null;
  activeOnly?: boolean | null;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface TaskStatisticsDto {
  totalTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  completedTasks: number;
  overdueTasks: number;
  cancelledTasks: number;
  tasksByPriority: Record<string, number>;
  tasksByType: Record<string, number>;
  averageCompletionTimeDays: number;
}

export interface BulkAssignTaskDto {
  workerIds: number[];
  fieldId?: number | null;
  cropCycleId?: number | null;
  taskName: string;
  dueDate?: string | null;
  priority?: string;
  notes?: string;
}

export interface BulkAssignResultDto {
  totalRequests: number;
  successCount: number;
  failedCount: number;
  errors: BulkAssignError[];
}

export interface BulkAssignError {
  rowNumber: number;
  workerId: number;
  errorMessage: string;
}

export interface UpdateTaskStatusDto {
  status: string;
}

export interface ReassignTaskDto {
  newWorkerId: number;
}

export interface BulkStatusUpdateDto {
  taskIds: number[];
  status: string;
}

export interface BulkReassignDto {
  taskIds: number[];
  newWorkerId: number;
}

// Task Status Options
export const TASK_STATUSES = [
  { value: 'PENDING', label: 'Pending', color: 'bg-yellow-100 text-yellow-700' },
  { value: 'IN_PROGRESS', label: 'In Progress', color: 'bg-blue-100 text-blue-700' },
  { value: 'COMPLETED', label: 'Completed', color: 'bg-green-100 text-green-700' },
  { value: 'OVERDUE', label: 'Overdue', color: 'bg-red-100 text-red-700' },
  { value: 'CANCELLED', label: 'Cancelled', color: 'bg-gray-100 text-gray-700' },
  { value: 'REASSIGNED', label: 'Reassigned', color: 'bg-purple-100 text-purple-700' }
];

// Task Priority Options
export const TASK_PRIORITIES = [
  { value: 'LOW', label: 'Low', color: 'bg-gray-100 text-gray-700' },
  { value: 'MEDIUM', label: 'Medium', color: 'bg-blue-100 text-blue-700' },
  { value: 'HIGH', label: 'High', color: 'bg-orange-100 text-orange-700' },
  { value: 'URGENT', label: 'Urgent', color: 'bg-red-100 text-red-700' }
];

// Task Type Options
export const TASK_TYPES = [
  'IRRIGATION', 'FERTILIZING', 'PEST_CONTROL', 'WEEDING', 'PRUNING',
  'HARVESTING', 'MONITORING', 'MAINTENANCE', 'SOIL_PREPARATION',
  'PLANTING', 'QUALITY_CHECK'
];

// Status Color Mapping
export const STATUS_COLORS: Record<string, string> = {
  'PENDING': 'bg-yellow-100 text-yellow-700',
  'IN_PROGRESS': 'bg-blue-100 text-blue-700',
  'COMPLETED': 'bg-green-100 text-green-700',
  'OVERDUE': 'bg-red-100 text-red-700',
  'CANCELLED': 'bg-gray-100 text-gray-700',
  'REASSIGNED': 'bg-purple-100 text-purple-700'
};

export const PRIORITY_COLORS: Record<string, string> = {
  'LOW': 'bg-gray-100 text-gray-700',
  'MEDIUM': 'bg-blue-100 text-blue-700',
  'HIGH': 'bg-orange-100 text-orange-700',
  'URGENT': 'bg-red-100 text-red-700'
};

// src/app/features/admin/models/task.model.ts
// Add these interfaces for Excel operations

export interface BulkAssignTaskExcelDto {
  workerName: string;
  fieldName: string;
  cropCycleName: string;
  taskName: string;
  dueDate?: string | null;
  priority?: string;
  notes?: string;
}

export interface BulkStatusUpdateExcelDto {
  taskName: string;
  status: string;
}

export interface BulkReassignExcelDto {
  taskName: string;
  newWorkerName: string;
}