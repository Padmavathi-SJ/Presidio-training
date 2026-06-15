// Application/Interfaces/IWeatherService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherService
{
    // Read operations (Both Admin & Worker)
    Task<ApiResponse<WeatherDataDto>> GetCurrentWeatherAsync(int fieldId, int farmId, int? adminId = null);
    Task<ApiResponse<WeatherForecastDto>> GetForecastAsync(int fieldId, int farmId);
    Task<ApiResponse<PagedResult<WeatherDataDto>>> GetWeatherHistoryAsync(WeatherHistoryFilterDto filter, int farmId);
    Task<ApiResponse<List<WeatherAlertDto>>> GetActiveWeatherAlertsAsync(int farmId);
    
    // Admin operations
    Task<ApiResponse<WeatherDataDto>> AddManualWeatherEntryAsync(ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> UpdateWeatherDataAsync(int id, ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteWeatherDataAsync(int id, int farmId, int adminId);
    
    // Change return type to Task<ApiResponse<bool>>
    Task<ApiResponse<bool>> RefreshWeatherDataAsync(int fieldId, int farmId, int adminId);
    Task<ApiResponse<bool>> RefreshAllFieldsWeatherAsync(int farmId, int adminId);
    
    // Settings
    Task<ApiResponse<WeatherApiSettingsDto>> GetApiSettingsAsync(int farmId);
    Task<ApiResponse<bool>> UpdateApiSettingsAsync(WeatherApiSettingsDto dto, int farmId);
}