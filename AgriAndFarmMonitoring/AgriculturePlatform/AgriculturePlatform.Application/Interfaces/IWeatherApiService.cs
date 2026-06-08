// AgriculturePlatform.Application/Interfaces/IWeatherApiService.cs
using AgriculturePlatform.Application.DTOs.Weather;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWeatherApiService
{
    Task<CurrentWeatherDto> GetCurrentWeatherAsync(double latitude, double longitude);
    Task<WeatherForecastDto> GetWeatherForecastAsync(double latitude, double longitude);
    Task<List<WeatherAlertDto>> GetWeatherAlertsAsync(double latitude, double longitude);
}