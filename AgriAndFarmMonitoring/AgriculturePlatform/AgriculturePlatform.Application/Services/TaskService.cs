// AgriculturePlatform.Application/Services/TaskService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IExcelTaskService _excelTaskService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public TaskService(
        ITaskRepository taskRepository,
        IWorkerRepository workerRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IAuditLogService auditLogService,
        IExcelTaskService excelTaskService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _workerRepository = workerRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _auditLogService = auditLogService;
        _excelTaskService = excelTaskService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    // =============================================
    // BASIC CRUD OPERATIONS
    // =============================================

    public async Task<ApiResponse<TaskDto>> CreateAsync(CreateTaskDto dto, int farmId, int adminId)
    {
        // Validate worker
        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId, farmId);
        if (worker == null)
        {
            return ApiResponse<TaskDto>.Fail($"Worker with ID {dto.WorkerId} not found");
        }

        // Validate field if provided
        if (dto.FieldId.HasValue && dto.FieldId.Value > 0)
        {
            var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
            if (field == null)
            {
                return ApiResponse<TaskDto>.Fail($"Field with ID {dto.FieldId} not found");
            }
        }

        // Validate crop cycle if provided
        if (dto.CropCycleId.HasValue)
        {
            var cropCycle = await _cropCycleRepository.GetByIdAsync(dto.CropCycleId.Value, farmId);
            if (cropCycle == null)
            {
                return ApiResponse<TaskDto>.Fail($"Crop cycle with ID {dto.CropCycleId} not found");
            }
        }

        var task = new WorkerTask
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = dto.WorkerId,
            FieldId = dto.FieldId,
            CropCycleId = dto.CropCycleId,
            TaskName = Enum.Parse<TaskTypeEnum>(dto.TaskName, true),
            AssignedDate = DateTime.UtcNow,
            DueDate = dto.DueDate,
            Priority = Enum.Parse<TaskPriorityEnum>(dto.Priority ?? "MEDIUM", true),
            Notes = dto.Notes,
            Status = TaskStatusEnum.PENDING,
            CreatedBy = adminId
        };

        var created = await _taskRepository.CreateAsync(task);
        await _auditLogService.LogCreateAsync(farmId, adminId, "Task", created.Id, created, null, null);

        await _notificationService.CreateNotificationAsync(
            farmId,
            null,
            dto.WorkerId,
            "New Task Assigned",
            $"You have been assigned a new task: {created.TaskName}",
            "TaskAssigned",
            "/worker/tasks"
        );

        var result = _mapper.Map<TaskDto>(created);
        result.WorkerName = worker.Name;
        
        return ApiResponse<TaskDto>.Ok(result, "Task created successfully");
    }

    public async Task<ApiResponse<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto, int farmId, int adminId)
    {
        var task = await _taskRepository.GetByIdAsync(id, farmId);
        if (task == null)
        {
            return ApiResponse<TaskDto>.Fail($"Task with ID {id} not found");
        }

        var oldTask = _mapper.Map<WorkerTask>(task);

        if (dto.WorkerId.HasValue && dto.WorkerId != task.WorkerId)
        {
            var worker = await _workerRepository.GetByIdAsync(dto.WorkerId.Value, farmId);
            if (worker == null)
            {
                return ApiResponse<TaskDto>.Fail($"Worker with ID {dto.WorkerId} not found");
            }
            task.WorkerId = dto.WorkerId.Value;
        }

        if (dto.FieldId.HasValue)
            task.FieldId = dto.FieldId;
        if (dto.CropCycleId.HasValue)
            task.CropCycleId = dto.CropCycleId;
        if (!string.IsNullOrWhiteSpace(dto.TaskName))
            task.TaskName = Enum.Parse<TaskTypeEnum>(dto.TaskName, true);
        if (dto.DueDate.HasValue)
            task.DueDate = dto.DueDate;
        if (!string.IsNullOrWhiteSpace(dto.Status))
            task.Status = Enum.Parse<TaskStatusEnum>(dto.Status, true);
        if (!string.IsNullOrWhiteSpace(dto.Priority))
            task.Priority = Enum.Parse<TaskPriorityEnum>(dto.Priority, true);
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            task.Notes = dto.Notes;

        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = adminId;

        await _taskRepository.UpdateAsync(task);
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Task", task.Id, oldTask, task, null, null);

        var result = _mapper.Map<TaskDto>(task);
        return ApiResponse<TaskDto>.Ok(result, "Task updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int farmId, int adminId)
    {
        var task = await _taskRepository.GetByIdAsync(id, farmId);
        if (task == null)
        {
            return ApiResponse<bool>.Fail($"Task with ID {id} not found");
        }

        await _taskRepository.SoftDeleteAsync(task, adminId);
        await _auditLogService.LogSoftDeleteAsync(farmId, adminId, "Task", task.Id, task, null, null);

        return ApiResponse<bool>.Ok(true, "Task deleted successfully");
    }

    public async Task<ApiResponse<TaskDto>> GetByIdAsync(int id, int farmId)
    {
        var task = await _taskRepository.GetByIdAsync(id, farmId);
        if (task == null)
        {
            return ApiResponse<TaskDto>.Fail($"Task with ID {id} not found");
        }

        var result = _mapper.Map<TaskDto>(task);
        result.WorkerName = task.Worker?.Name ?? string.Empty;
        result.FieldName = task.Field?.FieldName ?? string.Empty;
        result.CropType = task.CropCycle?.CropType?.ToString() ?? string.Empty;
        result.IsOverdue = task.DueDate < DateTime.UtcNow && task.Status != TaskStatusEnum.COMPLETED;

        return ApiResponse<TaskDto>.Ok(result);
    }

    // =============================================
    // QUERY METHODS
    // =============================================

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetAllAsync(TaskFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        var pagedResult = await _taskRepository.GetPagedAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.CropCycleId,
            filter.Status,
            filter.Priority,
            filter.TaskName,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.DueDateFrom,
            filter.DueDateTo,
            filter.IsOverdue,
            filter.ActiveOnly,
            paginationParams);

        var dtos = _mapper.Map<List<TaskDto>>(pagedResult.Items);
        
        foreach (var dto in dtos)
        {
            dto.WorkerName = pagedResult.Items.FirstOrDefault(t => t.Id == dto.Id)?.Worker?.Name ?? string.Empty;
            dto.FieldName = pagedResult.Items.FirstOrDefault(t => t.Id == dto.Id)?.Field?.FieldName ?? string.Empty;
            dto.IsOverdue = dto.DueDate < DateTime.UtcNow && dto.Status != "COMPLETED";
        }

        var result = new PagedResult<TaskDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<TaskDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetTasksByWorkerAsync(int workerId, int farmId)
    {
        var tasks = await _taskRepository.GetTasksByWorkerAsync(workerId, farmId);
        var dtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        return ApiResponse<IEnumerable<TaskDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetTasksByFieldAsync(int fieldId, int farmId)
    {
        var tasks = await _taskRepository.GetTasksByFieldAsync(fieldId, farmId);
        var dtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        return ApiResponse<IEnumerable<TaskDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetOverdueTasksAsync(int farmId)
    {
        var tasks = await _taskRepository.GetOverdueTasksAsync(farmId);
        var dtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        return ApiResponse<IEnumerable<TaskDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetActiveTasksAsync(int farmId)
    {
        var tasks = await _taskRepository.GetTasksByStatusAsync(farmId, "PENDING");
        var inProgress = await _taskRepository.GetTasksByStatusAsync(farmId, "IN_PROGRESS");
        var allActive = tasks.Concat(inProgress);
        var dtos = _mapper.Map<IEnumerable<TaskDto>>(allActive);
        return ApiResponse<IEnumerable<TaskDto>>.Ok(dtos);
    }

    // =============================================
    // TASK MANAGEMENT
    // =============================================

    public async Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id, string status, int farmId, int adminId)
    {
        var task = await _taskRepository.GetByIdAsync(id, farmId);
        if (task == null)
        {
            return ApiResponse<TaskDto>.Fail($"Task with ID {id} not found");
        }

        var oldStatus = task.Status?.ToString() ?? string.Empty;
        task.Status = Enum.Parse<TaskStatusEnum>(status, true);
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = adminId;

        await _taskRepository.UpdateAsync(task);
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Task", task.Id, null, new { Status = status }, null, null);

        var result = _mapper.Map<TaskDto>(task);
        return ApiResponse<TaskDto>.Ok(result, $"Task status updated to {status}");
    }

    public async Task<ApiResponse<TaskDto>> ReassignTaskAsync(int id, int newWorkerId, int farmId, int adminId)
    {
        var task = await _taskRepository.GetByIdAsync(id, farmId);
        if (task == null)
        {
            return ApiResponse<TaskDto>.Fail($"Task with ID {id} not found");
        }

        var newWorker = await _workerRepository.GetByIdAsync(newWorkerId, farmId);
        if (newWorker == null)
        {
            return ApiResponse<TaskDto>.Fail($"Worker with ID {newWorkerId} not found");
        }

        var oldWorkerId = task.WorkerId;
        task.WorkerId = newWorkerId;
        task.Status = TaskStatusEnum.REASSIGNED;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = adminId;

        await _taskRepository.UpdateAsync(task);
        await _auditLogService.LogUpdateAsync(farmId, adminId, "Task", task.Id, new { WorkerId = oldWorkerId }, new { WorkerId = newWorkerId }, null, null);

        await _notificationService.CreateNotificationAsync(
            farmId,
            null,
            newWorkerId,
            "New Task Assigned",
            $"A task has been reassigned to you: {task.TaskName}",
            "TaskAssigned",
            "/worker/tasks"
        );

        var result = _mapper.Map<TaskDto>(task);
        result.WorkerName = newWorker.Name;
        
        return ApiResponse<TaskDto>.Ok(result, $"Task reassigned to {newWorker.Name}");
    }

    // =============================================
    // BULK OPERATIONS (JSON)
    // =============================================

    public async Task<ApiResponse<BulkAssignResultDto>> BulkAssignTasksAsync(BulkAssignTaskDto dto, int farmId, int adminId)
    {
        var result = new BulkAssignResultDto { TotalRequests = dto.WorkerIds.Count };
        var validTasks = new List<WorkerTask>();
        var errors = new List<BulkAssignError>();

        foreach (var workerId in dto.WorkerIds)
        {
            var worker = await _workerRepository.GetByIdAsync(workerId, farmId);
            if (worker == null)
            {
                errors.Add(new BulkAssignError 
                { 
                    WorkerId = workerId, 
                    ErrorMessage = $"Worker with ID {workerId} not found in this farm" 
                });
                continue;
            }

            if (dto.FieldId.HasValue && dto.FieldId.Value > 0)
            {
                var field = await _fieldRepository.GetByIdAsync(dto.FieldId.Value, farmId);
                if (field == null)
                {
                    errors.Add(new BulkAssignError 
                    { 
                        WorkerId = workerId, 
                        ErrorMessage = $"Field with ID {dto.FieldId.Value} not found in this farm" 
                    });
                    continue;
                }
            }

            if (!Enum.TryParse<TaskTypeEnum>(dto.TaskName, true, out var taskType))
            {
                errors.Add(new BulkAssignError 
                { 
                    WorkerId = workerId, 
                    ErrorMessage = $"Invalid task name '{dto.TaskName}'" 
                });
                continue;
            }

            validTasks.Add(new WorkerTask
            {
                FarmId = farmId,
                AdminId = adminId,
                WorkerId = workerId,
                FieldId = dto.FieldId > 0 ? dto.FieldId : null,
                CropCycleId = dto.CropCycleId > 0 ? dto.CropCycleId : null,
                TaskName = taskType,
                AssignedDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Priority = Enum.TryParse<TaskPriorityEnum>(dto.Priority, true, out var priority) ? priority : TaskPriorityEnum.MEDIUM,
                Notes = dto.Notes,
                Status = TaskStatusEnum.PENDING,
                CreatedBy = adminId
            });
        }

        if (validTasks.Any())
        {
            var savedCount = await _taskRepository.BulkCreateAsync(validTasks);
            result.SuccessCount = savedCount;
        }

        result.FailedCount = errors.Count;
        result.Errors = errors;

        await _auditLogService.LogAsync(farmId, adminId, null, "BULK_ASSIGN", "Task", null, null, 
            new { Total = result.TotalRequests, Success = result.SuccessCount, Failed = result.FailedCount }, null, null);

        return ApiResponse<BulkAssignResultDto>.Ok(result, 
            $"Assigned {result.SuccessCount} of {result.TotalRequests} tasks");
    }

    public async Task<ApiResponse<BulkAssignResultDto>> BulkUpdateStatusAsync(List<int> taskIds, string status, int farmId, int adminId)
    {
        var result = new BulkAssignResultDto { TotalRequests = taskIds.Count };
        
        var updatedCount = await _taskRepository.BulkUpdateStatusAsync(taskIds, status, adminId);
        result.SuccessCount = updatedCount;
        result.FailedCount = taskIds.Count - updatedCount;

        await _auditLogService.LogAsync(farmId, adminId, null, "BULK_STATUS_UPDATE", "Task", null, null, new { Status = status, Count = updatedCount }, null, null);

        return ApiResponse<BulkAssignResultDto>.Ok(result, $"Updated status for {updatedCount} tasks");
    }

    public async Task<ApiResponse<BulkAssignResultDto>> BulkReassignTasksAsync(List<int> taskIds, int newWorkerId, int farmId, int adminId)
    {
        var result = new BulkAssignResultDto { TotalRequests = taskIds.Count };
        
        var updatedCount = await _taskRepository.BulkReassignAsync(taskIds, newWorkerId, adminId);
        result.SuccessCount = updatedCount;
        result.FailedCount = taskIds.Count - updatedCount;

        await _auditLogService.LogAsync(farmId, adminId, null, "BULK_REASSIGN", "Task", null, null, new { NewWorkerId = newWorkerId, Count = updatedCount }, null, null);

        return ApiResponse<BulkAssignResultDto>.Ok(result, $"Reassigned {updatedCount} tasks");
    }

    // =============================================
    // BULK OPERATIONS (EXCEL)
    // =============================================

// AgriculturePlatform.Application/Services/TaskService.cs

public async Task<ApiResponse<BulkAssignResultDto>> BulkAssignTasksFromExcelAsync(Stream fileStream, int farmId, int adminId)
{
    var result = new BulkAssignResultDto();
    var excelTasks = await _excelTaskService.ReadBulkAssignTasksFromExcelAsync(fileStream);
    
    result.TotalRequests = excelTasks.Count;
    var validTasks = new List<WorkerTask>();
    var errors = new List<BulkAssignError>();

    for (int i = 0; i < excelTasks.Count; i++)
    {
        var excelTask = excelTasks[i];
        var rowNumber = i + 2;

        // ✅ Find worker by name
        var worker = await _workerRepository.GetByNameAsync(excelTask.WorkerName, farmId);
        if (worker == null)
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = 0, 
                ErrorMessage = $"Worker '{excelTask.WorkerName}' not found" 
            });
            continue;
        }

        // ✅ Find field by name (optional)
        int? fieldId = null;
        if (!string.IsNullOrWhiteSpace(excelTask.FieldName))
        {
            var field = await _fieldRepository.GetByNameAsync(excelTask.FieldName, farmId);
            if (field == null)
            {
                errors.Add(new BulkAssignError 
                { 
                    RowNumber = rowNumber, 
                    WorkerId = worker.Id, 
                    ErrorMessage = $"Field '{excelTask.FieldName}' not found" 
                });
                continue;
            }
            fieldId = field.Id;
        }

        // ✅ Find crop cycle by name (optional)
        int? cropCycleId = null;
        if (!string.IsNullOrWhiteSpace(excelTask.CropCycleName))
        {
            var cropCycle = await _cropCycleRepository.GetByNameAsync(excelTask.CropCycleName, farmId);
            if (cropCycle == null)
            {
                errors.Add(new BulkAssignError 
                { 
                    RowNumber = rowNumber, 
                    WorkerId = worker.Id, 
                    ErrorMessage = $"Crop Cycle '{excelTask.CropCycleName}' not found" 
                });
                continue;
            }
            cropCycleId = cropCycle.Id;
        }

        if (!Enum.TryParse<TaskTypeEnum>(excelTask.TaskName, true, out var taskType))
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = worker.Id, 
                ErrorMessage = $"Invalid task name '{excelTask.TaskName}'" 
            });
            continue;
        }

        validTasks.Add(new WorkerTask
        {
            FarmId = farmId,
            AdminId = adminId,
            WorkerId = worker.Id,
            FieldId = fieldId,
            CropCycleId = cropCycleId,
            TaskName = taskType,
            AssignedDate = DateTime.UtcNow,
            DueDate = excelTask.DueDate?.ToUniversalTime(),
            Priority = Enum.TryParse<TaskPriorityEnum>(excelTask.Priority, true, out var priority) ? priority : TaskPriorityEnum.MEDIUM,
            Notes = excelTask.Notes,
            Status = TaskStatusEnum.PENDING,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        });
    }

    if (validTasks.Any())
    {
        var savedCount = await _taskRepository.BulkCreateAsync(validTasks);
        result.SuccessCount = savedCount;
    }

    result.FailedCount = errors.Count;
    result.Errors = errors;

    return ApiResponse<BulkAssignResultDto>.Ok(result, $"Assigned {result.SuccessCount} of {result.TotalRequests} tasks");
}


