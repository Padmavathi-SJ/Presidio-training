// AgriculturePlatform.Application/Interfaces/ICropCycleRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface ICropCycleRepository
{
    Task<CropCycle?> GetByIdAsync(int id, int farmId, bool includeDeleted = false);
    Task<IEnumerable<CropCycle>> GetAllAsync(int farmId, bool includeDeleted = false);
    Task<CropCycle> CreateAsync(CropCycle cropCycle);
    Task UpdateAsync(CropCycle cropCycle);
    Task SoftDeleteAsync(CropCycle cropCycle, int deletedBy);
    Task<bool> ExistsAsync(int id, int farmId);
    Task<bool> HasActiveCropCycleAsync(int fieldId, int? excludeId = null);
    
    Task<PagedResult<CropCycle>> GetPagedAsync(
        int farmId,
        int? fieldId,
        string? cropType,
        string? growthStage,
        string? status,
        DateTime? expectedHarvestDateFrom,
        DateTime? expectedHarvestDateTo,
        bool? activeOnly,
        bool? overdueOnly,
        bool includeDeleted,
        PaginationParams paginationParams);
    
    Task<int> GetActiveCountByFieldAsync(int fieldId);
    Task<IEnumerable<CropCycle>> GetOverdueCropCyclesAsync(int farmId);
}