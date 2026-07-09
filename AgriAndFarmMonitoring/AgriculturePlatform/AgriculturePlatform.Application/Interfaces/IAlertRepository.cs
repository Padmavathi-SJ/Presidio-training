// AgriculturePlatform.Application/Interfaces/IAlertRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(int id, int farmId);
    Task<Alert> CreateAsync(Alert alert);
    Task UpdateAsync(Alert alert);
    Task<PagedResult<Alert>> GetPagedAsync(
        int farmId, int? fieldId, int? cropCycleId, string? alertType,
        string? severity, bool? isResolved, DateTime? fromDate, DateTime? toDate,
        PaginationParams paginationParams, List<int>? allowedFieldIds = null);
    
    Task<int> GetUnresolvedCountAsync(int farmId);
    Task<AlertStatisticsDto> GetStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    Task<int> BulkResolveAsync(IEnumerable<int> alertIds, int resolvedBy);
    Task<IEnumerable<Alert>> GetCriticalAlertsAsync(int farmId);
}