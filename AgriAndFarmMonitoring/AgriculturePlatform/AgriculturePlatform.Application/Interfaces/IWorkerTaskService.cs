// AgriculturePlatform.Application/Interfaces/IWorkerTaskService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.WorkerTask;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerTaskService
{
    // Get tasks for the current worker
    Task<ApiResponse<PagedResult<WorkerTaskDto>>> GetMyTasksAsync(WorkerTaskFilterDto filter, int workerId, int farmId);
    
    // Get task by ID (with access validation)
    Task<ApiResponse<WorkerTaskDto>> GetTaskByIdAsync(int taskId, int workerId, int farmId);
    
    // Update task status (PENDING -> IN_PROGRESS -> COMPLETED)
    Task<ApiResponse<WorkerTaskDto>> UpdateTaskStatusAsync(int taskId, UpdateWorkerTaskStatusDto dto, int workerId, int farmId);
    
    // Get task history/completed tasks
    Task<ApiResponse<PagedResult<WorkerTaskDto>>> GetTaskHistoryAsync(WorkerTaskFilterDto filter, int workerId, int farmId);
    
    // Get task statistics for worker dashboard
    Task<ApiResponse<WorkerTaskStatisticsDto>> GetTaskStatisticsAsync(int workerId, int farmId);
}