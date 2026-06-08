// AgriculturePlatform.Application/Interfaces/IWorkerFieldService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerFieldService
{
    /// <summary>
    /// Get all fields assigned to the worker
    /// </summary>
    Task<ApiResponse<List<WorkerFieldListDto>>> GetMyAssignedFieldsAsync(int workerId, int farmId);
    
    /// <summary>
    /// Get detailed information about a specific assigned field including crop cycles
    /// </summary>
    Task<ApiResponse<WorkerFieldDetailDto>> GetAssignedFieldDetailAsync(int fieldId, int workerId, int farmId);
}