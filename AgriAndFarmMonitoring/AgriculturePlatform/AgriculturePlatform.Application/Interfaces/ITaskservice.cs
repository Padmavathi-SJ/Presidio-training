// AgriculturePlatform.Application/Interfaces/ITaskService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Task;

namespace AgriculturePlatform.Application.Interfaces;

public interface ITaskService
{
    // Basic CRUD
    Task<ApiResponse<TaskDto>> CreateAsync(CreateTaskDto dto, int farmId, int adminId);
    Task<ApiResponse<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int farmId, int adminId);
    Task<ApiResponse<TaskDto>> GetByIdAsync(int id, int farmId);
    
    // Query methods
    Task<ApiResponse<PagedResult<TaskDto>>> GetAllAsync(TaskFilterDto filter, int farmId);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetTasksByWorkerAsync(int workerId, int farmId);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetTasksByFieldAsync(int fieldId, int farmId);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetOverdueTasksAsync(int farmId);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetActiveTasksAsync(int farmId);
    
    // Task management - ✅ Keep only one version (without IP/UserAgent for now)
    Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id, string status, int farmId, int adminId);
    Task<ApiResponse<TaskDto>> ReassignTaskAsync(int id, int newWorkerId, int farmId, int adminId);
    
    // Bulk operations
    Task<ApiResponse<BulkAssignResultDto>> BulkAssignTasksAsync(BulkAssignTaskDto dto, int farmId, int adminId);
    Task<ApiResponse<BulkAssignResultDto>> BulkUpdateStatusAsync(List<int> taskIds, string status, int farmId, int adminId);
    Task<ApiResponse<BulkAssignResultDto>> BulkReassignTasksAsync(List<int> taskIds, int newWorkerId, int farmId, int adminId);
    
    // Statistics
    Task<ApiResponse<TaskStatisticsDto>> GetTaskStatisticsAsync(int farmId);
    Task<ApiResponse<IEnumerable<TaskDto>>> GetTaskCompletionHistoryAsync(int farmId, DateTime? fromDate, DateTime? toDate);

    // Excel bulk operations
    Task<ApiResponse<BulkAssignResultDto>> BulkAssignTasksFromExcelAsync(Stream fileStream, int farmId, int adminId);
    Task<ApiResponse<BulkAssignResultDto>> BulkUpdateStatusFromExcelAsync(Stream fileStream, int farmId, int adminId);
    Task<ApiResponse<BulkAssignResultDto>> BulkReassignFromExcelAsync(Stream fileStream, int farmId, int adminId);
}