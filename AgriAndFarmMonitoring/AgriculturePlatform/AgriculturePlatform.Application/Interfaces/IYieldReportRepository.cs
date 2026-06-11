// Application/Interfaces/IYieldReportRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.YieldReports;

namespace AgriculturePlatform.Application.Interfaces;

public interface IYieldReportRepository
{
    // Basic CRUD
    Task<YieldReport?> GetByIdAsync(int id, int farmId);
    Task<YieldReport> CreateAsync(YieldReport report);
    Task UpdateAsync(YieldReport report);
    Task DeleteAsync(YieldReport report);
    Task<bool> ExistsAsync(int id, int farmId);
    
    // Query methods
    Task<PagedResult<YieldReport>> GetPagedAsync(
        int farmId,
        int? cropCycleId,
        int? fieldId,
        string? reportType,
        DateTime? fromDate,
        DateTime? toDate,
        bool? isScheduled,
        PaginationParams paginationParams);
    
    Task<IEnumerable<YieldReport>> GetByCropCycleAsync(int cropCycleId, int farmId);
    Task<IEnumerable<YieldReport>> GetByFieldAsync(int fieldId, int farmId);
    Task<IEnumerable<YieldReport>> GetScheduledReportsAsync(int farmId);
    Task<YieldReport?> GetLatestReportAsync(int farmId, int? cropCycleId, int? fieldId);
    
    // Statistics
    Task<decimal> GetTotalYieldForPeriodAsync(int farmId, DateTime startDate, DateTime endDate, int? fieldId = null);
}