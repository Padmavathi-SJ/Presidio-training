// AgriculturePlatform.Application/Interfaces/IAlertService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertService
{
    Task<ApiResponse<PagedResult<AlertDto>>> GetAllAlertsAsync(AlertFilterDto filter, int farmId);
    Task<ApiResponse<AlertDto>> ResolveAlertAsync(int id, ResolveAlertDto dto, int farmId, int adminId);
    Task<ApiResponse<AlertStatisticsDto>> GetStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<AlertDto>> CheckAndCreateAlertAsync(int fieldId, int cropCycleId, string sensorType, decimal value, int farmId, int adminId);
    Task<ApiResponse<int>> GetUnresolvedCountAsync(int farmId);
    Task<ApiResponse<IEnumerable<AlertDto>>> GetCriticalAlertsAsync(int farmId);
}