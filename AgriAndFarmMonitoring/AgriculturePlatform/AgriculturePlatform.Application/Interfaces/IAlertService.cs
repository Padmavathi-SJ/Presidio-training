// Application/Interfaces/IAlertService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertService
{
    // Query methods
    Task<ApiResponse<PagedResult<AlertDto>>> GetAllAlertsAsync(AlertFilterDto filter, int farmId);
    Task<ApiResponse<AlertDto>> GetAlertByIdAsync(int id, int farmId);
    Task<ApiResponse<AlertStatisticsDto>> GetAlertStatisticsAsync(int farmId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<int>> GetUnresolvedCountAsync(int farmId);
    Task<ApiResponse<IEnumerable<AlertDto>>> GetCriticalAlertsAsync(int farmId);
    
    // Action methods
    Task<ApiResponse<AlertDto>> ResolveAlertAsync(int id, ResolveAlertDto dto, int farmId, int? adminId = null, int? workerId = null);
    
    // Alert creation
    Task<Alert?> CheckAndCreateAlertAsync(int fieldId, int cropCycleId, string sensorType, decimal value, int farmId, int adminId);
    Task<ApiResponse<AlertDashboardDto>> GetDashboardAlertsAsync(int farmId);
}