// AgriculturePlatform.Application/Interfaces/IWeatherService.cs
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherService
{
    // Worker/Admin read operations (adminId is optional - null for workers)
    Task<ApiResponse<WeatherDataDto>> GetCurrentWeatherAsync(int fieldId, int farmId, int? adminId = null);
    Task<ApiResponse<WeatherForecastDto>> GetForecastAsync(int fieldId, int farmId);
    Task<ApiResponse<PagedResult<WeatherDataDto>>> GetWeatherHistoryAsync(WeatherHistoryFilterDto filter, int farmId);
    Task<ApiResponse<List<WeatherAlertDto>>> GetActiveWeatherAlertsAsync(int farmId);
    
    // Admin only operations (adminId required)
    Task<ApiResponse<WeatherDataDto>> AddManualWeatherEntryAsync(ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> UpdateWeatherDataAsync(int id, ManualWeatherEntryDto dto, int farmId, int adminId);
    Task<ApiResponse<bool>> DeleteWeatherDataAsync(int id, int farmId, int adminId);
    Task<ApiResponse<bool>> RefreshWeatherDataAsync(int fieldId, int farmId, int adminId);
    Task<ApiResponse<bool>> RefreshAllFieldsWeatherAsync(int farmId, int adminId);
    Task<ApiResponse<WeatherApiSettingsDto>> GetApiSettingsAsync(int farmId);
    Task<ApiResponse<bool>> UpdateApiSettingsAsync(WeatherApiSettingsDto dto, int farmId);
}