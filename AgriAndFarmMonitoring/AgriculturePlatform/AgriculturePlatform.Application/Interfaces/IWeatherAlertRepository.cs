// AgriculturePlatform.Application/Interfaces/IWeatherAlertRepository.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherAlertRepository
{
    // Read operations
    Task<WeatherAlert?> GetByIdAsync(int id, int farmId);
    Task<List<WeatherAlert>> GetActiveAlertsAsync(int farmId, List<int>? allowedFieldIds = null);
    Task<List<WeatherAlert>> GetAlertsByFieldAsync(int fieldId, int farmId);
    Task<List<WeatherAlert>> GetAlertsBySeverityAsync(int farmId, string severity);
    Task<PagedResult<WeatherAlert>> GetPagedAlertsAsync(
        int farmId, 
        int? fieldId, 
        string? severity, 
        bool? isAcknowledged, 
        PaginationParams paginationParams,
        List<int>? allowedFieldIds = null);
    
    // Write operations
    Task<WeatherAlert> CreateAsync(WeatherAlert alert);
    Task UpdateAsync(WeatherAlert alert);
    Task DeleteAsync(WeatherAlert alert);
    Task<int> AcknowledgeAlertAsync(int id, int acknowledgedBy, int farmId);
    Task<int> AcknowledgeAllByFieldAsync(int fieldId, int acknowledgedBy, int farmId);
    
    // Utility
    Task<bool> ExistsAsync(int id, int farmId);
    Task<int> GetActiveAlertCountAsync(int farmId);
    Task<int> GetCriticalAlertCountAsync(int farmId);
}