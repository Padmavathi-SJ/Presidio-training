// AgriculturePlatform.Application/DTOs/Weather/WeatherAlertCreateDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherAlertCreateDto
{
    public int FieldId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? Temperature { get; set; }
    public double? WindSpeed { get; set; }
    public double? RainfallMm { get; set; }
    public DateTime? ExpiresAt { get; set; }
}