// AgriculturePlatform.Application/Services/TaskService.cs

public async Task<ApiResponse<BulkAssignResultDto>> BulkUpdateStatusFromExcelAsync(Stream fileStream, int farmId, int adminId)
{
    var result = new BulkAssignResultDto();
    var excelStatusUpdates = await _excelTaskService.ReadBulkStatusUpdateFromExcelAsync(fileStream);
    
    result.TotalRequests = excelStatusUpdates.Count;
    var updatedCount = 0;
    var errors = new List<BulkAssignError>();

    for (int i = 0; i < excelStatusUpdates.Count; i++)
    {
        var update = excelStatusUpdates[i];
        var rowNumber = i + 2;

        // ✅ Find task by name
        var task = await _taskRepository.GetByNameAsync(update.TaskName, farmId);
        if (task == null)
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = 0, 
                ErrorMessage = $"Task '{update.TaskName}' not found" 
            });
            continue;
        }

        if (!Enum.TryParse<TaskStatusEnum>(update.Status, true, out var status))
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = 0, 
                ErrorMessage = $"Invalid status '{update.Status}'" 
            });
            continue;
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = adminId;
        await _taskRepository.UpdateAsync(task);
        updatedCount++;
    }

    result.SuccessCount = updatedCount;
    result.FailedCount = errors.Count;
    result.Errors = errors;

    return ApiResponse<BulkAssignResultDto>.Ok(result, $"Updated {result.SuccessCount} of {result.TotalRequests} tasks");
}

