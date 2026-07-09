// AgriculturePlatform.Application/Interfaces/IWeatherService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherService
{
    // Read operations (Both Admin & Worker)
    Task<ApiResponse<WeatherDataDto>> GetCurrentWeatherAsync(int fieldId, int farmId, int? adminId = null);
    Task<ApiResponse<WeatherForecastDto>> GetForecastAsync(int fieldId, int farmId);
    Task<ApiResponse<PagedResult<WeatherDataDto>>> GetWeatherHistoryAsync(WeatherHistoryFilterDto filter, int farmId);
    
    // Weather Alert operations
    Task<ApiResponse<List<WeatherAlertDto>>> GetActiveWeatherAlertsAsync(int farmId);
    Task<ApiResponse<PagedResult<WeatherAlertDto>>> GetWeatherAlertsAsync(WeatherAlertFilterDto filter, int farmId);
    Task<ApiResponse<WeatherAlertDto>> GetWeatherAlertByIdAsync(int id, int farmId);
    
    // Admin operations - Weather Data
    Task<ApiResponse<WeatherDataDto>> AddManualWeatherEntryAsync(ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> UpdateWeatherDataAsync(int id, ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteWeatherDataAsync(int id, int farmId, int adminId);
    Task<ApiResponse<bool>> RefreshWeatherDataAsync(int fieldId, int farmId, int adminId);
    Task<ApiResponse<bool>> RefreshAllFieldsWeatherAsync(int farmId, int adminId);
    
    // Admin operations - Weather Alerts
    Task<ApiResponse<WeatherAlertDto>> CreateWeatherAlertAsync(WeatherAlertCreateDto dto, int farmId, int adminId);
    Task<ApiResponse<WeatherAlertDto>> UpdateWeatherAlertAsync(int id, WeatherAlertUpdateDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteWeatherAlertAsync(int id, int farmId, int adminId);
    Task<ApiResponse<bool>> AcknowledgeWeatherAlertAsync(int id, int farmId, int adminId);
    Task<ApiResponse<bool>> AcknowledgeAllAlertsForFieldAsync(int fieldId, int farmId, int adminId);
    Task<ApiResponse<bool>> ResolveWeatherAlertAsync(int id, ResolveWeatherAlertDto dto, int farmId, int workerId);
    
    // Settings
    Task<ApiResponse<WeatherApiSettingsDto>> GetApiSettingsAsync(int farmId);
    Task<ApiResponse<bool>> UpdateApiSettingsAsync(WeatherApiSettingsDto dto, int farmId);
Task<ApiResponse<WeatherStatisticsDto>> GetWeatherStatisticsAsync(int farmId);
}