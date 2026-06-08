// AgriculturePlatform.Infrastructure/Repositories/TaskRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Infrastructure.Specifications;
using AgriculturePlatform.Application.DTOs.Task;


namespace AgriculturePlatform.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkerTask?> GetByIdAsync(int id, int farmId)
    {
        return await _context.Tasks
            .Include(t => t.Worker)
            .Include(t => t.Field)
            .Include(t => t.CropCycle)
            .FirstOrDefaultAsync(t => t.Id == id && t.FarmId == farmId && !t.IsDeleted);
    }

    public async Task<WorkerTask> CreateAsync(WorkerTask task)
    {
         // Ensure all DateTime fields are UTC
    if (task.AssignedDate.Kind != DateTimeKind.Utc)
        task.AssignedDate = DateTime.SpecifyKind(task.AssignedDate, DateTimeKind.Utc);
    
    if (task.DueDate.HasValue && task.DueDate.Value.Kind != DateTimeKind.Utc)
        task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);
    
    if (task.CreatedAt.Kind != DateTimeKind.Utc)
        task.CreatedAt = DateTime.SpecifyKind(task.CreatedAt, DateTimeKind.Utc);
    
        task.CreatedAt = DateTime.UtcNow;
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(WorkerTask task)
    {
        // Ensure UpdatedAt is UTC
    task.UpdatedAt = DateTime.UtcNow;
    
    if (task.DueDate.HasValue && task.DueDate.Value.Kind != DateTimeKind.Utc)
        task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);

        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(WorkerTask task, int deletedBy)
    {
        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        task.DeletedBy = deletedBy;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<WorkerTask>> GetPagedAsync(
        int farmId,
        int? workerId,
        int? fieldId,
        int? cropCycleId,
        string? status,
        string? priority,
        string? taskName,
        DateTime? assignedDateFrom,
        DateTime? assignedDateTo,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        bool? isOverdue,
        bool? activeOnly,
        PaginationParams paginationParams)
    {
        var specification = new TaskSpecification(
            farmId, workerId, fieldId, cropCycleId, status, priority, taskName,
            assignedDateFrom, assignedDateTo, dueDateFrom, dueDateTo, isOverdue, activeOnly);

        var query = _context.Tasks
            .Include(t => t.Worker)
            .Include(t => t.Field)
            .Include(t => t.CropCycle)
            .Where(specification.Criteria!);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            query = paginationParams.IsDescending
                ? query.OrderByDescending(t => EF.Property<object>(t, paginationParams.SortBy))
                : query.OrderBy(t => EF.Property<object>(t, paginationParams.SortBy));
        }
        else
        {
            query = query.OrderByDescending(t => t.AssignedDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<WorkerTask>
        {
            Items = items,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<IEnumerable<WorkerTask>> GetTasksByWorkerAsync(int workerId, int farmId)
    {
        return await _context.Tasks
            .Include(t => t.Field)
            .Include(t => t.CropCycle)
            .Where(t => t.WorkerId == workerId && t.FarmId == farmId && !t.IsDeleted)
            .OrderByDescending(t => t.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerTask>> GetTasksByFieldAsync(int fieldId, int farmId)
    {
        return await _context.Tasks
            .Include(t => t.Worker)
            .Where(t => t.FieldId == fieldId && t.FarmId == farmId && !t.IsDeleted)
            .OrderByDescending(t => t.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerTask>> GetOverdueTasksAsync(int farmId)
    {
        var today = DateTime.UtcNow.Date;
        
        return await _context.Tasks
            .Include(t => t.Worker)
            .Include(t => t.Field)
            .Where(t => t.FarmId == farmId &&
                       t.DueDate < today &&
                       t.Status != TaskStatusEnum.COMPLETED &&
                       t.Status != TaskStatusEnum.CANCELLED &&
                       !t.IsDeleted)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerTask>> GetTasksByStatusAsync(int farmId, string status)
    {
        var parsedStatus = Enum.Parse<TaskStatusEnum>(status, true);
        
        return await _context.Tasks
            .Include(t => t.Worker)
            .Where(t => t.FarmId == farmId && t.Status == parsedStatus && !t.IsDeleted)
            .OrderByDescending(t => t.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerTask>> GetTaskCompletionHistoryAsync(int farmId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Tasks
            .Where(t => t.FarmId == farmId && t.Status == TaskStatusEnum.COMPLETED && !t.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(t => t.UpdatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.UpdatedAt <= toDate.Value);

        return await query
            .Include(t => t.Worker)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

// In TaskRepository.cs, update the method to return TaskStatisticsDto

public async Task<TaskStatisticsDto> GetTaskStatisticsAsync(int farmId)
{
    var tasks = await _context.Tasks
        .Where(t => t.FarmId == farmId && !t.IsDeleted)
        .ToListAsync();

    return new TaskStatisticsDto
    {
        TotalTasks = tasks.Count,
        PendingTasks = tasks.Count(t => t.Status == TaskStatusEnum.PENDING),
        InProgressTasks = tasks.Count(t => t.Status == TaskStatusEnum.IN_PROGRESS),
        CompletedTasks = tasks.Count(t => t.Status == TaskStatusEnum.COMPLETED),
        OverdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatusEnum.COMPLETED),
        CancelledTasks = tasks.Count(t => t.Status == TaskStatusEnum.CANCELLED),
        TasksByPriority = tasks.GroupBy(t => t.Priority?.ToString() ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Count()),
        TasksByType = tasks.GroupBy(t => t.TaskName?.ToString() ?? "UNKNOWN")
            .ToDictionary(g => g.Key, g => g.Count()),
        AverageCompletionTimeDays = tasks.Where(t => t.Status == TaskStatusEnum.COMPLETED && t.UpdatedAt.HasValue)
            .DefaultIfEmpty()
            .Average(t => t?.UpdatedAt.HasValue == true ? (t.UpdatedAt.Value - t.CreatedAt).TotalDays : 0)
    };
}
    public async Task<int> GetTaskCountByStatusAsync(int farmId, string status)
    {
        var parsedStatus = Enum.Parse<TaskStatusEnum>(status, true);
        
        return await _context.Tasks
            .CountAsync(t => t.FarmId == farmId && t.Status == parsedStatus && !t.IsDeleted);
    }

    public async Task<int> BulkCreateAsync(IEnumerable<WorkerTask> tasks)
    {
         var taskList = tasks.ToList();
    
    foreach (var task in taskList)
    {
        // Ensure all DateTime fields are UTC
        task.AssignedDate = DateTime.UtcNow;
        task.CreatedAt = DateTime.UtcNow;
        
        if (task.DueDate.HasValue && task.DueDate.Value.Kind != DateTimeKind.Utc)
            task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);
    }
        await _context.Tasks.AddRangeAsync(tasks);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> BulkUpdateStatusAsync(IEnumerable<int> taskIds, string status, int updatedBy)
    {
        var parsedStatus = Enum.Parse<TaskStatusEnum>(status, true);
        var tasks = await _context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.Status = parsedStatus;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = updatedBy;
        }

        _context.Tasks.UpdateRange(tasks);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> BulkReassignAsync(IEnumerable<int> taskIds, int newWorkerId, int updatedBy)
    {
        var tasks = await _context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.WorkerId = newWorkerId;
            task.Status = TaskStatusEnum.REASSIGNED;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = updatedBy;
        }

        _context.Tasks.UpdateRange(tasks);
        return await _context.SaveChangesAsync();
    }
}

public class TaskStatistics
{
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CancelledTasks { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByType { get; set; } = new();
    public double AverageCompletionTimeDays { get; set; }
}