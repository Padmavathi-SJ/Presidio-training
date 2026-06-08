// AgriculturePlatform.Application/Interfaces/ITaskRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Interfaces;

public interface ITaskRepository
{
    // Basic CRUD
    Task<WorkerTask?> GetByIdAsync(int id, int farmId);
    Task<WorkerTask> CreateAsync(WorkerTask task);
    Task UpdateAsync(WorkerTask task);
    Task SoftDeleteAsync(WorkerTask task, int deletedBy);
    
    // Query methods
    Task<PagedResult<WorkerTask>> GetPagedAsync(
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
        PaginationParams paginationParams);
    
    Task<IEnumerable<WorkerTask>> GetTasksByWorkerAsync(int workerId, int farmId);
    Task<IEnumerable<WorkerTask>> GetTasksByFieldAsync(int fieldId, int farmId);
    Task<IEnumerable<WorkerTask>> GetOverdueTasksAsync(int farmId);
    Task<IEnumerable<WorkerTask>> GetTasksByStatusAsync(int farmId, string status);
    Task<IEnumerable<WorkerTask>> GetTaskCompletionHistoryAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    
    // Statistics
    Task<TaskStatisticsDto> GetTaskStatisticsAsync(int farmId);  // Changed from TaskStatistics to TaskStatisticsDto
    Task<int> GetTaskCountByStatusAsync(int farmId, string status);
    
    // Bulk operations
    Task<int> BulkCreateAsync(IEnumerable<WorkerTask> tasks);
    Task<int> BulkUpdateStatusAsync(IEnumerable<int> taskIds, string status, int updatedBy);
    Task<int> BulkReassignAsync(IEnumerable<int> taskIds, int newWorkerId, int updatedBy);
}