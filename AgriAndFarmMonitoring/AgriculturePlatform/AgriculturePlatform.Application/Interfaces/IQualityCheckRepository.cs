// Application/Interfaces/IQualityCheckRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Domain.Entities.YieldReports;

namespace AgriculturePlatform.Application.Interfaces;

public interface IQualityCheckRepository
{
    // Basic CRUD
    Task<QualityCheck?> GetByIdAsync(int id, int farmId);
    Task<QualityCheck> CreateAsync(QualityCheck qualityCheck);
    Task UpdateAsync(QualityCheck qualityCheck);
    Task SoftDeleteAsync(QualityCheck qualityCheck, int deletedBy);
    Task<bool> ExistsAsync(int id, int farmId);
    
    // Query methods
    Task<PagedResult<QualityCheck>> GetPagedAsync(
        int farmId,
        int? harvestId,
        int? workerId,
        string? approvalStatus,
        string? finalGrade,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeDeleted,
        PaginationParams paginationParams);
    
    Task<IEnumerable<QualityCheck>> GetByHarvestAsync(int harvestId, int farmId);
    Task<IEnumerable<QualityCheck>> GetByWorkerAsync(int workerId, int farmId);
    Task<IEnumerable<QualityCheck>> GetPendingApprovalsAsync(int farmId);
    Task<IEnumerable<QualityCheck>> GetByDateRangeAsync(int farmId, DateTime fromDate, DateTime toDate);
    
    // Statistics
    Task<QualityStatisticsDto> GetQualityStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    
    // Ownership and permissions
    Task<bool> IsOwnerAsync(int qualityCheckId, int workerId, int farmId);
    Task<bool> CanWorkerEditAsync(int qualityCheckId, int workerId, int farmId);
    Task<bool> HasPendingApprovalAsync(int workerId, int farmId);
}