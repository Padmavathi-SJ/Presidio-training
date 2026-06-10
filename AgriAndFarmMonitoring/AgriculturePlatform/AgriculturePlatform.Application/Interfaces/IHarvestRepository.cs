// Application/Interfaces/IHarvestRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Application.DTOs.Harvest;

namespace AgriculturePlatform.Application.Interfaces;

public interface IHarvestRepository
{
    // Basic CRUD
    Task<Harvest?> GetByIdAsync(int id, int farmId);
    Task<Harvest> CreateAsync(Harvest harvest);
    Task UpdateAsync(Harvest harvest);
    Task SoftDeleteAsync(Harvest harvest, int deletedBy);
    Task<bool> ExistsAsync(int id, int farmId);
    
    // Query methods
    Task<PagedResult<Harvest>> GetPagedAsync(
        int farmId,
        int? fieldId,
        int? cropCycleId,
        int? workerId,
        string? approvalStatus,
        string? qualityGrade,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted,
        PaginationParams paginationParams);
    
    Task<IEnumerable<Harvest>> GetByFieldAsync(int fieldId, int farmId);
    Task<IEnumerable<Harvest>> GetByCropCycleAsync(int cropCycleId, int farmId);
    Task<IEnumerable<Harvest>> GetByWorkerAsync(int workerId, int farmId);
    Task<IEnumerable<Harvest>> GetPendingApprovalsAsync(int farmId);
    Task<IEnumerable<Harvest>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate);
    
    // Statistics
    Task<YieldStatisticsDto> GetYieldStatisticsAsync(int farmId, int? cropCycleId, DateTime? fromDate, DateTime? toDate);
    Task<Dictionary<string, decimal>> GetYieldByFieldAsync(int farmId, int year);
    Task<decimal> GetTotalYieldForSeasonAsync(int farmId, int cropCycleId);
    
    // Ownership and permissions
    Task<bool> IsOwnerAsync(int harvestId, int workerId, int farmId);
    Task<bool> CanWorkerEditAsync(int harvestId, int workerId, int farmId);
    
    // Validation
    Task<bool> HasPendingApprovalAsync(int workerId, int farmId);
}