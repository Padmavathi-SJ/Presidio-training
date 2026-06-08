// AgriculturePlatform.Application/Interfaces/IWorkerService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerService
{
    // Basic CRUD
    Task<ApiResponse<WorkerDto>> CreateAsync(CreateWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<WorkerDto>> UpdateAsync(int id, UpdateWorkerDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<WorkerDto>> GetByIdAsync(int id, int farmId);
    Task<ApiResponse<PagedResult<WorkerDto>>> GetAllAsync(WorkerFilterDto filter, int farmId);
    
    // Activation
    Task<ApiResponse<bool>> ActivateAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> DeactivateAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    
    // Password management
    Task<ApiResponse<bool>> ResetPasswordAsync(int id, int farmId, int adminId, string newPassword, string ipAddress, string userAgent);
    
    // Login tracking
    Task<ApiResponse<WorkerLoginHistoryDto>> GetLoginHistoryAsync(int id, int farmId);
    
    // Validation
    Task<bool> ValidateWorkerOwnershipAsync(int workerId, int farmId);
}