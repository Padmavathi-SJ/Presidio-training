// AgriculturePlatform.Application/Services/WorkerTaskService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.WorkerTask;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class WorkerTaskService : IWorkerTaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public WorkerTaskService(
        ITaskRepository taskRepository,
        IWorkerRepository workerRepository,
        IAuditLogService auditLogService,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _workerRepository = workerRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<WorkerTaskDto>>> GetMyTasksAsync(WorkerTaskFilterDto filter, int workerId, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = filter.SortBy,
            IsDescending = filter.IsDescending
        };

        // Get tasks assigned to this worker, excluding completed
        var pagedResult = await _taskRepository.GetPagedAsync(
            farmId,
            workerId,  // Only this worker's tasks
            null,      // fieldId
            null,      // cropCycleId
            filter.Status,
            filter.Priority,
            filter.TaskName,
            null,      // assignedDateFrom
            null,      // assignedDateTo
            filter.DueDateFrom,
            filter.DueDateTo,
            filter.IsOverdue,
            true,      // activeOnly - exclude completed
            paginationParams);

        var dtos = _mapper.Map<List<WorkerTaskDto>>(pagedResult.Items);
        
        // Calculate additional fields
        foreach (var dto in dtos)
        {
            dto.IsOverdue = dto.DueDate < DateTime.UtcNow && dto.Status != "COMPLETED";
            if (dto.Status == "COMPLETED" && dto.CompletedAt.HasValue)
            {
                dto.DaysToComplete = (int)(dto.CompletedAt.Value - dto.AssignedDate).TotalDays;
            }
        }

        var result = new PagedResult<WorkerTaskDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<WorkerTaskDto>>.Ok(result);
    }

    public async Task<ApiResponse<WorkerTaskDto>> GetTaskByIdAsync(int taskId, int workerId, int farmId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, farmId);
        
        if (task == null)
        {
            return ApiResponse<WorkerTaskDto>.Fail($"Task with ID {taskId} not found");
        }

        // Verify worker has access to this task
        if (task.WorkerId != workerId)
        {
            return ApiResponse<WorkerTaskDto>.Fail("You don't have permission to view this task");
        }

        var result = _mapper.Map<WorkerTaskDto>(task);
        result.IsOverdue = task.DueDate < DateTime.UtcNow && task.Status != TaskStatusEnum.COMPLETED;
        
        if (task.Status == TaskStatusEnum.COMPLETED && task.UpdatedAt.HasValue)
        {
            result.CompletedAt = task.UpdatedAt;
            result.DaysToComplete = (int)(task.UpdatedAt.Value - task.CreatedAt).TotalDays;
        }

        return ApiResponse<WorkerTaskDto>.Ok(result);
    }

    public async Task<ApiResponse<WorkerTaskDto>> UpdateTaskStatusAsync(int taskId, UpdateWorkerTaskStatusDto dto, int workerId, int farmId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, farmId);
        
        if (task == null)
        {
            return ApiResponse<WorkerTaskDto>.Fail($"Task with ID {taskId} not found");
        }

        // Verify worker has access to this task
        if (task.WorkerId != workerId)
        {
            return ApiResponse<WorkerTaskDto>.Fail("You don't have permission to update this task");
        }

        var oldStatus = task.Status?.ToString() ?? string.Empty;
        var newStatus = Enum.Parse<TaskStatusEnum>(dto.Status, true);

        // Validate status progression
        if (!IsValidStatusTransition(oldStatus, newStatus.ToString()))
        {
            return ApiResponse<WorkerTaskDto>.Fail($"Invalid status transition from {oldStatus} to {newStatus}");
        }

        task.Status = newStatus;
        task.UpdatedAt = DateTime.UtcNow;
        task.UpdatedBy = workerId;

        // Add completion notes if provided and status is COMPLETED
        if (newStatus == TaskStatusEnum.COMPLETED && !string.IsNullOrWhiteSpace(dto.CompletionNotes))
        {
            task.Notes = string.IsNullOrEmpty(task.Notes) 
                ? dto.CompletionNotes 
                : $"{task.Notes}\nCompletion Notes: {dto.CompletionNotes}";
        }

        await _taskRepository.UpdateAsync(task);

        // Audit log
        await _auditLogService.LogAsync(farmId, null, workerId, "UPDATE_TASK_STATUS", "Task", task.Id, 
            new { Status = oldStatus }, new { Status = newStatus.ToString() }, null, null);

        var result = _mapper.Map<WorkerTaskDto>(task);
        result.IsOverdue = task.DueDate < DateTime.UtcNow && task.Status != TaskStatusEnum.COMPLETED;
        
        if (task.Status == TaskStatusEnum.COMPLETED)
        {
            result.CompletedAt = task.UpdatedAt;
        }

        return ApiResponse<WorkerTaskDto>.Ok(result, $"Task status updated to {newStatus}");
    }

    public async Task<ApiResponse<PagedResult<WorkerTaskDto>>> GetTaskHistoryAsync(WorkerTaskFilterDto filter, int workerId, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 10,
            SortBy = "UpdatedAt",
            IsDescending = true
        };

        // Get completed tasks for this worker
        var pagedResult = await _taskRepository.GetPagedAsync(
            farmId,
            workerId,
            null,
            null,
            "COMPLETED",  // Only completed tasks
            filter.Priority,
            filter.TaskName,
            null,
            null,
            filter.DueDateFrom,
            filter.DueDateTo,
            null,
            false,
            paginationParams);

        var dtos = _mapper.Map<List<WorkerTaskDto>>(pagedResult.Items);
        
        foreach (var dto in dtos)
        {
            if (dto.CompletedAt.HasValue)
            {
                dto.DaysToComplete = (int)(dto.CompletedAt.Value - dto.AssignedDate).TotalDays;
            }
        }

        var result = new PagedResult<WorkerTaskDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<WorkerTaskDto>>.Ok(result);
    }

    public async Task<ApiResponse<WorkerTaskStatisticsDto>> GetTaskStatisticsAsync(int workerId, int farmId)
    {
        var tasks = await _taskRepository.GetTasksByWorkerAsync(workerId, farmId);
        var taskList = tasks.ToList();

        var stats = new WorkerTaskStatisticsDto
        {
            TotalTasks = taskList.Count,
            PendingTasks = taskList.Count(t => t.Status == TaskStatusEnum.PENDING),
            InProgressTasks = taskList.Count(t => t.Status == TaskStatusEnum.IN_PROGRESS),
            CompletedTasks = taskList.Count(t => t.Status == TaskStatusEnum.COMPLETED),
            OverdueTasks = taskList.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatusEnum.COMPLETED),
            HighPriorityTasks = taskList.Count(t => t.Priority == TaskPriorityEnum.HIGH),
            UrgentPriorityTasks = taskList.Count(t => t.Priority == TaskPriorityEnum.URGENT),
            CompletionRate = taskList.Count > 0 
                ? Math.Round((double)taskList.Count(t => t.Status == TaskStatusEnum.COMPLETED) / taskList.Count * 100, 2)
                : 0,
            AverageCompletionTimeDays = taskList.Where(t => t.Status == TaskStatusEnum.COMPLETED && t.UpdatedAt.HasValue)
                .DefaultIfEmpty()
                .Average(t => t?.UpdatedAt.HasValue == true ? (t.UpdatedAt.Value - t.CreatedAt).TotalDays : 0)
        };

        return ApiResponse<WorkerTaskStatisticsDto>.Ok(stats);
    }

    private bool IsValidStatusTransition(string oldStatus, string newStatus)
    {
        var validTransitions = new Dictionary<string, List<string>>
        {
            { "PENDING", new List<string> { "IN_PROGRESS", "CANCELLED" } },
            { "IN_PROGRESS", new List<string> { "COMPLETED", "PENDING", "CANCELLED" } },
            { "COMPLETED", new List<string>() }, // Cannot change from COMPLETED
            { "CANCELLED", new List<string>() }  // Cannot change from CANCELLED
        };

        if (!validTransitions.ContainsKey(oldStatus))
            return false;

        return validTransitions[oldStatus].Contains(newStatus);
    }
}