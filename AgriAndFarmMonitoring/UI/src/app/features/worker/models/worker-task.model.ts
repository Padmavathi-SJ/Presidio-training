export interface WorkerTaskDto {
  id: number;
  workerId: number;
  workerName: string;
  fieldId?: number;
  fieldName?: string;
  cropCycleId?: number;
  cropType?: string;
  taskName?: string;
  assignedDate: string;
  dueDate?: string;
  status?: string;
  priority?: string;
  notes?: string;
  completionNotes?: string;
  isOverdue: boolean;
  completedAt?: string;
  daysToComplete?: number;
}

export interface WorkerTaskStatisticsDto {
  totalTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  completedTasks: number;
  overdueTasks: number;
  highPriorityTasks: number;
  urgentPriorityTasks: number;
  completionRate: number;
  averageCompletionTimeDays: number;
}

export interface UpdateWorkerTaskStatusDto {
  status: string;
  completionNotes?: string;
}

export interface WorkerTaskFilterDto {
  status?: string;
  priority?: string;
  taskName?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  isOverdue?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface PaginatedResult<T> {
  data?: T[];
  items?: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
