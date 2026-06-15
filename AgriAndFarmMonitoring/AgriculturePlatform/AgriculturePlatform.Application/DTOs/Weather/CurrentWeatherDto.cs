// AgriculturePlatform.Application/DTOs/Weather/CurrentWeatherDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class CurrentWeatherDto
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Condition { get; set; } = string.Empty;
    public double? RainfallMm { get; set; }
    public DateTime ObservedAt { get; set; }
}