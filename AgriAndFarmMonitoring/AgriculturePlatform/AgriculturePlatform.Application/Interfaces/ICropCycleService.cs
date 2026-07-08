// AgriculturePlatform.Application/Interfaces/ICropCycleService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.CropCycle;

namespace AgriculturePlatform.Application.Interfaces;

public interface ICropCycleService
{
    Task<ApiResponse<CropCycleDto>> CreateAsync(CreateCropCycleDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<CropCycleDto>> UpdateAsync(int id, UpdateCropCycleDto dto, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<bool>> SoftDeleteAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
    Task<ApiResponse<CropCycleDto>> GetByIdAsync(int id, int farmId);
    Task<ApiResponse<PagedResult<CropCycleDto>>> GetAllAsync(CropCycleFilterDto filter, int farmId);
    Task<ApiResponse<IEnumerable<CropCycleDto>>> GetOverdueAsync(int farmId);
    Task<bool> ValidateCropCycleOwnershipAsync(int cropCycleId, int farmId);
    Task<ApiResponse<CropCycleDto>> UpdateGrowthStageManuallyAsync(int id, int farmId, int adminId, string ipAddress, string userAgent);
}
