// AgriculturePlatform.Application/DTOs/Weather/WeatherAlertDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherAlertDto
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? Temperature { get; set; }
    public double? WindSpeed { get; set; }
    public double? RainfallMm { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime AlertTime { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Keep ONLY ONE version - remove the duplicate
// public class AcknowledgeWeatherAlertDto { ... }  // ❌ REMOVE THIS DUPLICATE