// AgriculturePlatform.Application/Services/TaskService.cs

public async Task<ApiResponse<BulkAssignResultDto>> BulkReassignFromExcelAsync(Stream fileStream, int farmId, int adminId)
{
    var result = new BulkAssignResultDto();
    var excelReassignments = await _excelTaskService.ReadBulkReassignFromExcelAsync(fileStream);
    
    result.TotalRequests = excelReassignments.Count;
    var reassignedCount = 0;
    var errors = new List<BulkAssignError>();

    for (int i = 0; i < excelReassignments.Count; i++)
    {
        var reassign = excelReassignments[i];
        var rowNumber = i + 2;

        // ✅ Find task by name
        var task = await _taskRepository.GetByNameAsync(reassign.TaskName, farmId);
        if (task == null)
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = 0, 
                ErrorMessage = $"Task '{reassign.TaskName}' not found" 
            });
            continue;
        }

        // ✅ Find new worker by name
        var newWorker = await _workerRepository.GetByNameAsync(reassign.NewWorkerName, farmId);
        if (newWorker == null)
        {
            errors.Add(new BulkAssignError 
            { 
                RowNumber = rowNumber, 
                WorkerId = task.WorkerId, 
                ErrorMessage = $"Worker '{reassign.NewWorkerName}' not found" 
            });
            continue;
        }

        task.WorkerId = newWorker.Id;
        task.Status = TaskStatusEnum.REASSIGNED;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = adminId;
        await _taskRepository.UpdateAsync(task);
        reassignedCount++;
    }

    result.SuccessCount = reassignedCount;
    result.FailedCount = errors.Count;
    result.Errors = errors;

    return ApiResponse<BulkAssignResultDto>.Ok(result, $"Reassigned {result.SuccessCount} of {result.TotalRequests} tasks");
}

    // =============================================
    // STATISTICS
    // =============================================

    public async Task<ApiResponse<TaskStatisticsDto>> GetTaskStatisticsAsync(int farmId)
    {
        var stats = await _taskRepository.GetTaskStatisticsAsync(farmId);
        
        var result = new TaskStatisticsDto
        {
            TotalTasks = stats.TotalTasks,
            PendingTasks = stats.PendingTasks,
            InProgressTasks = stats.InProgressTasks,
            CompletedTasks = stats.CompletedTasks,
            OverdueTasks = stats.OverdueTasks,
            CancelledTasks = stats.CancelledTasks,
            TasksByPriority = stats.TasksByPriority,
            TasksByType = stats.TasksByType,
            AverageCompletionTimeDays = Math.Round(stats.AverageCompletionTimeDays, 2)
        };

        return ApiResponse<TaskStatisticsDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<TaskDto>>> GetTaskCompletionHistoryAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var tasks = await _taskRepository.GetTaskCompletionHistoryAsync(farmId, fromDate, toDate);
        var dtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);
        
        foreach (var dto in dtos)
        {
            dto.CompletedDaysAgo = (DateTime.UtcNow - (dto.UpdatedAt ?? dto.CreatedAt)).Days;
        }
        
        return ApiResponse<IEnumerable<TaskDto>>.Ok(dtos);
    }
}