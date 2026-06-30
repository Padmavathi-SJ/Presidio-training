// AgriculturePlatform.Application/DTOs/Weather/WeatherAlertUpdateDto.cs
namespace AgriculturePlatform.Application.DTOs.Weather;

public class WeatherAlertUpdateDto
{
    public string? Severity { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public bool? IsAcknowledged { get; set; }
    public DateTime? ExpiresAt { get; set; }
}