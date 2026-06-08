// AgriculturePlatform.Application/Interfaces/IWorkerProfileService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerProfileService
{
    Task<ApiResponse<WorkerProfileDto>> GetProfileAsync(int workerId, int farmId);
    Task<ApiResponse<WorkerProfileDto>> UpdateProfileAsync(int workerId, int farmId, UpdateWorkerProfileDto dto);
    Task<ApiResponse<bool>> ChangePasswordAsync(int workerId, int farmId, ChangeWorkerPasswordDto dto);
}