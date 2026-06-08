// AgriculturePlatform.Application/DTOs/Weather/WeatherApiSettingsDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherApiSettingsDto
{
    public string ApiProvider { get; set; } = "OpenWeatherMap";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int UpdateIntervalMinutes { get; set; } = 60;
    public bool AutoUpdateEnabled { get; set; } = true;
}